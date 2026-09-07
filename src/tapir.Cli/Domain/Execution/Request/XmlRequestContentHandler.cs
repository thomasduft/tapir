using System.Text;
using System.Xml.Linq;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class XmlRequestContentHandler : IRequestContentHandler
{
  public string ContentType => Constants.ContentTypes.Xml;

  public async Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var instruction = instructions[0];
    var xmlContent = !string.IsNullOrEmpty(instruction.File)
      ? await File.ReadAllTextAsync(
        TestCaseContentFileResolver.LocateExistingFile(instruction),
        cancellationToken
      )
      : instruction.Value;
    xmlContent = VariablesHelper.ResolveVariables(xmlContent!, instruction.TestStep.TestCase.Variables);

    var document = XDocument.Parse(xmlContent!);
    var normalizedXml = document.ToString(SaveOptions.DisableFormatting);
    Log.Logger.Verbose("  - setting XML content: {XmlContent}", normalizedXml);

    return new StringContent(normalizedXml, Encoding.UTF8, Constants.ContentTypes.Xml);
  }
}
