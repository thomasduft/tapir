using Microsoft.Extensions.DependencyInjection;

namespace tomware.Tapir.Cli.Domain;

public static class ActionsServiceConfiguration
{
  internal static IServiceCollection RegisterAction<TAction>(this IServiceCollection services)
      where TAction : class, IAction
  {
    services.AddSingleton<IAction, TAction>();

    return services;
  }
}
