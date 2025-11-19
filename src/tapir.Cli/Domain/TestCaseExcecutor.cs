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

    var requestMessage = HttpRequestMessageBuilder
      .Create(instructions)
      .WithDomain(domain)
      .Build();

    var response = await client.SendAsync(requestMessage, cancellationToken);

    var testStepResults = HttpResponseMessageValidator
      .Create(instructions)
      .WithStatusCode(response.StatusCode)
      .WithReasonPhrase(response.ReasonPhrase)
      .WithContent(response.Content)
      .WithHeaders(response.Headers)
      .Validate();

    return new TestCaseExecutionResult(
      testStepResults,
      new Dictionary<string, string>()
    );
  }
}
