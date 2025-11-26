using Fluid;

using McMaster.Extensions.CommandLineUtils;

using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli;

internal class ManCommand : CommandLineApplication
{
  private readonly IEnumerable<IValidator> _validators;

  public ManCommand(
    IEnumerable<IValidator> validators
  )
  {
    _validators = validators;

    Name = "man";
    Description = "Displays a man page that helps writing the Test-Data syntax for a Test Case.";

    OnExecuteAsync(ExecuteAsync);
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    var content = await GetContent(
      Templates.Manual,
      new
      {
        Actions =  _validators
                    .OrderBy(v => v.Name)
                    .ToArray()
      }
    );

    Console.Write(content);

    return await Task.FromResult(0);
  }

  private async ValueTask<string> GetContent(
    string templateName,
    object model
  )
  {
    var source = ResourceLoader.GetTemplate(templateName);
    var template = new FluidParser().Parse(source);

    var options = new TemplateOptions();
    options.MemberAccessStrategy.Register<IValidator>();

    return await template
      .RenderAsync(new TemplateContext(model, options));
  }
}
