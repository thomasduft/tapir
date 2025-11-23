using Microsoft.Extensions.DependencyInjection;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;
using tomware.Tapir.Cli.Domain.Actions;

var services = new ServiceCollection()
    .AddHttpClient()
    .AddCliCommand<TestCaseCommand>()
    .AddCliCommand<RunCommand>()
    .RegisterAction<AddHeader>()
    .RegisterAction<AddQueryParam>()
    .RegisterAction<AddBody>()
    .RegisterAction<Send>()
    .AddSingleton<ITestCaseExecutor, TestCaseExecutor>()
    .AddSingleton<Cli>();

var provider = services.BuildServiceProvider();
var cli = provider.GetRequiredService<Cli>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
  Console.WriteLine("Cancelling...");
  cts.Cancel();
  e.Cancel = true;
};

return await cli.ExecuteAsync(args, cts.Token);
