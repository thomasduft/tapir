namespace tomware.Tapir.Cli.Domain;

internal class LogResponseContentValidator : IValidator
{
  private readonly string[] _validContentTypes = [
    Constants.ContentTypes.Text,
    Constants.ContentTypes.Json
  ];

  public string Name => Constants.Actions.LogResponseContent;

  public string Description => "Logs the HTTP response content.";

  public IEnumerable<string> SupportedProperties => [];

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    // ContentType is required
    if (string.IsNullOrEmpty(testStepInstruction.ContentType))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "ContentType is required."
      ));
    }

    // ContentType must be valid
    if (!string.IsNullOrEmpty(testStepInstruction.ContentType)
      && !_validContentTypes.Contains(testStepInstruction.ContentType.ToLower()))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          $"ContentType '{testStepInstruction.ContentType}' is not valid. Valid types are: {string.Join(", ", _validContentTypes)}."
        )
      );
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
