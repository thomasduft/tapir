namespace tomware.Tapir.Cli.Domain;

internal class AddQueryParameterActionValidator : IValidator
{
  public string Name => Constants.Actions.AddQueryParameter;

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
          Message = "Query parameter Name is required."
        }
      );
    }

    if (string.IsNullOrWhiteSpace(testStepInstruction.Value))
    {
      results.Add(
       new TestStepValidationResult
       {
         IsValid = false,
         Message = "Query parameter Value is required."
       }
     );
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
