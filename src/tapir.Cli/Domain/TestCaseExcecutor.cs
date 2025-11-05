namespace tomware.Tapir.Cli.Domain;

internal interface ITestCaseExecutor
{
  Task<IEnumerable<TestStepResult>> ExecuteAsync(
    string domain,
    CancellationToken cancellationToken
  );
}

internal class TestCaseExecutor : ITestCaseExecutor
{
  private readonly IHttpClientFactory _factory;

  public TestCaseExecutor(IHttpClientFactory factory)
  {
    _factory = factory;
  }

  public async Task<IEnumerable<TestStepResult>> ExecuteAsync(
    string domain,
    CancellationToken cancellationToken
  )
  {
    using var client = _factory.CreateClient();

    // Arrange

    // Add Headers

    // Add Body

    // Create proper BaseAddress with all QueryParameters

    client.BaseAddress = new Uri(domain);

    // Act
    // Send proper Method
    var request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
    var response = await client.SendAsync(request, cancellationToken);

    // Assert

    return Enumerable.Empty<TestStepResult>();
  }
}
