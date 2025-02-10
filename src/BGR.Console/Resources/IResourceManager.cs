namespace BGR.Console.Resources;

internal interface IResourceManager
{
  Stream GetResource(string resourceName);
}