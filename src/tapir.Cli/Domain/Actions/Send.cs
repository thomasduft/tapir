namespace tomware.Tapir.Cli.Domain;

internal class Send : IAction
{
  public Area Area => Area.Execution;

  public string Name => nameof(Send);

  public Task ExecuteAsync(ActionContext actionContext, CancellationToken cancellationToken)
  {
    // Set Method
    actionContext.Method = new HttpMethod(actionContext.Instruction.Method!);
    actionContext.Endpoint = actionContext.Instruction.Value;

    return Task.CompletedTask;
  }

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    ActionContext actionContext,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    // Method is required
    if (string.IsNullOrEmpty(actionContext.Instruction.Method))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "HTTP Method must be provided."
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
          Message = "Value must be provided."
        }
      );
    }

    // Method must be either GET, POST, PUT, DELETE, PATCH
    var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
    if (!string.IsNullOrEmpty(actionContext.Instruction.Method)
      && !validMethods.Contains(actionContext.Instruction.Method.ToUpper()))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = $"HTTP Method '{actionContext.Instruction.Method}' is not valid. Valid methods are: {string.Join(", ", validMethods)}."
        }
      );
    }

    // Value must not start with a /
    if (actionContext.Instruction.Value.StartsWith("/"))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Value must not start with a '/'."
        }
      );
    }

    return Task.FromResult<IEnumerable<TestStepValidationResult>>(results);
  }
}
