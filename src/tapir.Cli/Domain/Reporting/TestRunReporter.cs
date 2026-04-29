namespace tomware.Tapir.Cli.Domain;

internal class TestRunReporter
{
  internal async Task<TestRunReport> BuildReportAsync(
    string inputDirectory,
    CancellationToken cancellationToken
  )
  {
    var files = FindRunFiles(inputDirectory);

    var testCases = new List<TestCase>();
    foreach (var file in files)
    {
      var testCase = await TestCase.FromTestCaseFileAsync(file, cancellationToken);
      testCases.Add(testCase);
    }

    var modules = testCases
      .GroupBy(tc => tc.Module ?? "Unknown")
      .OrderBy(g => g.Key)
      .Select(g => new TestRunReportModule(
        g.Key,
        g.OrderBy(tc => tc.Id).Select(MapTestCase).ToList()
      ))
      .ToList();

    var total = testCases.Count;
    var passed = testCases.Count(tc => tc.Status == Constants.TestCaseStatus.Passed);
    var failed = testCases.Count(tc => tc.Status == Constants.TestCaseStatus.Failed);
    var date = testCases
      .Select(tc => tc.Date)
      .Where(d => !string.IsNullOrEmpty(d))
      .OrderDescending()
      .FirstOrDefault() ?? DateTime.Today.ToString("yyyy-MM-dd");

    return new TestRunReport(date, total, passed, failed, modules);
  }

  private static TestRunReportTestCase MapTestCase(TestCase tc)
  {
    var steps = tc.Tables
      .SelectMany(t => t.Steps)
      .Select(s => new TestRunReportStep(
        s.Id,
        s.Description,
        s.ExpectedResult,
        s.ActualResult,
        s.IsSuccess
      ))
      .ToList();

    return new TestRunReportTestCase(
      tc.Id,
      tc.Title,
      tc.Status,
      tc.Date,
      tc.Domain,
      steps
    );
  }

  private static string[] FindRunFiles(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
      throw new DirectoryNotFoundException($"Directory '{directoryPath}' not found!");

    return Directory
      .GetFiles(directoryPath, "*.md", SearchOption.AllDirectories)
      .Where(IsRunFile)
      .OrderBy(f => f)
      .ToArray();
  }

  private static bool IsRunFile(string filePath)
  {
    try
    {
      if (!File.Exists(filePath)) return false;

      var lines = File.ReadLines(filePath).Take(15).ToArray();
      if (lines.Length < 5) return false;
      if (!lines[0].Contains(':')) return false;

      var content = string.Join(Environment.NewLine, lines);

      return content.Contains("**Type**: Run", StringComparison.Ordinal);
    }
    catch
    {
      return false;
    }
  }
}
