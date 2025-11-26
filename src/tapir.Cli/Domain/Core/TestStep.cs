namespace tomware.Tapir.Cli.Domain;

internal class TestStep
{
  public int Id { get; set; } = 0;
  public string Description { get; set; } = string.Empty;
  public string TestData { get; set; } = string.Empty;
  public string ExpectedResult { get; set; } = string.Empty;
  public bool IsSuccess { get; set; } = false;
}