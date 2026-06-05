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

    // 3. Build a tree from the found files grouped by directory.
    var root = new TreeNode(Path.GetFullPath(inputDirectory));
    foreach (var file in files)
    {
      var testCase = await TestCase.FromTestCaseFileAsync(file, cancellationToken);
      var relativePath = Path.GetRelativePath(inputDirectory, file);
      var parts = relativePath.Split(Path.DirectorySeparatorChar);

      var current = root;
      for (var i = 0; i < parts.Length - 1; i++)
      {
        var existing = current.Children.FirstOrDefault(c => c.Name == parts[i] && c.TestCase is null);
        if (existing is null)
        {
          existing = new TreeNode(parts[i]);
          current.Children.Add(existing);
        }
        current = existing;
      }

      current.Children.Add(new TreeNode(Path.GetFileNameWithoutExtension(file))
      {
        TestCase = (testCase.Id, testCase.Title)
      });
    }

    // 4. Print the tree.
    Console.WriteLine($"{root.Name}/");
    PrintTree(root, string.Empty);

    return 0;
  }

  private static void PrintTree(TreeNode node, string prefix)
  {
    for (var i = 0; i < node.Children.Count; i++)
    {
      var child = node.Children[i];
      var isLast = i == node.Children.Count - 1;
      var connector = isLast ? "└── " : "├── ";
      var childPrefix = prefix + (isLast ? "    " : "│   ");

      if (child.TestCase is var (id, title))
      {
        Console.WriteLine($"{prefix}{connector}{id}: {title}");
      }
      else
      {
        Console.WriteLine($"{prefix}{connector}{child.Name}/");
        PrintTree(child, childPrefix);
      }
    }
  }

  private sealed class TreeNode(string name)
  {
    public string Name { get; } = name;
    public (string Id, string Title)? TestCase { get; init; }
    public List<TreeNode> Children { get; } = [];
  }
}
