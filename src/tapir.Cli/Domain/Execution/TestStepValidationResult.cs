namespace tomware.Tapir.Cli.Domain;

internal record TestStepValidationResult
{
  public bool IsValid { get; init; }
  public string Message { get; init; } = string.Empty;

  public static TestStepValidationResult Valid()
  {
    return new TestStepValidationResult
    {
      IsValid = true
    };
  }

  public static TestStepValidationResult Invalid(string message)
  {
    return new TestStepValidationResult
    {
      IsValid = false,
      Message = message
    };
  }
}
