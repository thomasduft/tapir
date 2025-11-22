using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class MarkdownTableTests
{
  [Fact]
  public void ParseTestSteps_WithValidTable_ShouldReturnTestSteps()
  {
    // Arrange
    var markdown = @"
# Test Case

## Steps

| Step ID | Description             | Test Data                                                      | Expected Result                  | Actual Result |
| ------: | ----------------------- | ---------------------------------------------------------------| -------------------------------- | ------------- |
| 1       | Call users api          | Action=Send Method=GET Value=users                             | Request successful               | -             |
| 2       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                              | -             |
| 3       | Inspect content         | Action=VerifyContent File=users.json                           | Should be identical              | -             |
| 4       | Contains Alice          | Action=CheckContent Path=\""$[?@.name==\""Alice\""].name\"" Value=Alice  | Content contains Alice | -             |
| 5       | Get Alice ID            | Action=StoreVariable Path=\""$[?@.name==\""Alice\""].id\"" Name=AliceId  | Returns Alice's ID     | -             |

## Postcondition
";

    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Equal(5, testSteps.Count);

    var firstStep = testSteps[0];
    Assert.Equal(1, firstStep.Id);
    Assert.Equal("Call users api", firstStep.Description);
    Assert.Equal("Action=Send Method=GET Value=users", firstStep.TestData);
    Assert.Equal("Request successful", firstStep.ExpectedResult);
  }

  [Fact]
  public void ExtractAllTableSections_WithMultipleTables_ShouldReturnSeparateSections()
  {
    // Arrange
    var markdown = @"
# Test Case

## Steps

| Step ID | Description             | Test Data                                                      | Expected Result        | Actual Result |
| ------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 1       | Call users api          | Action=Send Method=GET Value=users                             | Request successful     | -             |
| 2       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                    | -             |

| Step ID | Description             | Test Data                                                      | Expected Result        | Actual Result |
| ------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 1       | Get Alice Details       | Action=Send Method=GET Value=users/{@@AliceId}                 | Request successful     | -             |
| 2       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                    | -             |

## Postcondition
";

    var table = new MarkdownTable(markdown);

    // Act
    var tableSections = table.ExtractAllTableSections().ToList();

    // Assert
    Assert.Equal(2, tableSections.Count);

    // Verify first table
    var firstTableSteps = new MarkdownTable(tableSections[0]).ParseTestSteps().ToList();
    Assert.Equal(2, firstTableSteps.Count);
    Assert.Equal("Call users api", firstTableSteps[0].Description);
    Assert.Equal("Verify response code", firstTableSteps[1].Description);

    // Verify second table
    var secondTableSteps = new MarkdownTable(tableSections[1]).ParseTestSteps().ToList();
    Assert.Equal(2, secondTableSteps.Count);
    Assert.Equal("Get Alice Details", secondTableSteps[0].Description);
    Assert.Equal("Verify response code", secondTableSteps[1].Description);
  }
}
