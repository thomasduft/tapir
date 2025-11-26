namespace tomware.Tapir.Cli.Domain;

internal interface IValidator
{
  public string Name { get; }

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  );
}
