# Manual

A command-line tool for managing and executing automated api test cases.

## Description

`tapir` is a command-line tool designed to simplify the management and execution of automated API test cases.
With `tapir` you can create, organize, and run tests for your APIs, ensuring they function as expected.

## Domain Model

### TestCase

Represents a complete test case with metadata and test steps.

**Key Properties:**

- `Id`: Unique identifier (e.g., TC-Users-001)
- `Title`: Human-readable test case title
- `Module`: Logical grouping/module name
- `Type`: Either "Definition" or "Run"
- `Status`: "Passed", "Failed", or "Unknown"
- `Tables`: Collection of test step tables
- `Domain`: Base URL for API calls
- `Variables`: Dictionary of variables for substitution

### TestStep

Represents a single step in a test case.

**Key Properties:**

- `Id`: Sequential step number
- `Description`: Human-readable description
- `TestData`: Configuration data (Action=Send Method=GET Endpoint=users)
- `ExpectedResult`: Expected outcome description
- `IsSuccess`: Execution result flag

### TestStepInstruction

Parsed representation of a test step's test data.

**Supported Parameters:**

- `Action`: The action to perform (Send, AddHeader, CheckContent, etc.)
- `Method`: HTTP method (GET, POST, PUT, DELETE, PATCH)
- `Endpoint`: API endpoint path
- `Domain`: The domain to send the request to (will override the global domain and be prepended to the endpoint)
- `Name`: Header/parameter name
- `Value`: Header/parameter/content value
- `File`: File path for content or verification
- `JsonPath`: JSON path expression for extraction or verification
- `ContentType`: Content type (application/json, text/plain, multipart/form-data)

### Available Commands

**1. new** - Create a new test case definition

```bash
tapir new TC-Example-001 "My Test Title"
```

**2. validate** - Validate test case syntax

```bash
tapir validate TC-Example-001 -i ./tests
```

**3. run** - Execute test cases

```bash
tapir run https://api.example.com -tc TC-Example-001 -i ./tests -o ./results
```

**4. man** - Display action reference

```bash
tapir man
```

### Variable System

Variables enable request chaining by storing values from responses and using them in subsequent requests.

#### Variable Syntax

Variables use the `@@VariableName@@` syntax:

```markdown
| 05 | Store Alice's ID | Action=StoreVariable JsonPath=$[?@.name=="Alice"].id Name=AliceId | ID stored | - |
| 06 | Get Alice details | Action=Send Method=GET Endpoint=users/@@AliceId@@ | Request sent | - |
```

#### Variable Substitution

Variables are replaced in:

- `Value` fields
- `Endpoint` fields
- `JsonPath` expressions
- `Domain` fields

#### Variable Storage

Variables are stored using the `StoreVariable` action:

- Extract from JSON responses using JsonPath
- Store with a named key
- Available for all subsequent steps in the test case

### Test Case File Format

Test cases are written in Markdown with specific metadata:

```markdown
# TC-ID-001: Test Case Title

- **Date**: YYYY-MM-DD
- **Author**: author-name
- **Test Priority**: High|Medium|Low
- **Module**: ModuleName
- **Type**: Definition|Run
- **Status**: Passed|Failed|Unknown

## Description
Detailed description of what the test validates.

## Preconditions
- Any setup requirements

## Steps

| Step ID | Description | Test Data | Expected Result | Actual Result |
| ------: | ----------- | --------- | --------------- | ------------- |
| 01 | Step description | Action=Send Method=GET Endpoint=api/endpoint | Expected | - |

## Postcondition
- Any cleanup or final state
```

### Observability

tapir includes OpenTelemetry instrumentation for monitoring test execution.

#### Enable OTLP Export

```bash
tapir run https://api.example.com -tc TC-Example-001 --otel-endpoint http://localhost:4317
```

## Action Reference

`tapir` supports the following actions for Test Steps:

### AddContent

Adds content to the HTTP request.

**Supported Properties:**

- ContentType: Content type (e.g., text/plain, application/json, multipart/form-data)
- File: Path to the content file
- Name: Name for multipart/form-data content
- Value: Direct content value or corresponding value for multipart/form-data name (aka key)

### AddHeader

Adds headers to the HTTP request.

**Supported Properties:**

- Name: The name of the header
- Value: The value of the header

### AddQueryParameter

Adds query parameters to the HTTP request.

**Supported Properties:**

- Name: The name of the query parameter
- Value: The value of the query parameter

### CheckContent

Checks content in the HTTP response.

**Supported Properties:**

- ContentType: Content type (e.g., text/plain, application/json)
- JsonPath: JSON path to the content to check
- Value: The content value to check

### CheckContentHeader

Checks headers in the HTTP response.

**Supported Properties:**

- Name: The name of the header to check
- Value: The value of the header to check

### CheckReasonPhrase

Checks the reason phrase in the HTTP response.

**Supported Properties:**

- Value: The reason phrase to check

### CheckStatusCode

Checks the status code in the HTTP response.

**Supported Properties:**

- Value: The status code to check

### LogResponseContent

Logs the HTTP response content.

### Send

Sends an HTTP request.

**Supported Properties:**

- Method: The HTTP method to use (e.g. GET, POST, PUT, DELETE, PATCH)
- Domain: The domain to send the request to (will override the global domain and be prepended to the endpoint)
- Endpoint: The endpoint to send the request to

### StoreVariable

Stores a variable from the HTTP response. Enables request chaining.

**Supported Properties:**

- Name: The name of the variable to store
- JsonPath: The JSON path to the variable to store

### VerifyContent

Verifies content in the HTTP response.

**Supported Properties:**

- File: The file path to the content to verify
- Value: The content value to verify
