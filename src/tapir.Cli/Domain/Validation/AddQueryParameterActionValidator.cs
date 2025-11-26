namespace tomware.Tapir.Cli.Domain;

internal class AddQueryParameterActionValidator : IValidator
{
  public string Name => Constants.Actions.AddQueryParameter;

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationError>();

    if (string.IsNullOrWhiteSpace(testStepInstruction.Name))
    {
      results.Add(
        new TestStepValidationError(
          testStepInstruction.TestStep.Id,
          "Query parameter Name is required."
      ));
    }

    if (string.IsNullOrWhiteSpace(testStepInstruction.Value))
    {
      results.Add(
       new TestStepValidationError(
         testStepInstruction.TestStep.Id,
        "Query parameter Value is required."
       ));
    }

    return Task.FromResult(results.AsEnumerable());
  }
}
