using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class FormUrlEncodedRequestContentHandler : IRequestContentHandler
{
  public string ContentType => Constants.ContentTypes.FormUrlEncoded;

  public Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var formData = instructions
      .Where(instruction => !string.IsNullOrEmpty(instruction.Name) && !string.IsNullOrEmpty(instruction.Value))
      .Select(instruction => new KeyValuePair<string, string>(instruction.Name!, instruction.Value!))
      .ToList();

    Log.Logger.Verbose(
      "  - setting form URL encoded content with {FormData} key-value pairs.",
      string.Join(", ", formData.Select(pair => $"{pair.Key}={pair.Value}"))
    );

    return Task.FromResult<HttpContent>(new FormUrlEncodedContent(formData));
  }
}