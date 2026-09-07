using System.Xml.Linq;
using System.Xml.XPath;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class XmlResponseContentValidator : IResponseContentValidator
{
  public string ContentType => Constants.ContentTypes.Xml;

  public Task<IEnumerable<TestStepResult>> CheckContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrEmpty(content))
    {
      return Task.FromResult<IEnumerable<TestStepResult>>([
        TestStepResult.Failed(instructions[0].TestStep, "Response content is empty.")
      ]);
    }

    Log.Logger.Verbose("  - received content is: {@Xml}", content);
    XDocument document;
    try
    {
      document = XDocument.Parse(content);
    }
    catch (System.Xml.XmlException exception)
    {
      return Task.FromResult<IEnumerable<TestStepResult>>(
      [
        TestStepResult.Failed(instructions[0].TestStep, $"Response content is not valid XML: {exception.Message}")
      ]);
    }
    var results = instructions.Select(instruction =>
    {
      var actualValue = document.XPathSelectElement(instruction.Selector)?.Value;
      return instruction.Value == actualValue
        ? TestStepResult.Success(instruction.TestStep)
        : TestStepResult.Failed(
          instruction.TestStep,
          $"Expected content value '{instruction.Value}' but was '{actualValue}'."
        );
    });

    return Task.FromResult(results);
  }

  public async Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  )
  {
    if (string.IsNullOrEmpty(content))
    {
      return [TestStepResult.Failed(instructions[0].TestStep, "Response content is empty.")];
    }

    string normalizedActual;
    try
    {
      normalizedActual = NormalizeXml(content);
    }
    catch (System.Xml.XmlException exception)
    {
      return [TestStepResult.Failed(instructions[0].TestStep, $"Response content is not valid XML: {exception.Message}")];
    }
    var results = new List<TestStepResult>();
    foreach (var instruction in instructions)
    {
      var expectedXml = !string.IsNullOrEmpty(instruction.File)
        ? await File.ReadAllTextAsync(TestCaseContentFileResolver.LocateExistingFile(instruction), cancellationToken)
        : instruction.Value ?? string.Empty;
      try
      {
        results.Add(
          normalizedActual == NormalizeXml(expectedXml)
            ? TestStepResult.Success(instruction.TestStep)
            : TestStepResult.Failed(instruction.TestStep, "Response content does not match the expected content.")
        );
      }
      catch (System.Xml.XmlException exception)
      {
        results.Add(TestStepResult.Failed(instruction.TestStep, $"Expected content is not valid XML: {exception.Message}"));
      }
    }

    return results;
  }

  private static string NormalizeXml(string xml) => XDocument.Parse(xml).ToString(SaveOptions.DisableFormatting);
}
