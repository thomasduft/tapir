using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal interface ITestCaseExecutor
{
  Task<TestCaseExecutionResult> ExecuteAsync(
    string domain,
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  );
}

internal class TestCaseExecutor : ITestCaseExecutor
{
  private readonly IHttpClientFactory _factory;
  private readonly IHttpRequestMessageBuilder _requestMessageBuilder;
  private readonly IResponseContentValidatorFactory _responseContentValidatorFactory;

  public TestCaseExecutor(
    IHttpClientFactory factory,
    IHttpRequestMessageBuilder requestMessageBuilder,
    IResponseContentValidatorFactory responseContentValidatorFactory
  )
  {
    _factory = factory;
    _requestMessageBuilder = requestMessageBuilder;
    _responseContentValidatorFactory = responseContentValidatorFactory;
  }

  public async Task<TestCaseExecutionResult> ExecuteAsync(
    string domain,
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    using var client = _factory.CreateClient(Constants.HttpClientName);

    var requestMessage = await _requestMessageBuilder
      .BuildAsync(domain, instructions, cancellationToken);

    Log.Logger.Information("- sending request: {RequestUri} ({Method})",
      requestMessage.RequestUri,
      requestMessage.Method);

    var response = await client
      .SendAsync(requestMessage, cancellationToken);

    var responseValidationResult = await new HttpResponseMessageValidator(
      _responseContentValidatorFactory,
      instructions)
      .WithStatusCode(response.StatusCode)
      .WithReasonPhrase(response.ReasonPhrase)
      .WithContentHeaders(response.Content.Headers)
      .WithContent(response.Content)
      .ValidateAsync(cancellationToken);

    var variableExtractionResult = await VariableExtractor
      .Create(instructions)
      .FromContent(response.Content)
      .ExtractAsync(cancellationToken);

    return new TestCaseExecutionResult(
      responseValidationResult.Concat(variableExtractionResult.TestStepResults),
      variableExtractionResult.Variables
    );
  }
}
