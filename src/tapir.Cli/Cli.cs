using McMaster.Extensions.CommandLineUtils;

public class Cli : CommandLineApplication
{
  public Cli(IEnumerable<CommandLineApplication> commands)
  {
    Name = "tapir";
    Description = "A command-line tool for managing and executing automated api test cases.";

    HelpOption("-? | -h | --help", true);

    foreach (var cmd in commands)
    {
      AddSubcommand(cmd);
    }
  }
}