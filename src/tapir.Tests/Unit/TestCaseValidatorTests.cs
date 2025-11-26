using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class TestCaseValidatorTests
{
  #region ValidateAsync - Type Validation

  [Fact]
  public async Task ValidateAsync_WithValidDefinitionType_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-001",
      Title = "Test Case 1",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithValidRunType_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-002",
      Title = "Test Case 2",
      Type = Constants.TestCaseType.Run,
      Status = Constants.TestCaseStatus.Passed,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithInvalidType_ShouldReturnTypeError()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-003",
      Title = "Test Case 3",
      Type = "InvalidType",
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Single(result.Errors);
    Assert.Contains("Type must be 'Definition' or 'Run'.", result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithEmptyType_ShouldReturnTypeError()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-004",
      Title = "Test Case 4",
      Type = "",
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains("Type must be 'Definition' or 'Run'.", result.Errors);
  }

  #endregion

  #region ValidateAsync - Status Validation

  [Fact]
  public async Task ValidateAsync_WithPassedStatus_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-005",
      Title = "Test Case 5",
      Type = Constants.TestCaseType.Run,
      Status = Constants.TestCaseStatus.Passed,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithFailedStatus_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-006",
      Title = "Test Case 6",
      Type = Constants.TestCaseType.Run,
      Status = Constants.TestCaseStatus.Failed,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithUnknownStatus_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-007",
      Title = "Test Case 7",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithInvalidStatus_ShouldReturnStatusError()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-008",
      Title = "Test Case 8",
      Type = Constants.TestCaseType.Definition,
      Status = "InvalidStatus",
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Single(result.Errors);
    Assert.Contains("Status must be either 'Passed', 'Failed' or 'Unknown'.", result.Errors);
  }

  #endregion

  #region ValidateAsync - LinkedFile Validation

  [Fact]
  public async Task ValidateAsync_WithNoLinkedFile_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-009",
      Title = "Test Case 9",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      LinkedFile = "",
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithNonExistentLinkedFile_ShouldReturnLinkError()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-010",
      Title = "Test Case 10",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      LinkedFile = "/non/existent/file.md",
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains("Linked file /non/existent/file.md does not exist.", result.Errors);
  }

  #endregion

  #region ValidateAsync - Multiple Errors

  [Fact]
  public async Task ValidateAsync_WithMultipleErrors_ShouldReturnAllErrors()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-011",
      Title = "Test Case 11",
      Type = "InvalidType",
      Status = "InvalidStatus",
      LinkedFile = "/non/existent/file.md",
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Equal(3, result.Errors.Count());
    Assert.Contains("Type must be 'Definition' or 'Run'.", result.Errors);
    Assert.Contains("Status must be either 'Passed', 'Failed' or 'Unknown'.", result.Errors);
    Assert.Contains("Linked file /non/existent/file.md does not exist.", result.Errors);
  }

  #endregion

  #region ValidateAsync - Test Step Validation

  [Fact]
  public async Task ValidateAsync_WithEmptyTestSteps_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-012",
      Title = "Test Case 12",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>()
        }
      }
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithStepsWithEmptyTestData_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-013",
      Title = "Test Case 13",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>
          {
            new TestStep
            {
              Id = 1,
              Description = "Step 1",
              TestData = "",
              ExpectedResult = "Result"
            }
          }
        }
      }
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithStepsWithWhitespaceTestData_ShouldReturnValidResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-014",
      Title = "Test Case 14",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>
          {
            new TestStep
            {
              Id = 1,
              Description = "Step 1",
              TestData = "   ",
              ExpectedResult = "Result"
            }
          }
        }
      }
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public async Task ValidateAsync_WithValidTestStep_ShouldCallValidator()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-015",
      Title = "Test Case 15",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>
          {
            new TestStep
            {
              Id = 1,
              Description = "Send request",
              TestData = "Action=Send Method=GET Endpoint=api/users",
              ExpectedResult = "Success"
            }
          }
        }
      }
    };
    var mockValidator = new MockValidator("Send", null);
    var validators = new List<IValidator> { mockValidator };
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.True(result.IsValid);
    Assert.True(mockValidator.WasCalled);
  }

  [Fact]
  public async Task ValidateAsync_WithValidatorReturningErrors_ShouldIncludeErrors()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-016",
      Title = "Test Case 16",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>
          {
            new TestStep
            {
              Id = 1,
              Description = "Send request",
              TestData = "Action=Send Method=GET Endpoint=api/users",
              ExpectedResult = "Success"
            }
          }
        }
      }
    };
    var errors = new List<TestStepValidationError>
    {
      new TestStepValidationError(1, "Invalid endpoint")
    };
    var mockValidator = new MockValidator("Send", errors);
    var validators = new List<IValidator> { mockValidator };
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Single(result.Errors);
    Assert.Contains("Invalid endpoint", result.Errors.First());
  }

  [Fact]
  public async Task ValidateAsync_WithExceptionDuringValidation_ShouldCaptureException()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-017",
      Title = "Test Case 17",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>
          {
            new TestStep
            {
              Id = 1,
              Description = "Invalid step",
              TestData = "InvalidData",
              ExpectedResult = "Result"
            }
          }
        }
      }
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Single(result.Errors);
    Assert.Contains("Exception during validation", result.Errors.First());
  }

  [Fact]
  public async Task ValidateAsync_WithMultipleStepsAndMultipleErrors_ShouldReturnAllErrors()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-018",
      Title = "Test Case 18",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>
      {
        new Table
        {
          Steps = new List<TestStep>
          {
            new TestStep
            {
              Id = 1,
              Description = "Send request",
              TestData = "Action=Send Method=GET Endpoint=api/users",
              ExpectedResult = "Success"
            },
            new TestStep
            {
              Id = 2,
              Description = "Check status",
              TestData = "Action=CheckStatusCode Value=200",
              ExpectedResult = "200"
            }
          }
        }
      }
    };
    var errors1 = new List<TestStepValidationError>
    {
      new TestStepValidationError(1, "Error in step 1")
    };
    var errors2 = new List<TestStepValidationError>
    {
      new TestStepValidationError(2, "Error in step 2")
    };
    var mockValidator1 = new MockValidator("Send", errors1);
    var mockValidator2 = new MockValidator("CheckStatusCode", errors2);
    var validators = new List<IValidator> { mockValidator1, mockValidator2 };
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.False(result.IsValid);
    Assert.Equal(2, result.Errors.Count());
    Assert.Contains("Step 01: Error in step 1", result.Errors);
    Assert.Contains("Step 02: Error in step 2", result.Errors);
  }

  #endregion

  #region ValidateAsync - Result Properties

  [Fact]
  public async Task ValidateAsync_ShouldSetTestCaseIdInResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-019",
      Title = "Test Case 19",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.Equal("TC-019", result.TestCaseId);
  }

  [Fact]
  public async Task ValidateAsync_ShouldSetTestCaseTitleInResult()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-020",
      Title = "My Test Case Title",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Unknown,
      Tables = new List<Table>()
    };
    var validators = new List<IValidator>();
    var validator = new TestCaseValidator(validators);

    // Act
    var result = await validator.ValidateAsync(testCase, CancellationToken.None);

    // Assert
    Assert.Equal("My Test Case Title", result.TestCaseTitle);
  }

  #endregion

  #region Mock Helper Classes

  private class MockValidator : IValidator
  {
    private readonly IEnumerable<TestStepValidationError>? _errorsToReturn;

    public string Name { get; }
    public string Description => "Mock validator";
    public IEnumerable<string> SupportedProperties => new[] { "Action", "Value" };
    public bool WasCalled { get; private set; }

    public MockValidator(string name, IEnumerable<TestStepValidationError>? errorsToReturn)
    {
      Name = name;
      _errorsToReturn = errorsToReturn;
    }

    public Task<IEnumerable<TestStepValidationError>> ValidateAsync(
      TestStepInstruction testStepInstruction,
      CancellationToken cancellationToken)
    {
      WasCalled = true;
      return Task.FromResult(_errorsToReturn ?? Enumerable.Empty<TestStepValidationError>());
    }
  }

  #endregion
}