namespace tomware.Tapir.Cli.Domain;

internal interface IAction
{
  public Area Area { get; }
  public string Name { get; }

  public Task ExecuteAsync(
    ActionContext actionContext,
    CancellationToken cancellationToken
  );

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    ActionContext actionContext,
    CancellationToken cancellationToken
  );
}