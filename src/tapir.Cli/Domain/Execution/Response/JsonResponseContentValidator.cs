using System.Text.Json;
using System.Text.Json.Nodes;

using Json.Path;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class JsonResponseContentValidator : IResponseContentValidator
{
  public string ContentType => Constants.ContentTypes.Json;

  public Task<IEnumerable<TestStepResult>> CheckContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepResult>();

    if (string.IsNullOrEmpty(content))
    {
      results.Add(
        TestStepResult.Failed(
          instructions.First().TestStep,
          "Response content is empty."
        ));

      return Task.FromResult(results.AsEnumerable());
    }

    Log.Logger.Verbose("  - received content is: {@Json}", content);

    var jsonNode = JsonNode.Parse(content);

    foreach (var contentInstruction in instructions)
    {
      var jsonPath = JsonPath.Parse(contentInstruction.JsonPath);
      var evaluationResults = jsonPath.Evaluate(jsonNode);

      var actualValue = evaluationResults.Matches.FirstOrDefault()?.Value?.ToString();
      var expectedValue = contentInstruction.Value;

      results.Add(
        expectedValue == actualValue
          ? TestStepResult.Success(contentInstruction.TestStep)
          : TestStepResult.Failed(
            contentInstruction.TestStep,
            $"Expected content value '{expectedValue}' but was '{actualValue}'."
          )
      );
    }

    return Task.FromResult(results.AsEnumerable());
  }

  public async Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepResult>();

    if (string.IsNullOrEmpty(content))
    {
      results.Add(
        TestStepResult.Failed(
          instructions.First().TestStep,
          "Response content is empty."
        ));

      return results;
    }

    foreach (var contentInstruction in instructions)
    {
      var expectedJson = !string.IsNullOrEmpty(contentInstruction.File)
        ? await File.ReadAllTextAsync(
          TestCaseContentFileResolver.LocateExistingFile(contentInstruction),
          cancellationToken
        )
        : contentInstruction.Value ?? string.Empty;

      // Normalize both JSON strings by parsing and re-serializing without formatting
      var normalizedActual = NormalizeJson(content);
      var normalizedExpected = NormalizeJson(expectedJson);

      results.Add(
        normalizedActual == normalizedExpected
          ? TestStepResult.Success(contentInstruction.TestStep)
          : TestStepResult.Failed(
            contentInstruction.TestStep,
            "Response content does not match the expected content."
          )
      );
    }

    return results;
  }

  private static string NormalizeJson(string json, bool writeIndented = false)
  {
    try
    {
      var jsonNode = JsonNode.Parse(json);
      return jsonNode?.ToJsonString(new JsonSerializerOptions { WriteIndented = writeIndented }) ?? json;
    }
    catch
    {
      // If parsing fails, return original string
      return json;
    }
  }
}