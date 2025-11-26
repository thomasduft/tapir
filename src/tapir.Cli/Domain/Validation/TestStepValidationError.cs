namespace tomware.Tapir.Cli.Domain;

internal record TestStepValidationError(int StepId, string ErrorMessage)
{
  public override string ToString()
  {
    return $"Step {StepId:D2}: {ErrorMessage}";
  }
}