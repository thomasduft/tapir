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

| Step ID | Description             | Test Data                                                             | Expected Result        | Actual Result |
| ------: | ----------------------- | --------------------------------------------------------------------- | ---------------------- | ------------- |
| 1       | Call api/users         | Step=Act    Action=Send Method=GET Value=api/users                     | 200 OK                 | -             |
| 2       | Verify response code    | Step=Assert Action=CheckResponseCode Value=200                        | 200                    | -             |
| 3       | Inspect content         | Step=Assert Action=VerifyContent File=users.json                      | Should be identical    | -             |
| 4       | Contains Alice          | Step=Assert Action=CheckContent Value=\""$[?(@.name == 'Alice')]\""   | Content contains Alice | -             |
| 5       | Get Alice ID            | Step=Assert Action=StoreVariable Value=\""$[?(@.name == 'Alice')].id\"" | Returns Alice's ID     | -             |


## Postcondition
";

    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Equal(5, testSteps.Count);

    var firstStep = testSteps[0];
    Assert.Equal(1, firstStep.Id);
    Assert.Equal("Call api/users", firstStep.Description);
    Assert.Equal("Step=Act    Action=Send Method=GET Value=api/users", firstStep.TestData);
    Assert.Equal("200 OK", firstStep.ExpectedResult);
  }
}
