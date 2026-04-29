using McMaster.Extensions.CommandLineUtils;

using tomware.Tapir.Cli.Utils;

public class Cli : CommandLineApplication
{
  public Cli(IEnumerable<CommandLineApplication> commands)
  {
    Name = "tapir";
    Description = "A command-line tool for managing and executing automated api test cases.";

    HelpOption("-? | -h | --help", true);

    AddOption(new CommandOption(
      OtelHelper.OtlpOption,
      CommandOptionType.SingleValue)
    {
      Description = "Enable OpenTelemetry metrics export to the specified OTLP endpoint (e.g., http://localhost:4318/v1/metrics).",
      Inherited = true
    });
    AddOption(new CommandOption(
      LogHelper.VerboseOption,
      CommandOptionType.NoValue)
    {
      Description = "Enable verbose logging.",
      Inherited = true
    });

    foreach (var cmd in commands)
    {
      AddSubcommand(cmd);
    }
  }
}