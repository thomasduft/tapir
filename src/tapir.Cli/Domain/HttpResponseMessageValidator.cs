using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;

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

  public IEnumerable<TestStepResult> Validate()
  {
    var results = new List<TestStepResult>();

    results.AddRange(CheckStatusCode());
    results.AddRange(CheckReasonPhrase());
    // results.AddRange(CheckContent());
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
}
