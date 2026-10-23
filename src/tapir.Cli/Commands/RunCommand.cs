using System.Diagnostics;
using System.Diagnostics.Metrics;

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Cli;

internal class RunCommand : CommandLineApplication
{
  private static readonly Meter TestCaseMeter
    = new("tomware.Tapir.Cli.Metrics");
  private static readonly Gauge<int> TestCaseGauge
    = TestCaseMeter.CreateGauge<int>("tapir_test_case");

  private readonly ITestCaseValidator _testCaseValidator;
  private readonly ITestCaseExecutor _testCaseExecutor;

  private readonly CommandArgument<string> _domain;
  private readonly CommandOption<string> _testCaseId;
  private readonly CommandOption<string> _inputDirectory;
  private readonly CommandOption<string> _outputDirectory;
  private readonly CommandOption<string> _variables;
  private readonly CommandOption<bool> _continueOnFailure;
  private readonly CommandOption<bool> _insecure;

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
      "Continues remaining request tables and test cases after failures; returns a non-zero exit code if any fail.",
      CommandOptionType.NoValue,
      cfg => cfg.DefaultValue = false,
      true
    );

    _insecure = Option<bool>(
      "--insecure",
      "Disable TLS certificate verification for all requests in this run. For local development only.",
      CommandOptionType.NoValue,
      cfg => cfg.DefaultValue = false,
      false
    );

    OnExecuteAsync(ExecuteAsync);
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    if (_insecure.ParsedValue)
    {
      Log.Logger.Warning("TLS certificate verification is disabled (--insecure). Use only for local development.");
    }

    // Locate the Test Case definition files
    var files = TestCaseDefinitionFinder.FindFiles(
      _inputDirectory.ParsedValue,
      _testCaseId.ParsedValue
    );

    if (files.Length == 0)
    {
      Log.Logger.Error("No Test Case definitions found in '{InputDirectory}'.", _inputDirectory.ParsedValue);
      return 1;
    }

    var outputDirectory = _outputDirectory.HasValue()
      ? _outputDirectory.ParsedValue
      : null;

    var exitCode = 0;
    foreach (var file in files)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var result = await RunTestCaseAsync(
        outputDirectory,
        file,
        cancellationToken
      );

      if (result != 0)
      {
        exitCode = result;
        if (!_continueOnFailure.ParsedValue)
        {
          break;
        }
      }
    }

    return exitCode;
  }

  private async Task<int> RunTestCaseAsync(
    string? outputDirectory,
    string file,
    CancellationToken cancellationToken
  )
  {
    var domain = _domain.ParsedValue;

    var testCase = await TestCase.FromTestCaseFileAsync(file, cancellationToken);
    testCase
      .WithDomain(domain)
      .WithVariables(VariablesHelper.CreateVariables(_variables.ParsedValues));

    if (!testCase.Tables.Any())
    {
      Log.Logger.Error("'{Title} ({TestCaseId})' has no request tables to execute.", testCase.Title, testCase.Id);
      return 1;
    }

    // Run the Test Case
    Log.Logger.Information("Starting '{TestCaseTitle} ({TestCaseId})'",
      testCase.Title,
      testCase.Id);

    var stopwatch = Stopwatch.StartNew();

    // for each table in the test case, execute the test steps
    var results = new List<TestCaseExecutionResult>();
    foreach (var table in testCase.Tables)
    {
      cancellationToken.ThrowIfCancellationRequested();

      // Validate immediately before each request, after variables from earlier
      // responses are available. Invalid tables must never send HTTP requests.
      var validationResult = await _testCaseValidator.ValidateAsync(
        testCase,
        cancellationToken,
        [table]
      );
      cancellationToken.ThrowIfCancellationRequested();

      TestCaseExecutionResult executionResult;
      if (!validationResult.IsValid)
      {
        var error = string.Join("; ", validationResult.Errors);
        executionResult = new TestCaseExecutionResult(
          table.Steps.Select(step => TestStepResult.Failed(step, $"Validation error: {error}")).ToList(),
          new Dictionary<string, string>()
        );
      }
      else
      {
        var instructions = table.Steps
          .Select(TestStepInstruction.FromTestStep)
          .ToList();

        executionResult = await _testCaseExecutor.ExecuteAsync(
          domain,
          instructions,
          _insecure.ParsedValue,
          cancellationToken
        );
      }

      cancellationToken.ThrowIfCancellationRequested();
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

    stopwatch.Stop();

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
      new("duration", stopwatch.ElapsedMilliseconds),
      new("timestamp", DateTimeOffset.UtcNow)
    ]);

    // Saving Test Case Run results
    if (!string.IsNullOrWhiteSpace(outputDirectory))
    {
      // Store the Test Case run
      var run = new TestCaseRun(
        testCase,
        results.SelectMany(r => r.TestStepResults).ToList()
      );

      await run.SaveAsync(
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

    Log.Logger.Information(string.Empty); // for better readability in the console output

    return overallSuccess ? 0 : 1;
  }
}
