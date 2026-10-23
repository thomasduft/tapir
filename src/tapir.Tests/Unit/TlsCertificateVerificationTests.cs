using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using tomware.Tapir.Cli;
using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class TlsCertificateVerificationTests
{
  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_WithSelfSignedCertificate_RequiresInsecureOption(bool insecure)
  {
    await using var server = new LocalServer(useTls: true);
    using var provider = CreateServices();

    if (insecure)
    {
      Assert.Equal(0, await RunAsync(provider, server, insecure));
    }
    else
    {
      var error = await Assert.ThrowsAsync<HttpRequestException>(
        () => RunAsync(provider, server, insecure));
      Assert.IsType<AuthenticationException>(error.InnerException);
    }
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Run_WithPlainHttp_StillWorks(bool insecure)
  {
    await using var server = new LocalServer(useTls: false);
    using var provider = CreateServices();

    Assert.Equal(0, await RunAsync(provider, server, insecure));
  }

  [Fact]
  public async Task Run_InsecureOption_DoesNotDisableVerificationForSubsequentRuns()
  {
    await using var server = new LocalServer(useTls: true);
    using var provider = CreateServices();

    Assert.Equal(0, await RunAsync(provider, server, insecure: true));
    await Assert.ThrowsAsync<HttpRequestException>(
      () => RunAsync(provider, server, insecure: false));
  }

  private static ServiceProvider CreateServices()
  {
    return new ServiceCollection()
      .WithExecution()
      .WithValidation()
      .AddSingleton<ITestCaseExecutor, TestCaseExecutor>()
      .BuildServiceProvider();
  }

  private static async Task<int> RunAsync(
    ServiceProvider provider,
    LocalServer server,
    bool insecure)
  {
    var directory = Path.Combine(Path.GetTempPath(), $"tapir-tls-{Guid.NewGuid():N}");
    Directory.CreateDirectory(directory);
    try
    {
      await File.WriteAllTextAsync(Path.Combine(directory, "TC-Tls-001.md"), """
        # TC-Tls-001: TLS verification

        - **Date**: 2026-01-01
        - **Module**: TLS
        - **Type**: Definition
        - **Status**: Unknown

        ## Steps

        | Step ID | Description | Expected Result | Test Data | Actual Result |
        | ------- | ----------- | --------------- | --------- | ------------- |
        | 01 | Send request | Success | Action=Send Method=GET Endpoint=health | - |
        | 02 | Check status | 200 | Action=CheckStatusCode Value=200 | - |
        """);

      var command = new RunCommand(
        provider.GetRequiredService<ITestCaseValidator>(),
        provider.GetRequiredService<ITestCaseExecutor>());
      var cli = new global::Cli([command]);
      var args = new List<string> { "run", server.Address, "-i", directory };
      if (insecure) args.Add("--insecure");

      return await cli.ExecuteAsync(args.ToArray(), server.CancellationToken);
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  // A real loopback TLS endpoint keeps these tests independent of public
  // services, machine trust stores, and installed development certificates.
  private sealed class LocalServer : IAsyncDisposable
  {
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(15));
    private readonly X509Certificate2? _certificate;
    private readonly Task _serverTask;

    public string Address { get; }
    public CancellationToken CancellationToken => _cts.Token;

    public LocalServer(bool useTls)
    {
      if (useTls)
      {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
          "CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var names = new SubjectAlternativeNameBuilder();
        names.AddDnsName("localhost");
        names.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(names.Build());
        _certificate = request.CreateSelfSigned(
          DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
      }

      _listener.Start();
      var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
      Address = $"{(useTls ? "https" : "http")}://127.0.0.1:{port}";
      _serverTask = ServeAsync();
    }

    private async Task ServeAsync()
    {
      try
      {
        while (!_cts.IsCancellationRequested)
        {
          using var client = await _listener.AcceptTcpClientAsync(_cts.Token);
          try
          {
            using Stream stream = _certificate == null
              ? client.GetStream()
              : new SslStream(client.GetStream());

            if (stream is SslStream tls)
            {
              await tls.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
              {
                ServerCertificate = _certificate
              }, _cts.Token);
            }

            using var reader = new StreamReader(stream, leaveOpen: true);
            while (await reader.ReadLineAsync(_cts.Token) is { Length: > 0 })
            {
            }

            var response = Encoding.ASCII.GetBytes(
              "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nOK");
            await stream.WriteAsync(response, _cts.Token);
          }
          catch (AuthenticationException)
          {
            // Expected when the client rejects our untrusted certificate.
          }
          catch (IOException)
          {
            // The rejecting client may close the connection during the handshake.
          }
        }
      }
      catch (OperationCanceledException) when (_cts.IsCancellationRequested)
      {
      }
    }

    public async ValueTask DisposeAsync()
    {
      await _cts.CancelAsync();
      _listener.Stop();
      await _serverTask;
      _certificate?.Dispose();
      _cts.Dispose();
    }
  }
}
