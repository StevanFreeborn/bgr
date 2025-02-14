namespace BGR.Console.Removal.Models;

internal class ModelFactory(IResourceManager resourceManager) : IModelFactory
{
  private readonly IResourceManager _resourceManager = resourceManager;

  public Model Create(string resourceName)
  {
    var resource = _resourceManager.GetResource(resourceName);
    var model = new byte[resource.Length];
    resource.ReadExactly(model);

    return resourceName switch
    {
      $"{U2NetModel.Id}.onnx" => new U2NetModel(model),
      $"{RmbgModel.Id}.onnx" => new RmbgModel(model),
      $"{ModNetModel.Id}.onnx" => new ModNetModel(model),
      _ => throw new ArgumentException($"Unknown model name: {resourceName}")
    };
  }
}