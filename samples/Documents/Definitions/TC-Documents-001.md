# TC-Documents-001: Upload and Retrieve Document

- **Date**: 2025-11-05
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Documents
- **Type**: Definition
- **Status**: Unknown

## Description

This test case verifies the complete lifecycle of a document in the system: uploading a PDF file with a title, retrieving it back to confirm the content type and size are correct, and then deleting it.

The test validates that the API returns proper HTTP status codes (201 for creation, 200 for retrieval, 204 for deletion) and that the downloaded document has the correct headers (application/pdf content type and expected file size of 12959 bytes).

## Preconditions

- The following Test Case variables must be set:
  - `@@FileName@@` must be set to a valid PDF file name (e.g., "empty.pdf") that exists in the specified input directory.

## Steps

| Step ID  | Description             | Expected Result    | Test Data                                                                                                                | Actual Result |
| -------: | ----------------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------ | ------------- |
| 01       | Add Document            | Document prepared  | Action=AddContent ContentType=multipart/form-data Name=file Value=empty.pdf File=samples/Documents/Definitions/@@FileName@@.pdf | -             |
| 02       | Set Document Title      | Document prepared  | Action=AddContent ContentType=multipart/form-data Name=title Value="Test Document"                                       | -             |
| 03       | Upload Document         | Request successful | Action=Send Method=POST Endpoint=documents                                                                               | -             |
| 04       | Check response code     | 201                | Action=CheckStatusCode Value=201                                                                                         | -             |
| 05       | Retain DocumentId       | ID of new document | Action=StoreVariable JsonPath=$ Name=DocumentId                                                                          | -             |

| Step ID  | Description             | Expected Result        | Test Data                                                         | Actual Result |
| -------: | ----------------------- | ---------------------- | ----------------------------------------------------------------- | ------------- |
| 11       | Retrieve the document   | Request successful     | Action=Send Method=GET Endpoint=documents/@@DocumentId@@/download | -             |
| 12       | Verify response code    | 200                    | Action=CheckStatusCode Value=200                                  | -             |
| 13       | Check Headers           | Header is present      | Action=CheckContentHeader Name=Content-Type Value=application/pdf | -             |
| 14       | Check Headers           | Header is present      | Action=CheckContentHeader Name=Content-Length Value=12959         | -             |

| Step ID  | Description             | Expected Result        | Test Data                                                         | Actual Result |
| -------: | ----------------------- | ---------------------- | ----------------------------------------------------------------- | ------------- |
| 21       | Delete the document     | Request successful     | Action=Send Method=DELETE Endpoint=documents/@@DocumentId@@       | -             |
| 22       | Check response code     | 204                    | Action=CheckStatusCode Value=204                                  | -             |

## Postcondition

- no post-conditions
