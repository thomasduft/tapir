using McMaster.Extensions.CommandLineUtils;

using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli;

internal class RunCommand : CommandLineApplication
{
  private readonly ITestCaseExecutor _testCaseExecutor;

  private readonly CommandArgument<string> _domain;
  private readonly CommandOption<string> _testCaseId;
  private readonly CommandOption<string> _inputDirectory;
  private readonly CommandOption<string> _outputDirectory;
  private readonly CommandOption<string> _variables;
  private readonly CommandOption<bool> _continueOnFailure;

  public RunCommand(ITestCaseExecutor testCaseExecutor)
  {
    Name = "run";
    Description = "Runs Test Case definition (i.e. \"https://localhost:5001\" -tc TC-Audit-001).";

    _testCaseExecutor = testCaseExecutor;

    _domain = Argument<string>(
      "domain",
      "The domain to run the Test Case against.",
      cfg => cfg.IsRequired()
    );

    _testCaseId = Option<string>(
      "-tc|--test-case-id",
      "The Test Case ID (e.g. TC-Audit-001).",
      CommandOptionType.SingleValue,
      cfg => cfg.DefaultValue = null,
      true
    );

    _inputDirectory = Option<string>(
      "-i|--input-directory",
      "The input directory where the Test Case definition is located.",
      CommandOptionType.SingleValue,
      cfg => cfg.DefaultValue = ".",
      true
    );

    _outputDirectory = Option<string>(
      "-o|--output-directory",
      "The output directory where the Test Case result will be stored.",
      CommandOptionType.SingleValue,
      cfg => cfg.DefaultValue = null,
      true
    );

    _variables = Option<string>(
      "-v|--variable",
      "Key-Value based variable used for replacing property values in a Test Step data configuration.",
      CommandOptionType.MultipleValue,
      cfg => cfg.DefaultValue = null,
      true
    );

    _continueOnFailure = Option<bool>(
      "--continue-on-failure",
      "Continues the Test Case execution even if the Test Case fails.",
      CommandOptionType.NoValue,
      cfg => cfg.DefaultValue = false,
      true
    );

    OnExecuteAsync(ExecuteAsync);
    _testCaseExecutor = testCaseExecutor;
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    // Locate the Test Case definition files
    var files = TestCaseFileLocator.FindFiles(
      _inputDirectory.ParsedValue,
      _testCaseId.ParsedValue
    );

    var outputDirectory = _outputDirectory.HasValue()
      ? _outputDirectory.ParsedValue
      : null;

    foreach (var file in files)
    {
      var result = await RunTestCaseAsync(
        _inputDirectory.ParsedValue,
        outputDirectory,
        file,
        cancellationToken
      );
      if (result != 0)
      {
        return result;
      }
    }

    return 0;
  }

  private async Task<int> RunTestCaseAsync(
    string inputDirectory,
    string? outputDirectory,
    string file,
    CancellationToken cancellationToken
  )
  {
    var domain = _domain.ParsedValue;

    var testCase = await TestCase.FromTestCaseFileAsync(file, cancellationToken);
    testCase
      .WithDomain(domain)
      .WithVariables(VariablesHelper.CreateVariables(_variables.Values));

    ConsoleHelper.WriteLineYellow($"Running test case '{testCase.Title}' ({testCase.Id})");

    // TODO: prepare Test Case instructions


    var testStepResults = await _testCaseExecutor.ExecuteAsync(
      domain,
      cancellationToken
    );

    var success = testStepResults.All(r => r.IsSuccess);
    if (!success)
    {
      foreach (var result in testStepResults.Where(r => !r.IsSuccess))
      {
        ConsoleHelper.WriteLineError($"Test case step '{result.TestStepId}' failed with error: {result.Error}");
      }

      if (!_continueOnFailure.ParsedValue)
      {
        return await Task.FromResult(1);
      }
    }

    if (!string.IsNullOrWhiteSpace(outputDirectory))
    {
      // Store the Test Case run
      var run = new TestCaseRun(testCase, testStepResults);
      await run.SaveAsync(
        inputDirectory,
        outputDirectory,
        cancellationToken
      );
    }

    if (success)
    {
      ConsoleHelper.WriteLineSuccess($"Test case '{testCase.Title}' ({testCase.Id}) executed successfully.");
    }

    return await Task.FromResult(0);
  }
}
