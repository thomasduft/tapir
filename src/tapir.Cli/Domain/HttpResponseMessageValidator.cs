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
    // results.AddRange(CheckHeaders());

    return results;
  }

  private IEnumerable<TestStepResult> CheckStatusCode()
  {
    var statusCodeInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckStatusCode);
    if (statusCodeInstruction == null)
    {
      return [];
    }

    var expectedStatusCode = (HttpStatusCode)int.Parse(statusCodeInstruction.Value);

    return expectedStatusCode == _statusCode
      ? [TestStepResult.Success(statusCodeInstruction.TestStep)]
      : [TestStepResult.Failed(
        statusCodeInstruction.TestStep,
        $"Expected status code '{(int)expectedStatusCode}' but was '{(int)_statusCode}'."
      )];
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
    var contentInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckContent);
    if (contentInstruction == null || _content == null)
    {
      return [];
    }

    var json = await _content.ReadAsStringAsync(cancellationToken);
    if (string.IsNullOrEmpty(json))
    {
      return [TestStepResult.Failed(
        contentInstruction.TestStep,
        "Response content is empty."
      )];
    }

    // see https://docs.json-everything.net/path/basics/

    var jsonNode = JsonNode.Parse(json);
    var jsonPath = JsonPath.Parse(contentInstruction.Path);
    var results = jsonPath.Evaluate(jsonNode);
    var actualValue = results.Matches.FirstOrDefault()?.Value?.ToString();

    var expectedValue = contentInstruction.Value;

    return expectedValue == actualValue
      ? [TestStepResult.Success(contentInstruction.TestStep)]
      : [TestStepResult.Failed(
        contentInstruction.TestStep,
        $"Expected value '{expectedValue}' but was '{actualValue}'."
      )];
  }
}
