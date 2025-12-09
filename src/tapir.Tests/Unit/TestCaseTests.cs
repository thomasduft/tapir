using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class TestCaseTests
{
  #region Constructor and Property Initialization Tests

  [Fact]
  public void Constructor_InitializesPropertiesWithDefaultValues()
  {
    // Act
    var testCase = new TestCase();

    // Assert
    Assert.Equal(string.Empty, testCase.Id);
    Assert.Equal(string.Empty, testCase.Title);
    Assert.Equal("Unkown", testCase.Module);
    Assert.Equal(string.Empty, testCase.Type);
    Assert.Equal(string.Empty, testCase.Status);
    Assert.Empty(testCase.Tables);
    Assert.Equal(string.Empty, testCase.File);
    Assert.Equal(string.Empty, testCase.LinkedFile);
    Assert.Equal(string.Empty, testCase.Domain);
    Assert.Empty(testCase.Variables);
  }

  [Fact]
  public void Id_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedId = "TC-001";

    // Act
    testCase.Id = expectedId;

    // Assert
    Assert.Equal(expectedId, testCase.Id);
  }

  [Fact]
  public void Title_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedTitle = "Test Case Title";

    // Act
    testCase.Title = expectedTitle;

    // Assert
    Assert.Equal(expectedTitle, testCase.Title);
  }

  [Fact]
  public void Module_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedModule = "Users";

    // Act
    testCase.Module = expectedModule;

    // Assert
    Assert.Equal(expectedModule, testCase.Module);
  }

  [Fact]
  public void Type_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedType = Constants.TestCaseType.Definition;

    // Act
    testCase.Type = expectedType;

    // Assert
    Assert.Equal(expectedType, testCase.Type);
  }

  [Fact]
  public void Status_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedStatus = Constants.TestCaseStatus.Passed;

    // Act
    testCase.Status = expectedStatus;

    // Assert
    Assert.Equal(expectedStatus, testCase.Status);
  }

  [Fact]
  public void Tables_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var tables = new List<Table> { new Table(), new Table() };

    // Act
    testCase.Tables = tables;

    // Assert
    Assert.Equal(2, testCase.Tables.Count());
  }

  [Fact]
  public void File_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedFile = "/path/to/test.md";

    // Act
    testCase.File = expectedFile;

    // Assert
    Assert.Equal(expectedFile, testCase.File);
  }

  [Fact]
  public void LinkedFile_CanBeSetAndRetrieved()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedLinkedFile = "/path/to/linked.md";

    // Act
    testCase.LinkedFile = expectedLinkedFile;

    // Assert
    Assert.Equal(expectedLinkedFile, testCase.LinkedFile);
  }

  #endregion

  #region IsDefinition Property Tests

  [Fact]
  public void IsDefinition_ReturnsTrueWhenTypeIsDefinition()
  {
    // Arrange
    var testCase = new TestCase { Type = Constants.TestCaseType.Definition };

    // Act
    var result = testCase.IsDefinition;

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void IsDefinition_ReturnsFalseWhenTypeIsRun()
  {
    // Arrange
    var testCase = new TestCase { Type = Constants.TestCaseType.Run };

    // Act
    var result = testCase.IsDefinition;

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void IsDefinition_ReturnsFalseWhenTypeIsEmpty()
  {
    // Arrange
    var testCase = new TestCase { Type = string.Empty };

    // Act
    var result = testCase.IsDefinition;

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void IsDefinition_ReturnsFalseWhenTypeIsNull()
  {
    // Arrange
    var testCase = new TestCase { Type = null! };

    // Act
    var result = testCase.IsDefinition;

    // Assert
    Assert.False(result);
  }

  #endregion

  #region HasLinkedFile Property Tests

  [Fact]
  public void HasLinkedFile_ReturnsTrueWhenLinkedFileIsSet()
  {
    // Arrange
    var testCase = new TestCase { LinkedFile = "/path/to/file.md" };

    // Act
    var result = testCase.HasLinkedFile;

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void HasLinkedFile_ReturnsFalseWhenLinkedFileIsEmpty()
  {
    // Arrange
    var testCase = new TestCase { LinkedFile = string.Empty };

    // Act
    var result = testCase.HasLinkedFile;

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void HasLinkedFile_ReturnsFalseWhenLinkedFileIsNull()
  {
    // Arrange
    var testCase = new TestCase { LinkedFile = null! };

    // Act
    var result = testCase.HasLinkedFile;

    // Assert
    Assert.False(result);
  }

  #endregion

  #region HasDomain Property Tests

  [Fact]
  public void HasDomain_ReturnsTrueWhenDomainIsSet()
  {
    // Arrange
    var testCase = new TestCase().WithDomain("https://example.com");

    // Act
    var result = testCase.HasDomain;

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void HasDomain_ReturnsFalseWhenDomainIsEmpty()
  {
    // Arrange
    var testCase = new TestCase();

    // Act
    var result = testCase.HasDomain;

    // Assert
    Assert.False(result);
  }

  #endregion

  #region HasVariables Property Tests

  [Fact]
  public void HasVariables_ReturnsTrueWhenVariablesExist()
  {
    // Arrange
    var testCase = new TestCase();
    testCase.Variables.Add("key", "value");

    // Act
    var result = testCase.HasVariables;

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void HasVariables_ReturnsFalseWhenVariablesAreEmpty()
  {
    // Arrange
    var testCase = new TestCase();

    // Act
    var result = testCase.HasVariables;

    // Assert
    Assert.False(result);
  }

  #endregion

  #region WithDomain Method Tests

  [Fact]
  public void WithDomain_SetsDomainAndReturnsInstance()
  {
    // Arrange
    var testCase = new TestCase();
    var expectedDomain = "https://api.example.com";

    // Act
    var result = testCase.WithDomain(expectedDomain);

    // Assert
    Assert.Same(testCase, result);
    Assert.Equal(expectedDomain, result.Domain);
  }

  [Fact]
  public void WithDomain_CanBeChained()
  {
    // Arrange
    var testCase = new TestCase();

    // Act
    var result = testCase
      .WithDomain("https://example.com")
      .WithVariables(new Dictionary<string, string> { { "key", "value" } });

    // Assert
    Assert.Equal("https://example.com", result.Domain);
    Assert.Single(result.Variables);
  }

  [Fact]
  public void WithDomain_OverwritesPreviousDomain()
  {
    // Arrange
    var testCase = new TestCase().WithDomain("https://first.com");

    // Act
    var result = testCase.WithDomain("https://second.com");

    // Assert
    Assert.Equal("https://second.com", result.Domain);
  }

  #endregion

  #region WithVariables Method Tests

  [Fact]
  public void WithVariables_SetsVariablesAndReturnsInstance()
  {
    // Arrange
    var testCase = new TestCase();
    var variables = new Dictionary<string, string>
    {
      { "key1", "value1" },
      { "key2", "value2" }
    };

    // Act
    var result = testCase.WithVariables(variables);

    // Assert
    Assert.Same(testCase, result);
    Assert.Equal(2, result.Variables.Count);
    Assert.Equal("value1", result.Variables["key1"]);
    Assert.Equal("value2", result.Variables["key2"]);
  }

  [Fact]
  public void WithVariables_CanBeChained()
  {
    // Arrange
    var testCase = new TestCase();
    var variables = new Dictionary<string, string> { { "key", "value" } };

    // Act
    var result = testCase
      .WithVariables(variables)
      .WithDomain("https://example.com");

    // Assert
    Assert.Single(result.Variables);
    Assert.Equal("https://example.com", result.Domain);
  }

  [Fact]
  public void WithVariables_ReplacesExistingVariables()
  {
    // Arrange
    var testCase = new TestCase().WithVariables(new Dictionary<string, string>
    {
      { "old", "value" }
    });
    var newVariables = new Dictionary<string, string>
    {
      { "new", "value" }
    };

    // Act
    var result = testCase.WithVariables(newVariables);

    // Assert
    Assert.Single(result.Variables);
    Assert.True(result.Variables.ContainsKey("new"));
    Assert.False(result.Variables.ContainsKey("old"));
  }

  [Fact]
  public void WithVariables_WithEmptyDictionary_ClearsVariables()
  {
    // Arrange
    var testCase = new TestCase().WithVariables(new Dictionary<string, string>
    {
      { "key", "value" }
    });

    // Act
    var result = testCase.WithVariables(new Dictionary<string, string>());

    // Assert
    Assert.Empty(result.Variables);
  }

  #endregion

  #region AddOrMergeVariables Method Tests

  [Fact]
  public void AddOrMergeVariables_AddsNewVariables()
  {
    // Arrange
    var testCase = new TestCase();
    var variables = new Dictionary<string, string>
    {
      { "key1", "value1" },
      { "key2", "value2" }
    };

    // Act
    testCase.AddOrMergeVariables(variables);

    // Assert
    Assert.Equal(2, testCase.Variables.Count);
    Assert.Equal("value1", testCase.Variables["key1"]);
    Assert.Equal("value2", testCase.Variables["key2"]);
  }

  [Fact]
  public void AddOrMergeVariables_MergesWithExistingVariables()
  {
    // Arrange
    var testCase = new TestCase();
    testCase.Variables.Add("existing", "value");
    var newVariables = new Dictionary<string, string>
    {
      { "new", "newValue" }
    };

    // Act
    testCase.AddOrMergeVariables(newVariables);

    // Assert
    Assert.Equal(2, testCase.Variables.Count);
    Assert.Equal("value", testCase.Variables["existing"]);
    Assert.Equal("newValue", testCase.Variables["new"]);
  }

  [Fact]
  public void AddOrMergeVariables_OverwritesExistingVariablesWithSameKey()
  {
    // Arrange
    var testCase = new TestCase();
    testCase.Variables.Add("key", "oldValue");
    var variables = new Dictionary<string, string>
    {
      { "key", "newValue" }
    };

    // Act
    testCase.AddOrMergeVariables(variables);

    // Assert
    Assert.Single(testCase.Variables);
    Assert.Equal("newValue", testCase.Variables["key"]);
  }

  [Fact]
  public void AddOrMergeVariables_WithNullVariables_DoesNotThrow()
  {
    // Arrange
    var testCase = new TestCase();

    // Act
    var exception = Record.Exception(() => testCase.AddOrMergeVariables(null!));

    // Assert
    Assert.Null(exception);
  }

  [Fact]
  public void AddOrMergeVariables_WithEmptyDictionary_DoesNotChangeVariables()
  {
    // Arrange
    var testCase = new TestCase();
    testCase.Variables.Add("existing", "value");
    var emptyVariables = new Dictionary<string, string>();

    // Act
    testCase.AddOrMergeVariables(emptyVariables);

    // Assert
    Assert.Single(testCase.Variables);
    Assert.Equal("value", testCase.Variables["existing"]);
  }

  [Fact]
  public void AddOrMergeVariables_WithMultipleCallsAccumulates()
  {
    // Arrange
    var testCase = new TestCase();
    var variables1 = new Dictionary<string, string> { { "key1", "value1" } };
    var variables2 = new Dictionary<string, string> { { "key2", "value2" } };
    var variables3 = new Dictionary<string, string> { { "key3", "value3" } };

    // Act
    testCase.AddOrMergeVariables(variables1);
    testCase.AddOrMergeVariables(variables2);
    testCase.AddOrMergeVariables(variables3);

    // Assert
    Assert.Equal(3, testCase.Variables.Count);
    Assert.Equal("value1", testCase.Variables["key1"]);
    Assert.Equal("value2", testCase.Variables["key2"]);
    Assert.Equal("value3", testCase.Variables["key3"]);
  }

  #endregion

  #region FromTestCaseFileAsync Method Tests

  [Fact]
  public async Task FromTestCaseFileAsync_WithValidFile_ReturnsTestCase()
  {
    // Arrange
    var testFile = Path.Combine(
      Directory.GetCurrentDirectory(),
      "TestData",
      "TC-Users-001.md"
    );

    // Act
    var result = await TestCase.FromTestCaseFileAsync(testFile, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
  }

  [Fact]
  public async Task FromTestCaseFileAsync_CallsParserWithCorrectFile()
  {
    // Arrange
    var testFile = Path.Combine(
      Directory.GetCurrentDirectory(),
      "TestData",
      "TC-Users-001.md"
    );

    // Act
    var result = await TestCase.FromTestCaseFileAsync(testFile, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(testFile, result.File);
  }

  [Fact]
  public async Task FromTestCaseFileAsync_WithCancellationToken_PassesToParser()
  {
    // Arrange
    var testFile = Path.Combine(
      Directory.GetCurrentDirectory(),
      "TestData",
      "TC-Users-001.md"
    );
    var cts = new CancellationTokenSource();

    // Act
    var result = await TestCase.FromTestCaseFileAsync(testFile, cts.Token);

    // Assert
    Assert.NotNull(result);
  }

  #endregion

  #region Integration Tests

  [Fact]
  public void TestCase_CompleteWorkflow_AllPropertiesAndMethods()
  {
    // Arrange
    var testCase = new TestCase
    {
      Id = "TC-001",
      Title = "Test User Login",
      Module = "Authentication",
      Type = Constants.TestCaseType.Definition,
      Status = Constants.TestCaseStatus.Passed,
      File = "/tests/login.md",
      LinkedFile = "/tests/login-run.md"
    };

    // Act
    testCase
      .WithDomain("https://api.example.com")
      .WithVariables(new Dictionary<string, string>
      {
        { "username", "testuser" },
        { "password", "testpass" }
      });

    testCase.AddOrMergeVariables(new Dictionary<string, string>
    {
      { "apiKey", "12345" }
    });

    // Assert
    Assert.Equal("TC-001", testCase.Id);
    Assert.Equal("Test User Login", testCase.Title);
    Assert.Equal("Authentication", testCase.Module);
    Assert.True(testCase.IsDefinition);
    Assert.Equal(Constants.TestCaseStatus.Passed, testCase.Status);
    Assert.True(testCase.HasLinkedFile);
    Assert.True(testCase.HasDomain);
    Assert.Equal("https://api.example.com", testCase.Domain);
    Assert.True(testCase.HasVariables);
    Assert.Equal(3, testCase.Variables.Count);
    Assert.Equal("testuser", testCase.Variables["username"]);
    Assert.Equal("testpass", testCase.Variables["password"]);
    Assert.Equal("12345", testCase.Variables["apiKey"]);
  }

  #endregion
}