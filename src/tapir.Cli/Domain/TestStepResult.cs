namespace tomware.Tapir.Cli.Domain;

internal record TestStepResult
{
  public int TestStepId { get; private set; }
  public bool IsSuccess { get; private set; } = false;
  public string Error { get; private set; } = string.Empty;

  public TestStepResult(TestStep step)
  {
    TestStepId = step.Id;
  }

  public static TestStepResult Success(TestStep step)
  {
    return new TestStepResult(step)
    {
      IsSuccess = true
    };
  }

  public static TestStepResult Failed(TestStep step, string error)
  {
    return new TestStepResult(step)
    {
      IsSuccess = false,
      Error = SanitizeError(error)
    };
  }

  private static string SanitizeError(string error)
  {
    return error
      .Replace("\r\n", " ")
      .Replace("\n", " ");
  }
}