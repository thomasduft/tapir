namespace tomware.Tapir.DevHost.Documents;

internal static class Endpoints
{
  public static void MapDocumentsEndpoints(this IEndpointRouteBuilder builder)
  {
    builder
      .MapGet("/documents", DocumentsHandlers.GetAllDocuments)
      .WithTags("Documents")
      .WithDescription("Retrieves all PDF documents from the repository.");

    builder
      .MapGet("/documents/{id:guid}", DocumentsHandlers.GetDocumentById)
      .WithTags("Documents")
      .WithDescription("Retrieves a PDF document by its ID.");

    builder
      .MapPost("/documents", DocumentsHandlers.CreateDocument)
      .WithTags("Documents")
      .WithDescription("Uploads and creates a new PDF document.")
      .DisableAntiforgery()
      .Accepts<IFormFile>("multipart/form-data");

    builder
      .MapPut("/documents/{id:guid}", DocumentsHandlers.UpdateDocument)
      .WithTags("Documents")
      .WithDescription("Updates an existing PDF document. Optionally upload a new file.")
      .DisableAntiforgery()
      .Accepts<IFormFile>("multipart/form-data");

    builder
      .MapDelete("/documents/{id:guid}", DocumentsHandlers.DeleteDocument)
      .WithTags("Documents")
      .WithDescription("Deletes a PDF document by its ID.");

    builder
      .MapGet("/documents/{id:guid}/download", DocumentsHandlers.DownloadDocument)
      .WithTags("Documents")
      .WithDescription("Downloads the PDF file for a document.");
  }
}
