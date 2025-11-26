using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

using Json.Path;

namespace tomware.Tapir.Cli.Domain;

internal class HttpResponseMessageValidator
{
  private readonly IEnumerable<TestStepInstruction> _instructions;
  private HttpStatusCode _statusCode;
  private string? _reasonPhrase;
  private HttpContent? _content;
  private HttpResponseHeaders? _headers;

  private HttpResponseMessageValidator(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    _instructions = instructions;
  }

  public static HttpResponseMessageValidator Create(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    return new HttpResponseMessageValidator(instructions);
  }

  public HttpResponseMessageValidator WithStatusCode(HttpStatusCode statusCode)
  {
    _statusCode = statusCode;

    return this;
  }

  public HttpResponseMessageValidator WithReasonPhrase(string? reasonPhrase)
  {
    _reasonPhrase = reasonPhrase;

    return this;
  }

  public HttpResponseMessageValidator WithContent(HttpContent content)
  {
    _content = content;

    return this;
  }

  public HttpResponseMessageValidator WithHeaders(HttpResponseHeaders headers)
  {
    _headers = headers;

    return this;
  }

  public async Task<IEnumerable<TestStepResult>> ValidateAsync(CancellationToken cancellationToken)
  {
    var results = new List<TestStepResult>();

    results.AddRange(CheckStatusCode());
    results.AddRange(CheckReasonPhrase());
    results.AddRange(CheckHeaders());
    results.AddRange(await VerifyContentAsync(cancellationToken));
    results.AddRange(await CheckContentAsync(cancellationToken));

    return results;
  }

  private List<TestStepResult> CheckStatusCode()
  {
    var results = new List<TestStepResult>();

    var sendInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.Send)
        ?? throw new InvalidOperationException("No Send instruction found to validate status code.");
    results.Add(TestStepResult.Success(sendInstruction.TestStep));

    var statusCodeInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckStatusCode);
    if (statusCodeInstruction == null)
    {
      // It's okay if there's no status code to check
      return results;
    }

    // Now validate the status code
    var expectedStatusCode = (HttpStatusCode)int.Parse(statusCodeInstruction.Value);
    results.Add(expectedStatusCode == _statusCode
      ? TestStepResult.Success(statusCodeInstruction.TestStep)
      : TestStepResult.Failed(
        statusCodeInstruction.TestStep,
        $"Expected status code '{(int)expectedStatusCode}' but was '{(int)_statusCode}'."
      ));

    return results;
  }

  private List<TestStepResult> CheckReasonPhrase()
  {
    var reasonPhraseInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckReasonPhrase);
    if (reasonPhraseInstruction == null)
    {
      // It's okay if there's no reason phrase to check
      return [];
    }

    var expectedReasonPhrase = reasonPhraseInstruction.Value;

    return expectedReasonPhrase == _reasonPhrase
      ? [TestStepResult.Success(reasonPhraseInstruction.TestStep)]
      : [TestStepResult.Failed(
        reasonPhraseInstruction.TestStep,
        $"Expected reason phrase '{expectedReasonPhrase}' but was '{_reasonPhrase}'."
      )];
  }

  private List<TestStepResult> CheckHeaders()
  {
    var headerInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.CheckHeader);
    if (headerInstructions == null || _headers == null)
    {
      // It's okay if there's no headers to check
      return [];
    }

    var results = new List<TestStepResult>();

    foreach (var headerInstruction in headerInstructions)
    {
      if (_headers.TryGetValues(headerInstruction.Name, out var values))
      {
        var actualValue = string.Join(",", values);
        var expectedValue = headerInstruction.Value;

        results.Add(
          expectedValue == actualValue
            ? TestStepResult.Success(headerInstruction.TestStep)
            : TestStepResult.Failed(
              headerInstruction.TestStep,
              $"Expected header '{headerInstruction.Name}' value '{expectedValue}' but was '{actualValue}'."
            )
        );
      }
      else
      {
        results.Add(
          TestStepResult.Failed(
            headerInstruction.TestStep,
            $"Expected header '{headerInstruction.Name}' was not found."
          )
        );
      }
    }

    return results;
  }

  private async Task<List<TestStepResult>> CheckContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.CheckContent)
      .ToList();
    if (!contentInstructions.Any() || _content == null)
    {
      // It's okay if there's no content to check
      return [];
    }

    var json = await _content.ReadAsStringAsync(cancellationToken);
    if (string.IsNullOrEmpty(json))
    {
      return [TestStepResult.Failed(
        contentInstructions.First().TestStep,
        "Response content is empty."
      )];
    }

    var results = new List<TestStepResult>();

    foreach (var contentInstruction in contentInstructions)
    {
      // see https://docs.json-everything.net/path/basics/
      var jsonNode = JsonNode.Parse(json);
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

    return results;
  }

  private async Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.VerifyContent)
      .ToList();
    if (!contentInstructions.Any() || _content == null)
    {
      // It's okay if there's no content to check
      return [];
    }

    var json = await _content.ReadAsStringAsync(cancellationToken);
    if (string.IsNullOrEmpty(json))
    {
      return [TestStepResult.Failed(
        contentInstructions.First().TestStep,
        "Response content is empty."
      )];
    }

    var results = new List<TestStepResult>();

    foreach (var contentInstruction in contentInstructions)
    {
      // if File is present load from File otherwise use Value

      // Read the file relative to the execution directory
      var relativeFilePath = Path.Combine(Directory.GetCurrentDirectory(), contentInstruction.File);
      var expectedJson = !string.IsNullOrEmpty(contentInstruction.File)
        && File.Exists(relativeFilePath)
        ? await File.ReadAllTextAsync(relativeFilePath, cancellationToken)
        : contentInstruction.Value
          ?? string.Empty;

      // Normalize both JSON strings by parsing and re-serializing without formatting
      var normalizedActual = NormalizeJson(json);
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

  private static string NormalizeJson(string json)
  {
    try
    {
      var jsonNode = JsonNode.Parse(json);
      return jsonNode?.ToJsonString() ?? json;
    }
    catch
    {
      // If parsing fails, return original string
      return json;
    }
  }
}
