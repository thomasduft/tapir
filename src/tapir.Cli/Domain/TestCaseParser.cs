using System.Text.RegularExpressions;

namespace tomware.Tapir.Cli.Domain;

internal class TestCaseParser
{
  private readonly string _file;

  public TestCaseParser(string file)
  {
    _file = file;
  }

  internal async Task<TestCase> ToTestCaseAsync(CancellationToken cancellationToken)
  {
    var lines = await File.ReadAllLinesAsync(_file, cancellationToken);

    var (testCaseId, testCaseTitle) = GetTestCaseIdAndTitle(lines);
    var module = FindTag(lines, "Module");
    var type = FindTag(lines, "Type");
    var status = FindTag(lines, "Status");

    var markdownContent = string.Join(Environment.NewLine, lines);
    var steps = GetTestSteps(markdownContent);

    var link = FindTag(lines, "Link");
    var linkedFile = GetLinkedFile(_file, link);

    return new TestCase
    {
      Id = testCaseId,
      Title = testCaseTitle,
      Module = module,
      Type = type!,
      Status = status!,
      Steps = steps,
      File = _file,
      LinkedFile = linkedFile!
    };
  }

  private (string TestCaseId, string TestCaseTitle) GetTestCaseIdAndTitle(string[] lines)
  {
    // we are just reading the first line
    var line = lines.FirstOrDefault();

    var splittedItems = line!.Split(':');
    var testCaseId = splittedItems[0].Trim()
      .Replace(" ", string.Empty)
      .Replace("#", string.Empty)
      .Replace(":", string.Empty);
    var testCaseTitle = splittedItems[1].Trim();

    return (testCaseId, testCaseTitle);
  }

  private string? FindTag(string[] lines, string tag)
  {
    var line = lines.FirstOrDefault(l => l.StartsWith($"- **{tag}**:"));
    if (line == null) return null;

    var splittedItems = line!.Split(':');

    return splittedItems[1].Trim();
  }

  private IEnumerable<TestStep> GetTestSteps(string markdownContent)
  {
    var table = new MarkdownTable(markdownContent);

    return table.ParseTestSteps();
  }

  private string? GetLinkedFile(string file, string? link)
  {
    if (string.IsNullOrWhiteSpace(link)) return null;
    if (!link.Contains('(')) return null;

    // Format: [The administrator must be authenticated](TC-001-Login.md)
    string pattern = @"\(([^)]+)\)";
    Match match = Regex.Match(link, pattern);

    if (match.Success)
    {
      return Path.Combine(Path.GetDirectoryName(file)!, match.Groups[1].Value);
    }

    // Return null if no match is found
    return null;
  }
}
