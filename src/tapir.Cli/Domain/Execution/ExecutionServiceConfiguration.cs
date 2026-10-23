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
      .AddSingleton<IRequestContentHandler, XmlRequestContentHandler>()
      .AddSingleton<IRequestContentHandler, FormUrlEncodedRequestContentHandler>()
      .AddSingleton<IRequestContentHandler, MultipartFormDataRequestContentHandler>()
      .AddSingleton<IResponseContentValidatorFactory, ResponseContentValidatorFactory>()
      .AddSingleton<IResponseContentValidator, TextResponseContentValidator>()
      .AddSingleton<IResponseContentValidator, JsonResponseContentValidator>()
      .AddSingleton<IResponseContentValidator, XmlResponseContentValidator>();

    // Keep the default client secure. The insecure client is selected only by an
    // explicit run --insecure option, never by mutable global configuration.
    services.AddHttpClient(Constants.HttpClientName);
    services.AddHttpClient(Constants.InsecureHttpClientName)
      .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
      });

    return services;
  }
}
