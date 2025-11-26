namespace tomware.Tapir.Cli.Domain;

internal interface IValidator
{
  public string Name { get; }

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  );
}
