using System.ComponentModel.DataAnnotations;

namespace tomware.Tapir.DevHost.Users;

public record User
{
  public Guid Id { get; init; }

  [Required]
  [MaxLength(50)]
  public string Name { get; init; } = string.Empty;

  [Required]
  [Range(0, 150)]
  public int Age { get; init; }
}