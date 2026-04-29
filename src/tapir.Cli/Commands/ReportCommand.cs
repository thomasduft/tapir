using System.Text.Json;
using System.Text.Json.Serialization;

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli;

internal class ReportCommand : CommandLineApplication
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };

  private readonly CommandOption<string> _inputDirectory;
  private readonly CommandOption<string> _outputFile;

  public ReportCommand()
  {
    Name = "report";
    Description = "Generates a self-contained HTML report from Test Case run output files.";

    _inputDirectory = Option<string>(
      "-i|--input-directory",
      "The directory containing Test Case run result files.",
      CommandOptionType.SingleValue,
      cfg => cfg.DefaultValue = ".",
      true
    );

    _outputFile = Option<string>(
      "-o|--output-file",
      "The output file path for the HTML report.",
      CommandOptionType.SingleValue,
      cfg => cfg.DefaultValue = "report.html",
      true
    );

    OnExecuteAsync(ExecuteAsync);
  }

  private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
  {
    var inputDirectory = _inputDirectory.ParsedValue;
    var outputFile = _outputFile.ParsedValue;

    Log.Logger.Information("Building report from '{InputDirectory}'...", inputDirectory);

    var reporter = new TestRunReporter();
    var report = await reporter.BuildReportAsync(inputDirectory, cancellationToken);

    Log.Logger.Information(
      "Found {Total} test case run(s): {Passed} passed, {Failed} failed.",
      report.Total, report.Passed, report.Failed
    );

    var html = GenerateHtml(report);

    var outputDir = Path.GetDirectoryName(outputFile);
    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
      Directory.CreateDirectory(outputDir);

    await File.WriteAllTextAsync(outputFile, html, cancellationToken);

    Log.Logger.Information("Report written to '{OutputFile}'.", outputFile);

    return 0;
  }

  private static string GenerateHtml(TestRunReport report)
  {
    var template = ResourceLoader.GetHtmlTemplate(Templates.Report);
    var reportJson = JsonSerializer.Serialize(report, JsonOptions);
    var generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    return template
      .Replace("%%REPORT_JSON%%", reportJson)
      .Replace("%%GENERATED_AT%%", generatedAt);
  }
}
