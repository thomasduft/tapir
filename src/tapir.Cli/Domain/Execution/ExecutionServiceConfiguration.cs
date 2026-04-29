using Microsoft.Extensions.DependencyInjection;

namespace tomware.Tapir.Cli.Domain;

internal static class ExecutionServiceConfiguration
{
  internal static IServiceCollection WithExecution(this IServiceCollection services)
  {
    services
      .AddSingleton<IHttpRequestMessageBuilder, HttpRequestMessageBuilder>()
      .AddSingleton<IRequestContentFactory, RequestContentFactory>()
      .AddSingleton<IRequestContentHandler, TextRequestContentHandler>()
      .AddSingleton<IRequestContentHandler, JsonRequestContentHandler>()
      .AddSingleton<IRequestContentHandler, FormUrlEncodedRequestContentHandler>()
      .AddSingleton<IRequestContentHandler, MultipartFormDataRequestContentHandler>()
      .AddHttpClient(Constants.HttpClientName)
      .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
      });

    return services;
  }
}