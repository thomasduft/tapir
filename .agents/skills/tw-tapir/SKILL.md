---
name: tw-tapir
description: Create or edit tapir CLI Markdown API test cases, payload files, assertions, and request chains; diagnose validation errors.
---

# tapir test cases

Create the definition and fixtures unless asked for guidance only. This skill is
self-contained: no tapir source checkout, repository docs, or samples required.

## CLI setup

- Use only the global `tapir` tool. Never build or invoke tapir source projects
  or project binaries.
- Check `tapir --help`. If missing, check `dotnet --info`, obtain installation
  approval, then run `dotnet tool install -g tomware.tapir`.
- Verify with `tapir --help`. If not on PATH, use the installed executable under
  `$HOME/.dotnet/tools` (Linux/macOS) or `%USERPROFILE%\.dotnet\tools` (Windows).
- Report installation errors. If blocked or declined, write files directly and
  state that CLI validation was not run. Do not silently install/upgrade tools.
- Consult `tapir <command> --help` or `tapir man` only as needed for unfamiliar
  syntax or version differences.

## Authoring workflow

1. Reuse existing test conventions and supplied API/OpenAPI documentation.
   Ask together for missing contract details: objective, method, endpoint,
   expected status/body, input/authentication, setup and cleanup.
   Never invent contracts, credentials, or author identity.
2. Propose ID/title/module/destination; default to `./test-cases/TC-<Module>-001.md`
   if no conventions exist. State assumptions; do not overwrite existing files.
3. Write Markdown directly or scaffold from the destination directory:
   `tapir new TC-Users-001 "Create and retrieve a user" --use-simple-template`.
   `new` writes in the current directory, can overwrite, and has no `-i` option.
4. Adapt the example below. Replace date/author placeholders with the current
   date and user-supplied author. Create every referenced fixture.

## Format rules

- First line: `# <id>: <title>`. Preserve metadata spelling; keep
  `Type: Definition` within the first ten lines and `Status: Unknown`.
- One blank-line-separated table per request, exactly one `Send` each.
  Use the five columns below; numeric IDs must increase and be unique file-wide.
  Keep `Actual Result` as `-`.
- Prepare body/headers/query before `Send`; assert/extract afterward.
  Repeat request configuration per table. Always check status and relevant
  content: `Expected Result` prose alone asserts nothing.
- Instructions are case-sensitive, space-separated `Key=Value` pairs.
  Quote values containing spaces; no surrounding backticks in actual cells.
  Escape literal pipes as `\|`. Prefer files for JSON or embedded quotes.
- `Endpoint` is relative, without leading `/`; pass the base URL to `run`.
  Use `AddQueryParameter` for queries.
- Keep fixtures beside the definition; use `File=user.json`.
  JSON is the default content type; specify others explicitly.
  Use only one request content type per table.
- `StoreVariable` extracts JSON via JsonPath; reuse `@@Name@@` in later tables.
  Match the response shape: `Selector=$` for a scalar ID, `Selector=$.id` for
  an object's ID. Use filters for arrays (see below).
  Variables are case-sensitive and test-case scoped; supported in `Value`,
  `File`, `Selector`, `Endpoint`, `Domain`, and JSON request fixture content.
- Setup/cleanup prose does not execute. Add tables if needed; fail-fast execution
  can skip cleanup, so never promise guaranteed cleanup.
- Keep secrets out of files and avoid verbose logging with sensitive data.
  Run requests only against a user-authorized target/scenario; keep TLS checks on.

## Complete example

Assumes POST `users` accepts `{"name":"Alice"}`, returns 201 with JSON `id`,
and GET `users/{id}` returns 200 with `name`. Create `user.json` containing
`{"name":"Alice"}` beside the definition. Adapt all contract-specific values.

```markdown
# TC-Users-001: Create and retrieve a user

- **Date**: YYYY-MM-DD
- **Author**: author-name
- **Test Priority**: Medium
- **Module**: Users
- **Type**: Definition
- **Status**: Unknown

## Description
Create a user and verify its name by retrieving it.

## Preconditions
- API available; permission to create test users.

## Steps

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 01 | Prepare body | Body ready | Action=AddContent File=user.json | - |
| 02 | Create user | Request sent | Action=Send Method=POST Endpoint=users | - |
| 03 | Check status | 201 | Action=CheckStatusCode Value=201 | - |
| 04 | Store ID | ID retained | Action=StoreVariable Selector=$.id Name=UserId | - |

| Step ID | Description | Expected Result | Test Data | Actual Result |
| ------: | ----------- | --------------- | --------- | ------------- |
| 11 | Retrieve user | Request sent | Action=Send Method=GET Endpoint=users/@@UserId@@ | - |
| 12 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
| 13 | Check name | Alice | Action=CheckContent Selector=$.name Value=Alice | - |

## Postcondition
- Created test user remains; no cleanup request is included.
```

Instruction alternatives (one per row; do not mix body content types):

```text
Action=AddHeader Name=Authorization Value="Bearer @@Token@@"
Action=AddQueryParameter Name=page Value=1
Action=VerifyContent File=expected.json
Action=CheckContent Selector=$[?@.name=="Alice"].name Value=Alice
Action=AddContent ContentType=application/xml File=user.xml
Action=CheckContent ContentType=application/xml Selector=/User/Name Value=Alice
Action=CheckContentHeader Name=Content-Type Value=application/pdf
Action=AddContent ContentType=multipart/form-data Name=file Value=document.pdf File=document.pdf
Action=AddContent ContentType=multipart/form-data Name=title Value="Test Document"
```

`CheckContent` compares a selected value; `VerifyContent` compares whole content
and suits stable fixtures. The two multipart rows belong to one upload table:
`Name` is the form field, `Value` the uploaded filename, and `File` the local file.
Use actual fixture bytes for any `Content-Length` assertion, not example sizes.
For automated cleanup, add a final DELETE table with its expected status.
Use `tapir man` for other actions.

## Validate and hand off

1. Check metadata, tables, IDs, assertions, fixture existence, and variable order.
2. Run: `tapir validate TC-Users-001 -i ./test-cases -v UserId=1`.
   Validation sends no requests and extracts no response variables: supply
   non-secret sample values for all required variables via repeated `-v Name=Value`;
   omit `-v` when unnecessary. These samples do not prove extraction works.
3. Fix errors and revalidate. Report files, assumptions, and actual validation
   outcome; syntax validation is not an API test pass.
4. Provide, but execute only with authorization:
   `tapir run https://api.example.com -tc TC-Users-001 -i ./test-cases -o ./results`.
   Replace the URL with the approved target. Keep `-tc` to avoid running the suite.
   Runs validate tables with earlier extracted variables and stop on failure;
   use `--continue-on-failure` only when later requests are safe after failures.
