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

| Step ID | Description             | Test Data                                                 | Expected Result        | Actual Result |
| ------: | ----------------------- | --------------------------------------------------------- | ---------------------- | ------------- |
| 1       | Call users api          | Action=Send Method=GET Value=users                        | Request successful     | tbd           |
| 2       | Verify response code    | Action=CheckStatusCode Value=200                          | 200                    | tbd           |
| 3       | Inspect content         | Action=VerifyContent File=users.json                      | Should be identical    | tbd           |
| 4       | Contains Alice          | Action=CheckContent Path=$[?@.name=="Alice"] Value=Alice     | Content contains Alice | tbd           |
| 5       | Get Alice ID            | Action=StoreVariable Path=\"$[?(@.name == 'Alice')].id\"  | Returns Alice's ID     | tbd           |

## Postcondition

- no post-conditions
