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

  public static string ResolveVariables(
    string value,
    IReadOnlyDictionary<string, string> variables
  )
  {
    if (string.IsNullOrEmpty(value) || !value.Contains(Constants.VariablePreAndSuffix))
    {
      return value;
    }

    // Pattern: users/@@AliceId@@
    var pattern = $@"\{Constants.VariablePreAndSuffix}([^@]+){Constants.VariablePreAndSuffix}";

    return Regex.Replace(value, pattern, match =>
    {
      var key = match.Groups[1].Value;
      return variables.TryGetValue(key, out var variableValue)
        ? variableValue
        : throw new InvalidOperationException($"Variable with key '{key}' could not be resolved!");
    });
  }
}
