# Todo

- [ ] Use ILogger through ctor injection instead of static Log class
  - make use of LoggerFactory while setting up the DI container
- [ ] rename JsonPath to just Path
  - when application/xml would be supported, JsonPath would be misleading

## Done

- [x] support for OpenTelemetry metrics
- [x] at the moment only support the following based ContentTypes:
  - application/text
  - application/json
  - multipart/form-data
  - application/x-www-form-urlencoded
- [x] improved docs / man
- [x] support colored Console Logger based on log-levels with Serilog instead of custom ConsoleHelper
  - https://github.com/serilog/serilog-sinks-console/issues/35#issuecomment-2577943657
