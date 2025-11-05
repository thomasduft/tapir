namespace tomware.Tapir.Cli.Utils;

internal static class ConsoleHelper
{
  internal static void WriteLineYellow(string value)
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(value);
    Console.ForegroundColor = ConsoleColor.White;
  }

  internal static void WriteLineSuccess(string value)
  {
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(value);
    Console.ForegroundColor = ConsoleColor.White;
  }

  internal static void WriteLineError(string value)
  {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(value);
    Console.ForegroundColor = ConsoleColor.White;
  }

  internal static void WriteLine(string value)
  {
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(value);
  }
}
