# TC-Users-004: Update a user

- **Date**: 2026-11-21
- **Author**: Thomas Duft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description

Create a user, update its name and age, and verify that the changes persist.

## Preconditions

- The DevHost Users API is available and permits creating, updating, and deleting users.

## Steps

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 01 | Prepare user | Body ready | Action=AddContent File=update-user-initial.json | - |
| 02 | Create user | Request sent | Action=Send Method=POST Endpoint=users | - |
| 03 | Check status | 201 | Action=CheckStatusCode Value=201 | - |
| 04 | Store user ID | ID retained | Action=StoreVariable Selector=$ Name=UserId | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 11 | Prepare update | Body ready | Action=AddContent File=update-user-changed.json | - |
| 12 | Update user | Request sent | Action=Send Method=PUT Endpoint=users/@@UserId@@ | - |
| 13 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
| 14 | Check updated name | Name is Morgan | Action=CheckContent Selector=$.name Value=Morgan | - |
| 15 | Check updated age | Age is 42 | Action=CheckContent Selector=$.age Value=42 | - |
| 16 | Check ID | ID is unchanged | Action=CheckContent Selector=$.id Value=@@UserId@@ | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 21 | Retrieve user | Request sent | Action=Send Method=GET Endpoint=users/@@UserId@@ | - |
| 22 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
| 23 | Check persisted name | Name is Morgan | Action=CheckContent Selector=$.name Value=Morgan | - |
| 24 | Check persisted age | Age is 42 | Action=CheckContent Selector=$.age Value=42 | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 31 | Delete user | Request sent | Action=Send Method=DELETE Endpoint=users/@@UserId@@ | - |
| 32 | Check status | 204 | Action=CheckStatusCode Value=204 | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 41 | Retrieve deleted user | Request sent | Action=Send Method=GET Endpoint=users/@@UserId@@ | - |
| 42 | Check status | 404 | Action=CheckStatusCode Value=404 | - |
| 43 | Check error | User is not found | Action=CheckContent Selector=$ Value="User with ID @@UserId@@ not found." | - |

## Postcondition

- The test deletes the created user if all preceding steps succeed. A failure can leave it behind.
