namespace tomware.Tapir.Cli.Domain;

internal class VerifyContentActionValidator : IValidator
{
  public string Name => Constants.Actions.VerifyContent;

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // File or Value is required
    if (string.IsNullOrEmpty(testStepInstruction.File)
      && string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Either File or Value must be provided."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
