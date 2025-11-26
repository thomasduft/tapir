namespace tomware.Tapir.Cli.Domain;

internal class TestCaseValidator
{
  private readonly TestCase _testCase;

  public string TestCaseId => _testCase.Id;
  public string TestCaseTitle => _testCase.Title;

  public TestCaseValidator(TestCase testCase)
  {
    _testCase = testCase;
  }

  public TestCaseValidationResult Validate()
  {
    var result = new TestCaseValidationResult(TestCaseId, TestCaseTitle);

    // check property Type - can be Definition or Run
    var allowedTypes = new[] {
      Constants.TestCaseType.Definition,
      Constants.TestCaseType.Run
    };
    if (!allowedTypes.Contains(_testCase.Type))
    {
      result.AddError("Type", $"Type must be '{Constants.TestCaseType.Definition}' or '{Constants.TestCaseType.Run}'.");
    }

    // check property Status
    var allowedStatus = new[] {
      Constants.TestCaseStatus.Passed,
      Constants.TestCaseStatus.Failed,
      Constants.TestCaseStatus.Unknown
    };
    if (!allowedStatus.Contains(_testCase.Status))
    {
      result.AddError("Status", $"Status must be either '{Constants.TestCaseStatus.Passed}', '{Constants.TestCaseStatus.Failed}' or '{Constants.TestCaseStatus.Unknown}'.");
    }

    // check property Link if contains link should be a valid markdown link pointing to a file
    //       Format: [The administrator must be authenticated](TC-001-Login.md)
    if (_testCase.HasLinkedFile && !File.Exists(_testCase.LinkedFile))
    {
      result.AddError("Link", $"Linked file {_testCase.LinkedFile} does not exist.");
    }

    foreach (var step in _testCase.Tables.SelectMany(t => t.Steps))
    {
      if (string.IsNullOrWhiteSpace(step.TestData))
        continue;

      try
      {
        TestStepInstruction.FromTestStep(step, _testCase.Variables);
      }
      catch (Exception ex)
      {
        result.AddError(step.Id, ex.Message);
      }
    }

    return result;
  }
}
