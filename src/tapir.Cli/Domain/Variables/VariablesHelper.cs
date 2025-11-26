using System.Text.RegularExpressions;

namespace tomware.Tapir.Cli.Domain;

internal static class VariablesHelper
{
  public static Dictionary<string, string> CreateVariables(
    IReadOnlyList<string?> variables)
  {
    if (variables == null || !variables.Any())
    {
      return [];
    }

    return variables
      .Select(v => v!.Split('='))
      .Where(parts => parts.Length == 2)
      .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
  }

  internal static Dictionary<string, string> CreateDummyVariables(IEnumerable<TestStep> steps)
  {
    // For each test step data that contains a variable starting with an
    // @@ sign, create a dummy variable
    var variables = new Dictionary<string, string>();
    foreach (var step in steps)
    {
      if (string.IsNullOrWhiteSpace(step.TestData))
      {
        continue;
      }

      var matches = Regex.Matches(step.TestData, $@"{Constants.VariablePreAndSuffix}(\w+)");
      foreach (Match match in matches)
      {
        if (!variables.ContainsKey(match.Groups[1].Value))
        {
          variables.Add(match.Groups[1].Value, "dummy-value");
        }
      }
    }

    return variables;
  }
}