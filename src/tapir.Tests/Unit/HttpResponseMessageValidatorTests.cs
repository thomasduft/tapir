using System.Net;
using System.Text;

using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class HttpResponseMessageValidatorTests
{
  #region CheckStatusCode Tests

  [Fact]
  public async Task ValidateAsync_WithMatchingStatusCode_ShouldReturnSuccessResults()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var statusCodeInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckStatusCode, value: "200");
    var instructions = new[] { sendInstruction, statusCodeInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(2, results.Count);
    Assert.True(results[0].IsSuccess); // Send instruction
    Assert.True(results[1].IsSuccess); // Status code check
    Assert.Equal(1, results[0].TestStepId);
    Assert.Equal(2, results[1].TestStepId);
  }

  [Fact]
  public async Task ValidateAsync_WithNonMatchingStatusCode_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var statusCodeInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckStatusCode, value: "200");
    var instructions = new[] { sendInstruction, statusCodeInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.NotFound);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(2, results.Count);
    Assert.True(results[0].IsSuccess); // Send instruction
    Assert.False(results[1].IsSuccess); // Status code check
    Assert.Equal("Expected status code '200' but was '404'.", results[1].Error);
  }

  [Fact]
  public async Task ValidateAsync_WithoutStatusCodeInstruction_ShouldOnlyReturnSendSuccess()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var instructions = new[] { sendInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Single(results);
    Assert.True(results[0].IsSuccess);
  }

  [Fact]
  public async Task ValidateAsync_WithoutSendInstruction_ShouldThrowException()
  {
    // Arrange
    var statusCodeInstruction = CreateTestStepInstruction(1, Constants.Actions.CheckStatusCode, value: "200");
    var instructions = new[] { statusCodeInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await validator.ValidateAsync(CancellationToken.None)
    );
  }

  #endregion

  #region CheckReasonPhrase Tests

  [Fact]
  public async Task ValidateAsync_WithMatchingReasonPhrase_ShouldReturnSuccessResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var reasonPhraseInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckReasonPhrase, value: "OK");
    var instructions = new[] { sendInstruction, reasonPhraseInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithReasonPhrase("OK");

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var reasonPhraseResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(reasonPhraseResult);
    Assert.True(reasonPhraseResult.IsSuccess);
  }

  [Fact]
  public async Task ValidateAsync_WithNonMatchingReasonPhrase_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var reasonPhraseInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckReasonPhrase, value: "OK");
    var instructions = new[] { sendInstruction, reasonPhraseInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithReasonPhrase("Not OK");

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var reasonPhraseResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(reasonPhraseResult);
    Assert.False(reasonPhraseResult.IsSuccess);
    Assert.Equal("Expected reason phrase 'OK' but was 'Not OK'.", reasonPhraseResult.Error);
  }

  [Fact]
  public async Task ValidateAsync_WithoutReasonPhraseInstruction_ShouldNotCheckReasonPhrase()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var instructions = new[] { sendInstruction };

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithReasonPhrase("OK");

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Single(results);
    Assert.DoesNotContain(results, r => r.TestStepId == 2);
  }

  #endregion

  #region CheckHeaders Tests

  [Fact]
  public async Task ValidateAsync_WithMatchingContentHeader_ShouldReturnSuccessResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var headerInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContentHeader, name: "X-Custom-Header", value: "test-value");
    var instructions = new[] { sendInstruction, headerInstruction };

    var response = new HttpResponseMessage(HttpStatusCode.OK);
    response.Content.Headers.Add("X-Custom-Header", "test-value");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContentHeaders(response.Content.Headers);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var headerResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(headerResult);
    Assert.True(headerResult.IsSuccess);
  }

  [Fact]
  public async Task ValidateAsync_WithNonMatchingContentHeaderValue_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var headerInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContentHeader, name: "X-Custom-Header", value: "expected-value");
    var instructions = new[] { sendInstruction, headerInstruction };

    var response = new HttpResponseMessage(HttpStatusCode.OK);
    response.Content.Headers.Add("X-Custom-Header", "actual-value");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContentHeaders(response.Content.Headers);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var headerResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(headerResult);
    Assert.False(headerResult.IsSuccess);
    Assert.Equal("Expected header 'X-Custom-Header' value 'expected-value' but was 'actual-value'.", headerResult.Error);
  }

  [Fact]
  public async Task ValidateAsync_WithMissingContentHeader_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var headerInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContentHeader, name: "X-Custom-Header", value: "CustomValue");
    var instructions = new[] { sendInstruction, headerInstruction };

    var response = new HttpResponseMessage(HttpStatusCode.OK);

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContentHeaders(response.Content.Headers);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var headerResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(headerResult);
    Assert.False(headerResult.IsSuccess);
    Assert.Equal("Expected header 'X-Custom-Header' was not found.", headerResult.Error);
  }

  [Fact]
  public async Task ValidateAsync_WithMultipleContentHeaders_ShouldCheckAllContentHeaders()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var header1Instruction = CreateTestStepInstruction(2, Constants.Actions.CheckContentHeader, name: "X-Header-One", value: "value1");
    var header2Instruction = CreateTestStepInstruction(3, Constants.Actions.CheckContentHeader, name: "X-Header-Two", value: "value2");
    var instructions = new[] { sendInstruction, header1Instruction, header2Instruction };

    var response = new HttpResponseMessage(HttpStatusCode.OK);
    response.Content.Headers.Add("X-Header-One", "value1");
    response.Content.Headers.Add("X-Header-Two", "value2");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContentHeaders(response.Content.Headers);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(3, results.Count);
    Assert.All(results, r => Assert.True(r.IsSuccess));
  }

  #endregion

  #region LogResponseContent Tests

  [Fact]
  public async Task ValidateAsync_WithLogResponseContentInstruction_ShouldLogContentAndReturnSuccess()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var logContentInstruction = CreateTestStepInstruction(2, Constants.Actions.LogResponseContent);
    var instructions = new[] { sendInstruction, logContentInstruction };
    var jsonContent = new StringContent("{\"message\":\"Hello, World!\"}", Encoding.UTF8, "application/json");
    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var logResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(logResult);
    Assert.True(logResult.IsSuccess);
  }

  #endregion

  #region CheckContent Tests

  [Fact]
  public async Task ValidateAsync_WithMatchingJsonPathContent_ShouldReturnSuccessResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var contentInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContent, value: "Alice", jsonPath: "$.name");
    var instructions = new[] { sendInstruction, contentInstruction };

    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var contentResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(contentResult);
    Assert.True(contentResult.IsSuccess);
  }

  [Fact]
  public async Task ValidateAsync_WithNonMatchingJsonPathContent_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var contentInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContent, value: "Bob", jsonPath: "$.name");
    var instructions = new[] { sendInstruction, contentInstruction };

    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var contentResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(contentResult);
    Assert.False(contentResult.IsSuccess);
    Assert.Contains("Expected content value 'Bob' but was 'Alice'", contentResult.Error);
  }

  [Fact]
  public async Task ValidateAsync_WithEmptyContent_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var contentInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContent, value: "Alice", jsonPath: "$.name");
    var instructions = new[] { sendInstruction, contentInstruction };

    var emptyContent = new StringContent("", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(emptyContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(2, results.Count);
    var contentResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(contentResult);
    Assert.False(contentResult.IsSuccess);
    Assert.Equal("Response content is empty.", contentResult.Error);
  }

  [Fact]
  public async Task ValidateAsync_WithComplexJsonPath_ShouldExtractCorrectValue()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var contentInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckContent, value: "Developer", jsonPath: "$.users[0].role");
    var instructions = new[] { sendInstruction, contentInstruction };

    var jsonContent = new StringContent(
      "{\"users\":[{\"name\":\"Alice\",\"role\":\"Developer\"},{\"name\":\"Bob\",\"role\":\"Manager\"}]}",
      Encoding.UTF8,
      "application/json"
    );

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var contentResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(contentResult);
    Assert.True(contentResult.IsSuccess);
  }

  #endregion

  #region VerifyContent Tests

  [Fact]
  public async Task ValidateAsync_WithMatchingJsonContent_ShouldReturnSuccessResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var verifyInstruction = CreateTestStepInstruction(2, Constants.Actions.VerifyContent, value: "{\"name\":\"Alice\",\"age\":30}");
    var instructions = new[] { sendInstruction, verifyInstruction };

    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var verifyResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(verifyResult);
    Assert.True(verifyResult.IsSuccess);
  }

  [Fact]
  public async Task ValidateAsync_WithMatchingJsonContentDifferentFormatting_ShouldReturnSuccessResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var verifyInstruction = CreateTestStepInstruction(2, Constants.Actions.VerifyContent, value: "{\n  \"name\": \"Alice\",\n  \"age\": 30\n}");
    var instructions = new[] { sendInstruction, verifyInstruction };

    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var verifyResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(verifyResult);
    Assert.True(verifyResult.IsSuccess);
  }

  [Fact]
  public async Task ValidateAsync_WithNonMatchingJsonContent_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var verifyInstruction = CreateTestStepInstruction(2, Constants.Actions.VerifyContent, value: "{\"name\":\"Bob\",\"age\":25}");
    var instructions = new[] { sendInstruction, verifyInstruction };

    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    var verifyResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(verifyResult);
    Assert.False(verifyResult.IsSuccess);
    Assert.Equal("Response content does not match the expected content.", verifyResult.Error);
  }

  [Fact]
  public async Task ValidateAsync_WithEmptyContentForVerify_ShouldReturnFailedResult()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var verifyInstruction = CreateTestStepInstruction(2, Constants.Actions.VerifyContent, value: "{\"name\":\"Alice\"}");
    var instructions = new[] { sendInstruction, verifyInstruction };

    var emptyContent = new StringContent("", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContent(emptyContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(2, results.Count);
    var verifyResult = results.FirstOrDefault(r => r.TestStepId == 2);
    Assert.NotNull(verifyResult);
    Assert.False(verifyResult.IsSuccess);
    Assert.Equal("Response content is empty.", verifyResult.Error);
  }

  #endregion

  #region Integration Tests

  [Fact]
  public async Task ValidateAsync_WithMultipleValidations_ShouldReturnAllResults()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var statusCodeInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckStatusCode, value: "200");
    var reasonPhraseInstruction = CreateTestStepInstruction(3, Constants.Actions.CheckReasonPhrase, value: "OK");
    var headerInstruction = CreateTestStepInstruction(4, Constants.Actions.CheckContentHeader, name: "X-API-Version", value: "v1");
    var contentInstruction = CreateTestStepInstruction(5, Constants.Actions.CheckContent, value: "Alice", jsonPath: "$.name");
    var instructions = new[] { sendInstruction, statusCodeInstruction, reasonPhraseInstruction, headerInstruction, contentInstruction };

    var response = new HttpResponseMessage(HttpStatusCode.OK);
    response.Content.Headers.Add("X-API-Version", "v1");
    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithReasonPhrase("OK")
      .WithContentHeaders(response.Content.Headers)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(5, results.Count);
    Assert.All(results, r => Assert.True(r.IsSuccess));
  }

  [Fact]
  public async Task ValidateAsync_WithMixedSuccessAndFailure_ShouldReturnMixedResults()
  {
    // Arrange
    var sendInstruction = CreateTestStepInstruction(1, Constants.Actions.Send);
    var statusCodeInstruction = CreateTestStepInstruction(2, Constants.Actions.CheckStatusCode, value: "200");
    var headerInstruction = CreateTestStepInstruction(3, Constants.Actions.CheckContentHeader, name: "X-Missing", value: "value");
    var contentInstruction = CreateTestStepInstruction(4, Constants.Actions.CheckContent, value: "Bob", jsonPath: "$.name");
    var instructions = new[] { sendInstruction, statusCodeInstruction, headerInstruction, contentInstruction };

    var response = new HttpResponseMessage(HttpStatusCode.OK);
    var jsonContent = new StringContent("{\"name\":\"Alice\",\"age\":30}", Encoding.UTF8, "application/json");

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK)
      .WithContentHeaders(response.Content.Headers)
      .WithContent(jsonContent);

    // Act
    var results = (await validator.ValidateAsync(CancellationToken.None)).ToList();

    // Assert
    Assert.Equal(4, results.Count);
    Assert.True(results[0].IsSuccess); // Send
    Assert.True(results[1].IsSuccess); // Status code
    Assert.False(results[2].IsSuccess); // Header missing
    Assert.False(results[3].IsSuccess); // Content mismatch
  }

  [Fact]
  public async Task ValidateAsync_WithNoInstructions_ShouldThrowException()
  {
    // Arrange
    var instructions = Array.Empty<TestStepInstruction>();

    var validator = HttpResponseMessageValidator.Create(instructions)
      .WithStatusCode(HttpStatusCode.OK);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await validator.ValidateAsync(CancellationToken.None)
    );
  }

  #endregion

  #region Helper Methods

  private static TestStepInstruction CreateTestStepInstruction(
    int stepId,
    string action,
    string name = "",
    string value = "",
    string jsonPath = "",
    string file = ""
  )
  {
    var testStep = new TestStep
    {
      Id = stepId,
      Description = $"Test step {stepId}"
    };

    var instruction = new TestStepInstruction(testStep)
    {
      Action = action,
      Name = name,
      Value = value,
      JsonPath = jsonPath,
      File = file
    };

    return instruction;
  }

  #endregion
}
