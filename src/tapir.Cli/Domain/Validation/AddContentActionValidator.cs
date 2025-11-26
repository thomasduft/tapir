namespace tomware.Tapir.Cli.Domain;

internal class AddContentActionValidator : IValidator
{
  public string Name => Constants.Actions.AddContent;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
