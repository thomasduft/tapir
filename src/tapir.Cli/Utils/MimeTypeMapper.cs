namespace tomware.Tapir.Cli.Utils;

internal static class MimeTypeMapper
{
  private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
  {
    { ".txt", "text/plain" },
    { ".pdf", "application/pdf" },
    { ".json", "application/json" },
    { ".xml", "application/xml" },
    { ".html", "text/html" },
    { ".csv", "text/csv" },
    { ".jpg", "image/jpeg" },
    { ".jpeg", "image/jpeg" },
    { ".png", "image/png" },
    { ".gif", "image/gif" }
  };

  public static string GetContentType(string filePath)
  {
    var extension = Path.GetExtension(filePath);
    return !string.IsNullOrEmpty(extension) && MimeTypes.TryGetValue(extension, out var contentType)
      ? contentType
      : "application/octet-stream";
  }
}
