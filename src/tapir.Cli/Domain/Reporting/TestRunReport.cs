namespace tomware.Tapir.Cli.Domain;

internal record TestRunReport(
  string Date,
  int Total,
  int Passed,
  int Failed,
  IReadOnlyList<TestRunReportModule> Modules
);

internal record TestRunReportModule(
  string Name,
  IReadOnlyList<TestRunReportTestCase> TestCases
);

internal record TestRunReportTestCase(
  string Id,
  string Title,
  string Status,
  string Date,
  string Domain,
  IReadOnlyList<TestRunReportStep> Steps
);

internal record TestRunReportStep(
  int Id,
  string Description,
  string ExpectedResult,
  string ActualResult,
  bool IsSuccess
);
