# TC-Users-002: Create new User

- **Date**: 2025-12-07
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Run
- **Status**: Passed
- **Domain**: https://localhost:5001

## Description

Tests the Users API by creating a new user named Tom with age 45.

It verifies that the user creation is successful and extracts the newly created user's ID for subsequent retrieval and validation of the user's details.

## Preconditions

- no pre-conditions

## Steps

| Step ID  | Description             | Test Data                                                       | Expected Result    | Actual Result |
| -------: | ----------------------- | --------------------------------------------------------------- | ------------------ | ------------- |
| 01 | Prepare Tom | Action=AddContent File=samples/Users/Definitions/tom.json | User tom prepared | - |
| 02 | Add Test Header | Action=AddHeader Name=X-Header-Test Value=test | Test Header Added | - |
| 03 | Get Tom details | Action=Send Method=POST Endpoint=users | Request successful | ✅ |
| 04 | Verify response code | Action=CheckStatusCode Value=201 | 201 | ✅ |
| 05 | Retain UserId | Action=StoreVariable JsonPath=$ Name=UserId | ID of new user | ✅ |

| Step ID  | Description             | Test Data                                         | Expected Result        | Actual Result |
| -------: | ----------------------- | ------------------------------------------------- | ---------------------- | ------------- |
| 11 | Retrieve the user | Action=Send Method=GET Endpoint=users/@@UserId@@ | Request successful | ✅ |
| 12 | Verify response code | Action=CheckStatusCode Value=200 | 200 | ✅ |
| 13 | Contains Tom | Action=CheckContent JsonPath=$.name Value=Tom | Content contains Tom | ✅ |
| 14 | Tom's age is 45 | Action=CheckContent JsonPath=$.age Value=45 | Age is 45 | ✅ |

## Postcondition

- no post-conditions

