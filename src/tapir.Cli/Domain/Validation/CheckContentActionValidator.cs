namespace tomware.Tapir.Cli.Domain;

internal class CheckContentActionValidator : IValidator
{
  private readonly string[] _validContentTypes = [
    Constants.ContentTypes.Json,
      Constants.ContentTypes.Xml,
      Constants.ContentTypes.Text
  ];

  public string Name => Constants.Actions.CheckContent;
  public string Description => "Checks content in the HTTP response.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.ContentType) + $": Content type (e.g., {string.Join(", ", _validContentTypes)})",
    nameof(TestStepInstruction.JsonPath) + ": JSON path to the content to check",
    nameof(TestStepInstruction.Value) + ": The content value to check",
  ];

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

    // Value is required
    if (string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Value must be provided."
      ));
    }

    // Path is optional but if provided, requires Value to be present
    if (!string.IsNullOrEmpty(testStepInstruction.JsonPath)
      && string.IsNullOrEmpty(testStepInstruction.Value))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Value must be provided when JsonPath is specified."
      ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
