using Microsoft.Extensions.DependencyInjection;

using Serilog;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

var services = new ServiceCollection()
    .AddCliCommand<ManCommand>()
    .AddCliCommand<NewTestCaseCommand>()
    .AddCliCommand<ValidateCommand>().WithValidation()
    .AddCliCommand<RunCommand>().WithExecution()
    .AddSingleton<ITestCaseExecutor, TestCaseExecutor>()
    .AddSingleton<Cli>();

Log.Logger = LogHelper.CreateLogger(ref args);

var provider = services.BuildServiceProvider();
var cli = provider.GetRequiredService<Cli>();

using var meterProvider = OtelHelper.CreateMeterProvider(ref args);
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
  Console.WriteLine("Cancelling...");
  cts.Cancel();
  e.Cancel = true;
};

var returnCode = await cli.ExecuteAsync(args, cts.Token);
meterProvider.Dispose();

return returnCode;