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
}
