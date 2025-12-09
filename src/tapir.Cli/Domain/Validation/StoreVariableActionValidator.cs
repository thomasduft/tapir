namespace tomware.Tapir.Cli.Domain;

internal class StoreVariableActionValidator : IValidator
{
  public string Name => Constants.Actions.StoreVariable;
  public string Description => "Stores a variable from the HTTP response. Enables request chaining.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.Name) + ": The name of the variable to store",
    nameof(TestStepInstruction.JsonPath) + ": The JSON path to the variable to store",
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
          "Variable Name must be provided."
      ));
    }

    // Either JsonPath or Value is required
    if (string.IsNullOrEmpty(testStepInstruction.JsonPath)
      && string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Either JsonPath or Value must be provided."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}