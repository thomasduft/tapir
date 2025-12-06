
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli.Domain;

internal class HttpRequestMessageBuilder
{
  private readonly IEnumerable<TestStepInstruction> _instructions;
  private string _domain = string.Empty;

  private HttpRequestMessageBuilder(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    _instructions = instructions;
  }

  public static HttpRequestMessageBuilder Create(IEnumerable<TestStepInstruction> instructions)
  {
    return new HttpRequestMessageBuilder(instructions);
  }

  public HttpRequestMessageBuilder WithDomain(string domain)
  {
    _domain = domain;

    return this;
  }

  public async Task<HttpRequestMessage> BuildAsync(CancellationToken cancellationToken)
  {
    var request = new HttpRequestMessage();

    SetHeaders(request);
    await SetContent(request, cancellationToken);
    SetMethod(request);
    SetBaseAddress(request);

    return request;
  }

  private void SetHeaders(HttpRequestMessage request)
  {
    request.Headers.Clear();
    var headerInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.AddHeader)
      .ToList();

    foreach (var headerInstruction in headerInstructions)
    {
      if (!string.IsNullOrEmpty(headerInstruction.Name)
        && !string.IsNullOrEmpty(headerInstruction.Value))
      {
        request.Headers.Add(headerInstruction.Name, headerInstruction.Value);
      }
    }
  }

  private async Task SetContent(
    HttpRequestMessage request,
    CancellationToken cancellationToken
  )
  {
    var instructions = _instructions
      .Where(i => i.Action == Constants.Actions.AddContent);
    if (instructions == null)
    {
      return;
    }

    foreach (var instruction in instructions)
    {
      var contentType = instruction.ContentType;

      if (contentType == Constants.ContentTypes.Text)
      {
        // Read the file relative to the execution directory
        var stringContent = !string.IsNullOrEmpty(instruction.File)
          ? await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), instruction.File), cancellationToken)
          : instruction.Value;
        request.Content = new StringContent(stringContent!, Encoding.UTF8, "text/plain");
        return;
      }

      if (contentType == Constants.ContentTypes.Json)
      {
        // Read the file relative to the execution directory
        var stringContent = !string.IsNullOrEmpty(instruction.File)
          ? await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), instruction.File), cancellationToken)
          : instruction.Value;
        request.Content = JsonContent.Create(JsonNode.Parse(stringContent!)!);
        return;
      }

      if (contentType == Constants.ContentTypes.MultipartFormData)
      {
        request.Content ??= new MultipartFormDataContent();
        var multipartContent = request.Content as MultipartFormDataContent;

        if (!string.IsNullOrEmpty(instruction.File))
        {
          var relativeFilePath = Path.Combine(Directory.GetCurrentDirectory(), instruction.File);
          var byteArrayContent = new ByteArrayContent(await File.ReadAllBytesAsync(relativeFilePath, cancellationToken));
          byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypeMapper.GetContentType(relativeFilePath));
          multipartContent?.Add(byteArrayContent, instruction.Name ?? "file", instruction.Value ?? Path.GetFileName(relativeFilePath));

          continue;
        }

        if (!string.IsNullOrEmpty(instruction.Name) && !string.IsNullOrEmpty(instruction.Value))
        {
          var stringContent = new StringContent(instruction.Value, Encoding.UTF8);
          multipartContent?.Add(stringContent, instruction.Name);
        }

        continue;
      }

      throw new InvalidOperationException(
        $"Unsupported content type '{instruction.ContentType}' in AddContent action."
      );
    }
  }

  private void SetMethod(HttpRequestMessage request)
  {
    var instruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.Send)
        ?? throw new InvalidOperationException("No Send instruction found to set HTTP Method.");

    request.Method = new HttpMethod(instruction.Method);
  }

  private void SetBaseAddress(HttpRequestMessage request)
  {
    var endpointInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.Send)
        ?? throw new InvalidOperationException("No Send instruction found to set Endpoint.");
    var endpoint = endpointInstruction.Endpoint;

    var queryParameterInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.AddQueryParameter)
      .ToList();

    var queryString = string.Join(
      "&",
      queryParameterInstructions
        .Select(i => $"{Uri.EscapeDataString(i.Name!)}={Uri.EscapeDataString(i.Value!)}")
    );

    var domain = !string.IsNullOrEmpty(queryString)
      ? $"{_domain}/{endpoint}?{queryString}"
      : $"{_domain}/{endpoint}";

    request.RequestUri = new Uri(domain);
  }
}
