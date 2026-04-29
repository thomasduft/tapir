namespace tomware.Tapir.Cli.Domain;

internal interface IRequestContentFactory
{
  Task<HttpContent?> CreateAsync(
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  );
}

internal class RequestContentFactory : IRequestContentFactory
{
  private readonly IReadOnlyDictionary<string, IRequestContentHandler> _handlers;

  public RequestContentFactory(IEnumerable<IRequestContentHandler> handlers)
  {
    _handlers = handlers.ToDictionary(
      handler => handler.ContentType,
      StringComparer.OrdinalIgnoreCase
    );
  }

  public async Task<HttpContent?> CreateAsync(
    IEnumerable<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = instructions
      .Where(instruction => instruction.Action == Constants.Actions.AddContent)
      .ToList();

    if (contentInstructions.Count == 0)
    {
      return null;
    }

    var contentTypeGroups = contentInstructions
      .GroupBy(instruction => instruction.ContentType, StringComparer.OrdinalIgnoreCase)
      .ToList();

    if (contentTypeGroups.Count > 1)
    {
      var contentTypes = string.Join(", ", contentTypeGroups.Select(group => group.Key));
      throw new InvalidOperationException(
        $"Multiple content types found in request: {contentTypes}. Only one content type is allowed per request."
      );
    }

    var contentTypeGroup = contentTypeGroups[0];
    if (!_handlers.TryGetValue(contentTypeGroup.Key, out var handler))
    {
      throw new InvalidOperationException(
        $"Unsupported content type '{contentTypeGroup.Key}' in AddContent action."
      );
    }

    return await handler.CreateAsync(contentTypeGroup.ToList(), cancellationToken);
  }
}