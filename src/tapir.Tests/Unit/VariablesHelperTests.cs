using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class VariablesHelperTests
{
  [Fact]
  public void CreateVariables_WithNull_ReturnsEmptyDictionary()
  {
    // Act
    var result = VariablesHelper.CreateVariables(null!);

    // Assert
    Assert.Empty(result);
  }

  [Fact]
  public void CreateVariables_WithEmptyList_ReturnsEmptyDictionary()
  {
    // Act
    var result = VariablesHelper.CreateVariables([]);

    // Assert
    Assert.Empty(result);
  }

  [Fact]
  public void CreateVariables_WithValidKeyValuePairs_ReturnsDictionary()
  {
    // Arrange
    var variables = new List<string?> { "key1=value1", "key2=value2" };

    // Act
    var result = VariablesHelper.CreateVariables(variables);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal("value1", result["key1"]);
    Assert.Equal("value2", result["key2"]);
  }

  [Fact]
  public void CreateVariables_WithWhitespace_TrimsKeysAndValues()
  {
    // Arrange
    var variables = new List<string?> { " key1 = value1 " };

    // Act
    var result = VariablesHelper.CreateVariables(variables);

    // Assert
    Assert.Single(result);
    Assert.Equal("value1", result["key1"]);
  }

  [Fact]
  public void CreateVariables_WithEntryMissingEquals_SkipsEntry()
  {
    // Arrange
    var variables = new List<string?> { "invalidentry", "key=value" };

    // Act
    var result = VariablesHelper.CreateVariables(variables);

    // Assert
    Assert.Single(result);
    Assert.Equal("value", result["key"]);
  }

  [Fact]
  public void CreateVariables_WithMultipleEqualsSigns_ParsesValueAfterFirstEquals()
  {
    // Arrange
    var variables = new List<string?> { "key=value=extra", "key2=value2" };

    // Act
    var result = VariablesHelper.CreateVariables(variables);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Equal("value=extra", result["key"]);
    Assert.Equal("value2", result["key2"]);
  }

  [Fact]
  public void CreateVariables_WithQuotedBase64Value_StripsQuotesAndPreservesEquals()
  {
    // Arrange
    var variables = new List<string?> { "Token=\"ZWRvc3NpZXJfZTJlX3VzZXI6RXExc0MtkzZ0xBaS16ekh1M05mUgo=\"" };

    // Act
    var result = VariablesHelper.CreateVariables(variables);

    // Assert
    Assert.Single(result);
    Assert.Equal("ZWRvc3NpZXJfZTJlX3VzZXI6RXExc0MtkzZ0xBaS16ekh1M05mUgo=", result["Token"]);
  }
}
