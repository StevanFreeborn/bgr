using Microsoft.Extensions.Logging;

using Spectre.Console.Cli;

namespace BGR.Console.Tests.Unit;

public class RemovalCommandTests : IDisposable
{
  private bool _isDisposed;
  private readonly Mock<IModelFactory> _modelFactoryMock = new();
  private readonly Mock<ImageProcessor> _imageProcessorMock = new();
  private readonly Mock<IInferenceRunner> _inferenceRunnerMock = new();
  private readonly TestConsole _console = new();
  private readonly Mock<ILogger<RemovalCommand>> _loggerMock = new();
  private readonly RemovalCommand _sut;

  public RemovalCommandTests()
  {
    _sut = new RemovalCommand(
      _modelFactoryMock.Object,
      _imageProcessorMock.Object,
      _inferenceRunnerMock.Object,
      _console,
      _loggerMock.Object
    );
  }

  [Theory]
  [InlineData("output.png", false)]
  [InlineData("", true)]
  public async Task ExecuteAsync_WhenCalled_ItShouldProcessImage(string outputPath, bool includeMask)
  {
    var imagePath = $"{Guid.NewGuid()}.png";

    using var testImage = new Image<Rgba32>(100, 100);
    await testImage.SaveAsPngAsync(imagePath);

    var resourceName = "u2net.onnx";

    var settings = new RemovalCommand.Settings()
    {
      Image = imagePath,
      Model = "u2net",
      IncludeMask = includeMask,
      Output = outputPath,
    };

    var model = new U2NetModel([1, 2, 3]);
    var image = new SharpImage(100, 100, new MemoryStream());
    var inputTensor = new OnnxTensor(1, 3, 320, 320);
    var outputTensor = new OnnxTensor(1, 1, 320, 320);
    var maskStream = new MemoryStream();
    var outputStream = new MemoryStream();

    _modelFactoryMock
      .Setup(m => m.Create(resourceName))
      .Returns(model);

    _imageProcessorMock
      .Setup(p => p.LoadImageAsync(imagePath))
      .ReturnsAsync(image);

    _imageProcessorMock
      .Setup(p => p.CreateTensorInputAsync(image.Data, model))
      .ReturnsAsync(inputTensor);

    _inferenceRunnerMock
      .Setup(r => r.Run(model.Bytes, inputTensor))
      .Returns(outputTensor);

    _imageProcessorMock
      .Setup(p => p.GenerateMaskAsync(outputTensor, image.Width, image.Height))
      .ReturnsAsync(maskStream);

    _imageProcessorMock
      .Setup(p => p.RemoveBackgroundAsync(image.Data, maskStream))
      .ReturnsAsync(outputStream);

    var commandContext = new CommandContext(
      [],
      new Mock<IRemainingArguments>().Object,
      "test",
      new object()
    );

    var result = await _sut.ExecuteAsync(commandContext, settings);

    result.ShouldBe(0);

    _modelFactoryMock.Verify(m => m.Create(resourceName), Times.Once);
    _imageProcessorMock.Verify(p => p.LoadImageAsync(imagePath), Times.Once);
    _imageProcessorMock.Verify(p => p.CreateTensorInputAsync(image.Data, model), Times.Once);
    _inferenceRunnerMock.Verify(r => r.Run(model.Bytes, inputTensor), Times.Once);
    _imageProcessorMock.Verify(p => p.GenerateMaskAsync(outputTensor, image.Width, image.Height), Times.Once);
    _imageProcessorMock.Verify(p => p.RemoveBackgroundAsync(image.Data, maskStream), Times.Once);
    _imageProcessorMock.Verify(p => p.SaveImageAsync(outputStream, It.IsAny<string>()), Times.AtLeastOnce);

    File.Delete(imagePath);
  }

  [Fact]
  public void Validate_WhenCalledAndFileDoesNotExist_ItShouldReturnError()
  {
    var settings = new RemovalCommand.Settings()
    {
      Image = "invalid.png",
      Model = "u2net",
      IncludeMask = false,
      Output = "output.png",
    };

    var result = settings.Validate();

    result.Successful.ShouldBeFalse();
  }

  [Fact]
  public void Validate_WhenCalledAndModelIsInvalid_ItShouldReturnError()
  {
    var imagePath = $"{Guid.NewGuid()}.png";

    using var testImage = new Image<Rgba32>(100, 100);
    testImage.SaveAsPng(imagePath);

    var settings = new RemovalCommand.Settings()
    {
      Image = imagePath,
      Model = "invalid",
      IncludeMask = false,
      Output = "output.png",
    };

    var result = settings.Validate();

    result.Successful.ShouldBeFalse();
  }

  [Fact]
  public void Validate_WhenCalledAndSettingsValid_ItShouldReturnSuccess()
  {
    var imagePath = $"{Guid.NewGuid()}.png";

    using var testImage = new Image<Rgba32>(100, 100);
    testImage.SaveAsPng(imagePath);

    var settings = new RemovalCommand.Settings()
    {
      Image = imagePath,
      Model = "u2net",
      IncludeMask = false,
      Output = "output.png",
    };

    var result = settings.Validate();

    result.Successful.ShouldBeTrue();

    File.Delete(imagePath);
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (_isDisposed)
    {
      return;
    }

    if (disposing)
    {
      _console.Dispose();
    }

    _isDisposed = true;
  }
}