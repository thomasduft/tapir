using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class HttpRequestMessageBuilderTests
{
  #region HTTP Method Tests

  [Fact]
  public async Task BuildAsync_WithBasicGetRequest_ReturnsHttpRequestMessageWithCorrectMethod()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(HttpMethod.Get, request.Method);
  }

  [Fact]
  public async Task BuildAsync_WithPostRequest_ReturnsHttpRequestMessageWithPostMethod()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "POST",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(HttpMethod.Post, request.Method);
  }

  [Fact]
  public async Task BuildAsync_WithPutMethod_ReturnsHttpRequestMessageWithPutMethod()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "PUT",
        Endpoint = "api/users/1"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(HttpMethod.Put, request.Method);
  }

  [Fact]
  public async Task BuildAsync_WithDeleteMethod_ReturnsHttpRequestMessageWithDeleteMethod()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "DELETE",
        Endpoint = "api/users/1"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(HttpMethod.Delete, request.Method);
  }

  [Fact]
  public async Task BuildAsync_WithPatchMethod_ReturnsHttpRequestMessageWithPatchMethod()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "PATCH",
        Endpoint = "api/users/1"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(HttpMethod.Patch, request.Method);
  }

  #endregion

  #region Request URI Tests

  [Fact]
  public async Task BuildAsync_WithDomainAndEndpoint_SetsCorrectRequestUri()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(new Uri("https://example.com/api/users"), request.RequestUri);
  }

  #endregion

  #region Header Tests

  [Fact]
  public async Task BuildAsync_WithSingleHeader_AddsHeaderToRequest()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "Authorization",
        Value = "Bearer token123"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.True(request.Headers.Contains("Authorization"));
    Assert.Equal("Bearer token123", request.Headers.GetValues("Authorization").First());
  }

  [Fact]
  public async Task BuildAsync_WithMultipleHeaders_AddsAllHeadersToRequest()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "Authorization",
        Value = "Bearer token123"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "X-Custom-Header",
        Value = "custom-value"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.True(request.Headers.Contains("Authorization"));
    Assert.True(request.Headers.Contains("X-Custom-Header"));
    Assert.Equal("Bearer token123", request.Headers.GetValues("Authorization").First());
    Assert.Equal("custom-value", request.Headers.GetValues("X-Custom-Header").First());
  }

  [Fact]
  public async Task BuildAsync_WithEmptyHeaderName_DoesNotAddHeader()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "",
        Value = "some-value"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Empty(request.Headers);
  }

  [Fact]
  public async Task BuildAsync_WithEmptyHeaderValue_DoesNotAddHeader()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "Authorization",
        Value = ""
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Empty(request.Headers);
  }

  #endregion

  #region Content Tests

  [Fact]
  public async Task BuildAsync_WithStringContent_SetsRequestContent()
  {
    // Arrange
    var jsonContent = "{\"name\":\"John\",\"age\":30}";
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddContent,
        Value = jsonContent
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "POST",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.NotNull(request.Content);
    var content = await request.Content.ReadAsStringAsync();
    Assert.Equal(jsonContent, content);
  }

  [Fact]
  public async Task BuildAsync_WithFileContent_ReadsContentFromFile()
  {
    // Arrange
    var tempFile = Path.GetTempFileName();
    var fileContent = "{\"name\":\"Jane\",\"age\":25}";
    await File.WriteAllTextAsync(tempFile, fileContent);

    try
    {
      var instructions = new List<TestStepInstruction>
      {
        new TestStepInstruction(new TestStep())
        {
          Action = Constants.Actions.AddContent,
          File = tempFile
        },
        new TestStepInstruction(new TestStep())
        {
          Action = Constants.Actions.Send,
          Method = "POST",
          Endpoint = "api/users"
        }
      };
      var builder = HttpRequestMessageBuilder.Create(instructions)
        .WithDomain("https://example.com");

      // Act
      var request = await builder.BuildAsync(CancellationToken.None);

      // Assert
      Assert.NotNull(request.Content);
      var content = await request.Content.ReadAsStringAsync();
      Assert.Equal(fileContent, content);
    }
    finally
    {
      File.Delete(tempFile);
    }
  }

  [Fact]
  public async Task BuildAsync_WithNoContent_RequestContentIsNull()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Null(request.Content);
  }

  #endregion

  #region Query Parameter Tests

  [Fact]
  public async Task BuildAsync_WithSingleQueryParameter_AddsQueryParameterToUri()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddQueryParameter,
        Name = "page",
        Value = "1"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(new Uri("https://example.com/api/users?page=1"), request.RequestUri);
  }

  [Fact]
  public async Task BuildAsync_WithMultipleQueryParameters_AddsAllQueryParametersToUri()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddQueryParameter,
        Name = "page",
        Value = "1"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddQueryParameter,
        Name = "pageSize",
        Value = "10"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    var expectedUri = new Uri("https://example.com/api/users?page=1&pageSize=10");
    Assert.Equal(expectedUri, request.RequestUri);
  }

  [Fact]
  public async Task BuildAsync_WithSpecialCharactersInQueryParameter_EscapesSpecialCharacters()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddQueryParameter,
        Name = "search",
        Value = "test & value"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    var uriString = request.RequestUri!.ToString();
    Assert.Contains("search=", uriString);
    Assert.Contains("test", uriString);
    Assert.Contains("value", uriString);
  }

  #endregion

  #region Error Handling Tests

  [Fact]
  public async Task BuildAsync_WithNoSendInstruction_ThrowsInvalidOperationException()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "Authorization",
        Value = "Bearer token"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      () => builder.BuildAsync(CancellationToken.None)
    );
    Assert.Contains("No Send instruction found", exception.Message);
  }

  #endregion

  #region Integration Tests

  [Fact]
  public async Task BuildAsync_WithAllFeaturesCombined_BuildsCompleteRequest()
  {
    // Arrange
    var jsonContent = "{\"name\":\"Alice\"}";
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "Authorization",
        Value = "Bearer token123"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddHeader,
        Name = "X-Api-Version",
        Value = "v1"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddContent,
        Value = jsonContent
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.AddQueryParameter,
        Name = "version",
        Value = "v1"
      },
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "POST",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");

    // Act
    var request = await builder.BuildAsync(CancellationToken.None);

    // Assert
    Assert.Equal(HttpMethod.Post, request.Method);
    Assert.Equal(new Uri("https://example.com/api/users?version=v1"), request.RequestUri);
    Assert.True(request.Headers.Contains("Authorization"));
    Assert.True(request.Headers.Contains("X-Api-Version"));
    Assert.NotNull(request.Content);
    var content = await request.Content.ReadAsStringAsync();
    Assert.Equal(jsonContent, content);
  }

  #endregion

  #region Builder Pattern Tests

  [Fact]
  public void Create_WithInstructions_ReturnsBuilderInstance()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };

    // Act
    var builder = HttpRequestMessageBuilder.Create(instructions);

    // Assert
    Assert.NotNull(builder);
  }

  [Fact]
  public void WithDomain_SetsDomain_ReturnsBuilderForChaining()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions);

    // Act
    var result = builder.WithDomain("https://example.com");

    // Assert
    Assert.NotNull(result);
    Assert.Same(builder, result);
  }

  [Fact]
  public async Task BuildAsync_WithCancellationToken_CompletesSuccessfully()
  {
    // Arrange
    var instructions = new List<TestStepInstruction>
    {
      new TestStepInstruction(new TestStep())
      {
        Action = Constants.Actions.Send,
        Method = "GET",
        Endpoint = "api/users"
      }
    };
    var builder = HttpRequestMessageBuilder.Create(instructions)
      .WithDomain("https://example.com");
    var cts = new CancellationTokenSource();

    // Act
    var request = await builder.BuildAsync(cts.Token);

    // Assert
    Assert.NotNull(request);
  }

  #endregion
}