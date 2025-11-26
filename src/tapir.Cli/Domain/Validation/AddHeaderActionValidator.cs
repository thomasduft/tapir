namespace tomware.Tapir.Cli.Domain;

internal class AddHeaderActionValidator : IValidator
{
  public string Name => Constants.Actions.AddHeader;

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
