using Microsoft.Extensions.DependencyInjection;

using tomware.Tapir.Cli;

var services = new ServiceCollection()
    .AddCliCommand<SampleCommand>()
    .AddSingleton<Cli>();
var provider = services.BuildServiceProvider();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Cancelling...");
    cts.Cancel();
    e.Cancel = true;
};

var cli = provider.GetRequiredService<Cli>();

return await cli.ExecuteAsync(args, cts.Token);