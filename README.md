# tapir

A command-line tool for managing and executing automated api test cases.

## Introduction

`tapir` is a command-line tool designed to simplify the management and execution of automated API test cases. With `tapir`, you can easily create, organize, and run tests for your APIs, ensuring they function as expected and meet quality standards.

## Maintainers

- Thomas Duft

## Usage

```bash
A command-line tool for managing and executing automated api test cases.

Usage: tapir [command] [options]

Options:
  -?|-h|--help  Show help information.

Commands:
  man           Displays a man page that helps writing the Test-Data syntax for a Test Case.
  new           Creates a new Test Case definition (i.e. test-case TC-Audit-001 "My TestCase Title").
  run           Runs Test Case definition (i.e. "https://localhost:5001" -tc TC-Audit-001).
  validate      Validates a Test Case definition (i.e. TC-Audit-001).

Run 'tapir [command] -?|-h|--help' for more information about a command.
```
