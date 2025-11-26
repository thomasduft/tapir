namespace tomware.Tapir.Cli.Domain;

internal class SendActionValidator : IValidator
{
  public string Name => Constants.Actions.Send;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    // Method is required
    if (string.IsNullOrEmpty(testStepInstruction.Method))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "HTTP Method must be provided."
        }
      );
    }

    // Value is required
    if (string.IsNullOrEmpty(testStepInstruction.Endpoint))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Endpoint must be provided."
        }
      );
    }

    // Method must be either GET, POST, PUT, DELETE, PATCH
    var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
    if (!string.IsNullOrEmpty(testStepInstruction.Method)
      && !validMethods.Contains(testStepInstruction.Method.ToUpper()))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = $"HTTP Method '{testStepInstruction.Method}' is not valid. Valid methods are: {string.Join(", ", validMethods)}."
        }
      );
    }

    // Value must not start with a /
    if (testStepInstruction.Endpoint.StartsWith('/'))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Endpoint must not start with a '/'."
        }
      );
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
