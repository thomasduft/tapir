using System.Text;

using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class TestCaseContentFileResolverTests
{
  [Fact]
  public async Task TryLocateExistingFile_WithRelativePath_ResolvesAgainstTestCaseDirectory()
  {
    var fixture = await CreateFixtureAsync("expected.json", "{\"name\":\"Alice\"}");

    try
    {
      var instruction = CreateInstruction(Constants.Actions.VerifyContent, file: "expected.json");
      instruction.TestStep.TestCase.File = fixture.TestCaseFile;

      var resolvedPath = TestCaseContentFileResolver.TryLocateExistingFile(instruction);

      Assert.Equal(fixture.ContentFile, resolvedPath);
    }
    finally
    {
      Directory.Delete(fixture.DirectoryPath, true);
    }
  }

  [Fact]
  public async Task AddContentActionValidator_WithRelativePath_UsesTestCaseDirectory()
  {
    var fixture = await CreateFixtureAsync("payload.json", "{\"name\":\"Alice\"}");

    try
    {
      var validator = new AddContentActionValidator();
      var instruction = CreateInstruction(
        Constants.Actions.AddContent,
        file: "payload.json",
        contentType: Constants.ContentTypes.Json
      );
      instruction.TestStep.TestCase.File = fixture.TestCaseFile;

      var results = (await validator.ValidateAsync(instruction, CancellationToken.None)).ToList();

      Assert.Empty(results);
    }
    finally
    {
      Directory.Delete(fixture.DirectoryPath, true);
    }
  }

  [Fact]
  public async Task VerifyContentActionValidator_WithRelativePath_UsesTestCaseDirectory()
  {
    var fixture = await CreateFixtureAsync("expected.json", "{\"name\":\"Alice\"}");

    try
    {
      var validator = new VerifyContentActionValidator();
      var instruction = CreateInstruction(
        Constants.Actions.VerifyContent,
        file: "expected.json",
        contentType: Constants.ContentTypes.Json
      );
      instruction.TestStep.TestCase.File = fixture.TestCaseFile;

      var results = (await validator.ValidateAsync(instruction, CancellationToken.None)).ToList();

      Assert.Empty(results);
    }
    finally
    {
      Directory.Delete(fixture.DirectoryPath, true);
    }
  }

  private static TestStepInstruction CreateInstruction(
    string action,
    string file = "",
    string value = "",
    string contentType = "application/json"
  )
  {
    return new TestStepInstruction(new TestStep { Id = 1, TestCase = new TestCase() })
    {
      Action = action,
      File = file,
      Value = value,
      ContentType = contentType
    };
  }

  private static async Task<(string DirectoryPath, string TestCaseFile, string ContentFile)> CreateFixtureAsync(
    string contentFileName,
    string content
  )
  {
    var directoryPath = Path.Combine(Path.GetTempPath(), $"tapir-tests-{Guid.NewGuid():N}");
    Directory.CreateDirectory(directoryPath);

    var testCaseFile = Path.Combine(directoryPath, "TC-Users-001.md");
    await File.WriteAllTextAsync(
      testCaseFile,
      "# TC-Users-001: Test" + Environment.NewLine + "- **Type**: Definition",
      Encoding.UTF8
    );

    var contentFile = Path.Combine(directoryPath, contentFileName);
    await File.WriteAllTextAsync(contentFile, content, Encoding.UTF8);

    return (directoryPath, testCaseFile, contentFile);
  }
}
