[![build](https://github.com/thomasduft/tapir/actions/workflows/build.yaml/badge.svg)](https://github.com/thomasduft/tapir/actions/workflows/build.yaml)
[![NuGet](https://img.shields.io/nuget/vpre/tomware.tapir.svg)](https://www.nuget.org/packages/tomware.tapir)

# tapir

A command-line tool for managing and executing automated API test cases.

## Introduction

`tapir` is a command-line tool designed to simplify the management and execution of automated API test cases. With `tapir`, you can easily create, organize, and run tests for your APIs, ensuring they function as expected and meet quality standards.

### Why tapir?

Traditional API testing tools like [Postman](https://www.postman.com/) and [Insomnia](https://insomnia.rest/) are great, but they often fall short when it comes to:
- **Integration with development workflows** - They're standalone applications that don't fit naturally into your CI/CD pipeline
- **Complete test scenarios** - Most tools execute one request at a time, making it cumbersome to test multi-step workflows
- **Version control & documentation** - Test collections aren't always easy to review, version, or document alongside your code

`tapir` solves these problems by using **Markdown-based test cases** that are:
- ✅ Human-readable and self-documenting
- ✅ Easy to version control alongside your code
- ✅ Simple to run from the command line or CI/CD pipelines => it's just a CLI - dotnet global - tool
- ✅ Capable of executing complete multi-step test scenarios

### Key Features

- 📝 **Markdown Test Cases** - Define your API tests in simple, readable Markdown files
- 🔗 **Multi-Step Scenarios** - Chain multiple API calls together, extract values from responses, and use them in subsequent requests
- 🔍 **Smart Validation** - Verify status codes, response content, and JSON values using JsonPath expressions
- 💾 **Variable Storage** - Extract and reuse data between test steps (e.g., retrieve an ID from one endpoint and use it in another)
- 📊 **Clear Results** - Get detailed test execution reports showing exactly what passed or failed

## Quick Example

Here's a glimpse of what a tapir test case looks like:

```markdown
# TC-Users-001: List all Users

- **Date**: 2025-11-05
- **Author**: name-of-author
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description

Tests the Users API by first retrieving all users, verifying the response contains Alice, and extracting her ID for subsequent operations.

It then uses Alice's ID to fetch her individual user details and validates the returned data matches expected values (name: Alice, age: 20). This test demonstrates both list and detail API endpoints while showcasing variable extraction and reuse between test steps.

## Preconditions

- no pre-conditions

## Steps

| Step ID  | Description         | Expected Result        | Test Data                                                         | Actual Result |
| -------: | ------------------- | ---------------------- | ----------------------------------------------------------------- | ------------- |
| 01       | Call users api      | Request successful     | Action=Send Method=GET Endpoint=users                             | -             |
| 02       | Check response code | 200                    | Action=CheckStatusCode Value=200                                  | -             |
| 03       | Inspect content     | Should be identical    | Action=VerifyContent File=samples/Users/Definitions/users.json    | -             |
| 04       | Check Alice         | Content contains Alice | Action=CheckContent JsonPath=$[?@.name=="Alice"].name Value=Alice | -             |
| 05       | Retain ID of Alice  | ID of Alice stored     | Action=StoreVariable JsonPath=$[?@.name=="Alice"].id Name=AliceId | -             |

| Step ID  | Description         | Expected Result        | Test Data                                                         | Actual Result |
| -------: | ------------------- | ---------------------- | ----------------------------------------------------------------- | ------------- |
| 11       | Get Alice details   | Request successful     | Action=Send Method=GET Endpoint=users/@@AliceId@@                 | -             |
| 12       | Check response code | 200                    | Action=CheckStatusCode Value=200                                  | -             |
| 13       | Inspect content     | Should be identical    | Action=VerifyContent File=samples/Users/Definitions/alice.json    | -             |
| 14       | Check name          | Name is Alice          | Action=CheckContent JsonPath=$.name Value=Alice                   | -             |
| 15       | Check age           | Age is 30              | Action=CheckContent JsonPath=$.age Value=30                       | -             |

## Postcondition

- no post-conditions

```

This test case:
1. Fetches a list of users from the API
2. Verifies the response contains a user named "Alice"
3. Extracts Alice's ID and stores it in a variable
4. Uses that ID to fetch Alice's detailed information
5. Validates the returned data

Run it with: `tapir run https://your-api.com -tc TC-Users-001`

## Usage

```bash
A command-line tool for managing and executing automated api test cases.

Usage: tapir [command] [options]

Options:
  -?|-h|--help  Show help information.

Commands:
  man           Displays a man page that helps writing the Test-Data syntax for a Test Case.
  new           Creates a new Test Case definition (i.e. test-case TC-Audit-001 "My TestCase Title").
  new-step      Appends a new Test Step to an existing Test Case definition (i.e. new-step TC-Audit-001).
  run           Runs Test Case definition (i.e. "https://localhost:5001" -tc TC-Audit-001).
  validate      Validates a Test Case definition (i.e. TC-Audit-001).

Run 'tapir [command] -?|-h|--help' for more information about a command.
```
