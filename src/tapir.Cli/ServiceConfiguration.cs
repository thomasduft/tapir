using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceConfiguration
{
  public static IServiceCollection AddCliCommand<TCommand>(this IServiceCollection services)
      where TCommand : CommandLineApplication
  {
    services.AddSingleton<CommandLineApplication, TCommand>();

    return services;
  }
}