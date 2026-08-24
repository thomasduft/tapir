namespace tomware.Tapir.Cli.Domain;

internal class SaveContentActionValidator : IValidator
{
  public string Name => Constants.Actions.SaveContent;

  public string Description => "Saves the HTTP response content to a file.";

  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.File) + ": Path of the file the response content is saved to"
  ];

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // File is required
    if (string.IsNullOrEmpty(testStepInstruction.File))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "File is required."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
