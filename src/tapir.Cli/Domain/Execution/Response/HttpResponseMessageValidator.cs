using System.Net;
using System.Net.Http.Headers;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class HttpResponseMessageValidator
{
  private readonly IResponseContentValidatorFactory _contentValidatorFactory;
  private readonly IEnumerable<TestStepInstruction> _instructions;
  private HttpStatusCode _statusCode;
  private string? _reasonPhrase;
  private HttpContent? _content;
  private HttpContentHeaders? _contentHeaders;

  public HttpResponseMessageValidator(
    IResponseContentValidatorFactory contentValidatorFactory,
    IEnumerable<TestStepInstruction> instructions
  )
  {
    _contentValidatorFactory = contentValidatorFactory;
    _instructions = instructions;
  }

  private HttpResponseMessageValidator(
    IEnumerable<TestStepInstruction> instructions
  ) : this(CreateDefaultValidatorFactory(), instructions)
  {
  }

  public static HttpResponseMessageValidator Create(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    return new HttpResponseMessageValidator(instructions);
  }

  private static ResponseContentValidatorFactory CreateDefaultValidatorFactory()
  {
    return new ResponseContentValidatorFactory(
    [
      new TextResponseContentValidator(),
      new JsonResponseContentValidator()
    ]);
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

  public HttpResponseMessageValidator WithContentHeaders(HttpContentHeaders contentHeaders)
  {
    _contentHeaders = contentHeaders;

    return this;
  }

  public async Task<IEnumerable<TestStepResult>> ValidateAsync(CancellationToken cancellationToken)
  {
    var results = new List<TestStepResult>();

    results.AddRange(CheckStatusCode());
    results.AddRange(CheckReasonPhrase());
    results.AddRange(CheckContentHeaders());
    results.AddRange(await CheckContentAsync(cancellationToken));
    results.AddRange(await VerifyContentAsync(cancellationToken));
    results.AddRange(await LogResponseContentAsync(cancellationToken));

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

    Log.Logger.Information("- received status code is: {StatusCode}", (int)_statusCode);

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

    Log.Logger.Verbose($"  - received reason phrase is: {_reasonPhrase}");

    var expectedReasonPhrase = reasonPhraseInstruction.Value;
    return expectedReasonPhrase == _reasonPhrase
      ? [TestStepResult.Success(reasonPhraseInstruction.TestStep)]
      : [TestStepResult.Failed(
        reasonPhraseInstruction.TestStep,
        $"Expected reason phrase '{expectedReasonPhrase}' but was '{_reasonPhrase}'."
      )];
  }

  private List<TestStepResult> CheckContentHeaders()
  {
    var headerInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.CheckContentHeader);
    if (headerInstructions == null || _contentHeaders == null)
    {
      // It's okay if there's no headers to check
      return [];
    }

    foreach (var contentHeader in _contentHeaders)
    {
      Log.Logger.Verbose("  - received content header is: {ContentHeaderKey}={ContentHeaderValue}",
      contentHeader.Key,
      string.Join(",", contentHeader.Value));
    }

    var results = new List<TestStepResult>();
    foreach (var headerInstruction in headerInstructions)
    {
      if (_contentHeaders.TryGetValues(headerInstruction.Name, out var values))
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
    if (contentInstructions.Count == 0 || _content == null)
    {
      // It's okay if there's no content to check
      return [];
    }

    var content = await _content!.ReadAsStringAsync(cancellationToken);
    var contentType = contentInstructions.First().ContentType;
    var validator = _contentValidatorFactory.ResolveValidator(contentType);

    var results = await validator.CheckContentAsync(contentInstructions, content, cancellationToken);
    return results.ToList();
  }

  private async Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.VerifyContent)
      .ToList();
    if (contentInstructions.Count == 0 || _content == null)
    {
      // It's okay if there's no content to verify
      return [];
    }

    var content = await _content!.ReadAsStringAsync(cancellationToken);
    var contentType = contentInstructions.First().ContentType;
    var validator = _contentValidatorFactory.ResolveValidator(contentType);

    return await validator.VerifyContentAsync(contentInstructions, content, cancellationToken);
  }

  private async Task<IEnumerable<TestStepResult>> LogResponseContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.LogResponseContent)
      .ToList();
    if (contentInstructions.Count == 0 || _content == null)
    {
      // It's okay if there's no content to log
      return [];
    }

    var results = new List<TestStepResult>();
    foreach (var contentInstruction in contentInstructions)
    {
      var contentString = await _content!.ReadAsStringAsync(cancellationToken);
      Log.Logger.Information("- logging response content: {ContentString}", contentString);

      results.Add(TestStepResult.Success(contentInstruction.TestStep));
    }

    return results;
  }
}