using System.Reflection;

namespace tomware.Tapir.Cli.Utils;

internal static class Templates
{
  public const string Manual = "Manual";
  public const string TestCase = "TestCase";
  public const string TestCaseSimple = "TestCaseSimple";
  public const string TestStep = "TestStep";
  public const string Report = "Report";
}

internal static class ResourceLoader
{
  public static string GetTemplate(string template)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resourcePath = assembly?.ManifestModule.Name.Replace(".dll", string.Empty);
    var resourceName = $"{resourcePath}.Templates.{template}.liquid";

    using (Stream stream = assembly!.GetManifestResourceStream(resourceName)!)
    using (StreamReader reader = new(stream!))
    {
      return reader.ReadToEnd();
    }

    throw new FileNotFoundException($"Template with name '{template}' does not exist!");
  }

  public static string GetHtmlTemplate(string template)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resourcePath = assembly?.ManifestModule.Name.Replace(".dll", string.Empty);
    var resourceName = $"{resourcePath}.Templates.{template}.html";

    using var stream = assembly!.GetManifestResourceStream(resourceName)
      ?? throw new FileNotFoundException($"HTML template '{template}' not found as embedded resource.");
    using var reader = new StreamReader(stream);

    return reader.ReadToEnd();
  }
}