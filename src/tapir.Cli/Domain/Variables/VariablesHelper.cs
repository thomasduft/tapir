using System.Text.RegularExpressions;

namespace tomware.Tapir.Cli.Domain;

internal static class VariablesHelper
{
  public static Dictionary<string, string> CreateVariables(
    IReadOnlyList<string?> variables)
  {
    // An entry with an escaped value like Token="AbcD="
    // should be properly parsed into a key-value pair,
    // where the key is "Token" and the value is AbcD=

    if (variables == null || !variables.Any())
    {
      return [];
    }

    return variables
      .Select(v =>
      {
        var idx = v!.IndexOf('=');
        return idx > 0
          ? new[] { v[..idx], v[(idx + 1)..] }
          : null;
      })
      .Where(parts => parts != null)
      .ToDictionary(
        parts => parts![0].Trim(),
        parts =>
        {
          var value = parts![1].Trim();
          if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
            value = value[1..^1];
          return value;
        });
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
