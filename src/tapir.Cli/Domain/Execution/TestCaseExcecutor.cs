using tomware.Tapir.Cli.Utils;

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

    ConsoleHelper.WriteLineYellow($"- sending request: {requestMessage.RequestUri} ({requestMessage.Method})");

    var response = await client
      .SendAsync(requestMessage, cancellationToken);

    var responseValidationResult = await HttpResponseMessageValidator
      .Create(instructions)
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
