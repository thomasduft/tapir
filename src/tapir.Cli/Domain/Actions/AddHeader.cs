namespace tomware.Tapir.Cli.Domain.Actions;

internal class AddHeader : IAction
{
  public Area Area => Area.Request;

  public string Name => nameof(AddHeader);

  public Task ExecuteAsync(ActionContext actionContext, CancellationToken cancellationToken)
  {
    var headerName = actionContext.Instruction.Name;
    var headerValue = actionContext.Instruction.Value;

    actionContext.Headers.Add(headerName, headerValue);

    return Task.CompletedTask;
  }

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    ActionContext actionContext,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    // Name is required
    if (string.IsNullOrEmpty(actionContext.Instruction.Name))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Header name is required."
        }
      );
    }

    // Value is required
    if (string.IsNullOrEmpty(actionContext.Instruction.Value))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Header value is required."
        }
      );
    }

    return Task.FromResult<IEnumerable<TestStepValidationResult>>(results);
  }
}
