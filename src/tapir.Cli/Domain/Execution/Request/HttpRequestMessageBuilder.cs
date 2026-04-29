using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal interface IHttpRequestMessageBuilder
{
  Task<HttpRequestMessage> BuildAsync(
    string domain,
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  );
}

internal class HttpRequestMessageBuilder
  : IHttpRequestMessageBuilder
{
  private readonly IRequestContentFactory _requestContentFactory;
  private readonly IEnumerable<TestStepInstruction> _instructions;
  private string _domain = string.Empty;

  public HttpRequestMessageBuilder(
    IRequestContentFactory requestContentFactory
  )
  {
    _requestContentFactory = requestContentFactory;
    _instructions = [];
  }

  private HttpRequestMessageBuilder(
    IEnumerable<TestStepInstruction> instructions
  ) : this(CreateDefaultRequestContentFactory())
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
    return await BuildAsync(_domain, _instructions, cancellationToken);
  }

  public async Task<HttpRequestMessage> BuildAsync(
    string domain,
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var request = new HttpRequestMessage();

    SetMethod(request, instructions);
    SetBaseAddress(request, instructions, domain);
    SetHeaders(request, instructions);
    request.Content = await _requestContentFactory.CreateAsync(instructions, cancellationToken);

    return request;
  }

  private static IRequestContentFactory CreateDefaultRequestContentFactory()
  {
    return new RequestContentFactory(
    [
      new TextRequestContentHandler(),
      new JsonRequestContentHandler(),
      new FormUrlEncodedRequestContentHandler(),
      new MultipartFormDataRequestContentHandler()
    ]);
  }

  private static void SetMethod(
    HttpRequestMessage request,
    IEnumerable<TestStepInstruction> instructions
  )
  {
    var instruction = instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.Send)
        ?? throw new InvalidOperationException("No Send instruction found to set HTTP Method.");

    Log.Logger.Verbose("  - setting HTTP method: {HttpMethod}", instruction.Method);

    request.Method = new HttpMethod(instruction.Method);
  }

  private static void SetBaseAddress(
    HttpRequestMessage request,
    IEnumerable<TestStepInstruction> instructions,
    string domain
  )
  {
    var endpointInstruction = instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.Send)
        ?? throw new InvalidOperationException("No Send instruction found to set Endpoint.");

    var requestDomain = !string.IsNullOrEmpty(endpointInstruction.Domain)
      ? endpointInstruction.Domain
      : domain;
    var endpoint = endpointInstruction.Endpoint;

    var queryParameterInstructions = instructions
      .Where(i => i.Action == Constants.Actions.AddQueryParameter)
      .ToList();

    var queryString = string.Join(
      "&",
      queryParameterInstructions
        .Select(i => $"{Uri.EscapeDataString(i.Name!)}={Uri.EscapeDataString(i.Value!)}")
    );

    var uriString = !string.IsNullOrEmpty(queryString)
      ? $"{requestDomain}/{endpoint}?{queryString}"
      : $"{requestDomain}/{endpoint}";

    Log.Logger.Verbose("  - setting request URI: {RequestUri}", uriString);

    request.RequestUri = new Uri(uriString);
  }

  private static void SetHeaders(
    HttpRequestMessage request,
    IEnumerable<TestStepInstruction> instructions
  )
  {
    request.Headers.Clear();
    var headerInstructions = instructions
      .Where(i => i.Action == Constants.Actions.AddHeader)
      .ToList();

    foreach (var headerInstruction in headerInstructions)
    {
      if (!string.IsNullOrEmpty(headerInstruction.Name)
        && !string.IsNullOrEmpty(headerInstruction.Value))
      {
        Log.Logger.Verbose("  - adding header: {HeaderName}: {HeaderValue}", headerInstruction.Name, headerInstruction.Value);

        request.Headers.Add(headerInstruction.Name, headerInstruction.Value);
      }
    }
  }
}
