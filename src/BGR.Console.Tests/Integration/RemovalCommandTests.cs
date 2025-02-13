namespace BGR.Console.Tests.Integration;

public class RemovalCommandTests
{
  private readonly CommandApp<RemovalCommand> _app = AppFactory.Create();

  [Fact]
  public async Task RunAsync_WhenCalled_ItShouldRemoveImageBackground()
  {
    var imagePath = $"{Guid.NewGuid()}.png";
    var outputPath = $"{Guid.NewGuid()}.png";

    using var testImage = new Image<Rgba32>(100, 100);
    await testImage.SaveAsPngAsync(imagePath);

    var result = await _app.RunAsync([imagePath, "--model", "u2net", "--output", outputPath]);

    result.ShouldBe(0);
    File.Exists(outputPath).ShouldBeTrue();

    File.Delete(imagePath);
    File.Delete(outputPath);
  }
}