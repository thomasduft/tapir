using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class FormUrlEncodedRequestContentHandler : IRequestContentHandler
{
  public string ContentType => Constants.ContentTypes.FormUrlEncoded;

  public async Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var formData = new List<KeyValuePair<string, string>>();

    foreach (var instruction in instructions)
    {
      if (string.IsNullOrEmpty(instruction.Name))
      {
        continue;
      }

      var value = !string.IsNullOrEmpty(instruction.File)
        ? await File.ReadAllTextAsync(
          TestCaseContentFileResolver.LocateExistingFile(instruction),
          cancellationToken
        )
        : instruction.Value;

      if (string.IsNullOrEmpty(value))
      {
        continue;
      }

      value = VariablesHelper.ResolveVariables(value, instruction.TestStep.TestCase.Variables);
      formData.Add(new KeyValuePair<string, string>(instruction.Name, value));
    }

    Log.Logger.Verbose(
      "  - setting form URL encoded content with {FormData} key-value pairs.",
      string.Join(", ", formData.Select(pair => $"{pair.Key}={pair.Value}"))
    );

    return new FormUrlEncodedContent(formData);
  }
}
