# TC-Users-003: Create User with XML

- **Date**: 2026-09-07
- **Author**: tapir
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description

Creates a user from XML, then retrieves users in XML.

## Preconditions

- no pre-conditions

## Steps

| Step ID  | Description         | Test Data                                                                     | Expected Result    | Actual Result |
| -------: | ------------------- | ----------------------------------------------------------------------------- | ------------------ | ------------- |
| 01       | Add XML user        | Action=AddContent ContentType=application/xml File=xml-user.xml               | XML added          | -             |
| 02       | Create XML user     | Action=Send Method=POST Endpoint=users/xml                                    | Request successful | -             |
| 03       | Check response code | Action=CheckStatusCode Value=201                                              | 201                | -             |
| 04       | Check name          | Action=CheckContent ContentType=application/xml Selector=/user/name Value=Eve | Name is Eve        | -             |

| Step ID  | Description         | Test Data                                                                                           | Expected Result    | Actual Result |
| -------: | ------------------- | --------------------------------------------------------------------------------------------------- | ------------------ | ------------- |
| 11       | Get XML users       | Action=Send Method=GET Endpoint=users/xml                                                           | Request successful | -             |
| 12       | Check response code | Action=CheckStatusCode Value=200                                                                    | 200                | -             |
| 13       | Check Alice         | Action=CheckContent ContentType=application/xml Selector=/users/user[name='Alice']/name Value=Alice | Name is Alice      | -             |

## Postcondition

- no post-conditions
