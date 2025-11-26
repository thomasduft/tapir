namespace tomware.Tapir.Cli.Domain;

internal class CheckStatusCodeActionValidator : IValidator
{
  public string Name => Constants.Actions.CheckStatusCode;
  public string Description => "Checks the status code in the HTTP response.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.Value) + ": The status code to check"
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
          "Status code Value must be provided."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}