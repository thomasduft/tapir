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
    public const string AddContent = "AddContent";
    public const string AddHeader = "AddHeader";
    public const string AddQueryParameter = "AddQueryParameter";
    public const string Send = "Send";
    public const string CheckContent = "CheckContent";
    public const string CheckHeader = "CheckHeader";
    public const string CheckReasonPhrase = "CheckReasonPhrase";
    public const string CheckStatusCode = "CheckStatusCode";
    public const string StoreVariable = "StoreVariable";
    public const string VerifyContent = "VerifyContent";
  }

  internal static class ContentTypes
  {
    public const string Json = "application/json";
    public const string Xml = "application/xml";
    public const string Text = "text/plain";
    public const string ByteArrayContent = "application/octet-stream";
    public const string FormUrlEncoded = "application/x-www-form-urlencoded";
  }
}
