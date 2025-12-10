using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

using Json.Path;

using Serilog;

namespace tomware.Tapir.Cli.Domain;

internal class HttpResponseMessageValidator
{
  private readonly IEnumerable<TestStepInstruction> _instructions;
  private HttpStatusCode _statusCode;
  private string? _reasonPhrase;
  private HttpContent? _content;
  private HttpContentHeaders? _contentHeaders;

  private HttpResponseMessageValidator(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    _instructions = instructions;
  }

  public static HttpResponseMessageValidator Create(
    IEnumerable<TestStepInstruction> instructions
  )
  {
    return new HttpResponseMessageValidator(instructions);
  }

  public HttpResponseMessageValidator WithStatusCode(HttpStatusCode statusCode)
  {
    _statusCode = statusCode;

    return this;
  }

  public HttpResponseMessageValidator WithReasonPhrase(string? reasonPhrase)
  {
    _reasonPhrase = reasonPhrase;

    return this;
  }

  public HttpResponseMessageValidator WithContent(HttpContent content)
  {
    _content = content;

    return this;
  }

  public HttpResponseMessageValidator WithContentHeaders(HttpContentHeaders contentHeaders)
  {
    _contentHeaders = contentHeaders;

    return this;
  }

  public async Task<IEnumerable<TestStepResult>> ValidateAsync(CancellationToken cancellationToken)
  {
    var results = new List<TestStepResult>();

    results.AddRange(CheckStatusCode());
    results.AddRange(CheckReasonPhrase());
    results.AddRange(CheckContentHeaders());
    results.AddRange(await VerifyContentAsync(cancellationToken));
    results.AddRange(await CheckContentAsync(cancellationToken));

    return results;
  }

  private List<TestStepResult> CheckStatusCode()
  {
    var results = new List<TestStepResult>();

    var sendInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.Send)
        ?? throw new InvalidOperationException("No Send instruction found to validate status code.");
    results.Add(TestStepResult.Success(sendInstruction.TestStep));

    var statusCodeInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckStatusCode);
    if (statusCodeInstruction == null)
    {
      // It's okay if there's no status code to check
      return results;
    }

    Log.Logger.Information("- received status code is: {StatusCode}", (int)_statusCode);

    // Now validate the status code
    var expectedStatusCode = (HttpStatusCode)int.Parse(statusCodeInstruction.Value);
    results.Add(expectedStatusCode == _statusCode
      ? TestStepResult.Success(statusCodeInstruction.TestStep)
      : TestStepResult.Failed(
        statusCodeInstruction.TestStep,
        $"Expected status code '{(int)expectedStatusCode}' but was '{(int)_statusCode}'."
      ));

    return results;
  }

  private List<TestStepResult> CheckReasonPhrase()
  {
    var reasonPhraseInstruction = _instructions
      .FirstOrDefault(i => i.Action == Constants.Actions.CheckReasonPhrase);
    if (reasonPhraseInstruction == null)
    {
      // It's okay if there's no reason phrase to check
      return [];
    }

    Log.Logger.Verbose($"- received reason phrase is: {_reasonPhrase}");

    var expectedReasonPhrase = reasonPhraseInstruction.Value;
    return expectedReasonPhrase == _reasonPhrase
      ? [TestStepResult.Success(reasonPhraseInstruction.TestStep)]
      : [TestStepResult.Failed(
        reasonPhraseInstruction.TestStep,
        $"Expected reason phrase '{expectedReasonPhrase}' but was '{_reasonPhrase}'."
      )];
  }

  private List<TestStepResult> CheckContentHeaders()
  {
    var headerInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.CheckContentHeader);
    if (headerInstructions == null || _contentHeaders == null)
    {
      // It's okay if there's no headers to check
      return [];
    }

    foreach (var contentHeader in _contentHeaders)
    {
      Log.Logger.Verbose("- received content header is: {ContentHeaderKey}={ContentHeaderValue}",
      contentHeader.Key,
      string.Join(",", contentHeader.Value));
    }

    var results = new List<TestStepResult>();
    foreach (var headerInstruction in headerInstructions)
    {
      if (_contentHeaders.TryGetValues(headerInstruction.Name, out var values))
      {
        var actualValue = string.Join(",", values);
        var expectedValue = headerInstruction.Value;

        results.Add(
          expectedValue == actualValue
            ? TestStepResult.Success(headerInstruction.TestStep)
            : TestStepResult.Failed(
              headerInstruction.TestStep,
              $"Expected header '{headerInstruction.Name}' value '{expectedValue}' but was '{actualValue}'."
            )
        );
      }
      else
      {
        results.Add(
          TestStepResult.Failed(
            headerInstruction.TestStep,
            $"Expected header '{headerInstruction.Name}' was not found."
          )
        );
      }
    }

    return results;
  }

  private async Task<List<TestStepResult>> CheckContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.CheckContent)
      .ToList();
    if (contentInstructions.Count == 0 || _content == null)
    {
      // It's okay if there's no content to check
      return [];
    }

    var results = new List<TestStepResult>();

    // Group by ContentType since we can have only one content type per response
    // and so we can check all instructions of the same content type in one go
    var groupedByContentType = contentInstructions
      .GroupBy(i => i.ContentType)
      .ToList();

    if (groupedByContentType.First().Key == Constants.ContentTypes.Text)
    {
      var text = await _content!.ReadAsStringAsync(cancellationToken);
      if (string.IsNullOrEmpty(text))
      {
        results.Add(
          TestStepResult.Failed(
            contentInstructions.First().TestStep,
            "Response content is empty."
          ));

        return results;
      }

      Log.Logger.Verbose("- received content is {Text}", text);

      foreach (var group in groupedByContentType)
      {
        CheckForTextContentAsync(group.ToList(), text, results);
      }
    }
    else if (groupedByContentType.First().Key == Constants.ContentTypes.Json)
    {
      var json = await _content!.ReadAsStringAsync(cancellationToken);
      if (string.IsNullOrEmpty(json))
      {
        results.Add(
          TestStepResult.Failed(
            contentInstructions.First().TestStep,
            "Response content is empty."
          ));

        return results;
      }

      Log.Logger.Verbose("- received content is: {@Json}", json);

      foreach (var group in groupedByContentType)
      {
        CheckForJsonContentAsync(group.ToList(), json, results);
      }
    }

    return results;
  }

  private void CheckForTextContentAsync(
    List<TestStepInstruction> contentInstructions,
    string text,
    List<TestStepResult> results
  )
  {
    foreach (var contentInstruction in contentInstructions)
    {
      var expectedValue = contentInstruction.Value;

      results.Add(
        expectedValue == text
          ? TestStepResult.Success(contentInstruction.TestStep)
          : TestStepResult.Failed(
            contentInstruction.TestStep,
            $"Expected content value '{expectedValue}' but was '{text}'."
          )
      );
    }
  }

  private void CheckForJsonContentAsync(
    List<TestStepInstruction> contentInstructions,
    string json,
    List<TestStepResult> results
  )
  {
    // see https://docs.json-everything.net/path/basics/
    var jsonNode = JsonNode.Parse(json);

    foreach (var contentInstruction in contentInstructions)
    {
      var jsonPath = JsonPath.Parse(contentInstruction.JsonPath);
      var evaluationResults = jsonPath.Evaluate(jsonNode);

      var actualValue = evaluationResults.Matches.FirstOrDefault()?.Value?.ToString();
      var expectedValue = contentInstruction.Value;

      results.Add(
        expectedValue == actualValue
          ? TestStepResult.Success(contentInstruction.TestStep)
          : TestStepResult.Failed(
            contentInstruction.TestStep,
            $"Expected content value '{expectedValue}' but was '{actualValue}'."
          )
      );
    }
  }

  private async Task<IEnumerable<TestStepResult>> VerifyContentAsync(
    CancellationToken cancellationToken
  )
  {
    var contentInstructions = _instructions
      .Where(i => i.Action == Constants.Actions.VerifyContent)
      .ToList();
    if (!contentInstructions.Any() || _content == null)
    {
      // It's okay if there's no content to check
      return [];
    }

    var results = new List<TestStepResult>();
    foreach (var contentInstruction in contentInstructions)
    {
      if (contentInstruction.ContentType == Constants.ContentTypes.Text)
      {
        await VerifyForTextContentAsync(
          contentInstructions,
          results,
          contentInstruction,
          cancellationToken
        );
      }
      if (contentInstruction.ContentType == Constants.ContentTypes.Json)
      {
        await VerifyForJsonContentAsync(
          contentInstructions,
          results,
          contentInstruction,
          cancellationToken
        );
      }
    }

    return results;
  }

  private async Task VerifyForTextContentAsync(
    List<TestStepInstruction> contentInstructions,
    List<TestStepResult> results,
    TestStepInstruction contentInstruction,
    CancellationToken cancellationToken
  )
  {
    var text = await _content!.ReadAsStringAsync(cancellationToken);
    if (string.IsNullOrEmpty(text))
    {
      results.Add(
        TestStepResult.Failed(
          contentInstructions.First().TestStep,
          "Response content is empty."
        ));

      return;
    }

    // Read the file relative to the execution directory
    var relativeFilePath = Path.Combine(Directory.GetCurrentDirectory(), contentInstruction.File);
    var expectedText = !string.IsNullOrEmpty(contentInstruction.File)
      && File.Exists(relativeFilePath)
        ? await File.ReadAllTextAsync(relativeFilePath, cancellationToken)
        : contentInstruction.Value
          ?? string.Empty;

    results.Add(
      expectedText == text
        ? TestStepResult.Success(contentInstruction.TestStep)
        : TestStepResult.Failed(
          contentInstruction.TestStep,
          "Response content does not match the expected content."
        )
    );
  }

  private async Task VerifyForJsonContentAsync(
    List<TestStepInstruction> contentInstructions,
    List<TestStepResult> results,
    TestStepInstruction contentInstruction,
    CancellationToken cancellationToken
  )
  {
    var json = await _content!.ReadAsStringAsync(cancellationToken);
    if (string.IsNullOrEmpty(json))
    {
      results.Add(
        TestStepResult.Failed(
          contentInstructions.First().TestStep,
          "Response content is empty."
        ));

      return;
    }

    // if File is present load from File otherwise use Value

    // Read the file relative to the execution directory
    var relativeFilePath = Path.Combine(Directory.GetCurrentDirectory(), contentInstruction.File);
    var expectedJson = !string.IsNullOrEmpty(contentInstruction.File)
      && File.Exists(relativeFilePath)
      ? await File.ReadAllTextAsync(relativeFilePath, cancellationToken)
      : contentInstruction.Value
        ?? string.Empty;

    // Normalize both JSON strings by parsing and re-serializing without formatting
    var normalizedActual = NormalizeJson(json);
    var normalizedExpected = NormalizeJson(expectedJson);

    results.Add(
      normalizedActual == normalizedExpected
        ? TestStepResult.Success(contentInstruction.TestStep)
        : TestStepResult.Failed(
          contentInstruction.TestStep,
          "Response content does not match the expected content."
        )
    );
  }

  private static string NormalizeJson(string json, bool writeIndented = false)
  {
    try
    {
      var jsonNode = JsonNode.Parse(json);
      return jsonNode?.ToJsonString(new JsonSerializerOptions { WriteIndented = writeIndented }) ?? json;
    }
    catch
    {
      // If parsing fails, return original string
      return json;
    }
  }
}
