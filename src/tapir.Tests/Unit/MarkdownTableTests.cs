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
| 21       | Get Alice Details       | Action=Send Method=GET Value=users/@@AliceId@@                 | Request successful     | -             |
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
    Assert.Contains("| 21 | Get Alice Details | Action=Send Method=GET Value=users/@@AliceId@@ | Request successful | ✅ |", updatedMarkdown);
    Assert.Contains("| 22 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ❌ Expected 200 but got 404 |", updatedMarkdown);
    Assert.Contains("| 23 | Inspect content | Action=VerifyContent File=alice.json | Should be identical | ❌ File not found |", updatedMarkdown);
    Assert.Contains("| 24 | Verify Name | Action=CheckContent Path=$.name Value=Alice | Name is Alice | ❌ Name mismatch |", updatedMarkdown);
    Assert.Contains("| 25 | Verify Age | Action=CheckContent Path=$.age Value=20 | Age is 20 | ❌ Age mismatch |", updatedMarkdown);
  }

  [Fact]
  public void ParseTestSteps_WithEmptyContent_ShouldReturnEmptyList()
  {
    // Arrange
    var markdown = string.Empty;
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Empty(testSteps);
  }

  [Fact]
  public void ParseTestSteps_WithNoValidTable_ShouldReturnEmptyList()
  {
    // Arrange
    var markdown = @"
# Test Case

Some text without a table.

## Description

More text but no table structure.
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Empty(testSteps);
  }

  [Fact]
  public void ParseTestSteps_WithMissingRequiredColumns_ShouldReturnEmptyList()
  {
    // Arrange
    var markdown = @"
| Step ID | Description |
| ------: | ----------- |
| 1       | Some step   |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Empty(testSteps);
  }

  [Fact]
  public void ParseTestSteps_WithInvalidStepId_ShouldSkipInvalidRow()
  {
    // Arrange
    var markdown = @"
| Step ID | Description      | Test Data    | Expected Result | Actual Result |
| ------: | ---------------- | ------------ | --------------- | ------------- |
| 1       | Valid step       | Some data    | Expected        | -             |
| invalid | Invalid step ID  | Some data    | Expected        | -             |
| 2       | Another valid    | More data    | Expected        | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Equal(2, testSteps.Count);
    Assert.Equal(1, testSteps[0].Id);
    Assert.Equal(2, testSteps[1].Id);
  }

  [Fact]
  public void ParseTestSteps_WithEscapedPipes_ShouldHandleCorrectly()
  {
    // Arrange
    var markdown = @"
| Step ID | Description      | Test Data                        | Expected Result | Actual Result |
| ------: | ---------------- | -------------------------------- | --------------- | ------------- |
| 1       | Test with pipe   | Value=""data\|with\|pipes""      | Success         | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Single(testSteps);
    Assert.Contains("data|with|pipes", testSteps[0].TestData);
  }

  [Fact]
  public void ParseTestSteps_WithHtmlEntities_ShouldUnescapeThem()
  {
    // Arrange
    var markdown = @"
| Step ID | Description      | Test Data              | Expected Result | Actual Result |
| ------: | ---------------- | ---------------------- | --------------- | ------------- |
| 1       | Test entities    | Value=&quot;test&quot; | Success         | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Single(testSteps);
    Assert.Contains("\"test\"", testSteps[0].TestData);
  }

  [Fact]
  public void ParseTestSteps_WithSuccessIndicator_ShouldSetIsSuccess()
  {
    // Arrange
    var markdown = @"
| Step ID | Description      | Test Data    | Expected Result | Actual Result |
| ------: | ---------------- | ------------ | --------------- | ------------- |
| 1       | Successful step  | Some data    | Expected        | ✅            |
| 2       | Failed step      | More data    | Expected        | ❌ Error      |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Equal(2, testSteps.Count);
    Assert.True(testSteps[0].IsSuccess);
    Assert.False(testSteps[1].IsSuccess);
  }

  [Fact]
  public void ParseTestSteps_WithUnorderedStepIds_ShouldReturnOrderedList()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 3       | Third step  | Data 3    | Result 3        | -             |
| 1       | First step  | Data 1    | Result 1        | -             |
| 2       | Second step | Data 2    | Result 2        | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Equal(3, testSteps.Count);
    Assert.Equal(1, testSteps[0].Id);
    Assert.Equal(2, testSteps[1].Id);
    Assert.Equal(3, testSteps[2].Id);
  }

  [Fact]
  public void ExtractAllTableSections_WithNoTables_ShouldReturnEmptyList()
  {
    // Arrange
    var markdown = @"
# Test Case

No tables here, just plain text.
";
    var table = new MarkdownTable(markdown);

    // Act
    var tableSections = table.ExtractAllTableSections().ToList();

    // Assert
    Assert.Empty(tableSections);
  }

  [Fact]
  public void ExtractAllTableSections_WithSingleTable_ShouldReturnOneSection()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var tableSections = table.ExtractAllTableSections().ToList();

    // Assert
    Assert.Single(tableSections);
  }

  [Fact]
  public void ExtractAllTableSections_WithNonStepsTable_ShouldIgnoreIt()
  {
    // Arrange
    var markdown = @"
| Column A | Column B |
| -------- | -------- |
| Data 1   | Data 2   |

| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var tableSections = table.ExtractAllTableSections().ToList();

    // Assert
    Assert.Single(tableSections);
    Assert.Contains("Step ID", tableSections[0]);
  }

  [Fact]
  public void ExtractAllTableSections_WithTextBetweenTables_ShouldReturnBothTables()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | First table | Data 1    | Result 1        | -             |

Some text between tables

| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 2       | Second table | Data 2   | Result 2        | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var tableSections = table.ExtractAllTableSections().ToList();

    // Assert
    Assert.Equal(2, tableSections.Count);
  }

  [Fact]
  public void UpdateTestStepsWithResults_WithEmptyResults_ShouldMarkAllAsSuccess()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |
| 2       | Step two    | Data 2    | Result 2        | -             |
";
    var table = new MarkdownTable(markdown);
    var testResults = new List<TestStepResult>();

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    Assert.Contains("| 1 | Step one | Data 1 | Result 1 | - |", updatedMarkdown);
    Assert.Contains("| 2 | Step two | Data 2 | Result 2 | - |", updatedMarkdown);
  }

  [Fact]
  public void UpdateTestStepsWithResults_WithAllSuccess_ShouldMarkAllWithCheckmark()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |
| 2       | Step two    | Data 2    | Result 2        | -             |
";
    var table = new MarkdownTable(markdown);
    var testResults = new List<TestStepResult>
    {
      TestStepResult.Success(new TestStep { Id = 1 }),
      TestStepResult.Success(new TestStep { Id = 2 })
    };

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    Assert.Contains("| 1 | Step one | Data 1 | Result 1 | ✅ |", updatedMarkdown);
    Assert.Contains("| 2 | Step two | Data 2 | Result 2 | ✅ |", updatedMarkdown);
  }

  [Fact]
  public void UpdateTestStepsWithResults_WithFailures_ShouldIncludeErrorMessages()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |
| 2       | Step two    | Data 2    | Result 2        | -             |
";
    var table = new MarkdownTable(markdown);
    var testResults = new List<TestStepResult>
    {
      TestStepResult.Success(new TestStep { Id = 1 }),
      TestStepResult.Failed(new TestStep { Id = 2 }, "Validation failed")
    };

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    Assert.Contains("| 1 | Step one | Data 1 | Result 1 | ✅ |", updatedMarkdown);
    Assert.Contains("| 2 | Step two | Data 2 | Result 2 | ❌ Validation failed |", updatedMarkdown);
  }

  [Fact]
  public void UpdateTestStepsWithResults_WithMissingActualResultColumn_ShouldAddIt()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result |
| ------: | ----------- | --------- | --------------- |
| 1       | Step one    | Data 1    | Result 1        |
";
    var table = new MarkdownTable(markdown);
    var testResults = new List<TestStepResult>
    {
      TestStepResult.Success(new TestStep { Id = 1 })
    };

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    Assert.Contains("| 1 | Step one | Data 1 | Result 1 | ✅ |", updatedMarkdown);
  }

  [Fact]
  public void UpdateTestStepsWithResults_WithNoMatchingResults_ShouldKeepOriginal()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |
";
    var table = new MarkdownTable(markdown);
    var testResults = new List<TestStepResult>
    {
      TestStepResult.Success(new TestStep { Id = 99 }) // Non-matching ID
    };

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    Assert.Contains("| 1 | Step one | Data 1 | Result 1 | - |", updatedMarkdown);
  }

  [Fact]
  public void UpdateTestStepsWithResults_PreservesNonTableContent()
  {
    // Arrange
    var markdown = @"# Test Case Title

## Description

This is a description.

| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Step one    | Data 1    | Result 1        | -             |

## Postcondition

Some postcondition text.
";
    var table = new MarkdownTable(markdown);
    var testResults = new List<TestStepResult>
    {
      TestStepResult.Success(new TestStep { Id = 1 })
    };

    // Act
    var updatedMarkdown = table.UpdateTestStepsWithResults(testResults);

    // Assert
    Assert.Contains("# Test Case Title", updatedMarkdown);
    Assert.Contains("## Description", updatedMarkdown);
    Assert.Contains("This is a description.", updatedMarkdown);
    Assert.Contains("## Postcondition", updatedMarkdown);
    Assert.Contains("Some postcondition text.", updatedMarkdown);
    Assert.Contains("| 1 | Step one | Data 1 | Result 1 | ✅ |", updatedMarkdown);
  }

  [Fact]
  public void ParseTestSteps_WithComplexTestData_ShouldPreserveContent()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Complex test | Action=Send Method=POST Path=/api/users Body={""name"":""John"",""age"":30} Headers=Content-Type:application/json | Status 201 | - |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Single(testSteps);
    var step = testSteps[0];
    Assert.Equal(1, step.Id);
    Assert.Equal("Complex test", step.Description);
    Assert.Contains("Body={\"name\":\"John\",\"age\":30}", step.TestData);
  }

  [Fact]
  public void ParseTestSteps_WithWhitespaceVariations_ShouldTrimCorrectly()
  {
    // Arrange
    var markdown = @"
| Step ID | Description          | Test Data      | Expected Result   | Actual Result |
| ------: | -------------------- | -------------- | ----------------- | ------------- |
| 1       |   Padded description |  Padded data   |  Padded result    | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Single(testSteps);
    Assert.Equal("Padded description", testSteps[0].Description);
    Assert.Equal("Padded data", testSteps[0].TestData);
    Assert.Equal("Padded result", testSteps[0].ExpectedResult);
  }

  [Fact]
  public void ParseTestSteps_WithEmptyCells_ShouldHandleGracefully()
  {
    // Arrange
    var markdown = @"
| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       |             |           |                 | -             |
| 2       | Valid step  | Some data | Expected        | -             |
";
    var table = new MarkdownTable(markdown);

    // Act
    var testSteps = table.ParseTestSteps().ToList();

    // Assert
    Assert.Equal(2, testSteps.Count);
    Assert.Equal(string.Empty, testSteps[0].Description);
    Assert.Equal("Valid step", testSteps[1].Description);
  }

  [Fact]
  public void ExtractAllTableSections_WithTableAtEndOfFile_ShouldIncludeIt()
  {
    // Arrange
    var markdown = @"# Test Case

| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 1       | Last table  | Data      | Result          | -             |";
    var table = new MarkdownTable(markdown);

    // Act
    var tableSections = table.ExtractAllTableSections().ToList();

    // Assert
    Assert.Single(tableSections);
    Assert.Contains("Last table", tableSections[0]);
  }
}