namespace tomware.Tapir.Cli.Domain;

internal class VerifyContentActionValidator : IValidator
{
  public string Name => Constants.Actions.VerifyContent;
  public string Description => "Verifies content in the HTTP response.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.File) + ": The file path to the content to verify",
    nameof(TestStepInstruction.Value) + ": The content value to verify"
  ];

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // File or Value is required
    if (string.IsNullOrEmpty(testStepInstruction.File)
      && string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Either File or Value must be provided."
      ));
    }

    // If File is provided it must exist and its content must not be empty
    if (!string.IsNullOrEmpty(testStepInstruction.File))
    {
      var filePath = TestCaseContentFileResolver.TryLocateExistingFile(testStepInstruction);
      if (filePath == null)
      {
        results.Add(
          new TestStepValidationError(
            testStepInstruction.TestStep.Id,
            $"File '{testStepInstruction.File}' does not exist."
          ));
      }
      else
      {
        var fileContent = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(fileContent))
        {
          results.Add(
            new TestStepValidationError(
              testStepInstruction.TestStep.Id,
              $"File '{testStepInstruction.File}' must not be empty or whitespace."
            ));
        }
      }
    }

    // If Value is provided it must not be empty
    if (!string.IsNullOrEmpty(testStepInstruction.Value)
      && string.IsNullOrWhiteSpace(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Value must not be empty or whitespace."
        ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
