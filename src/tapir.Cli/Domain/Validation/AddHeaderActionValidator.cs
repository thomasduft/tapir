namespace tomware.Tapir.Cli.Domain;

internal class AddHeaderActionValidator : IValidator
{
  public string Name => Constants.Actions.AddHeader;
  public string Description => "Adds headers to the HTTP request.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.Name) + ": The name of the header",
    nameof(TestStepInstruction.Value) + ": The value of the header"
  ];

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    if (string.IsNullOrWhiteSpace(testStepInstruction.Name))
    {
      results.Add(new TestStepValidationError(
        testStepInstruction.TestStep.Id,
        "Header Name is required."
      ));
    }

    if (string.IsNullOrWhiteSpace(testStepInstruction.Value))
    {
      results.Add(new TestStepValidationError(
        testStepInstruction.TestStep.Id,
        "Header Value is required."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}