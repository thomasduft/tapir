namespace tomware.Tapir.Cli.Domain;

internal record VariableExtractionResult(
  Dictionary<string, string> Variables,
  IEnumerable<TestStepResult> TestStepResults
);