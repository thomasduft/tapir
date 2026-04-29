using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class TextResponseContentValidator : IResponseContentValidator
{
  public string ContentType => Constants.ContentTypes.Text;

  public Task<IEnumerable<TestStepResult>> CheckContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepResult>();

    if (string.IsNullOrEmpty(content))
    {
      results.Add(TestStepResult.Failed(instructions[0].TestStep, "Response content is empty."));

      return Task.FromResult(results.AsEnumerable());
    }

    Log.Logger.Verbose("  - received content is {Text}", content);

    foreach (var contentInstruction in instructions)
    {
      var expectedValue = contentInstruction.Value;

      results.Add(
        expectedValue == content
          ? TestStepResult.Success(contentInstruction.TestStep)
          : TestStepResult.Failed(
            contentInstruction.TestStep,
            $"Expected content value '{expectedValue}' but was '{content}'."
          )
      );
    }

    return Task.FromResult(results.AsEnumerable());
  }

  public async Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    string content,
    CancellationToken cancellationToken
  )
  {
    var results = new List<TestStepResult>();

    if (string.IsNullOrEmpty(content))
    {
      results.Add(TestStepResult.Failed(instructions[0].TestStep, "Response content is empty."));

      return results;
    }

    foreach (var contentInstruction in instructions)
    {
      var expectedText = !string.IsNullOrEmpty(contentInstruction.File)
        ? await File.ReadAllTextAsync(
            TestCaseContentFileResolver.LocateExistingFile(contentInstruction), cancellationToken)
        : contentInstruction.Value ?? string.Empty;

      results.Add(
        expectedText == content
          ? TestStepResult.Success(contentInstruction.TestStep)
          : TestStepResult.Failed(
            contentInstruction.TestStep,
            "Response content does not match the expected content."
          )
      );
    }

    return results;
  }
}