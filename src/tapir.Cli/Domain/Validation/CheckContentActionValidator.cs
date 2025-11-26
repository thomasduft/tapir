namespace tomware.Tapir.Cli.Domain;

internal class CheckContentActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckContent;

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // Value is required
    if (string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Content Value must be provided."
      ));
    }

    // Path is optional but if provided, requires Value to be present
    if (!string.IsNullOrEmpty(testStepInstruction.Path)
      && string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Content Value must be provided when Path is specified."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
