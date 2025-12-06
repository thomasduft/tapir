namespace tomware.Tapir.Cli.Domain;

internal class AddContentActionValidator : IValidator
{
  private readonly string[] _validContentTypes = [
    Constants.ContentTypes.Text,
    Constants.ContentTypes.Json,
    Constants.ContentTypes.MultipartFormData
  ];

  public string Name => Constants.Actions.AddContent;
  public string Description => "Adds content to the HTTP request.";
  public IEnumerable<string> SupportedProperties =>
  [
    nameof(TestStepInstruction.ContentType) + $": Content type (e.g., {string.Join(", ", _validContentTypes)})",
    nameof(TestStepInstruction.File) + ": Path to the content file",
    nameof(TestStepInstruction.Value) + ": Direct content value"
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
        )
      );
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

    if (testStepInstruction.ContentType != Constants.ContentTypes.MultipartFormData)
    {
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
    }
    else
    {
      // For MultipartFormData, Name and Value must be provided
      if (string.IsNullOrEmpty(testStepInstruction.Name))
      {
        results.Add(
          new TestStepValidationError(
            testStepInstruction.TestStep.Id,
            "For MultipartFormData content, Name must be provided."
          )
        );
      }
      if (string.IsNullOrEmpty(testStepInstruction.Value))
      {
        results.Add(
          new TestStepValidationError(
            testStepInstruction.TestStep.Id,
            "For MultipartFormData content, Value must be provided."
          )
        );
      }
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
