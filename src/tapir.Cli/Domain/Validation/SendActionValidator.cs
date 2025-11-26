namespace tomware.Tapir.Cli.Domain;

internal class SendActionValidator : IValidator
{
  public string Name => Constants.Actions.Send;
  public string Description => "Sends an HTTP request.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.Method) + ": The HTTP method to use",
    nameof(TestStepInstruction.Endpoint) + ": The endpoint to send the request to"
  ];

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // Method is required
    if (string.IsNullOrEmpty(testStepInstruction.Method))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "HTTP Method must be provided."
      ));
    }

    // Value is required
    if (string.IsNullOrEmpty(testStepInstruction.Endpoint))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Endpoint must be provided."
      ));
    }

    // Method must be either GET, POST, PUT, DELETE, PATCH
    var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
    if (!string.IsNullOrEmpty(testStepInstruction.Method)
      && !validMethods.Contains(testStepInstruction.Method.ToUpper()))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          $"HTTP Method '{testStepInstruction.Method}' is not valid. Valid methods are: {string.Join(", ", validMethods)}."
      ));
    }

    // Value must not start with a /
    if (testStepInstruction.Endpoint.StartsWith('/'))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Endpoint must not start with a '/'."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
