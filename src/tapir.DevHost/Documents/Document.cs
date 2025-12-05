using System.ComponentModel.DataAnnotations;

namespace tomware.Tapir.DevHost.Documents;

public record Document
{
  public Guid Id { get; init; }

  [Required]
  [MaxLength(100)]
  public string Title { get; init; } = string.Empty;

  [Required]
  public string FilePath { get; init; } = string.Empty;

  public long FileSize { get; init; }

  public string ContentType { get; init; } = "application/pdf";

  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
