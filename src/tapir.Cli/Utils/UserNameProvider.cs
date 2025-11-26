using System.Diagnostics;

namespace tomware.Tapir.Cli.Utils;

internal static class UserNameProvider
{
  public static string GetUserName()
  {
    // 1. try to read the user name from the git username config
    using Process proc = new();
    proc.StartInfo.FileName = "git";
    proc.StartInfo.Arguments = "config user.name";
    proc.StartInfo.UseShellExecute = false;
    proc.StartInfo.RedirectStandardOutput = true;
    proc.Start();
    var userName = proc.StandardOutput
      .ReadToEnd()
      .Replace(Environment.NewLine, string.Empty)
      .Replace("\n", string.Empty);
    proc.WaitForExit();

    // 2. try to read the user name from the environment
    return userName ?? Environment.UserName;
  }
}