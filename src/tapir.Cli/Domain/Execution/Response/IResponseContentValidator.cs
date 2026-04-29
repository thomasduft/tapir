namespace tomware.Tapir.Cli.Domain;

internal interface IResponseContentValidator
{
  /// <summary>
  /// Gets the content type that this validator supports (e.g. "application/json", "text/plain", etc.).
  /// Use the constants defined in <see cref="Constants.ContentTypes"/> for common content types.
  /// </summary>
  string ContentType { get; }

  /// <summary>
  /// Checks the response content against the provided instructions and returns the results of the checks.
  /// The intention of this method is to check the response content against the provided instruction Value (i.e. JSON path).
  /// </summary>
  /// <param name="instructions"></param>
  /// <param name="content"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IEnumerable<TestStepResult>> CheckContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  );

  /// <summary>
  /// Verifies the response content against the provided instructions and returns the results of the verifications.
  /// The intention of this method is to verify the response content against the provided instruction File content (i.e. expected file content).
  /// </summary>
  /// <param name="instructions"></param>
  /// <param name="content"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  );
}