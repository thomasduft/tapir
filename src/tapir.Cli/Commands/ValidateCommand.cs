using McMaster.Extensions.CommandLineUtils;

using Serilog;

using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Cli;

internal class ValidateCommand : CommandLineApplication
{
  private readonly CommandArgument<string> _testCaseId;
  private readonly CommandOption<string> _inputDirectory;
  private readonly ITestCaseValidator _testCaseValidator;

  public ValidateCommand(
    ITestCaseValidator testCaseValidator
  )
  {
    _testCaseValidator = testCaseValidator;

    Name = "validate";
    Description = "Validates a Test Case definition (i.e. TC-Audit-001).";

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
    //1. Locate the Test Case definition file
    var file = TestCaseFileLocator.FindFile(_inputDirectory.ParsedValue, _testCaseId.ParsedValue);

    // 2. Read the Test Case definition
    var testCase = await TestCase.FromTestCaseFileAsync(file, cancellationToken);
    testCase.WithVariables(
      VariablesHelper.AssignDummyVariables(testCase.Tables.SelectMany(t => t.Steps))
    );

    // 3. Validate the Test Case definition
    var validationResult = await _testCaseValidator.ValidateAsync(testCase, cancellationToken);
    if (!validationResult.IsValid)
    {
      foreach (var error in validationResult.Errors)
      {
        Log.Logger.Error("Validation error: {Error}", error);
      }

      return await Task.FromResult(1);
    }

    Log.Logger.Information("'{TestCaseTitle} ({TestCaseId})' is valid!", testCase.Title, testCase.Id);

    return await Task.FromResult(0);
  }
}
