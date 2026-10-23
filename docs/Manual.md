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
- `Selector`: Expression (JsonPath or XPath) for extraction or verification
- `ContentType`: Content type (application/json, application/xml, text/plain, multipart/form-data)

### Available Commands

**1. list** - Lists all Test Case definitions from a given directory
```bash
tapir list ./test-cases
```

**2. new** - Create a new test case definition
```bash
tapir new TC-Example-001 "My Test Title"
```

**3. new-step** - Appends a new test step to an existing test case definition
```bash
tapir new-step TC-Example-001
```

**4. validate** - Validate test case syntax
```bash
tapir validate TC-Example-001 -i ./tests
```

**5. run** - Execute test cases
```bash
tapir run https://api.example.com -tc TC-Example-001 -i ./tests -o ./results
```

Omit `-tc` to run all definitions found recursively under `-i`. Execution stops
at the first failed table or test case by default. With `--continue-on-failure`,
remaining request tables and test cases are attempted, but the command still
returns `1` if any fail. It returns `0` only if all cases pass. An empty test
suite or a definition with no request tables returns `1`.

Each request table is validated immediately before execution, after variables
from earlier responses are available. Invalid tables do not send HTTP requests;
validation errors are recorded in run output when `-o` is specified. Continuing
after a failure may cause later tables to fail if required variables were not
extracted. Cancellation always stops execution, even with `--continue-on-failure`.

HTTPS server certificates are verified by default. Untrusted, expired, or
hostname-mismatched certificates cause the run to fail. Prefer trusting your local
development certificate (e.g. `dotnet dev-certs https --trust` where supported).

For local development only, use `--insecure` to disable certificate verification:

```bash
tapir run https://localhost:5001 -tc TC-Example-001 -i ./tests --insecure
```

This option logs a warning and applies to all requests in the run, including
steps with a `Domain` override. Do not use it against production systems.

**6. report** - Generates a self-contained HTML report from Test Case run output files
```bash
tapir report -i ./results -o ./results/report.html
```

**7. man** - Display action reference
```bash
tapir man
```

### Variable System

Variables enable request chaining by storing values from responses and using them in subsequent requests.

#### Variable Syntax

Variables use the `@@VariableName@@` syntax:

```markdown
| 05 | Store Alice's ID  | ID stored    | Action=StoreVariable Selector=$[?@.name=="Alice"].id Name=AliceId | - |
| 06 | Get Alice details | Request sent | Action=Send Method=GET Endpoint=users/@@AliceId@@                 | - |
```

#### Variable Substitution

Variables are replaced in:
- `Value` fields
- `File` fields
- `Selector` expressions
- `Endpoint` fields
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

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 01 | Step description | Expected | Action=Send Method=GET Endpoint=api/endpoint | - |

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
- ContentType: Content type (e.g., text/plain, application/json, application/xml, application/x-www-form-urlencoded, multipart/form-data)
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
- ContentType: Content type (e.g., text/plain, application/json, application/xml)
- Selector: JSON or XPath for the content to check
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


### SaveContent

Saves the HTTP response content to a file.

**Supported Properties:**
- File: Path of the file the response content is saved to

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
- Selector: The JSON or XPath for the variable to store

### VerifyContent

Verifies content in the HTTP response.

**Supported Properties:**
- File: The file path to the content to verify
- Value: The content value to verify
