namespace tomware.Tapir.Cli.Domain;

internal record TestCaseValidationResult
{
  private readonly List<TestCaseValidationError> _testCaseValidationErrors = [];
  private readonly List<TestStepValidationError> _testStepsValidationErrors = [];

  public string TestCaseId { get; private set; } = string.Empty;

  public string TestCaseTitle { get; private set; } = string.Empty;

  public bool IsValid
  {
    get
    {
      return _testCaseValidationErrors.Count == 0 && _testStepsValidationErrors.Count == 0;
    }
  }

  public IEnumerable<string> Errors
  {
    get
    {
      return _testCaseValidationErrors
        .Select(x => x.ToString())
          .Concat(_testStepsValidationErrors
            .Select(x => x.ToString()));
    }
  }

  public TestCaseValidationResult(string testCaseId, string testCaseTitle)
  {
    TestCaseId = testCaseId;
    TestCaseTitle = testCaseTitle;
  }

  public void AddError(string property, string errorMessage)
  {
    _testCaseValidationErrors.Add(new TestCaseValidationError(property, errorMessage));
  }

  public void AddError(int stepId, string errorMessage)
  {
    _testStepsValidationErrors.Add(new TestStepValidationError(stepId, errorMessage));
  }
}
