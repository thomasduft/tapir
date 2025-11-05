# TC-Users-001: List all Users

- **Date**: 2025-11-05
- **Author**: thomasduft
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description

Tests whether all users can be retrieved.

## Preconditions

- no pre-conditions

## Steps

| Step ID | Description             | Test Data                                                             | Expected Result        | Actual Result |
| ------: | ----------------------- | --------------------------------------------------------------------- | ---------------------- | ------------- |
| 1       | Call api/users         | Step=Act    Action=Send Method=GET Value=api/users                     | 200 OK                 | tbd           |
| 2       | Verify response code    | Step=Assert Action=CheckResponseCode Value=200                        | 200                    | tbd           |
| 3       | Inspect content         | Step=Assert Action=VerifyContent File=users.json                      | Should be identical    | tbd           |
| 4       | Contains Alice          | Step=Assert Action=CheckContent Value=\"$[?(@.name == 'Alice')]\"     | Content contains Alice | tbd           |
| 5       | Get Alice ID            | Step=Assert Action=StoreVariable Value=\"$[?(@.name == 'Alice')].id\" | Returns Alice's ID     | tbd           |

## Postcondition

- no post-conditions
