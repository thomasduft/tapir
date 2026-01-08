using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class TestStepInstructionTests
{
  #region Constructor Tests

  [Fact]
  public void Constructor_WithTestStep_InitializesPropertiesWithDefaultValues()
  {
    // Arrange
    var step = new TestStep { Id = 1 };

    // Act
    var instruction = new TestStepInstruction(step);

    // Assert
    Assert.Equal(string.Empty, instruction.Action);
    Assert.Equal(string.Empty, instruction.Name);
    Assert.Equal(string.Empty, instruction.Value);
    Assert.Equal(string.Empty, instruction.File);
    Assert.Equal(string.Empty, instruction.JsonPath);
    Assert.Equal("GET", instruction.Method);
    Assert.Equal(string.Empty, instruction.Endpoint);
    Assert.Equal(string.Empty, instruction.Domain);
    Assert.Equal(Constants.ContentTypes.Json, instruction.ContentType);
    Assert.Equal(step, instruction.TestStep);
  }

  #endregion

  #region FromTestStep - Basic Parsing Tests

  [Fact]
  public void FromTestStep_WithActionParameter_ParsesActionCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Send", instruction.Action);
  }

  [Fact]
  public void FromTestStep_WithNameParameter_ParsesNameCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddHeader Name=Authorization"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("AddHeader", instruction.Action);
    Assert.Equal("Authorization", instruction.Name);
  }

  [Fact]
  public void FromTestStep_WithValueParameter_ParsesValueCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddHeader Name=Authorization Value=Bearer123"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Bearer123", instruction.Value);
  }

  [Fact]
  public void FromTestStep_WithQuotedValue_RemovesQuotes()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddHeader Name=Authorization Value=\"Bearer Token\""
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Bearer Token", instruction.Value);
  }

  [Fact]
  public void FromTestStep_WithFileParameter_ParsesFileCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddContent File=data.json"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("AddContent", instruction.Action);
    Assert.Equal("data.json", instruction.File);
  }

  [Fact]
  public void FromTestStep_WithJsonPathParameter_ParsesJsonPathCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=StoreVariable JsonPath=$.id"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("StoreVariable", instruction.Action);
    Assert.Equal("$.id", instruction.JsonPath);
  }

  [Fact]
  public void FromTestStep_WithMethodParameter_ParsesMethodCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Method=POST"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("POST", instruction.Method);
  }

  [Fact]
  public void FromTestStep_WithLowercaseMethod_ConvertsToUppercase()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Method=post"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("POST", instruction.Method);
  }

  [Fact]
  public void FromTestStep_WithEndpointParameter_ParsesEndpointCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=api/users"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("api/users", instruction.Endpoint);
  }

  [Fact]
  public void FromTestStep_WithDomainParameter_ParsesDomainCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Domain=https://example.com"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("https://example.com", instruction.Domain);
  }

  [Fact]
  public void FromTestStep_WithContentTypeParameter_ParsesContentTypeCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddContent ContentType=text/plain"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("text/plain", instruction.ContentType);
  }

  [Fact]
  public void FromTestStep_WithMultipleParameters_ParsesAllCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Method=POST Endpoint=api/users ContentType=application/json"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Send", instruction.Action);
    Assert.Equal("POST", instruction.Method);
    Assert.Equal("api/users", instruction.Endpoint);
    Assert.Equal("application/json", instruction.ContentType);
  }

  #endregion

  #region FromTestStep - Variable Replacement Tests

  [Fact]
  public void FromTestStep_WithVariableInValue_ReplacesVariable()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddHeader Name=Authorization Value=\"Bearer @@Token@@\""
    };
    var variables = new Dictionary<string, string>
    {
      { "Token", "abc123xyz" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Bearer abc123xyz", instruction.Value);
  }

  [Fact]
  public void FromTestStep_WithVariableInEndpoint_ReplacesVariable()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=users/@@UserId@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "UserId", "12345" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("users/12345", instruction.Endpoint);
  }

  [Fact]
  public void FromTestStep_WithVariableInJsonPath_ReplacesVariable()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=StoreVariable JsonPath=$.@@PropertyName@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "PropertyName", "userId" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("$.userId", instruction.JsonPath);
  }

  [Fact]
  public void FromTestStep_WithVariableInDomain_ReplacesVariable()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Domain=@@DomainName@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "DomainName", "https://example.com" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("https://example.com", instruction.Domain);
  }

  [Fact]
  public void FromTestStep_WithMultipleVariablesInValue_ReplacesAllVariables()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=AddHeader Name=Custom Value=\"@@Prefix@@-@@Suffix@@\""
    };
    var variables = new Dictionary<string, string>
    {
      { "Prefix", "start" },
      { "Suffix", "end" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("start-end", instruction.Value);
  }

  [Fact]
  public void FromTestStep_WithMissingVariable_ThrowsInvalidOperationException()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=users/@@UserId@@"
    };
    var variables = new Dictionary<string, string>();

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
      TestStepInstruction.FromTestStep(step, variables)
    );
    Assert.Contains("UserId", exception.Message);
    Assert.Contains("could not be resolved", exception.Message);
  }

  [Fact]
  public void FromTestStep_WithNoVariables_ReturnsOriginalValue()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=users/123"
    };
    var variables = new Dictionary<string, string>
    {
      { "UserId", "456" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("users/123", instruction.Endpoint);
  }

  #endregion

  #region FromTestStep - Error Handling Tests

  [Fact]
  public void FromTestStep_WithEmptyTestData_ThrowsInvalidDataException()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = ""
    };
    var variables = new Dictionary<string, string>();

    // Act & Assert
    var exception = Assert.Throws<InvalidDataException>(() =>
      TestStepInstruction.FromTestStep(step, variables)
    );
    Assert.Contains("No TestData found", exception.Message);
    Assert.Contains("Test Step 1", exception.Message);
  }

  [Fact]
  public void FromTestStep_WithNullTestData_ThrowsInvalidDataException()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = null!
    };
    var variables = new Dictionary<string, string>();

    // Act & Assert
    var exception = Assert.Throws<InvalidDataException>(() =>
      TestStepInstruction.FromTestStep(step, variables)
    );
    Assert.Contains("No TestData found", exception.Message);
  }

  [Fact]
  public void FromTestStep_WithUnsupportedParameter_ThrowsInvalidDataException()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send UnsupportedParam=value"
    };
    var variables = new Dictionary<string, string>();

    // Act & Assert
    var exception = Assert.Throws<InvalidDataException>(() =>
      TestStepInstruction.FromTestStep(step, variables)
    );
    Assert.Contains("Unsupported parameter", exception.Message);
    Assert.Contains("UnsupportedParam", exception.Message);
    Assert.Contains("Test Step 1", exception.Message);
  }

  #endregion

  #region FromTestStep - Complex Scenarios Tests

  [Fact]
  public void FromTestStep_WithQuotedValueContainingSpaces_ParsesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=CheckContent Value=\"This is a test message\""
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("This is a test message", instruction.Value);
  }

  [Fact]
  public void FromTestStep_WithQuotedValueContainingSpecialCharacters_ParsesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=CheckContent Value=\"Test: value=123, key=abc\""
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Test: value=123, key=abc", instruction.Value);
  }

  [Fact]
  public void FromTestStep_WithEndpointContainingQueryParameters_ParsesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=api/users?page=1&size=10"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("api/users?page=1&size=10", instruction.Endpoint);
  }

  [Fact]
  public void FromTestStep_WithEndpointContainingVariableInQueryParameter_ReplacesVariable()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=api/users?id=@@UserId@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "UserId", "789" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("api/users?id=789", instruction.Endpoint);
  }

  [Fact]
  public void FromTestStep_WithComplexJsonPath_ParsesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=StoreVariable JsonPath=$.data.users[0].id"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("$.data.users[0].id", instruction.JsonPath);
  }

  [Fact]
  public void FromTestStep_WithAllParameters_ParsesAndReplacesVariablesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Method=POST Endpoint=api/users/@@UserId@@ " +
                 "ContentType=application/json Name=TestName Value=@@TestValue@@ " +
                 "File=test.json JsonPath=$.@@PropertyName@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "UserId", "123" },
      { "TestValue", "test" },
      { "PropertyName", "result" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("Send", instruction.Action);
    Assert.Equal("POST", instruction.Method);
    Assert.Equal("api/users/123", instruction.Endpoint);
    Assert.Equal("application/json", instruction.ContentType);
    Assert.Equal("TestName", instruction.Name);
    Assert.Equal("test", instruction.Value);
    Assert.Equal("test.json", instruction.File);
    Assert.Equal("$.result", instruction.JsonPath);
  }

  [Fact]
  public void FromTestStep_WithMixedCaseMethod_NormalizesToUppercase()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Method=PuT"
    };
    var variables = new Dictionary<string, string>();

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("PUT", instruction.Method);
  }

  [Fact]
  public void FromTestStep_WithVariableAtStartOfValue_ReplacesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=@@BaseUrl@@/users"
    };
    var variables = new Dictionary<string, string>
    {
      { "BaseUrl", "api/v1" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("api/v1/users", instruction.Endpoint);
  }

  [Fact]
  public void FromTestStep_WithVariableAtEndOfValue_ReplacesCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=users/@@UserId@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "UserId", "999" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("users/999", instruction.Endpoint);
  }

  [Fact]
  public void FromTestStep_WithConsecutiveVariables_ReplacesAllCorrectly()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 1,
      TestData = "Action=Send Endpoint=@@Part1@@@@Part2@@"
    };
    var variables = new Dictionary<string, string>
    {
      { "Part1", "api/" },
      { "Part2", "users" }
    };

    // Act
    var instruction = TestStepInstruction.FromTestStep(step, variables);

    // Assert
    Assert.Equal("api/users", instruction.Endpoint);
  }

  #endregion

  #region Property Tests

  [Fact]
  public void TestStep_Property_ReturnsOriginalTestStep()
  {
    // Arrange
    var step = new TestStep
    {
      Id = 42,
      Description = "Test Description",
      TestData = "Action=Send",
      ExpectedResult = "Success"
    };
    var instruction = new TestStepInstruction(step);

    // Act
    var retrievedStep = instruction.TestStep;

    // Assert
    Assert.Same(step, retrievedStep);
    Assert.Equal(42, retrievedStep.Id);
    Assert.Equal("Test Description", retrievedStep.Description);
    Assert.Equal("Action=Send", retrievedStep.TestData);
    Assert.Equal("Success", retrievedStep.ExpectedResult);
  }

  #endregion
}
