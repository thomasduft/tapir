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

  [Fact]
  public void UpdateTestStepsWithResults_WithMultipleTables_ShouldUpdateAllTables()
  {
    // Arrange
    var markdown = @"# TC-Users-001: List all Users

- **Date**: 2025-11-22
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Run
- **Status**: Failed
- **Domain**: https://localhost:5001

## Description

Tests the Users API by first retrieving all users, verifying the response contains Alice, and extracting her ID for subsequent operations.

## Steps

| Step ID  | Description             | Test Data                                                      | Expected Result        | Actual Result |
| -------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 11 | Call users api | Action=Send Method=GET Value=users | Request successful | - |
| 12 | Verify response code | Action=CheckStatusCode Value=200 | 200 | - |
| 13 | Inspect content | Action=VerifyContent File=users.json | Should be identical | - |
| 14 | Contains Alice | Action=CheckContent Path=$[?@.name==\""Alice\""].name Value=Alice | Content contains Alice | - |
| 15 | Retain ID of Alice ID | Action=StoreVariable Path=$[?@.name==\""Alice\""].id Name=AliceId | ID of Alice stored | - |

| Step ID  | Description             | Test Data                                                      | Expected Result        | Actual Result |
| -------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 21       | Get Alice Details       | Action=Send Method=GET Value=users/{@@AliceId@@}               | Request successful     | -             |
| 22       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                    | -             |
| 23       | Inspect content         | Action=VerifyContent File=alice.json                           | Should be identical    | -             |
| 24       | Verify Name             | Action=CheckContent Path=$.name Value=Alice                    | Name is Alice          | -             |
| 25       | Verify Age              | Action=CheckContent Path=$.age Value=20                        | Age is 20              | -             |

## Postcondition

- no post-conditions
";

    var table = new MarkdownTable(markdown);

    // Create test results with some failures
    var testResults = new List<TestStepResult>
    {
      TestStepResult.Success(new TestStep { Id = 11 }),
      TestStepResult.Success(new TestStep { Id = 12 }),
      TestStepResult.Success(new TestStep { Id = 13 }),
      TestStepResult.Success(new TestStep { Id = 14 }),
      TestStepResult.Success(new TestStep { Id = 15 }),
      TestStepResult.Success(new TestStep { Id = 21 }),
      TestStepResult.Failed(new TestStep { Id = 22 }, "Expected 200 but got 404"),
      TestStepResult.Failed(new TestStep { Id = 23 }, "File not found"),
      TestStepResult.Failed(new TestStep { Id = 24 }, "Name mismatch"),
      TestStepResult.Failed(new TestStep { Id = 25 }, "Age mismatch")
    };

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    // Verify first table is updated correctly (all success)
    Assert.Contains("| 11 | Call users api | Action=Send Method=GET Value=users | Request successful | ✅ |", updatedMarkdown);
    Assert.Contains("| 12 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ✅ |", updatedMarkdown);
    Assert.Contains("| 13 | Inspect content | Action=VerifyContent File=users.json | Should be identical | ✅ |", updatedMarkdown);
    Assert.Contains("| 14 | Contains Alice |", updatedMarkdown);
    Assert.Contains("| 15 | Retain ID of Alice ID |", updatedMarkdown);

    // Verify second table is updated correctly (with failures)
    Assert.Contains("| 21 | Get Alice Details | Action=Send Method=GET Value=users/{@@AliceId@@} | Request successful | ✅ |", updatedMarkdown);
    Assert.Contains("| 22 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ❌ Expected 200 but got 404 |", updatedMarkdown);
    Assert.Contains("| 23 | Inspect content | Action=VerifyContent File=alice.json | Should be identical | ❌ File not found |", updatedMarkdown);
    Assert.Contains("| 24 | Verify Name | Action=CheckContent Path=$.name Value=Alice | Name is Alice | ❌ Name mismatch |", updatedMarkdown);
    Assert.Contains("| 25 | Verify Age | Action=CheckContent Path=$.age Value=20 | Age is 20 | ❌ Age mismatch |", updatedMarkdown);
  }
}