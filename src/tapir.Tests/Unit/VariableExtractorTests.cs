using System.Net;
using System.Text;

using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class VariableExtractorTests
{
  #region Factory Method Tests

  [Fact]
  public void Create_WithInstructions_ReturnsVariableExtractorInstance()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "testVar",
        Value = "testValue"
      }
    };

    // Act
    var extractor = VariableExtractor.Create(instructions);

    // Assert
    Assert.NotNull(extractor);
  }

  [Fact]
  public void Create_WithEmptyInstructions_ReturnsVariableExtractorInstance()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>();

    // Act
    var extractor = VariableExtractor.Create(instructions);

    // Assert
    Assert.NotNull(extractor);
  }

  #endregion

  #region FromContent Tests

  [Fact]
  public void FromContent_WithHttpContent_ReturnsExtractorWithContent()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>();
    var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions);

    // Act
    var result = extractor.FromContent(content);

    // Assert
    Assert.NotNull(result);
    Assert.Same(extractor, result);
  }

  #endregion

  #region ExtractAsync - No Content Tests

  [Fact]
  public async Task ExtractAsync_WithNoContent_ReturnsEmptyVariables()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "testVar",
        Value = "testValue"
      }
    };
    var extractor = VariableExtractor.Create(instructions);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Empty(result.Variables);
    Assert.Empty(result.TestStepResults);
  }

  #endregion

  #region ExtractAsync - No StoreVariable Instructions Tests

  [Fact]
  public async Task ExtractAsync_WithNoStoreVariableInstructions_ReturnsEmptyVariables()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/test"
      }
    };
    var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Empty(result.Variables);
    Assert.Empty(result.TestStepResults);
  }

  [Fact]
  public async Task ExtractAsync_WithEmptyInstructions_ReturnsEmptyVariables()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>();
    var content = new StringContent("{\"key\":\"value\"}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Empty(result.Variables);
    Assert.Empty(result.TestStepResults);
  }

  #endregion

  #region ExtractAsync - Static Value Tests

  [Fact]
  public async Task ExtractAsync_WithStaticValue_ExtractsVariable()
  {
    // Arrange
    var testStep = new TestStep { Id = 1 };
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(testStep)
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        Value = "12345"
      }
    };
    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("12345", result.Variables["userId"]);
    Assert.Single(result.TestStepResults);
    Assert.True(result.TestStepResults.First().IsSuccess);
    Assert.Equal(1, result.TestStepResults.First().TestStepId);
  }

  [Fact]
  public async Task ExtractAsync_WithMultipleStaticValues_ExtractsAllVariables()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        Value = "12345"
      },
      new TestStepInstruction(new TestStep { Id = 2 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userName",
        Value = "john.doe"
      },
      new TestStepInstruction(new TestStep { Id = 3 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "email",
        Value = "john@example.com"
      }
    };
    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Equal(3, result.Variables.Count);
    Assert.Equal("12345", result.Variables["userId"]);
    Assert.Equal("john.doe", result.Variables["userName"]);
    Assert.Equal("john@example.com", result.Variables["email"]);
    Assert.Equal(3, result.TestStepResults.Count());
    Assert.All(result.TestStepResults, r => Assert.True(r.IsSuccess));
  }

  [Fact]
  public async Task ExtractAsync_WithEmptyValue_ExtractsEmptyString()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "emptyVar",
        Value = ""
      }
    };
    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Empty(result.Variables);
  }

  #endregion

  #region ExtractAsync - JsonPath Tests

  [Fact]
  public async Task ExtractAsync_WithJsonPath_ExtractsValueFromJson()
  {
    // Arrange
    var testStep = new TestStep { Id = 1 };
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(testStep)
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        JsonPath = "$.id"
      }
    };
    var jsonContent = "{\"id\":\"12345\",\"name\":\"John Doe\"}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("12345", result.Variables["userId"]);
    Assert.Single(result.TestStepResults);
    Assert.True(result.TestStepResults.First().IsSuccess);
  }

  [Fact]
  public async Task ExtractAsync_WithJsonPathToNestedProperty_ExtractsValue()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "city",
        JsonPath = "$.address.city"
      }
    };
    var jsonContent = "{\"address\":{\"street\":\"Main St\",\"city\":\"New York\"}}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("New York", result.Variables["city"]);
  }

  [Fact]
  public async Task ExtractAsync_WithJsonPathToArrayElement_ExtractsValue()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "firstItem",
        JsonPath = "$.items[0]"
      }
    };
    var jsonContent = "{\"items\":[\"apple\",\"banana\",\"orange\"]}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("apple", result.Variables["firstItem"]);
  }

  [Fact]
  public async Task ExtractAsync_WithMultipleJsonPaths_ExtractsAllVariables()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        JsonPath = "$.id"
      },
      new TestStepInstruction(new TestStep { Id = 2 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userName",
        JsonPath = "$.name"
      },
      new TestStepInstruction(new TestStep { Id = 3 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userEmail",
        JsonPath = "$.email"
      }
    };
    var jsonContent = "{\"id\":\"123\",\"name\":\"John\",\"email\":\"john@example.com\"}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Equal(3, result.Variables.Count);
    Assert.Equal("123", result.Variables["userId"]);
    Assert.Equal("John", result.Variables["userName"]);
    Assert.Equal("john@example.com", result.Variables["userEmail"]);
    Assert.Equal(3, result.TestStepResults.Count());
  }

  [Fact]
  public async Task ExtractAsync_WithInvalidJsonPath_DoesNotExtractVariable()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "missingValue",
        JsonPath = "$.nonexistent"
      }
    };
    var jsonContent = "{\"id\":\"123\",\"name\":\"John\"}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Empty(result.Variables);
    Assert.Empty(result.TestStepResults);
  }

  [Fact]
  public async Task ExtractAsync_WithJsonPathReturningNull_DoesNotExtractVariable()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "nullValue",
        JsonPath = "$.nullField"
      }
    };
    var jsonContent = "{\"id\":\"123\",\"nullField\":null}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Empty(result.Variables);
  }

  #endregion

  #region ExtractAsync - Mixed Instructions Tests

  [Fact]
  public async Task ExtractAsync_WithMixedStaticAndJsonPath_ExtractsAllVariables()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "staticVar",
        Value = "staticValue"
      },
      new TestStepInstruction(new TestStep { Id = 2 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "jsonVar",
        JsonPath = "$.id"
      }
    };
    var jsonContent = "{\"id\":\"123\"}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Equal(2, result.Variables.Count);
    Assert.Equal("staticValue", result.Variables["staticVar"]);
    Assert.Equal("123", result.Variables["jsonVar"]);
    Assert.Equal(2, result.TestStepResults.Count());
  }

  [Fact]
  public async Task ExtractAsync_WithMixedActionsIncludingStoreVariable_ExtractsOnlyStoreVariableInstructions()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/test"
      },
      new TestStepInstruction(new TestStep { Id = 2 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        Value = "12345"
      },
      new TestStepInstruction(new TestStep { Id = 3 })
      {
        Action = Constants.Actions.CheckStatusCode,
        Value = "200"
      }
    };
    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("12345", result.Variables["userId"]);
    Assert.Single(result.TestStepResults);
    Assert.Equal(2, result.TestStepResults.First().TestStepId);
  }

  #endregion

  #region ExtractAsync - Exception Tests

  [Fact]
  public async Task ExtractAsync_WithMissingVariableName_ThrowsInvalidOperationException()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "",
        Value = "testValue"
      }
    };
    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      () => extractor.ExtractAsync(CancellationToken.None)
    );
    Assert.Contains("Variable Name must be provided", exception.Message);
    Assert.Contains("Step ID: '1'", exception.Message);
  }

  [Fact]
  public async Task ExtractAsync_WithNullVariableName_ThrowsInvalidOperationException()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 5 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = null!,
        JsonPath = "$.id"
      }
    };
    var content = new StringContent("{\"id\":\"123\"}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      () => extractor.ExtractAsync(CancellationToken.None)
    );
    Assert.Contains("Variable Name must be provided", exception.Message);
    Assert.Contains("Step ID: '5'", exception.Message);
  }

  #endregion

  #region ExtractAsync - Complex JSON Tests

  [Fact]
  public async Task ExtractAsync_WithComplexJsonStructure_ExtractsNestedValues()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userName",
        JsonPath = "$.user.profile.name"
      },
      new TestStepInstruction(new TestStep { Id = 2 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "firstTag",
        JsonPath = "$.user.tags[0]"
      }
    };
    var jsonContent = @"{
      ""user"": {
        ""id"": 1,
        ""profile"": {
          ""name"": ""Alice"",
          ""age"": 30
        },
        ""tags"": [""admin"", ""developer""]
      }
    }";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Equal(2, result.Variables.Count);
    Assert.Equal("Alice", result.Variables["userName"]);
    Assert.Equal("admin", result.Variables["firstTag"]);
  }

  [Fact]
  public async Task ExtractAsync_WithNumericValue_ExtractsAsString()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "age",
        JsonPath = "$.age"
      }
    };
    var jsonContent = "{\"age\":42}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("42", result.Variables["age"]);
  }

  [Fact]
  public async Task ExtractAsync_WithBooleanValue_ExtractsAsString()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "isActive",
        JsonPath = "$.active"
      }
    };
    var jsonContent = "{\"active\":true}";
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("true", result.Variables["isActive"]);
  }

  #endregion

  #region ExtractAsync - Variable Overwriting Tests

  [Fact]
  public async Task ExtractAsync_WithDuplicateVariableName_OverwritesPreviousValue()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep { Id = 1 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        Value = "first-value"
      },
      new TestStepInstruction(new TestStep { Id = 2 })
      {
        Action = Constants.Actions.StoreVariable,
        Name = "userId",
        Value = "second-value"
      }
    };
    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    var extractor = VariableExtractor.Create(instructions)
      .FromContent(content);

    // Act
    var result = await extractor.ExtractAsync(CancellationToken.None);

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("second-value", result.Variables["userId"]);
    Assert.Equal(2, result.TestStepResults.Count());
  }

  #endregion
}