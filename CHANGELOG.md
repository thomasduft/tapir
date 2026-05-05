# Changelog

## [0.0.22] - 2026-05-05

### Added

- Adds variables support in content files

### Changed

- Refactord domain model and added TestCase reference to TestStep

## [0.0.21] - 2026-04-29

### Changed

- Refactored request content handlers and response validators

## [0.0.20] - 2026-04-29

### Added

- Added verbose logging for HTTP request construction

## [0.0.19] - 2026-04-18

### Fixed

- Allows proper content file resolving

## [0.0.18] - 2026-04-14

### Added

- Added a new 'tapir report' command that scans a directory of test case run output files and generates a single self-contained HTML report

## [0.0.17] - 2026-04-14

### Fixed

- Treats no found failures as success

## [0.0.16] - 2026-03-18

### Added

- Added variable support for File field

## [0.0.15] - 2026-03-16

### Removed

- Removed validation in RunCommand

## [0.0.14] - 2026-03-15

### Added

- Added `new-step`-command

## [0.0.13] - 2026-03-14

### Added

- Added the possibility to scaffold a simple Test Case (use the `--use-simple-template`-flag)
- Allow Test Data to be placed in any column index in markdown tables

## [0.0.12] - 2026-03-14

### Changed

- Runs now are stored beneath a subdirectory with the following naming convention `Runs/yyyy-MM-dd/...`

## [0.0.11] - 2026-01-26

### Added

- Supports logging the response content (see [LogResponseContent action](docs/Manual.md#logresponsecontent))

## [0.0.10] - 2026-01-22

### Fixed

- Validates the test case before running it and applies variables from the command-line correctly

## [0.0.9] - 2026-01-22

### Fixed

- Properly passing variables from the command-line

## [0.0.8] - 2026-01-21

### Added

- Added support for variables on `Domain`-fields

## [0.0.7] - 2026-01-05

### Removed

- Removed unused dependencies

## [0.0.6] - 2026-01-05

### Added

- Added support for overriding a Domain in `Send`-Action (use `Domain`-property)

## [0.0.5] - 2025-12-17

### Fixed

- Correct evaluation of overall test case success status

## [0.0.4] - 2025-12-10

### Changed

- Improved TestCaseCaseFileLocator to return only relevant TestCase files starting from a given directory

## [0.0.3] - 2025-12-10

### Added

- Added support for ContentType application/x-www-form-urlencoded

### Changed

- Improved console logs

## [0.0.2] - 2025-12-07

### Added

- Support for OpenTelemetry

## [0.0.1] - 2025-12-06

### Added

- Initial version of tapir

