namespace tomware.Tapir.Cli.Domain;

internal record TestCaseExecutionResult(
  IEnumerable<TestStepResult> TestStepResults,
  IDictionary<string, string> Variables
);