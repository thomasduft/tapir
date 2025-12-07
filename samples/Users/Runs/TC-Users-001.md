# TC-Users-001: List all Users

- **Date**: 2025-12-07
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Run
- **Status**: Passed
- **Domain**: https://localhost:5001

## Description

Tests the Users API by first retrieving all users, verifying the response contains Alice, and extracting her ID for subsequent operations.

It then uses Alice's ID to fetch her individual user details and validates the returned data matches expected values (name: Alice, age: 20). This test demonstrates both list and detail API endpoints while showcasing variable extraction and reuse between test steps.

## Preconditions

- no pre-conditions

## Steps

| Step ID  | Description             | Test Data                                                            | Expected Result        | Actual Result |
| -------: | ----------------------- | -------------------------------------------------------------------- | ---------------------- | ------------- |
| 01 | Call users api | Action=Send Method=GET Endpoint=users | Request successful | ✅ |
| 02 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ✅ |
| 03 | Inspect content | Action=VerifyContent File=samples/Users/Definitions/users.json | Should be identical | ✅ |
| 04 | Contains Alice | Action=CheckContent JsonPath=$[?@.name=="Alice"].name Value=Alice | Content contains Alice | ✅ |
| 05 | Retain ID of Alice | Action=StoreVariable JsonPath=$[?@.name=="Alice"].id Name=AliceId | ID of Alice stored | ✅ |

| Step ID  | Description             | Test Data                                                            | Expected Result        | Actual Result |
| -------: | ----------------------- | -------------------------------------------------------------------- | ---------------------- | ------------- |
| 11 | Get Alice details | Action=Send Method=GET Endpoint=users/@@AliceId@@ | Request successful | ✅ |
| 12 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ✅ |
| 13 | Inspect content | Action=VerifyContent File=samples/Users/Definitions/alice.json | Should be identical | ✅ |
| 14 | Verify name | Action=CheckContent JsonPath=$.name Value=Alice | Name is Alice | ✅ |
| 15 | Verify age | Action=CheckContent JsonPath=$.age Value=30 | Age is 30 | ✅ |

## Postcondition

- no post-conditions

