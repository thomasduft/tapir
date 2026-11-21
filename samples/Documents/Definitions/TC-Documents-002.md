# TC-Documents-002: Update a document title

- **Date**: 2026-11-21
- **Author**: Thomas Duft
- **Test Priority**: Medium
- **Module**: Documents
- **Type**: Definition
- **Status**: Unknown

## Description

Upload a PDF, update its title without replacing the file, and verify the updated metadata and original download.

## Preconditions

- The DevHost Documents API is available and permits uploading, updating, and deleting documents.
- The `empty.pdf` fixture is present beside this definition.

## Steps

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 01 | Add PDF | File ready | Action=AddContent ContentType=multipart/form-data Name=file Value=empty.pdf File=empty.pdf | - |
| 02 | Add title | Title ready | Action=AddContent ContentType=multipart/form-data Name=title Value="Original Title" | - |
| 03 | Upload document | Request sent | Action=Send Method=POST Endpoint=documents | - |
| 04 | Check status | 201 | Action=CheckStatusCode Value=201 | - |
| 05 | Store document ID | ID retained | Action=StoreVariable Selector=$ Name=DocumentId | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 11 | Prepare new title | Title ready | Action=AddContent ContentType=multipart/form-data Name=title Value="Updated Title" | - |
| 12 | Update document | Request sent | Action=Send Method=PUT Endpoint=documents/@@DocumentId@@ | - |
| 13 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
| 14 | Check title | Title updated | Action=CheckContent Selector=$.title Value="Updated Title" | - |
| 15 | Check ID | ID is unchanged | Action=CheckContent Selector=$.id Value=@@DocumentId@@ | - |
| 16 | Check file size | File size unchanged | Action=CheckContent Selector=$.fileSize Value=12959 | - |
| 17 | Check content type | PDF retained | Action=CheckContent Selector=$.contentType Value=application/pdf | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 21 | Retrieve metadata | Request sent | Action=Send Method=GET Endpoint=documents/@@DocumentId@@ | - |
| 22 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
| 23 | Check persisted title | Title updated | Action=CheckContent Selector=$.title Value="Updated Title" | - |
| 24 | Check file size | File size unchanged | Action=CheckContent Selector=$.fileSize Value=12959 | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 31 | Download document | Request sent | Action=Send Method=GET Endpoint=documents/@@DocumentId@@/download | - |
| 32 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
| 33 | Check content type | PDF returned | Action=CheckContentHeader Name=Content-Type Value=application/pdf | - |
| 34 | Check content length | Original size returned | Action=CheckContentHeader Name=Content-Length Value=12959 | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 41 | Delete document | Request sent | Action=Send Method=DELETE Endpoint=documents/@@DocumentId@@ | - |
| 42 | Check status | 204 | Action=CheckStatusCode Value=204 | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 51 | Retrieve deleted document | Request sent | Action=Send Method=GET Endpoint=documents/@@DocumentId@@ | - |
| 52 | Check status | 404 | Action=CheckStatusCode Value=404 | - |
| 53 | Check error | Document is not found | Action=CheckContent Selector=$ Value="Document with ID @@DocumentId@@ not found." | - |

## Postcondition

- The test deletes the uploaded document if all preceding steps succeed. A failure can leave it behind.
