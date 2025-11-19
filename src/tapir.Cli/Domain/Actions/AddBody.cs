namespace tomware.Tapir.Cli.Domain;

internal class AddBody : IAction
{
  public Area Area => Area.Request;

  public string Name => nameof(AddBody);

  public async Task ExecuteAsync(ActionContext actionContext, CancellationToken cancellationToken)
  {
    // if File is present, load from file otherwise use Value
    if (!string.IsNullOrEmpty(actionContext.Instruction.File))
    {
      var bodyContent = await File.ReadAllTextAsync(actionContext.Instruction.File, cancellationToken);
    }

    // if Value is present, use it
    if (!string.IsNullOrEmpty(actionContext.Instruction.Value))
    {
      var bodyContent = actionContext.Instruction.Value;
    }

    await Task.CompletedTask;
  }

  public Task<IEnumerable<TestStepValidationResult>> ValidateAsync(
    ActionContext actionContext,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepValidationResult>();

    // Either File must exist or Value must be present
    if (string.IsNullOrEmpty(actionContext.Instruction.File)
      && string.IsNullOrEmpty(actionContext.Instruction.Value))
    {
      results.Add(
        new TestStepValidationResult
        {
          IsValid = false,
          Message = "Either body File or Value must be provided."
        }
      );
    }

    // If File is provided, it must exist and must not be empty
    if (!string.IsNullOrEmpty(actionContext.Instruction.File))
    {
      if (!File.Exists(actionContext.Instruction.File))
      {
        results.Add(
          new TestStepValidationResult
          {
            IsValid = false,
            Message = $"Body file '{actionContext.Instruction.File}' does not exist."
          }
        );
      }
      else
      {
        var fileInfo = new FileInfo(actionContext.Instruction.File);
        if (fileInfo.Length == 0)
        {
          results.Add(
            new TestStepValidationResult
            {
              IsValid = false,
              Message = $"Body file '{actionContext.Instruction.File}' is empty."
            }
          );
        }
      }
    }

    // If Value is provided, it must not be empty
    if (!string.IsNullOrEmpty(actionContext.Instruction.Value))
    {
      if (string.IsNullOrWhiteSpace(actionContext.Instruction.Value))
      {
        results.Add(
          new TestStepValidationResult
          {
            IsValid = false,
            Message = "Body Value must not be empty."
          }
        );
      }
    }

    return Task.FromResult<IEnumerable<TestStepValidationResult>>(results);
  }
}
