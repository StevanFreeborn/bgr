namespace BGR.Console.Tests.Unit;

public class ResourceManagerTests
{
  private readonly ResourceManager _resourceManager = new();

  [Fact]
  public void GetResource_WhenResourceExists_ItShouldReturnStream()
  {
    var resourceName = "u2net.onnx";

    using var stream = _resourceManager.GetResource(resourceName);

    stream.ShouldNotBeNull();
  }

  [Fact]
  public void GetResource_WhenResourceDoesNotExist_ItShouldThrowException()
  {
    var resourceName = "nonexistent.onnx";

    var act = () => _resourceManager.GetResource(resourceName);

    act.ShouldThrow<FileNotFoundException>();
  }
}