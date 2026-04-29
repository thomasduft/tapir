using Microsoft.Extensions.DependencyInjection;

namespace tomware.Tapir.Cli.Domain;

internal static class ValidationServiceConfiguration
{
  internal static IServiceCollection WithValidation(this IServiceCollection services)
  {
    services.AddSingleton<ITestCaseValidator, TestCaseValidator>();
    services
      .RegisterValidator<AddHeaderActionValidator>()
      .RegisterValidator<AddQueryParameterActionValidator>()
      .RegisterValidator<AddContentActionValidator>()
      .RegisterValidator<SendActionValidator>()
      .RegisterValidator<LogResponseContentValidator>()
      .RegisterValidator<CheckStatusCodeActionValidator>()
      .RegisterValidator<CheckReasonPhraseActionValidator>()
      .RegisterValidator<CheckContentActionValidator>()
      .RegisterValidator<CheckContentHeaderActionValidator>()
      .RegisterValidator<VerifyContentActionValidator>()
      .RegisterValidator<StoreVariableActionValidator>();

    return services;
  }

  internal static IServiceCollection RegisterValidator<TValidator>(this IServiceCollection services)
      where TValidator : class, IValidator
  {
    services.AddSingleton<IValidator, TValidator>();

    return services;
  }
}
