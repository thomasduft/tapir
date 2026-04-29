namespace tomware.Tapir.Cli.Domain;

internal interface IValidator
{
  public string Name { get; }
  public string Description { get; }
  public IEnumerable<string> SupportedProperties { get; }

  public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
    TestStepInstruction testStepInstruction,
    CancellationToken cancellationToken
  );
}
