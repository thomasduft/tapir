using System.Diagnostics;
using System.Diagnostics.Metrics;

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli;

internal class RunCommand : CommandLineApplication
{
  private static readonly Meter TestCaseMeter
    = new("tomware.Tapir.Cli.Metrics");
  private static readonly Gauge<int> TestCaseGauge
    = TestCaseMeter.CreateGauge<int>("tapir_test_case");

  private readonly Stopwatch _stopwatch = new();

  private readonly ITestCaseValidator _testCaseValidator;
  private readonly ITestCaseExecutor _testCaseExecutor;

  private readonly CommandArgument<string> _domain;
  private readonly CommandOption<string> _testCaseId;
  private readonly CommandOption<string> _inputDirectory;
  private readonly CommandOption<string> _outputDirectory;
  private readonly CommandOption<string> _variables;
  private readonly CommandOption<bool> _continueOnFailure;

  public RunCommand(
    ITestCaseValidator testCaseValidator,
    ITestCaseExecutor testCaseExecutor
  )
  {
    _testCaseValidator = testCaseValidator;
    _testCaseExecutor = testCaseExecutor;

    Name = "run";
    Description = "Runs Test Case definition (i.e. \"https://localhost:5001\" -tc TC-Audit-001).";

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
      .WithVariables(VariablesHelper.AssignDummyVariables(testCase.Tables.SelectMany(t => t.Steps)));

    // Validate the Test Case definition
    var validationResult = await _testCaseValidator.ValidateAsync(testCase, cancellationToken);
    if (!validationResult.IsValid)
    {
      foreach (var error in validationResult.Errors)
      {
        Log.Logger.Error("Validation error: {Error}", error);
      }

      return await Task.FromResult(1);
    }

    // Run the Test Case
    Log.Logger.Information("Starting '{TestCaseTitle} ({TestCaseId})'",
      testCase.Title,
      testCase.Id);

    // patching variables from command line
    if (_variables.HasValue())
    {
      testCase.WithVariables(VariablesHelper.CreateVariables(_variables.ParsedValues));
    }

    _stopwatch.Start();

    // for each table in the test case, execute the test steps
    var results = new List<TestCaseExecutionResult>();
    foreach (var table in testCase.Tables)
    {
      var instructions = table.Steps
        .Select(step => TestStepInstruction.FromTestStep(step, testCase.Variables))
        .ToList();

      var executionResult = await _testCaseExecutor.ExecuteAsync(
        domain,
        instructions,
        cancellationToken
      );

      results.Add(executionResult);

      var success = executionResult.TestStepResults.All(r => r.IsSuccess);
      if (!success)
      {
        foreach (var result in executionResult.TestStepResults.Where(r => !r.IsSuccess))
        {
          Log.Logger.Error("Test case step '{TestStepId}' failed with error: {Error}",
            result.TestStepId,
            result.Error);
        }

        if (!_continueOnFailure.ParsedValue)
        {
          // abort further execution
          break;
        }
      }

      testCase.AddOrMergeVariables(executionResult.Variables);
    }

    _stopwatch.Stop();

    var overallSuccess = results.All(r => r.TestStepResults.All(tr => tr.IsSuccess));

    TestCaseGauge.Record(1, [
      new("domain", domain),
      new("test_case_id", testCase.Id),
      new("module", testCase.Module),
      new("status", overallSuccess
        ? Constants.TestCaseStatus.Passed
        : Constants.TestCaseStatus.Failed),
      new("error_message", overallSuccess
        ? string.Empty
        : string.Join(',', results.SelectMany(r => r.TestStepResults)
          .Where(r => !r.IsSuccess)
          .Select(r => r.Error))),
      new("duration", _stopwatch.ElapsedMilliseconds),
      new("timestamp", DateTimeOffset.UtcNow)
    ]);

    // Saving Test Case Run results
    if (!string.IsNullOrWhiteSpace(outputDirectory))
    {
      // Store the Test Case run
      var run = new TestCaseRun(testCase, results.SelectMany(r => r.TestStepResults).ToList());
      await run.SaveAsync(
        inputDirectory,
        outputDirectory,
        cancellationToken
      );
    }

    if (overallSuccess)
    {
      Log.Logger.Information("'{Title} ({TestCaseId})' executed successfully.", testCase.Title, testCase.Id);
    }
    else
    {
      Log.Logger.Error("'{Title} ({TestCaseId})' failed.", testCase.Title, testCase.Id);
    }

    return overallSuccess ? 0 : 1;
  }
}
