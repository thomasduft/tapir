using System.Text;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class TextRequestContentHandler : IRequestContentHandler
{
  public string ContentType => Constants.ContentTypes.Text;

  public async Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var instruction = instructions[0];
    var textContent = !string.IsNullOrEmpty(instruction.File)
      ? await File.ReadAllTextAsync(
          TestCaseContentFileResolver.LocateExistingFile(instruction),
          cancellationToken
        )
      : instruction.Value;

    Log.Logger.Verbose("  - setting text content: {TextContent}", textContent);

    return new StringContent(textContent!, Encoding.UTF8, Constants.ContentTypes.Text);
  }
}
