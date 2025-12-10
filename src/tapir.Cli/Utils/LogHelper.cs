using Serilog;

namespace tomware.Tapir.Cli.Utils;

internal static class LogHelper
{
  private const string VerboseOption = "--verbose";

  public static ILogger CreateLogger(ref string[] args)
  {
    var loggerConfiguration = new LoggerConfiguration()
      .MinimumLevel.Information()
      .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
      .Enrich.FromLogContext();

    if (args.Contains(VerboseOption))
    {
      // Remove the --verbose from the args
      args = args
        .Where(arg => arg != VerboseOption)
        .ToArray();

      loggerConfiguration.MinimumLevel.Verbose();
    }

    return loggerConfiguration.CreateLogger();
  }
}