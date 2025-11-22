# TC-Users-001: List all Users

- **Date**: 2025-11-22
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Run
- **Status**: Failed
- **Domain**: https://localhost:5001

## Description

Tests the Users API by first retrieving all users, verifying the response contains Alice, and extracting her ID for subsequent operations.

It then uses Alice's ID to fetch her individual user details and validates the returned data matches expected values (name: Alice, age: 20). This test demonstrates both list and detail API endpoints while showcasing variable extraction and reuse between test steps.

## Preconditions

- no pre-conditions

## Steps

| Step ID  | Description             | Test Data                                                      | Expected Result        | Actual Result |
| -------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 11 | Call users api | Action=Send Method=GET Value=users | Request successful | ✅ |
| 12 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ✅ |
| 13 | Inspect content | Action=VerifyContent File=users.json | Should be identical | ✅ |
| 14 | Contains Alice | Action=CheckContent Path=$[?@.name=="Alice"].name Value=Alice | Content contains Alice | ✅ |
| 15 | Retain ID of Alice ID | Action=StoreVariable Path=$[?@.name=="Alice"].id Name=AliceId | ID of Alice stored | ✅ |

| Step ID  | Description             | Test Data                                                      | Expected Result        | Actual Result |
| -------: | ----------------------- | ---------------------------------------------------------------| ---------------------- | ------------- |
| 21       | Get Alice Details       | Action=Send Method=GET Value=users/{@@AliceId@@}               | Request successful     | -             |
| 22       | Verify response code    | Action=CheckStatusCode Value=200                               | 200                    | -             |
| 23       | Inspect content         | Action=VerifyContent File=alice.json                           | Should be identical    | -             |
| 24       | Verify Name             | Action=CheckContent Path=$.name Value=Alice                    | Name is Alice          | -             |
| 25       | Verify Age              | Action=CheckContent Path=$.age Value=20                        | Age is 20              | -             |

## Postcondition

- no post-conditions

