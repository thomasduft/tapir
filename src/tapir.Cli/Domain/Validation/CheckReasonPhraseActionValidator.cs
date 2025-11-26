namespace tomware.Tapir.Cli.Domain;

internal class CheckReasonPhraseActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckReasonPhrase;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    return Task.FromResult(results.AsEnumerable());
  }
}
