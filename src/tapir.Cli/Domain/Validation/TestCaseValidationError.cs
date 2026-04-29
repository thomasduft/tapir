namespace tomware.Tapir.Cli.Domain;

internal record TestCaseValidationError(string Property, string ErrorMessage)
{
  public override string ToString()
  {
    return $"{ErrorMessage}";
  }
}
