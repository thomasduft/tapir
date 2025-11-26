namespace tomware.Tapir.Cli.Domain;

internal class CheckContentActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckContent;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
