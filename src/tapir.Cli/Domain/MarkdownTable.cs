using System.Text.RegularExpressions;

namespace tomware.Tapir.Cli.Domain;

/// <summary>
/// A lightweight markdown table parser specifically designed for parsing test steps
/// from markdown tables in the format used by TestR test cases.
/// </summary>
internal class MarkdownTable
{
  private readonly string _content;

  public MarkdownTable(string content)
  {
    _content = content;
  }

  // Holds the column index of each logical field as determined by the header row.
  // ActualResult may be -1 if no "Actual Result" column exists in the header.
  private record ColumnIndices(
    int StepId,
    int Description,
    int TestData,
    int ExpectedResult,
    int ActualResult
  );

  /// <summary>
  /// Parses test steps from markdown tables.
  /// Expected table format:
  /// | Step ID | Description | Test Data | Expected Result | Actual Result |
  /// | -------:| ----------- | --------- | --------------- | ------------- |
  /// | 1       | step desc   | test data | expected result | actual result |
  /// </summary>
  /// <returns>Collection of parsed test steps</returns>
  public IEnumerable<TestStep> ParseTestSteps()
  {
    var testSteps = new List<TestStep>();

    // Find the test steps section between comments
    var stepsSection = ExtractStepsSectionFromTables();
    if (string.IsNullOrEmpty(stepsSection))
    {
      return testSteps;
    }

    // Determine column positions from the header row
    var headerLine = stepsSection
      .Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .FirstOrDefault(l => l.Trim().StartsWith("|") && l.Trim().EndsWith("|"));
    if (headerLine == null)
      return testSteps;

    var columnIndices = ParseColumnIndices(headerLine.Trim());
    if (columnIndices == null)
      return testSteps;

    // Parse the table rows using detected column positions
    var tableRows = ExtractTableRows(stepsSection);
    foreach (var row in tableRows)
    {
      var testStep = ParseTableRow(row, columnIndices);
      if (testStep != null)
      {
        testSteps.Add(testStep);
      }
    }

    return testSteps.OrderBy(ts => ts.Id);
  }

  /// <summary>
  /// Extracts all markdown tables from the content as separate sections
  /// </summary>
  /// <returns>Collection of markdown table sections, each containing one complete table</returns>
  public IEnumerable<string> ExtractAllTableSections()
  {
    var tables = new List<string>();
    var lines = _content.Split('\n');
    var currentTable = new List<string>();
    var inTable = false;

    for (int i = 0; i < lines.Length; i++)
    {
      var line = lines[i].Trim();

      // Check if this line looks like a table row
      if (line.StartsWith("|") && line.EndsWith("|"))
      {
        if (!inTable)
        {
          // Check if this looks like our expected header
          if (IsValidStepsTableHeader(line))
          {
            inTable = true;
            currentTable.Clear();
            currentTable.Add(line);
          }
        }
        else
        {
          // We're already in a table, add this row
          currentTable.Add(line);
        }
      }
      else if (inTable)
      {
        // We were in a table but this line doesn't look like a table row
        // End the current table and start a new one
        if (currentTable.Count > 0)
        {
          tables.Add(string.Join("\n", currentTable));
          currentTable.Clear();
        }
        inTable = false;
      }
    }

    // Don't forget the last table if the content ends while still in a table
    if (inTable && currentTable.Count > 0)
    {
      tables.Add(string.Join("\n", currentTable));
    }

    return tables;
  }

  /// <summary>
  /// Updates the "Actual Result" column in test steps table with test execution results
  /// </summary>
  /// <param name="testResults">Collection of test step results to apply</param>
  /// <returns>Updated markdown content with results applied to all tables</returns>
  public string UpdateTestStepsWithResults(IEnumerable<TestStepResult> testResults)
  {
    var lines = _content.Split('\n');
    var inTable = false;
    ColumnIndices? columnIndices = null;

    // Process all tables in the document
    for (int i = 0; i < lines.Length; i++)
    {
      var line = lines[i].Trim();

      if (line.StartsWith("|") && line.EndsWith("|"))
      {
        if (!inTable && IsValidStepsTableHeader(line))
        {
          inTable = true;
          columnIndices = ParseColumnIndices(line);
        }
        else if (inTable && !IsSeparatorRow(line) && columnIndices != null)
        {
          // This is a data row in our table
          var updatedRow = UpdateTableRowWithResults(line, testResults, columnIndices);
          lines[i] = updatedRow;
        }
      }
      else if (inTable)
      {
        // End of current table, continue looking for more tables
        inTable = false;
        columnIndices = null;
      }
    }

    return string.Join('\n', lines);
  }

  private string ExtractStepsSectionFromTables()
  {
    var lines = _content.Split('\n');
    var result = new List<string>();
    var inTable = false;
    var foundValidHeader = false;

    for (int i = 0; i < lines.Length; i++)
    {
      var line = lines[i].Trim();

      // Check if this line looks like a table row
      if (line.StartsWith("|") && line.EndsWith("|"))
      {
        if (!inTable)
        {
          // Check if this looks like our expected header
          if (IsValidStepsTableHeader(line))
          {
            foundValidHeader = true;
            inTable = true;
            result.Add(line);
          }
        }
        else
        {
          // We're already in a table, add this row
          result.Add(line);
        }
      }
      else if (inTable)
      {
        // We were in a table but this line doesn't look like a table row
        // End the table extraction
        break;
      }
    }

    return foundValidHeader ? string.Join("\n", result) : string.Empty;
  }

  private static bool IsValidStepsTableHeader(string headerLine)
  {
    // Expected header should contain these key columns
    var normalizedHeader = headerLine.ToLowerInvariant();
    return normalizedHeader.Contains("step") &&
           normalizedHeader.Contains("description") &&
           normalizedHeader.Contains("expected") &&
           (normalizedHeader.Contains("test data") || normalizedHeader.Contains("testdata"));
  }

  private static ColumnIndices? ParseColumnIndices(string headerLine)
  {
    var cells = ParseTableCells(headerLine);
    int stepId = -1, description = -1, testData = -1, expectedResult = -1, actualResult = -1;

    for (int i = 0; i < cells.Count; i++)
    {
      var cell = cells[i].Trim().ToLowerInvariant();
      if (cell.Contains("step")) stepId = i;
      if (cell.Contains("description")) description = i;
      if (cell.Contains("test data") || cell.Contains("testdata")) testData = i;
      if (cell.Contains("expected")) expectedResult = i;
      if (cell.Contains("actual")) actualResult = i;
    }

    if (stepId < 0 || description < 0 || testData < 0 || expectedResult < 0)
      return null;

    return new ColumnIndices(stepId, description, testData, expectedResult, actualResult);
  }

  private static List<string> ExtractTableRows(string stepsSection)
  {
    var lines = stepsSection.Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .Select(line => line.Trim())
      .Where(line => !string.IsNullOrEmpty(line))
      .ToList();

    var tableRows = new List<string>();
    var foundHeader = false;
    var foundSeparator = false;

    foreach (var line in lines)
    {
      // Check if this is a table row (starts and ends with |)
      if (!line.StartsWith("|") || !line.EndsWith("|"))
      {
        continue;
      }

      // Skip the header row (first table row we encounter)
      if (!foundHeader)
      {
        foundHeader = true;
        continue;
      }

      // Skip the separator row (contains only |, -, :, and spaces)
      if (!foundSeparator && IsSeparatorRow(line))
      {
        foundSeparator = true;
        continue;
      }

      // This is a data row
      if (foundSeparator)
      {
        tableRows.Add(line);
      }
    }

    return tableRows;
  }

  private static bool IsSeparatorRow(string line)
  {
    // Remove outer pipes and check if content only contains allowed separator characters
    var content = line.Trim('|', ' ');
    return Regex.IsMatch(content, @"^[\s\-:|]+$");
  }

  private static TestStep? ParseTableRow(string row, ColumnIndices columnIndices)
  {
    try
    {
      var cells = ParseTableCells(row);

      var requiredCount = new[] {
        columnIndices.StepId,
        columnIndices.Description,
        columnIndices.TestData,
        columnIndices.ExpectedResult
      }.Max() + 1;

      if (cells.Count < requiredCount)
      {
        return null;
      }

      var testStep = new TestStep();

      // Parse Step ID (first cell)
      if (int.TryParse(cells[columnIndices.StepId].Trim(), out var stepId))
      {
        testStep.Id = stepId;
      }
      else
      {
        return null; // Invalid step ID
      }

      // Parse Description (second cell)
      testStep.Description = UnescapeMarkdown(cells[columnIndices.Description].Trim());

      // Parse Test Data (third cell)
      testStep.TestData = UnescapeMarkdown(cells[columnIndices.TestData].Trim());

      // Parse Expected Result (fourth cell)
      testStep.ExpectedResult = UnescapeMarkdown(cells[columnIndices.ExpectedResult].Trim());

      // Parse Actual Result (column position from header, optional)
      if (columnIndices.ActualResult >= 0 && cells.Count > columnIndices.ActualResult)
      {
        var actualResult = cells[columnIndices.ActualResult].Trim();
        testStep.IsSuccess = actualResult.Contains("✅");
      }

      return testStep;
    }
    catch
    {
      // If parsing fails for any reason, return null
      return null;
    }
  }

  private static List<string> ParseTableCells(string row)
  {
    var cells = new List<string>();
    var currentCell = string.Empty;

    // Remove leading and trailing pipes
    var content = row.Trim();
    if (content.StartsWith("|"))
    {
      content = content.Substring(1);
    }
    if (content.EndsWith("|"))
    {
      content = content.Substring(0, content.Length - 1);
    }

    for (int i = 0; i < content.Length; i++)
    {
      var c = content[i];

      if (c == '\\' && i + 1 < content.Length)
      {
        // Handle escaped characters
        var nextChar = content[i + 1];
        if (nextChar == '|' || nextChar == '\\')
        {
          currentCell += nextChar;
          i++; // Skip the next character
          continue;
        }
      }

      if (c == '|')
      {
        // Cell separator
        cells.Add(currentCell);
        currentCell = string.Empty;
      }
      else
      {
        currentCell += c;
      }
    }

    // Add the last cell
    cells.Add(currentCell);

    return cells;
  }
  private static string UpdateTableRowWithResults(
    string row,
    IEnumerable<TestStepResult> testResults,
    ColumnIndices columnIndices
  )
  {
    try
    {
      var cells = ParseTableCells(row);

      // Determine where to write the Actual Result
      var actualResultIndex = columnIndices.ActualResult >= 0
        ? columnIndices.ActualResult
        : new[] {
            columnIndices.StepId,
            columnIndices.Description,
            columnIndices.TestData,
            columnIndices.ExpectedResult
          }.Max() + 1;

      // Ensure enough cells exist up to the Actual Result column
      while (cells.Count <= actualResultIndex)
      {
        cells.Add(" - ");
      }

      // Parse Step ID (first cell)
      if (int.TryParse(cells[columnIndices.StepId].Trim(), out var stepId))
      {
        // Find matching test result
        var testResult = testResults
          .FirstOrDefault(r => r.TestStepId == stepId);

        if (testResult == null)
        {
          // No failure found, mark as success
          cells[actualResultIndex] = " - ";
        }
        else
        {
          // Update the Actual Result column (5th cell, index 4)
          cells[actualResultIndex] = testResult.IsSuccess
            ? " ✅ "
            : $" ❌ {testResult.Error} ";
        }
      }

      // Reconstruct the row
      return $"| {string.Join(" | ", cells.Select(c => c.Trim()))} |";
    }
    catch
    {
      // If parsing fails, return original row
      return row;
    }
  }

  private static string UnescapeMarkdown(string content)
  {
    if (string.IsNullOrEmpty(content))
    {
      return content;
    }

    // Handle HTML entities that might be present
    content = content
      .Replace("&quot;", "\"")
      .Replace("&amp;", "&")
      .Replace("&lt;", "<")
      .Replace("&gt;", ">")
      .Replace("&nbsp;", " ");

    return content;
  }
}
