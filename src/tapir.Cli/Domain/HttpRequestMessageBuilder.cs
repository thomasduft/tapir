
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
      .Where(i => i.Action == nameof(Constants.Actions.AddHeader))
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
    var instruction = _instructions
      .FirstOrDefault(i => i.Action == nameof(Constants.Actions.AddContent));
    if (instruction == null)
    {
      return;
    }

    var stringContent = !string.IsNullOrEmpty(instruction.File)
      ? await File.ReadAllTextAsync(instruction.File, cancellationToken)
      : instruction.Value;

    request.Content = new StringContent(stringContent);
  }

  private void SetMethod(HttpRequestMessage request)
  {
    var instruction = _instructions
      .FirstOrDefault(i => i.Action == nameof(Constants.Actions.Send))
        ?? throw new InvalidOperationException("No Send instruction found to set HTTP Method.");

    request.Method = new HttpMethod(instruction.Method);
  }

  private void SetBaseAddress(HttpRequestMessage request)
  {
    var endpointInstruction = _instructions
      .FirstOrDefault(i => i.Action == nameof(Constants.Actions.Send))
        ?? throw new InvalidOperationException("No Send instruction found to set Endpoint.");
    var endpoint = endpointInstruction.Value;

    var queryParameterInstructions = _instructions
      .Where(i => i.Action == nameof(Constants.Actions.AddQueryParameter))
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
