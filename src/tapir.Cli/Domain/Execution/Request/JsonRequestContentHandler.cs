using System.Net.Http.Json;
using System.Text.Json.Nodes;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class JsonRequestContentHandler : IRequestContentHandler
{
  public string ContentType => Constants.ContentTypes.Json;

  public async Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var instruction = instructions.First();
    var jsonContent = !string.IsNullOrEmpty(instruction.File)
      ? await File.ReadAllTextAsync(TestCaseContentFileResolver.LocateExistingFile(instruction), cancellationToken)
      : instruction.Value;

    Log.Logger.Verbose("  - setting JSON content: {@JsonContent}", jsonContent);

    return JsonContent.Create(JsonNode.Parse(jsonContent!)!);
  }
}