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

- no pre-conditions

## Steps

| Step ID  | Description             | Test Data                                                                                                                | Expected Result    | Actual Result |
| -------: | ----------------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------ | ------------- |
| 01       | Add Document            | Action=AddContent ContentType=multipart/form-data Name=file Value=empty.pdf File=samples/Documents/Definitions/empty.pdf | Document prepared  | -             |
| 02       | Set Document Title      | Action=AddContent ContentType=multipart/form-data Name=title Value=Test Document                                         | Document prepared  | -             |
| 03       | Upload Document         | Action=Send Method=POST Endpoint=documents                                                                               | Request successful | -             |
| 04       | Verify response code    | Action=CheckStatusCode Value=201                                                                                         | 201                | -             |
| 05       | Retain DocumentId       | Action=StoreVariable JsonPath=$ Name=DocumentId                                                                          | ID of new document | -             |

| Step ID  | Description             | Test Data                                                         | Expected Result        | Actual Result |
| -------: | ----------------------- | ----------------------------------------------------------------- | ---------------------- | ------------- |
| 11       | Retrieve the document   | Action=Send Method=GET Endpoint=documents/@@DocumentId@@/download | Request successful     | -             |
| 12       | Verify response code    | Action=CheckStatusCode Value=200                                  | 200                    | -             |
| 13       | Check Headers           | Action=CheckContentHeader Name=Content-Type Value=application/pdf | Header is present      | -             |
| 14       | Check Headers           | Action=CheckContentHeader Name=Content-Length Value=12959         | Header is present      | -             |

| Step ID  | Description             | Test Data                                                         | Expected Result        | Actual Result |
| -------: | ----------------------- | ----------------------------------------------------------------- | ---------------------- | ------------- |
| 21       | Retrieve the document   | Action=Send Method=DELETE Endpoint=documents/@@DocumentId@@       | Request successful     | -             |
| 22       | Verify response code    | Action=CheckStatusCode Value=204                                  | 204                    | -             |

## Postcondition

- no post-conditions
