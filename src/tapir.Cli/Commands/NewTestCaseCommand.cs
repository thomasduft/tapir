using Fluid;

using McMaster.Extensions.CommandLineUtils;

using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli;

internal class NewTestCaseCommand : CommandLineApplication
{
  private readonly CommandArgument<string> _testCaseId;
  private readonly CommandArgument<string> _title;
  private readonly CommandOption<bool> _useSimpleTemplate;

  public NewTestCaseCommand()
  {
    Name = "new";
    Description = "Creates a new Test Case definition (i.e. test-case TC-Audit-001 \"My TestCase Title\").";

    _testCaseId = Argument<string>(
      "test-case-id",
      "The Test Case ID (e.g. TC-Audit-001).",
      cfg => cfg.IsRequired()
    );

    _title = Argument<string>(
      "title",
      "The Test Case title.",
      cfg => cfg.DefaultValue = "A TestCase Title"
    );

    _useSimpleTemplate = Option<bool>(
      "--use-simple-template",
      "Uses the simple Test Case template.",
      CommandOptionType.NoValue,
      cfg => cfg.DefaultValue = false,
      true
    );

    OnExecuteAsync(ExecuteAsync);
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    var testCaseId = _testCaseId.Value;
    var title = _title.Value;

    var testCaseTemplate = _useSimpleTemplate.ParsedValue
      ? Templates.TestCaseSimple
      : Templates.TestCase;

    var content = await GetContent(
      testCaseTemplate,
      new
      {
        TestCaseId = testCaseId,
        TestCaseTitle = title!,
        Date = DateStringProvider.GetDateString(),
        Author = UserNameProvider.GetUserName()
      }
    );

    await File.WriteAllTextAsync(
      CreateFileName(testCaseId!),
      content,
      cancellationToken
    );

    return await Task.FromResult(0);
  }

  private async ValueTask<string> GetContent(
    string templateName,
    object model
  )
  {
    var source = ResourceLoader.GetTemplate(templateName);
    var template = new FluidParser().Parse(source);

    return await template.RenderAsync(new TemplateContext(model));
  }

  private string CreateFileName(string title)
  {
    return Path.Combine(
      Environment.CurrentDirectory,
      $"{SanitizeFileName(title)}.md"
    );
  }

  private string SanitizeFileName(string title)
  {
    return title.Replace(' ', '-');
  }
}