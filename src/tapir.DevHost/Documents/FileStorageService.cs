namespace tomware.Tapir.DevHost.Documents;

public interface IFileStorageService
{
  Task<string> SaveFileAsync(Guid documentId, IFormFile file);
  Task<byte[]?> GetFileAsync(string filePath);
  Task DeleteFileAsync(string filePath);
}

public class LocalFileStorageService : IFileStorageService
{
  private readonly string _storagePath;
  private readonly ILogger<LocalFileStorageService> _logger;

  public LocalFileStorageService(ILogger<LocalFileStorageService> logger, IConfiguration configuration)
  {
    _logger = logger;
    _storagePath = configuration.GetValue<string>("FileStorage:Path") ?? Path.Combine(Directory.GetCurrentDirectory(), "Files", "Documents");

    // Ensure storage directory exists
    if (!Directory.Exists(_storagePath))
    {
      Directory.CreateDirectory(_storagePath);
      _logger.LogInformation("Created storage directory: {StoragePath}", _storagePath);
    }
  }

  public async Task<string> SaveFileAsync(Guid documentId, IFormFile file)
  {
    try
    {
      var fileName = $"{documentId}_{Path.GetFileName(file.FileName)}";
      var filePath = Path.Combine(_storagePath, fileName);

      using var stream = new FileStream(filePath, FileMode.Create);
      await file.CopyToAsync(stream);

      _logger.LogInformation("Saved file {FileName} to {FilePath}", fileName, filePath);
      return filePath;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to save file for document {DocumentId}", documentId);
      throw;
    }
  }

  public async Task<byte[]?> GetFileAsync(string filePath)
  {
    try
    {
      if (!File.Exists(filePath))
      {
        _logger.LogWarning("File not found: {FilePath}", filePath);
        return null;
      }

      return await File.ReadAllBytesAsync(filePath);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to read file: {FilePath}", filePath);
      return null;
    }
  }

  public async Task DeleteFileAsync(string filePath)
  {
    try
    {
      if (File.Exists(filePath))
      {
        await Task.Run(() => File.Delete(filePath));
        _logger.LogInformation("Deleted file: {FilePath}", filePath);
      }
      else
      {
        _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
      throw;
    }
  }
}