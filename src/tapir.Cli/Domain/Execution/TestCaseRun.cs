using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli.Domain;

internal class TestCaseRun
{
  private readonly TestCase _testCase;
  private readonly IEnumerable<TestStepResult> _results;

  public TestCaseRun(
    TestCase testCase,
    IEnumerable<TestStepResult> results
  )
  {
    _testCase = testCase;
    _results = results;
  }

  internal async Task SaveAsync(
    string inputDirectory,
    string outputDirectory,
    CancellationToken cancellationToken
  )
  {
    var content = await File.ReadAllTextAsync(_testCase.File, cancellationToken);

    // Update test steps using MarkdownTable

    // TODO: Refactor to respect all tables
    var table = new MarkdownTable(content);
    var updatedContent = table.UpdateTestStepsWithResults(_results);

    var lines = updatedContent.Split('\n');

    SetProperties(lines, _results.All(r => r.IsSuccess));
    if (_testCase.HasDomain) lines = AppendDomainProperty(lines, _testCase.Domain);

    // Ensure directory structure based on the input directory
    var relativePath = Path.GetRelativePath(inputDirectory, _testCase.File);
    var outputDir = Path.Combine(outputDirectory, Path.GetDirectoryName(relativePath)!);
    if (!Directory.Exists(outputDir))
    {
      Directory.CreateDirectory(outputDir);
    }

    await File.WriteAllLinesAsync(
      $"{outputDir}/{_testCase.Id}.md",
      lines,
      cancellationToken
    );
  }

  private void SetProperties(string[] lines, bool result)
  {
    ReplacePropertyValue(lines, "Date", DateStringProvider.GetDateString());
    ReplacePropertyValue(lines, "Type", Constants.TestCaseType.Run);
    ReplacePropertyValue(lines, "Status", result
      ? Constants.TestCaseStatus.Passed
      : Constants.TestCaseStatus.Failed);
  }

  private void ReplacePropertyValue(string[] lines, string property, string value)
  {
    var line = lines.FirstOrDefault(l => l.StartsWith($"- **{property}**:"));
    var splittedItems = line!.Split(':');
    lines[Array.IndexOf(lines, line)] = line.Replace(splittedItems[1].Trim(), value);
  }

  private string[] AppendDomainProperty(string[] lines, string domain)
  {
    // find Status property
    var line = lines.FirstOrDefault(l => l.StartsWith($"- **Status**:"));
    // add Domain property after Status property
    var index = Array.IndexOf(lines, line) + 1;
    var lineToInsert = $"- **Domain**: {domain}";

    // insert new line at index
    return lines
      .Take(index)
      .Concat([lineToInsert])
      .Concat(lines.Skip(index))
      .ToArray();
  }
}