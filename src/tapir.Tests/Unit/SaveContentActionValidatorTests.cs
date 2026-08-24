using tomware.Tapir.Cli.Domain;

namespace tomware.Tapir.Tests.Unit;

public class SaveContentActionValidatorTests
{
  [Fact]
  public async Task ValidateAsync_WithFile_ShouldReturnValidResult()
  {
    // Arrange
    var validator = new SaveContentActionValidator();
    var instruction = CreateInstruction(file: "responses/user.json");

    // Act
    var results = (await validator.ValidateAsync(instruction, CancellationToken.None)).ToList();

    // Assert
    Assert.Empty(results);
  }

  [Fact]
  public async Task ValidateAsync_WithoutFile_ShouldReturnFileRequiredError()
  {
    // Arrange
    var validator = new SaveContentActionValidator();
    var instruction = CreateInstruction(file: "");

    // Act
    var results = (await validator.ValidateAsync(instruction, CancellationToken.None)).ToList();

    // Assert
    Assert.Single(results);
    Assert.Equal("File is required.", results[0].ErrorMessage);
  }

  private static TestStepInstruction CreateInstruction(string file)
  {
    return new TestStepInstruction(new TestStep { Id = 1, TestCase = new TestCase() })
    {
      Action = Constants.Actions.SaveContent,
      File = file
    };
  }
}
