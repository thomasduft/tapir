namespace tomware.Tapir.Cli.Domain;

internal class AddContentActionValidator : IValidator
{
  public string Name => Constants.Actions.AddContent;

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // Either File must exist or Value must be present
    if (string.IsNullOrEmpty(testStepInstruction.Value)
      && string.IsNullOrEmpty(testStepInstruction.File))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Either content File or Value must be provided."
        )
      );
    }

    // If File is provided, it must exist and must not be empty
    if (!string.IsNullOrEmpty(testStepInstruction.File))
    {
      if (!File.Exists(testStepInstruction.File))
      {
        results.Add(
          new TestStepValidationError(
            testStepInstruction.TestStep.Id,
            $"Content file '{testStepInstruction.File}' does not exist."
          )
        );
      }
      else
      {
        var fileInfo = new FileInfo(testStepInstruction.File);
        if (fileInfo.Length == 0)
        {
          results.Add(
            new TestStepValidationError(
              testStepInstruction.TestStep.Id,
              $"Content file '{testStepInstruction.File}' is empty."
            )
          );
        }
      }
    }

    // If Value is provided, it must not be empty
    if (!string.IsNullOrEmpty(testStepInstruction.Value))
    {
      if (string.IsNullOrWhiteSpace(testStepInstruction.Value))
      {
        results.Add(
          new TestStepValidationError(
            testStepInstruction.TestStep.Id,
            "Content Value must not be empty."
          )
        );
      }
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
