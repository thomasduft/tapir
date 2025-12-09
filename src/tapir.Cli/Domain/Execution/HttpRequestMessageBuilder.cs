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

    // Group instructions by content type and validate only one type is used
    var contentTypeGroups = instructions
      .GroupBy(i => i.ContentType)
      .ToList();

    if (contentTypeGroups.Count > 1)
    {
      var contentTypes = string.Join(", ", contentTypeGroups.Select(g => g.Key));
      throw new InvalidOperationException(
        $"Multiple content types found in request: {contentTypes}. Only one content type is allowed per request."
      );
    }

    if (contentTypeGroups.Count == 0)
    {
      return;
    }

    var contentType = contentTypeGroups[0].Key;
    var contentInstructions = contentTypeGroups[0].ToList();

    switch (contentType)
    {
      case Constants.ContentTypes.FormUrlEncoded:
        var formData = contentInstructions
          .Where(i => !string.IsNullOrEmpty(i.Name) && !string.IsNullOrEmpty(i.Value))
          .Select(i => new KeyValuePair<string, string>(i.Name!, i.Value!))
          .ToList();
        request.Content = new FormUrlEncodedContent(formData);
        break;

      case Constants.ContentTypes.Text:
        var textInstruction = contentInstructions.First();
        var textContent = !string.IsNullOrEmpty(textInstruction.File)
          ? await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), textInstruction.File), cancellationToken)
          : textInstruction.Value;
        request.Content = new StringContent(textContent!, Encoding.UTF8, "text/plain");
        break;

      case Constants.ContentTypes.Json:
        var jsonInstruction = contentInstructions.First();
        var jsonContent = !string.IsNullOrEmpty(jsonInstruction.File)
          ? await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), jsonInstruction.File), cancellationToken)
          : jsonInstruction.Value;
        request.Content = JsonContent.Create(JsonNode.Parse(jsonContent!)!);
        break;

      case Constants.ContentTypes.MultipartFormData:
        var multipartContent = new MultipartFormDataContent();
        foreach (var instruction in contentInstructions)
        {
          if (!string.IsNullOrEmpty(instruction.File))
          {
            var relativeFilePath = Path.Combine(Directory.GetCurrentDirectory(), instruction.File);
            var byteArrayContent = new ByteArrayContent(await File.ReadAllBytesAsync(relativeFilePath, cancellationToken));
            byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypeMapper.GetContentType(relativeFilePath));
            multipartContent.Add(byteArrayContent, instruction.Name ?? "file", instruction.Value ?? Path.GetFileName(relativeFilePath));
          }
          else if (!string.IsNullOrEmpty(instruction.Name) && !string.IsNullOrEmpty(instruction.Value))
          {
            var stringContent = new StringContent(instruction.Value, Encoding.UTF8);
            multipartContent.Add(stringContent, instruction.Name);
          }
        }
        request.Content = multipartContent;
        break;

      default:
        throw new InvalidOperationException(
          $"Unsupported content type '{contentType}' in AddContent action."
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