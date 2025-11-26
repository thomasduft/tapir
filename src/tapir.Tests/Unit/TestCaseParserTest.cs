using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class TestCaseParserTests
{
  [Fact]
  public async Task ToTestCaseAsync_WithMultipleTables_ShouldCreateMultipleRequests()
  {
    // Arrange
    var tempFile = Path.GetTempFileName();
    var markdown = @"# TC-Users-001: List all Users

- **Date**: 2025-11-05
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description

Tests whether all users can be retrieved and Alice's ID can be extracted so that it can be used in subsequent steps.

## Preconditions

- no pre-conditions

## Steps

| Step ID | Description             | Test Data                                                      | Expected Result        | Actual Result |
| ------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 1       | Call users api          | Action=Send Method=GET Value=users                             | Request successful     | -             |
| 2       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                    | -             |
| 3       | Inspect content         | Action=VerifyContent File=users.json                           | Should be identical    | -             |

| Step ID | Description             | Test Data                                                      | Expected Result        | Actual Result |
| ------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 1       | Get Alice Details       | Action=Send Method=GET Value=users/@@AliceId                   | Request successful     | -             |
| 2       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                    | -             |
| 3       | Inspect content         | Action=VerifyContent File=alice.json                           | Should be identical    | -             |

## Postcondition

- no post-conditions
";

    try
    {
      await File.WriteAllTextAsync(tempFile, markdown);
      var parser = new TestCaseParser(tempFile);

      // Act
      var testCase = await parser.ToTestCaseAsync(CancellationToken.None);

      // Assert
      Assert.Equal("TC-Users-001", testCase.Id);
      Assert.Equal("List all Users", testCase.Title);
      Assert.Equal("Users", testCase.Module);
      Assert.Equal("Definition", testCase.Type);
      Assert.Equal("Unknown", testCase.Status);

      // Verify we have 2 tables
      var tables = testCase.Tables.ToList();
      Assert.Equal(2, tables.Count);

      // Verify first table has 3 steps
      var firstTable = tables[0];
      var firstTableSteps = firstTable.Steps.ToList();
      Assert.Equal(3, firstTableSteps.Count);
      Assert.Equal("Call users api", firstTableSteps[0].Description);
      Assert.Equal("Verify response code", firstTableSteps[1].Description);
      Assert.Equal("Inspect content", firstTableSteps[2].Description);

      // Verify second table has 3 steps
      var secondTable = tables[1];
      var secondTableSteps = secondTable.Steps.ToList();
      Assert.Equal(3, secondTableSteps.Count);
      Assert.Equal("Get Alice Details", secondTableSteps[0].Description);
      Assert.Equal("Verify response code", secondTableSteps[1].Description);
      Assert.Equal("Inspect content", secondTableSteps[2].Description);

      // Verify total steps across all tables is 6
      var totalSteps = tables.SelectMany(t => t.Steps).Count();
      Assert.Equal(6, totalSteps);
    }
    finally
    {
      if (File.Exists(tempFile))
      {
        File.Delete(tempFile);
      }
    }
  }
}