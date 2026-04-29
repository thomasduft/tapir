namespace tomware.Tapir.Cli.Domain;

internal class CheckReasonPhraseActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckReasonPhrase;
  public string Description => "Checks the reason phrase in the HTTP response.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.Value) + ": The reason phrase to check"
  ];

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
          "Reason phrase Value must be provided."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
