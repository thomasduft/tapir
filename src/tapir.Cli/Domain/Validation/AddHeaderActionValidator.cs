namespace tomware.Tapir.Cli.Domain;

internal class AddHeaderActionValidator : IValidator
{
  public string Name => Constants.Actions.AddHeader;

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    if (string.IsNullOrWhiteSpace(testStepInstruction.Name))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Header Name is required."
        }
      );
    }

    if (string.IsNullOrWhiteSpace(testStepInstruction.Value))
    {
      results.Add(
       new TestStepValidationResult
       {
         IsValid = false,
         Message = "Header Value is required."
       }
     );
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
