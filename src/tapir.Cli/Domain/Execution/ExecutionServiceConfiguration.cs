using Microsoft.Extensions.DependencyInjection;

namespace tomware.Tapir.Cli.Domain;

internal static class ExecutionServiceConfiguration
{
  internal static IServiceCollection WithExecution(this IServiceCollection services)
  {
    services
      .AddHttpClient(Constants.HttpClientName)
      .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
      });

    return services;
  }
}