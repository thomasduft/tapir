using System.Text.Json.Nodes;

using Json.Path;

namespace tomware.Tapir.Cli.Domain;

internal class VariableExtractor
{
  private readonly IEnumerable<TestStepInstruction> _instructions;
  private HttpContent? _content;

  private VariableExtractor(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    _instructions = instructions;
  }

  public static VariableExtractor Create(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    return new VariableExtractor(instructions);
  }

  public VariableExtractor FromContent(HttpContent content)
  {
    _content = content;

    return this;
  }

  public async Task<Dictionary<string, string>> ExtractAsync(
    CancellationToken cancellationToken
  )
  {
    var variables = new Dictionary<string, string>();

    if (_content == null)
    {
      return variables;
    }

    var storeVariableInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.StoreVariable);

    if (!storeVariableInstructions.Any())
    {
      return variables;
    }

    var contentString = await _content.ReadAsStringAsync(cancellationToken);
    var jsonNode = JsonNode.Parse(contentString);

    foreach (var instruction in storeVariableInstructions)
    {
      if (string.IsNullOrEmpty(instruction.Path) || string.IsNullOrEmpty(instruction.Name))
      {
        continue;
      }

      var jsonPath = JsonPath.Parse(instruction.Path);
      var result = jsonPath.Evaluate(jsonNode!);
      var matches = result.Matches?.ToList();
      if (matches == null || !matches.Any())
      {
        continue;
      }

      var value = matches.First().Value?.ToString();
      if (value == null)
      {
        continue;
      }

      variables[instruction.Name] = value;
    }

    return variables;
  }
}
