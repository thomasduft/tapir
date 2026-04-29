namespace tomware.Tapir.Cli.Domain;

internal interface IRequestContentHandler
{
  string ContentType { get; }

  Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  );
}