namespace tomware.Tapir.Cli.Domain.Actions;

internal class AddQueryParam : IAction
{
  public Area Area => Area.Request;

  public string Name => nameof(AddQueryParam);

  public Task ExecuteAsync(ActionContext actionContext, CancellationToken cancellationToken)
  {
    var paramName = actionContext.Instruction.Name;
    var paramValue = actionContext.Instruction.Value;

    actionContext.QueryParameters.Add(paramName, paramValue);

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
          Message = "Query parameter name is required."
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
          Message = "Query parameter value is required."
        }
      );
    }

    return Task.FromResult<IEnumerable<TestStepValidationResult>>(results);
  }
}
