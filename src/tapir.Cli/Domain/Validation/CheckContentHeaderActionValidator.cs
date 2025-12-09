namespace tomware.Tapir.Cli.Domain;

internal class CheckContentHeaderActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckContentHeader;
  public string Description => "Checks headers in the HTTP response.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.Name) + ": The name of the header to check",
    nameof(TestStepInstruction.Value) + ": The value of the header to check"
  ];

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // Name is required
    if (string.IsNullOrEmpty(testStepInstruction.Name))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Header Name must be provided."
      ));
    }

    // Value is required
    if (string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Header Value must be provided."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}