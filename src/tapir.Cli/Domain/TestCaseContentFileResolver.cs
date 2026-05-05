namespace tomware.Tapir.Cli.Domain;

internal static class TestCaseContentFileResolver
{
  public static string? TryLocateExistingFile(TestStepInstruction instruction)
  {
    if (string.IsNullOrWhiteSpace(instruction.File))
    {
      return null;
    }

    foreach (var candidate in GetCandidatePaths(instruction.File, instruction.TestStep.TestCase.File))
    {
      if (File.Exists(candidate))
      {
        return candidate;
      }
    }

    return null;
  }

  public static string LocateExistingFile(TestStepInstruction instruction)
  {
    return TryLocateExistingFile(instruction)
      ?? throw new FileNotFoundException(
        $"File '{instruction.File}' could not be located for action '{instruction.Action}'."
      );
  }

  internal static IEnumerable<string> GetCandidatePaths(string filePath, string? testCaseFile)
  {
    if (string.IsNullOrWhiteSpace(filePath))
    {
      yield break;
    }

    if (Path.IsPathRooted(filePath))
    {
      yield return Path.GetFullPath(filePath);
      yield break;
    }

    if (!string.IsNullOrWhiteSpace(testCaseFile))
    {
      var testCaseDirectory = Path.GetDirectoryName(Path.GetFullPath(testCaseFile));
      if (!string.IsNullOrWhiteSpace(testCaseDirectory))
      {
        yield return Path.GetFullPath(Path.Combine(testCaseDirectory, filePath));
      }
    }

    yield return Path.GetFullPath(filePath);
  }
}
