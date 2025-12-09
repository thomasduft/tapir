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

  public async Task<VariableExtractionResult> ExtractAsync(
    CancellationToken cancellationToken
  )
  {
    var variables = new Dictionary<string, string>();

    if (_content == null)
    {
      return new VariableExtractionResult(variables, Enumerable.Empty<TestStepResult>());
    }

    var storeVariableInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.StoreVariable);

    if (!storeVariableInstructions.Any())
    {
      return new VariableExtractionResult(variables, Enumerable.Empty<TestStepResult>());
    }

    var testStepResults = new List<TestStepResult>();

    var contentString = await _content.ReadAsStringAsync(cancellationToken);
    var jsonNode = JsonNode.Parse(contentString);

    foreach (var instruction in storeVariableInstructions)
    {
      if (string.IsNullOrEmpty(instruction.Name))
      {
        throw new InvalidOperationException($"Variable Name must be provided in order to store a variable (Step ID: '{instruction.TestStep.Id}').");
      }

      if (!string.IsNullOrEmpty(instruction.Value))
      {
        variables[instruction.Name] = instruction.Value;
      }

      if (!string.IsNullOrEmpty(instruction.JsonPath))
      {
        var jsonPath = JsonPath.Parse(instruction.JsonPath);
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

      testStepResults.Add(TestStepResult.Success(instruction.TestStep));
    }

    return new VariableExtractionResult(variables, testStepResults);
  }
}