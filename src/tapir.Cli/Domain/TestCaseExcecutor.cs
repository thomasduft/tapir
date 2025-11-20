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

  public TestCaseExecutor(
    IHttpClientFactory factory
  )
  {
    _factory = factory;
  }

  public async Task<TestCaseExecutionResult> ExecuteAsync(
    string domain,
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    using var client = _factory.CreateClient();

    var requestMessage = await HttpRequestMessageBuilder
      .Create(instructions)
      .WithDomain(domain)
      .BuildAsync(cancellationToken);

    var response = await client
      .SendAsync(requestMessage, cancellationToken);

    var testStepResults = await HttpResponseMessageValidator
      .Create(instructions)
      .WithStatusCode(response.StatusCode)
      .WithReasonPhrase(response.ReasonPhrase)
      .WithContent(response.Content)
      .WithHeaders(response.Headers)
      .ValidateAsync(cancellationToken);

    var variables = await VariableExtractor
      .Create(instructions)
      .FromContent(response.Content)
      .ExtractAsync(cancellationToken);

    return new TestCaseExecutionResult(
      testStepResults,
      variables
    );
  }
}
