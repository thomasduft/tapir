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

    // Parse the table rows
    var tableRows = ExtractTableRows(stepsSection);
    foreach (var row in tableRows)
    {
      var testStep = ParseTableRow(row);
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

    // Process all tables in the document
    for (int i = 0; i < lines.Length; i++)
    {
      var line = lines[i].Trim();

      if (line.StartsWith("|") && line.EndsWith("|"))
      {
        if (!inTable && IsValidStepsTableHeader(line))
        {
          inTable = true;
        }
        else if (inTable && !IsSeparatorRow(line))
        {
          // This is a data row in our table
          var updatedRow = UpdateTableRowWithResults(line, testResults);
          lines[i] = updatedRow;
        }
      }
      else if (inTable)
      {
        // End of current table, continue looking for more tables
        inTable = false;
      }
    }

    return string.Join('\n', lines);
  }

  /// <summary>
  /// Extracts test steps by finding markdown tables with the expected column structure
  /// </summary>
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

  /// <summary>
  /// Checks if a table header row matches the expected structure for test steps
  /// </summary>
  private static bool IsValidStepsTableHeader(string headerLine)
  {
    // Expected header should contain these key columns
    var normalizedHeader = headerLine.ToLowerInvariant();
    return normalizedHeader.Contains("step") &&
           normalizedHeader.Contains("description") &&
           normalizedHeader.Contains("expected") &&
           (normalizedHeader.Contains("test data") || normalizedHeader.Contains("testdata"));
  }

  /// <summary>
  /// Extracts table rows from the steps section, excluding the header and separator rows
  /// </summary>
  private List<string> ExtractTableRows(string stepsSection)
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

  /// <summary>
  /// Checks if a line is a markdown table separator row
  /// </summary>
  private static bool IsSeparatorRow(string line)
  {
    // Remove outer pipes and check if content only contains allowed separator characters
    var content = line.Trim('|', ' ');
    return Regex.IsMatch(content, @"^[\s\-:|]+$");
  }

  /// <summary>
  /// Parses a single table row into a TestStep object
  /// </summary>
  private TestStep? ParseTableRow(string row)
  {
    try
    {
      var cells = ParseTableCells(row);

      // Ensure we have at least 4 cells (Step ID, Description, Test Data, Expected Result)
      // The 5th cell (Actual Result) is optional
      if (cells.Count < 4)
      {
        return null;
      }

      var testStep = new TestStep();

      // Parse Step ID (first cell)
      if (int.TryParse(cells[0].Trim(), out var stepId))
      {
        testStep.Id = stepId;
      }
      else
      {
        return null; // Invalid step ID
      }

      // Parse Description (second cell)
      testStep.Description = UnescapeMarkdown(cells[1].Trim());

      // Parse Test Data (third cell)
      testStep.TestData = UnescapeMarkdown(cells[2].Trim());

      // Parse Expected Result (fourth cell)
      testStep.ExpectedResult = UnescapeMarkdown(cells[3].Trim());

      // Parse Actual Result (fifth cell, optional)
      if (cells.Count > 4)
      {
        var actualResult = cells[4].Trim();
        // Check for success/failure indicators
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

  /// <summary>
  /// Parses table cells from a row, handling escaped pipes within cell content
  /// </summary>
  private List<string> ParseTableCells(string row)
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

  /// <summary>
  /// Updates a single table row with test execution results
  /// </summary>
  private string UpdateTableRowWithResults(string row, IEnumerable<TestStepResult> testResults)
  {
    try
    {
      var cells = ParseTableCells(row);

      // Ensure we have at least 5 cells (including Actual Result column)
      while (cells.Count < 5)
      {
        cells.Add(" - ");
      }

      // Parse Step ID (first cell)
      if (int.TryParse(cells[0].Trim(), out var stepId))
      {
        // Find matching test result
        var testResult = testResults
          .FirstOrDefault(r => r.TestStepId == stepId);

        if (testResult == null)
        {
          // No failure found, mark as success
          cells[4] = " - ";
        }
        else
        {
          // Update the Actual Result column (5th cell, index 4)
          cells[4] = testResult.IsSuccess
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

  /// <summary>
  /// Unescapes markdown content, particularly handling escaped quotes and special characters
  /// </summary>
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