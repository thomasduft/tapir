using System.Net.Http.Headers;
using System.Text;

using Serilog;

using tomware.Tapir.Cli.Utils;

namespace tomware.Tapir.Cli.Domain;

internal class MultipartFormDataRequestContentHandler : IRequestContentHandler
{
  public string ContentType => Constants.ContentTypes.MultipartFormData;

  public async Task<HttpContent> CreateAsync(
    IReadOnlyList<TestStepInstruction> instructions,
    CancellationToken cancellationToken
  )
  {
    var multipartContent = new MultipartFormDataContent();

    foreach (var instruction in instructions)
    {
      if (!string.IsNullOrEmpty(instruction.File))
      {
        var resolvedFilePath = TestCaseContentFileResolver.LocateExistingFile(instruction);
        var byteArrayContent = new ByteArrayContent(
          await File.ReadAllBytesAsync(resolvedFilePath, cancellationToken)
        );
        byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
          MimeTypeMapper.GetContentType(resolvedFilePath)
        );

        Log.Logger.Verbose(
          "  - adding multipart file content: {FilePath} as field '{FieldName}' with filename '{FileName}'",
          resolvedFilePath,
          instruction.Name ?? "file",
          instruction.Value ?? Path.GetFileName(resolvedFilePath)
        );

        multipartContent.Add(
          byteArrayContent,
          instruction.Name ?? "file",
          instruction.Value ?? Path.GetFileName(resolvedFilePath)
        );
        continue;
      }

      if (!string.IsNullOrEmpty(instruction.Name) && !string.IsNullOrEmpty(instruction.Value))
      {
        Log.Logger.Verbose(
          "  - adding multipart form data field: {FieldName}={FieldValue}",
          instruction.Name,
          instruction.Value
        );

        var stringContent = new StringContent(instruction.Value, Encoding.UTF8);
        multipartContent.Add(stringContent, instruction.Name);
      }
    }

    return multipartContent;
  }
}
