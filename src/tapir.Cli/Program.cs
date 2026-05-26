using Microsoft.Extensions.DependencyInjection;

using Serilog;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

var services = new ServiceCollection()
    .AddCliCommand<ManCommand>()
    .AddCliCommand<NewTestCaseCommand>()
    .AddCliCommand<NewTestStepCommand>()
    .AddCliCommand<ValidateCommand>().WithValidation()
    .AddCliCommand<RunCommand>().WithExecution()
    .AddCliCommand<ReportCommand>()
    .AddSingleton<ITestCaseExecutor, TestCaseExecutor>()
    .AddSingleton<Cli>();

Log.Logger = LogHelper.CreateLogger(ref args);

var provider = services.BuildServiceProvider();
var cli = provider.GetRequiredService<Cli>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
  Console.WriteLine("Cancelling...");
  cts.Cancel();
  e.Cancel = true;
};

var meterProvider = OtelHelper.CreateMeterProvider(ref args);

try
{
  return await cli.ExecuteAsync(args, cts.Token);
}
catch (OperationCanceledException)
{
  Log.Logger.Warning("Execution cancelled.");
  return 130;
}
catch (Exception ex)
{
  Log.Logger.Fatal(ex, "Unhandled exception while executing command.");
  return 1;
}
finally
{
  meterProvider.Dispose();
  Log.CloseAndFlush();
}
