# TC-Users-005: Reject invalid users

- **Date**: 2026-11-21
- **Author**: Thomas Duft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description

Verify that user creation rejects a blank name and a non-positive age.

## Preconditions

- The DevHost Users API is available.

## Steps

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 01 | Prepare user without a name | Body ready | Action=AddContent File=invalid-user-name.json | - |
| 02 | Create user | Request sent | Action=Send Method=POST Endpoint=users | - |
| 03 | Check status | 400 | Action=CheckStatusCode Value=400 | - |
| 04 | Check error | Name is required | Action=CheckContent Selector=$ Value="Name is required." | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 11 | Prepare user with zero age | Body ready | Action=AddContent File=invalid-user-age.json | - |
| 12 | Create user | Request sent | Action=Send Method=POST Endpoint=users | - |
| 13 | Check status | 400 | Action=CheckStatusCode Value=400 | - |
| 14 | Check error | Age must be positive | Action=CheckContent Selector=$ Value="Age must be a positive number." | - |

## Postcondition

- Neither invalid request should create a user.
