using Fluid;

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli;

internal class NewTestStepCommand : CommandLineApplication
{
  private readonly CommandArgument<string> _testCaseId;
  private readonly CommandOption<string> _inputDirectory;
  public NewTestStepCommand()
  {
    Name = "new-step";
    Description = "Appends a new Test Step to an existing Test Case definition (i.e. new-step TC-Audit-001).";

    _testCaseId = Argument<string>(
      "test-case-id",
      "The Test Case ID (e.g. TC-Audit-001).",
      cfg => cfg.IsRequired()
    );

    _inputDirectory = Option<string>(
      "-i|--input-directory",
      "The input directory where the Test Case definition is located.",
      CommandOptionType.SingleValue,
      cfg => cfg.DefaultValue = ".",
      true
    );

    OnExecuteAsync(ExecuteAsync);
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    // 1. Locate file
    var files = TestCaseDefinitionFinder.FindFiles(
      _inputDirectory.ParsedValue,
      _testCaseId.ParsedValue
    );

    if (files.Length == 0)
    {
      Log.Logger.Error($"No file found for Test Case ID '{_testCaseId.ParsedValue}' in directory '{_inputDirectory.ParsedValue}'.");
      return 1;
    }

    if (files.Length != 1)
    {
      Log.Logger.Error($"No suitable Test Case ID '{_testCaseId.ParsedValue}' in directory '{_inputDirectory.ParsedValue}'  found.");
      return 1;
    }

    var path = files[0];

    // 2. Load Test Step template;
    var content = await GetContent(
      Templates.TestStep,
      new { }
    );

    // 3. Open file and add content after the last markdown table
    var testCaseLines = await File.ReadAllLinesAsync(path, cancellationToken);

    var lastTableIndex = testCaseLines
      .Select((line, index) => new { line, index })
      .Where(x => x.line.Trim().StartsWith("|") && x.line.Trim().EndsWith("|"))
      .Select(x => x.index)
      .LastOrDefault();

    if (lastTableIndex == 0 || lastTableIndex == testCaseLines.Length - 1)
    {
      Log.Logger.Error($"The Test Case file '{path}' does not contain any markdown table to append the new Test Step after.");
      return 1;
    }

    var newTestCaseLines = testCaseLines
      .Take(lastTableIndex + 2) // so we have an empty line
      .Concat([content])
      .Concat(testCaseLines.Skip(lastTableIndex + 1))
      .ToArray();

    // 4. Save the file
    await File.WriteAllLinesAsync(path, newTestCaseLines, cancellationToken);

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
}
