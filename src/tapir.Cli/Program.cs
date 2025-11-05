using Microsoft.Extensions.DependencyInjection;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;

var services = new ServiceCollection()
    .AddHttpClient()
    .AddSingleton<ITestCaseExecutor, TestCaseExecutor>()
    .AddCliCommand<RunCommand>()
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
