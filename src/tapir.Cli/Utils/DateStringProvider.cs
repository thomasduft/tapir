namespace tomware.Tapir.Cli.Utils;

internal static class DateStringProvider
{
  public static string GetDateString()
  {
    return DateTime.Now.ToString("yyyy-MM-dd");
  }
}
