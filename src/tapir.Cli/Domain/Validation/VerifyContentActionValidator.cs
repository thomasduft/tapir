namespace tomware.Tapir.Cli.Domain;

internal class VerifyContentActionValidator : IValidator
{
  public string Name => Constants.Actions.VerifyContent;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
