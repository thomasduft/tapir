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

    return Directory.GetFiles(directoryPath, "*.md", SearchOption.AllDirectories);
  }

  public static string FindFile(string directory, string testCaseId)
  {
    foreach (var file in Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories))
    {
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
}
