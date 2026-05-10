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

    var result = new Dictionary<string, string>(StringComparer.Ordinal);

    foreach (var variable in variables)
    {
      if (string.IsNullOrWhiteSpace(variable))
      {
        continue;
      }

      var idx = variable.IndexOf('=');
      if (idx <= 0)
      {
        continue;
      }

      var key = variable[..idx].Trim();
      if (string.IsNullOrWhiteSpace(key))
      {
        continue;
      }

      var value = variable[(idx + 1)..].Trim();

      // Remove surrounding quotes ("") if present
      if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
      {
        value = value[1..^1];
      }

      // Last value wins when a variable is passed multiple times.
      result[key] = value;
    }

    return result;
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
