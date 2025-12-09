using Microsoft.AspNetCore.Mvc;

namespace tomware.Tapir.DevHost.Documents;

internal static class DocumentsHandlers
{
  public static IResult GetAllDocuments(IDocumentsRepository repository)
  {
    var documents = repository.GetAll();
    return Results.Ok(documents);
  }

  public static IResult GetDocumentById(
    Guid id,
    IDocumentsRepository repository
  )
  {
    var document = repository.GetById(id);
    if (document == null)
    {
      return Results.NotFound($"Document with ID {id} not found.");
    }

    return Results.Ok(document);
  }

  public static async Task<IResult> CreateDocument(
    IFormFile file,
    [FromForm] string title,
    IDocumentsRepository repository,
    IFileStorageService fileStorage
  )
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Results.BadRequest("Title is required.");
    }

    if (file == null || file.Length == 0)
    {
      return Results.BadRequest("PDF file is required.");
    }

    if (file.ContentType != "application/pdf")
    {
      return Results.BadRequest("Only PDF files are allowed.");
    }

    // Validate file size (max 10MB)
    if (file.Length > 10 * 1024 * 1024)
    {
      return Results.BadRequest("File size cannot exceed 10MB.");
    }

    var documentId = Guid.NewGuid();
    var filePath = await fileStorage.SaveFileAsync(documentId, file);

    var document = new Document
    {
      Id = documentId,
      Title = title,
      FilePath = filePath,
      FileSize = file.Length,
      ContentType = file.ContentType,
      CreatedAt = DateTime.UtcNow
    };

    var id = repository.Add(document);

    return Results.Created($"/documents/{id}", id);
  }

  public static async Task<IResult> UpdateDocument(
    Guid id,
    IFormFile? file,
    [FromForm] string title,
    IDocumentsRepository repository,
    IFileStorageService fileStorage
  )
  {
    var existingDocument = repository.GetById(id);
    if (existingDocument == null)
    {
      return Results.NotFound($"Document with ID {id} not found.");
    }

    if (string.IsNullOrWhiteSpace(title))
    {
      return Results.BadRequest("Title is required.");
    }

    var updatedDocument = existingDocument;

    // If a new file is provided, validate and store it
    if (file != null && file.Length > 0)
    {
      if (file.ContentType != "application/pdf")
      {
        return Results.BadRequest("Only PDF files are allowed.");
      }

      if (file.Length > 10 * 1024 * 1024)
      {
        return Results.BadRequest("File size cannot exceed 10MB.");
      }

      // Delete old file and save new one
      await fileStorage.DeleteFileAsync(existingDocument.FilePath);
      var newFilePath = await fileStorage.SaveFileAsync(id, file);

      updatedDocument = existingDocument with
      {
        Title = title,
        FilePath = newFilePath,
        FileSize = file.Length,
        ContentType = file.ContentType
      };
    }
    else
    {
      // Only update title if no new file provided
      updatedDocument = existingDocument with { Title = title };
    }

    repository.Update(updatedDocument);
    return Results.Ok(updatedDocument);
  }

  public static async Task<IResult> DeleteDocument(
    Guid id,
    IDocumentsRepository repository,
    IFileStorageService fileStorage
  )
  {
    var document = repository.GetById(id);
    if (document == null)
    {
      return Results.NotFound($"Document with ID {id} not found.");
    }

    // Delete the physical file
    await fileStorage.DeleteFileAsync(document.FilePath);

    repository.Delete(id);
    return Results.NoContent();
  }

  public static async Task<IResult> DownloadDocument(
    Guid id,
    IDocumentsRepository repository,
    IFileStorageService fileStorage
  )
  {
    var document = repository.GetById(id);
    if (document == null)
    {
      return Results.NotFound($"Document with ID {id} not found.");
    }

    var fileBytes = await fileStorage.GetFileAsync(document.FilePath);
    if (fileBytes == null)
    {
      return Results.NotFound("File not found on disk.");
    }

    return Results.File(fileBytes, document.ContentType, $"{document.Title}.pdf");
  }
}