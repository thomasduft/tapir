using System.Text.RegularExpressions;

namespace tomware.Tapir.Cli.Domain;

internal class TestStepInstruction
{
  private readonly TestStep _step;

  public string Action { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
  public string File { get; set; } = string.Empty;
  public string Path { get; set; } = string.Empty;
  public string Method { get; set; } = "GET";

  public TestStep TestStep => _step;

  public TestStepInstruction(TestStep step)
  {
    _step = step;
  }

  public static TestStepInstruction FromTestStep(
    TestStep step,
    Dictionary<string, string> variables
  )
  {
    var testData = ParseTestData(step.TestData);
    if (testData.Count == 0)
      throw new InvalidDataException($"No TestData found for Test Step {step.Id}");

    TestStepInstruction instruction = new(step);
    foreach (var parameter in testData)
    {
      switch (parameter.Key)
      {
        case nameof(Action):
          instruction.Action = parameter.Value;
          break;
        case nameof(Name):
          instruction.Name = parameter.Value;
          break;
        case nameof(Value):
          instruction.Value = ReplaceVariables(parameter.Value, variables);
          break;
        case nameof(File):
          instruction.File = parameter.Value;
          break;
        case nameof(Path):
          instruction.Path = ReplaceVariables(parameter.Value, variables);
          break;
        case nameof(Method):
          instruction.Method = parameter.Value.ToUpperInvariant();
          break;
        default:
          throw new InvalidDataException($"Unsupported parameter '{parameter.Key}' found in TestData of Test Step {step.Id}");
      }
    }

    return instruction;
  }

  private static string ReplaceVariables(
    string value,
    Dictionary<string, string> variables
  )
  {
    if (!value.Contains("@@")) return value;

    var key = value.Replace("@@", string.Empty);
    return variables.TryGetValue(key, out var variableValue)
      ? variableValue
      : throw new InvalidOperationException($"Variable with key '{value}' could not be resolved!");
  }

  private static Dictionary<string, string> ParseTestData(
    string testData
  )
  {
    var result = new Dictionary<string, string>();
    if (string.IsNullOrEmpty(testData))
      return result;

    var pattern = @"(\w+)=(""[^""]*""|\S+)";
    var matches = Regex.Matches(testData, pattern);

    foreach (Match match in matches)
    {
      var key = match.Groups[1].Value;
      var value = match.Groups[2].Value;
      result[key] = value;
    }

    return result;
  }
}
