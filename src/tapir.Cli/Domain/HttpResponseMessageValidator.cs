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
    results.AddRange(await CheckContentAsync(cancellationToken));

    return results;
  }

  private IEnumerable<TestStepResult> CheckStatusCode()
  {
    var results = new List<TestStepResult>();

    var statusCodeInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckStatusCode);
    if (statusCodeInstruction == null)
    {
      return [];
    }

    // Add a success result for the send instruction
    var sendInstruction = _instructions.First(i => i.Action == Constants.Actions.Send);
    results.Add(TestStepResult.Success(sendInstruction.TestStep));

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

  private IEnumerable<TestStepResult> CheckReasonPhrase()
  {
    var reasonPhraseInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckReasonPhrase);
    if (reasonPhraseInstruction == null)
    {
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

  private async Task<IEnumerable<TestStepResult>> CheckContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.CheckContent);
    if (contentInstructions == null || _content == null)
    {
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
      var jsonPath = JsonPath.Parse(contentInstruction.Path);
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
}