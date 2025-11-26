namespace tomware.Tapir.Cli.Domain;

internal interface ITestCaseValidator
{
  public Task<TestCaseValidationResult> ValidateAsync(
    TestCase testCase,
    CancellationToken cancellationToken
  );
}

internal class TestCaseValidator : ITestCaseValidator
{
  private readonly IEnumerable<IValidator> _validators;

  public TestCaseValidator(
    IEnumerable<IValidator> validators
  )
  {
    _validators = validators;
  }

  public async Task<TestCaseValidationResult> ValidateAsync(
    TestCase testCase,
    CancellationToken cancellationToken
  )
  {
    var result = new TestCaseValidationResult(testCase.Id, testCase.Title);

    // check property Type - can be Definition or Run
    var allowedTypes = new[] {
      Constants.TestCaseType.Definition,
      Constants.TestCaseType.Run
    };
    if (!allowedTypes.Contains(testCase.Type))
    {
      result.AddError("Type", $"Type must be '{Constants.TestCaseType.Definition}' or '{Constants.TestCaseType.Run}'.");
    }

    // check property Status
    var allowedStatus = new[] {
      Constants.TestCaseStatus.Passed,
      Constants.TestCaseStatus.Failed,
      Constants.TestCaseStatus.Unknown
    };
    if (!allowedStatus.Contains(testCase.Status))
    {
      result.AddError("Status", $"Status must be either '{Constants.TestCaseStatus.Passed}', '{Constants.TestCaseStatus.Failed}' or '{Constants.TestCaseStatus.Unknown}'.");
    }

    // check property Link if contains link should be a valid markdown link pointing to a file
    //       Format: [The administrator must be authenticated](TC-001-Login.md)
    if (testCase.HasLinkedFile && !File.Exists(testCase.LinkedFile))
    {
      result.AddError("Link", $"Linked file {testCase.LinkedFile} does not exist.");
    }

    foreach (var step in testCase.Tables.SelectMany(t => t.Steps))
    {
      if (string.IsNullOrWhiteSpace(step.TestData))
        continue;

      try
      {
        var instruction = TestStepInstruction.FromTestStep(step, testCase.Variables);
        var validationErrors = await _validators
          .FirstOrDefault(v => v.Name == instruction.Action)!
          .ValidateAsync(instruction, cancellationToken);
        if (validationErrors != null)
        {
          foreach (var validationError in validationErrors)
          {
            result.AddError(validationError);
          }
        }
      }
      catch (Exception ex)
      {
        result.AddError(
          new TestStepValidationError(
            step.Id,
            $"Exception during validation of Test Step {step.Id}: {ex.Message}"
          ));
      }
    }

    return result;
  }
}
