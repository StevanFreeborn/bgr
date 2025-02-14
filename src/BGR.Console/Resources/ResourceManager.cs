namespace BGR.Console.Resources;

internal sealed class ResourceManager : IResourceManager
{
  public Stream GetResource(string resourceName)
  {
    var name = $"{nameof(BGR)}.{nameof(Console)}.{nameof(Resources)}.Files.{resourceName}";
    var assembly = Assembly.GetExecutingAssembly();
    var names = assembly.GetManifestResourceNames();
    var stream = assembly.GetManifestResourceStream(name);
    return stream ?? throw new FileNotFoundException("Model not found in embedded resources.");
  }
}