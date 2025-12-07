using Microsoft.Extensions.DependencyInjection;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Utils;

var services = new ServiceCollection()
    .AddHttpClient()
    .AddCliCommand<ManCommand>()
    .AddCliCommand<NewTestCaseCommand>()
    .AddCliCommand<ValidateCommand>().WithValidation()
    .AddCliCommand<RunCommand>()
    .AddSingleton<ITestCaseExecutor, TestCaseExecutor>()
    .AddSingleton<Cli>();

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
