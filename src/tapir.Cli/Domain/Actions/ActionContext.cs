using System.Collections.Specialized;

namespace tomware.Tapir.Cli.Domain;

internal record ActionContext(
  TestStepInstruction Instruction,
  NameValueCollection Headers,
  NameValueCollection QueryParameters,
  IDictionary<string, string> Variables
)
{
  public StringContent StringContent { get; set; } = new StringContent(string.Empty);
  public HttpMethod Method { get; set; } = HttpMethod.Get;
  public string Endpoint { get; set; } = string.Empty;
}
