namespace tomware.Tapir.Cli.Domain;

internal class TestStep
{
  public int Id { get; set; } = 0;
  public string Description { get; set; } = string.Empty;
  public string TestData { get; set; } = string.Empty;
  public string ExpectedResult { get; set; } = string.Empty;
  public string ActualResult { get; set; } = string.Empty;
  public bool IsSuccess { get; set; } = false;

  // TODO: TestSteps should contain a reference to the TestCase they belong to
  // (i.e. `public TestCase TestCase { get; set; }`) so we can easily access the TestCase
  // variables and domain when executing the TestStep.
}