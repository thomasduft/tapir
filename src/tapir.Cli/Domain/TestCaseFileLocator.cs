namespace tomware.Tapir.Cli.Domain;

internal static class TestCaseFileLocator
{
  internal static string[] FindFiles(string directoryPath, string testCaseId)
  {
    if (!Directory.Exists(directoryPath))
    {
      throw new DirectoryNotFoundException($"Directory '{directoryPath}' not found!");
    }

    if (!string.IsNullOrEmpty(testCaseId))
    {
      return [FindFile(directoryPath, testCaseId)];
    }

    return Directory
      .GetFiles(directoryPath, "*.md", SearchOption.AllDirectories)
      .Where(f => IsValidTestCaseFile(f))
      .OrderBy(f => f)
      .ToArray();
  }

  public static string FindFile(string directory, string testCaseId)
  {
    foreach (var file in Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories))
    {
      if (!IsValidTestCaseFile(file))
      {
        continue;
      }

      var splittedItems = File
        .ReadAllLines(file!)
        .FirstOrDefault()!
        .Split(":");
      if (splittedItems[0].Trim().Contains(testCaseId, StringComparison.CurrentCultureIgnoreCase))
      {
        return new FileInfo(file).FullName;
      }
    }

    throw new FileNotFoundException($"TestCase definition for '{testCaseId}' not found!");
  }

  private static bool IsValidTestCaseFile(string filePath)
  {
    try
    {
      if (!File.Exists(filePath)) return false;

      var lines = File.ReadLines(filePath).Take(10).ToArray();
      if (lines.Length < 5) return false;

      // Check if first line contains a colon separator
      var firstLine = lines[0];
      if (!firstLine.Contains(':'))
      {
        return false;
      }

      // Check if file contains "**Type**: Definition"
      var content = string.Join(Environment.NewLine, lines);
      var hasType = content.Contains("**Type**:", StringComparison.Ordinal);
      var isOfTypeDefinition = content.Contains("Definition", StringComparison.Ordinal);

      return hasType && isOfTypeDefinition;
    }
    catch
    {
      return false;
    }
  }
}
