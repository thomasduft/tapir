namespace tomware.Tapir.Cli.Domain;

internal class StoreVariableActionValidator : IValidator
{
  public string Name => Constants.Actions.StoreVariable;

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

    // Either Path or Value is required
    if (string.IsNullOrEmpty(testStepInstruction.Path)
      && string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Either Path or Value must be provided."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
