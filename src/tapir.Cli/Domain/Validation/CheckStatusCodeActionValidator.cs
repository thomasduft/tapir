namespace tomware.Tapir.Cli.Domain;

internal class CheckStatusCodeActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckStatusCode;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
