namespace tomware.Tapir.Cli.Domain;

internal class CheckHeaderActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckHeader;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
