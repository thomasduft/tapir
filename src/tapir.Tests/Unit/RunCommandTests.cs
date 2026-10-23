using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.Extensions.DependencyInjection;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class RunCommandTests : IDisposable
{
  private readonly string _directory = Path.Combine(Path.GetTempPath(), $"tapir-run-{Guid.NewGuid():N}");
  private readonly string _idPrefix = $"TC-{Guid.NewGuid():N}";
  private readonly RecordingExecutor _executor = new();
  private readonly ServiceProvider _provider = new ServiceCollection().WithValidation().BuildServiceProvider();

  public RunCommandTests()
  {
    Directory.CreateDirectory(_directory);
  }

  [Fact]
  public async Task Run_NoDefinitions_ReturnsFailure()
  {
    await File.WriteAllTextAsync(Path.Combine(_directory, "README.md"), "Not a test case.");

    Assert.Equal(1, await RunAsync());
    Assert.Empty(_executor.Calls);
  }

  [Fact]
  public async Task Run_DefinitionWithoutTables_ReturnsFailure()
  {
    await WriteCaseAsync(1, []);

    Assert.Equal(1, await RunAsync());
    Assert.Empty(_executor.Calls);
  }

  [Fact]
  public async Task Run_ValidSuite_ReturnsSuccessAndRunsAllCases()
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=first"]);
    await WriteCaseAsync(2, ["Action=Send Method=GET Endpoint=second"]);

    Assert.Equal(0, await RunAsync());
    Assert.Equal([CaseId(1), CaseId(2)], ExecutedCaseIds());
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_FailedCase_ContinuesOnlyWhenRequestedAndReturnsFailure(bool continueOnFailure)
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=first"]);
    await WriteCaseAsync(2, ["Action=Send Method=GET Endpoint=second"]);
    _executor.Handler = (instructions, _) => Task.FromResult(
      instructions[0].TestStep.TestCase.Id == CaseId(1)
        ? Failure(instructions)
        : Success(instructions));

    Assert.Equal(1, await RunAsync(continueOnFailure));
    Assert.Equal(continueOnFailure ? [CaseId(1), CaseId(2)] : [CaseId(1)], ExecutedCaseIds());
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_FailedTable_ContinuesRemainingTablesOnlyWhenRequested(bool continueOnFailure)
  {
    await WriteCaseAsync(1, [
      "Action=Send Method=GET Endpoint=first",
      "Action=Send Method=GET Endpoint=second"
    ]);
    _executor.Handler = (instructions, _) => Task.FromResult(
      instructions[0].Endpoint == "first" ? Failure(instructions) : Success(instructions));

    Assert.Equal(1, await RunAsync(continueOnFailure));
    Assert.Equal(continueOnFailure ? 2 : 1, _executor.Calls.Count);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_InvalidCase_SkipsItsRequestAndHonorsContinueOnFailure(bool continueOnFailure)
  {
    await WriteCaseAsync(1, ["Action=Send Method=INVALID Endpoint=first"]);
    await WriteCaseAsync(2, ["Action=Send Method=GET Endpoint=second"]);

    Assert.Equal(1, await RunAsync(continueOnFailure));
    Assert.Equal(continueOnFailure ? [CaseId(2)] : Array.Empty<string>(), ExecutedCaseIds());
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_InvalidTable_IsReportedAndHonorsContinueOnFailure(bool continueOnFailure)
  {
    await WriteCaseAsync(1, [
      "Action=Send Method=INVALID Endpoint=first",
      "Action=Send Method=GET Endpoint=second"
    ]);
    var output = Path.Combine(_directory, "results");

    Assert.Equal(1, await RunAsync(continueOnFailure, "-o", output));
    Assert.Equal(continueOnFailure ? 1 : 0, _executor.Calls.Count);
    var report = await File.ReadAllTextAsync(Assert.Single(Directory.GetFiles(output, "*.md", SearchOption.AllDirectories)));
    Assert.Contains("**Status**: Failed", report);
    Assert.Contains("Validation error:", report);
    Assert.Contains("INVALID", report);
  }

  [Fact]
  public async Task Run_InvalidLaterTable_DoesNotExecuteItsRequest()
  {
    await WriteCaseAsync(1, [
      "Action=Send Method=GET Endpoint=first",
      "Action=Send Method=INVALID Endpoint=second"
    ]);

    Assert.Equal(1, await RunAsync());
    Assert.Equal("first", Assert.Single(Assert.Single(_executor.Calls)).Endpoint);
  }

  [Fact]
  public async Task Run_UnknownAction_IsAValidationFailureInsteadOfAnException()
  {
    await WriteCaseAsync(1, ["Action=Unknown Endpoint=first"]);

    Assert.Equal(1, await RunAsync());
    Assert.Empty(_executor.Calls);
  }

  [Fact]
  public async Task Run_InvalidMetadata_DoesNotExecuteRequests()
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=first"], status: "Invalid");

    Assert.Equal(1, await RunAsync());
    Assert.Empty(_executor.Calls);
  }

  [Fact]
  public async Task Run_CommandLineVariables_AreAvailableDuringValidation()
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=@@Endpoint@@"]);

    Assert.Equal(0, await RunAsync(false, "-v", "Endpoint=users"));
    Assert.Equal("users", Assert.Single(Assert.Single(_executor.Calls)).Endpoint);
  }

  [Fact]
  public async Task Run_StoredVariables_AreAvailableWhenValidatingLaterTables()
  {
    await WriteCaseAsync(1, [
      "Action=Send Method=GET Endpoint=first",
      "Action=Send Method=GET Endpoint=users/@@UserId@@"
    ]);
    _executor.Handler = (instructions, _) => Task.FromResult(new TestCaseExecutionResult(
      instructions.Select(i => TestStepResult.Success(i.TestStep)).ToList(),
      new Dictionary<string, string> { ["UserId"] = "42" }));

    Assert.Equal(0, await RunAsync());
    Assert.Equal(["first", "users/42"], _executor.Calls.Select(c => c[0].Endpoint));
  }

  [Fact]
  public async Task Run_MissingVariable_IsAValidationFailureInsteadOfAnException()
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=users/@@Missing@@"]);

    Assert.Equal(1, await RunAsync());
    Assert.Empty(_executor.Calls);
  }

  [Fact]
  public async Task Run_Durations_AreMeasuredIndependentlyPerCase()
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=first"]);
    await WriteCaseAsync(2, ["Action=Send Method=GET Endpoint=second"]);
    _executor.Handler = async (instructions, token) =>
    {
      if (instructions[0].TestStep.TestCase.Id == CaseId(1))
      {
        await Task.Delay(200, token);
      }

      return Success(instructions);
    };

    var durations = new List<long>();
    var intervalSinceFirstMeasurement = new Stopwatch();
    using var listener = new MeterListener();
    listener.InstrumentPublished = (instrument, meterListener) =>
    {
      if (instrument.Meter.Name == "tomware.Tapir.Cli.Metrics" && instrument.Name == "tapir_test_case")
      {
        meterListener.EnableMeasurementEvents(instrument);
      }
    };
    listener.SetMeasurementEventCallback<int>((_, _, tags, _) =>
    {
      string? caseId = null;
      long duration = 0;
      foreach (var tag in tags)
      {
        if (tag.Key == "test_case_id") caseId = tag.Value as string;
        if (tag.Key == "duration") duration = (long)tag.Value!;
      }

      // Other command tests can record measurements concurrently.
      if (caseId == CaseId(1))
      {
        durations.Add(duration);
        intervalSinceFirstMeasurement.Start();
      }
      else if (caseId == CaseId(2))
      {
        intervalSinceFirstMeasurement.Stop();
        durations.Add(duration);
      }
    });
    listener.Start();

    Assert.Equal(0, await RunAsync());
    Assert.Equal(2, durations.Count);
    Assert.True(durations[0] >= 150);
    // The second case cannot have taken longer than the entire interval since
    // the first measurement. A cumulative stopwatch includes the initial delay.
    Assert.True(durations[1] <= intervalSinceFirstMeasurement.ElapsedMilliseconds + 10,
      $"Second duration {durations[1]}ms exceeds its measurement interval {intervalSinceFirstMeasurement.ElapsedMilliseconds}ms.");
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_Cancellation_DoesNotContinueToRemainingCases(bool continueOnFailure)
  {
    await WriteCaseAsync(1, ["Action=Send Method=GET Endpoint=first"]);
    await WriteCaseAsync(2, ["Action=Send Method=GET Endpoint=second"]);
    using var cts = new CancellationTokenSource();
    _executor.Handler = (instructions, _) =>
    {
      cts.Cancel();
      return Task.FromResult(Failure(instructions));
    };
    var cli = CreateCli();
    var args = new List<string> { "run", "https://example.com", "-i", _directory };
    if (continueOnFailure) args.Add("--continue-on-failure");

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cli.ExecuteAsync(args.ToArray(), cts.Token));
    Assert.Single(_executor.Calls);
  }

  private string CaseId(int number) => $"{_idPrefix}-{number:D3}";

  private IEnumerable<string> ExecutedCaseIds()
    => _executor.Calls.Select(c => c[0].TestStep.TestCase.Id);

  private global::Cli CreateCli()
    => new([new RunCommand(_provider.GetRequiredService<ITestCaseValidator>(), _executor)]);

  private Task<int> RunAsync(bool continueOnFailure = false, params string[] extraArgs)
  {
    var args = new List<string> { "run", "https://example.com", "-i", _directory };
    if (continueOnFailure) args.Add("--continue-on-failure");
    args.AddRange(extraArgs);
    return CreateCli().ExecuteAsync(args.ToArray());
  }

  private async Task WriteCaseAsync(int number, string[] tables, string status = "Unknown")
  {
    var content = $"""
      # {CaseId(number)}: Runner test

      - **Date**: 2026-01-01
      - **Module**: Runner
      - **Type**: Definition
      - **Status**: {status}

      ## Steps

      """;
    for (var i = 0; i < tables.Length; i++)
    {
      content += $"\n\n| Step ID | Description | Expected Result | Test Data | Actual Result |\n"
        + "| ------- | ----------- | --------------- | --------- | ------------- |\n"
        + $"| {i + 1} | Send request | Success | {tables[i]} | - |\n";
    }

    await File.WriteAllTextAsync(Path.Combine(_directory, $"{CaseId(number)}.md"), content);
  }

  private static TestCaseExecutionResult Success(List<TestStepInstruction> instructions)
    => new(instructions.Select(i => TestStepResult.Success(i.TestStep)).ToList(), new Dictionary<string, string>());

  private static TestCaseExecutionResult Failure(List<TestStepInstruction> instructions)
    => new(instructions.Select(i => TestStepResult.Failed(i.TestStep, "Simulated failure")).ToList(), new Dictionary<string, string>());

  private sealed class RecordingExecutor : ITestCaseExecutor
  {
    public List<List<TestStepInstruction>> Calls { get; } = [];
    public Func<List<TestStepInstruction>, CancellationToken, Task<TestCaseExecutionResult>> Handler { get; set; }
      = (instructions, _) => Task.FromResult(Success(instructions));

    public Task<TestCaseExecutionResult> ExecuteAsync(
      string domain,
      IEnumerable<TestStepInstruction> instructions,
      bool insecure,
      CancellationToken cancellationToken)
    {
      var list = instructions.ToList();
      Calls.Add(list);
      return Handler(list, cancellationToken);
    }
  }

  public void Dispose()
  {
    _provider.Dispose();
    Directory.Delete(_directory, recursive: true);
  }
}
