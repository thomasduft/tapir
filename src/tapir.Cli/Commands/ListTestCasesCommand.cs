using McMaster.Extensions.CommandLineUtils;

using Serilog;

using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Cli;

internal class ListTestCasesCommand : CommandLineApplication
{
  private readonly CommandArgument<string> _inputDirectory;

  public ListTestCasesCommand()
  {
    Name = "list";
    Description = "Lists all Test Case definitions from a given directory (e.g. ./test-cases).";

    _inputDirectory = Argument<string>(
      "input-directory",
      "The directory containing the Test Case definitions.",
      cfg => cfg.DefaultValue = "."
    );

    OnExecuteAsync(ExecuteAsync);
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    // 1. Check if the input directory exists.
    var inputDirectory = _inputDirectory.Value;
    if (!Directory.Exists(inputDirectory))
    {
      Log.Logger.Error("The input directory '{InputDirectory}' does not exist.", inputDirectory);

      return 1;
    }

    // 2. Locate all Test Case definitions.
    var files = TestCaseDefinitionFinder.FindFiles(
      inputDirectory,
      string.Empty
    );
    if (files.Length == 0)
    {
      Log.Logger.Warning(
        "No Test Case definitions found in the directory '{InputDirectory}'.",
        inputDirectory
      );

      return 0;
    }

    // 3. Display the list of Test Cases with its relative directory prefix to the executing
    // directory, along with their ID and Title.
    foreach (var file in files)
    {
      var testCase = await TestCase.FromTestCaseFileAsync(file, cancellationToken);
      var path = Path.GetFullPath(file, Environment.CurrentDirectory);
      Console.WriteLine($"- {testCase.Id}: {testCase.Title} ({path})");
    }

    return await Task.FromResult(0);
  }
}
