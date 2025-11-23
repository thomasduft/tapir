namespace tomware.Tapir.Cli.Domain;

internal static class Constants
{
  public const string VariablePreAndSuffix = "@@";

  internal static class TestCaseType
  {
    public const string Definition = "Definition";
    public const string Run = "Run";
  }

  internal static class TestCaseStatus
  {
    public const string Passed = "Passed";
    public const string Failed = "Failed";
    public const string Unknown = "Unknown";
  }

  internal static class Actions
  {
    public const string AddHeader = "AddHeader";
    public const string AddQueryParameter = "AddQueryParameter";
    public const string Send = "Send";
    public const string AddContent = "AddContent";
    public const string CheckStatusCode = "CheckStatusCode";
    public const string CheckReasonPhrase = "CheckReasonPhrase";
    public const string CheckContent = "CheckContent";
    public const string CheckHeader = "CheckHeader";
    public const string VerifyContent = "VerifyContent";
    public const string StoreVariable = "StoreVariable";
  }

}