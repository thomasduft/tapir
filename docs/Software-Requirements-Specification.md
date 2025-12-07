# Tapir Software Requirements Specification

## Introduction

This software requirements specification (SRS) describes what the software will do and how it will be expected to perform. It also describes the functionality the product needs to fulfill the needs of all stakeholders (business, users).

Weights used to categorize the requirements:

- **Shall**: Indicates a mandatory requirement that must be implemented to meet the system's objectives.
- **Should**: Represents a highly desirable requirement that is not mandatory but would significantly enhance the system if included.
- **May**: Denotes an optional requirement that could be implemented if resources and time allow, but is not critical to the system's success.

## Customer Problems or Needs

For doing API Tests different tools like Postman, Insomnia etc. exist. They even allow to store collections and reuse them.

However, these tools are often standalone applications that do not integrate well with existing development workflows. On top of that it is hard to get their setup and usually they only execute one specific request at a time.

What would be needed is something where API tests can be easily integrated into the development workflow which are described and already documented in a manner where one would execute a complete scenario aka Test Case (i.e. creating a new document and if successfully created as well deleting it again).

## Stakeholders

|  Id   | Stakeholder    | Role            |
| ----- | -------------- | --------------- |
| SH01  | Thomas Duft    | Project Sponsor |

## Overall description

`tapir` is a command-line tool designed to simplify the management and execution of automated API test cases. With `tapir, you can easily create, organize, and run tests for your APIs, ensuring they function as expected and meet quality standards.

A test case in its core is a markdown document that describes the API request and the expected response which can be read and executed by tapir.

## Use case specification

- UC-01 Create a new API test case
- UC-02 Validate an existing API test case
- UC-03 Execute an existing API test case
- UC-04 Generate a report for API test case run

## Functional Requirements

| ID     | Name       | Description    | Weight | Stakeholder    |
| ------ | -----------| ---------------| ------ | -------------- |
| FR-001 | CLI Command Execution | The system shall allow users to execute CLI commands for managing Tapir projects. | Shall | SH01 |
| FR-002 | Test Case Templates   | The system shall provide templates for generating test cases and documentation. | Shall | SH01 |
| FR-003 | Configurability       | The system shall support configuration via command-line arguments. | Shall | SH01 |
| FR-004 | Input Validation      | The system shall validate user input and provide meaningful error messages. | Shall | SH01 |
| FR-005 | Test Case Reading     | The system shall read and parse test case files in the specified format which allows tapir to extract all the required information in order to execute the API test case. | Shall | SH01 |
| FR-006 | Support for variables | The system shall support passing in variables that will be resolved at runtime for configuration and security reasons. | Should | SH01 |
| FR-007 | Request Examination | The system shall examine API responses for correctness and be able to lookup values in the response that can be fed into subsequent requests. | Shall | SH01 |

## Non Functional Requirements

| ID     | Name       | Description    | Weight | Stakeholder    |
| ------ | -----------| ---------------| ------ | -------------- |
| NFR-001 | Compatibility   | The system shall be compatible with .NET 9.0 and run on Linux environments. | Shall | SH01 |
| NFR-002 | Performance     | The system shall provide fast response times for CLI operations (less than 2 seconds for typical commands). | Shall | SH01 |
| NFR-003 | Maintainability | The system shall be maintainable and follow clean code practices. | Shall | SH01 |
| NFR-004 | Logging         | The system shall log errors and important events for troubleshooting. | Should | SH01 |
