namespace tomware.Tapir.Cli.Domain;

internal class StoreVariableActionValidator : IValidator
{
  public string Name => Constants.Actions.StoreVariable;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
