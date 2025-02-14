namespace BGR.Console.Tests.Unit;

public class ImageSharpProcessorTests : IDisposable
{
  private const string TestImagePath = "test.jpg";
  private bool _isDisposed;
  private readonly ImageSharpProcessor _sut = new();
  private readonly Mock<Model> _modelMock = new();
  private readonly Stream _testImageStream;

  public ImageSharpProcessorTests()
  {
    if (File.Exists(TestImagePath) is false)
    {
      using var testImage = new Image<Rgba32>(100, 100);
      testImage.SaveAsJpeg(TestImagePath);
    }

    _modelMock.Setup(static x => x.InputWidth).Returns(320);
    _modelMock.Setup(static x => x.InputHeight).Returns(320);

    var stream = new MemoryStream();
    using var image = new Image<Rgba32>(100, 100);

    for (var y = 0; y < image.Height; y++)
    {
      for (var x = 0; x < image.Width; x++)
      {
        image[x, y] = new Rgba32((byte)x, (byte)y, 128, 255);
      }
    }

    image.SaveAsPng(stream);
    stream.Position = 0;
    _testImageStream = stream;
  }

  [Fact]
  public async Task LoadImageAsync_WhenCalledWithValidPath_ItShouldReturnImage()
  {
    var result = await _sut.LoadImageAsync(TestImagePath);

    result.ShouldBeOfType<SharpImage>();
    result.ShouldNotBeNull();
    result.Width.ShouldBe(100);
    result.Height.ShouldBe(100);
    result.Data.Length.ShouldBeGreaterThan(0);
  }

  [Fact]
  public async Task LoadImageAsync_WhenCalledWithValidPath_ItShouldReturnReusableStream()
  {
    var result = await _sut.LoadImageAsync(TestImagePath);

    result.Data.Position.ShouldBe(0);
    result.Data.CanRead.ShouldBeTrue();

    var buffer = new byte[100];
    await result.Data.ReadExactlyAsync(buffer);

    result.Data.Position = 0;
    await result.Data.ReadExactlyAsync(buffer);
  }

  [Fact]
  public async Task CreateTensorInputAsync_WhenCalled_ItShouldResizeImageToModelDimensions()
  {
    const int modelWidth = 64;
    const int modelHeight = 48;

    _modelMock.Setup(static x => x.InputWidth).Returns(modelWidth);
    _modelMock.Setup(static x => x.InputHeight).Returns(modelHeight);

    var result = await _sut.CreateTensorInputAsync(_testImageStream, _modelMock.Object);

    result.Width.ShouldBe(modelWidth);
    result.Height.ShouldBe(modelHeight);
  }

  [Fact]
  public async Task CreateTensorInputAsync_WhenCalled_ItShouldCreateTensorWithCorrectDimensions()
  {
    var result = await _sut.CreateTensorInputAsync(_testImageStream, _modelMock.Object);

    result.ShouldBeOfType<OnnxTensor>();
    Should.NotThrow(() => result.GetValue(0, 2, 0, 0));
  }

  [Fact]
  public async Task CreateTensorInputAsync_WhenCalled_ItShouldNormalizePixelValues()
  {
    var normalizedValue = 0.5f;
    _modelMock.Setup(static x => x.NormalizeRed(It.IsAny<float>())).Returns(normalizedValue);
    _modelMock.Setup(static x => x.NormalizeGreen(It.IsAny<float>())).Returns(normalizedValue);
    _modelMock.Setup(static x => x.NormalizeBlue(It.IsAny<float>())).Returns(normalizedValue);

    var result = await _sut.CreateTensorInputAsync(_testImageStream, _modelMock.Object);

    for (var y = 0; y < result.Height; y++)
    {
      for (var x = 0; x < result.Width; x++)
      {
        result.GetValue(0, 0, y, x).ShouldBe(normalizedValue); // Red
        result.GetValue(0, 1, y, x).ShouldBe(normalizedValue); // Green
        result.GetValue(0, 2, y, x).ShouldBe(normalizedValue); // Blue
      }
    }
  }

  [Fact]
  public async Task CreateTensorInputAsync_WhenCalled_ItShouldCallNormalizeForEachChannel()
  {
    await _sut.CreateTensorInputAsync(_testImageStream, _modelMock.Object);

    _modelMock.Verify(static x => x.NormalizeRed(It.IsAny<float>()), Times.AtLeast(1));
    _modelMock.Verify(static x => x.NormalizeGreen(It.IsAny<float>()), Times.AtLeast(1));
    _modelMock.Verify(static x => x.NormalizeBlue(It.IsAny<float>()), Times.AtLeast(1));
  }

  [Fact]
  public async Task GenerateMaskAsync_WhenCalled_ItShouldCreateMaskWithCorrectDimensions()
  {
    const int width = 64;
    const int height = 48;

    var tensor = new OnnxTensor(1, 1, height, width);
    var stream = await _sut.GenerateMaskAsync(tensor, width, height);

    using var mask = await Image.LoadAsync<Rgba32>(stream);

    mask.Width.ShouldBe(width);
    mask.Height.ShouldBe(height);
  }

  [Fact]
  public async Task GenerateMaskAsync_WhenCalled_ItShouldCreateGreyscaleMask()
  {
    var tensor = new OnnxTensor(1, 1, 100, 100);
    var stream = await _sut.GenerateMaskAsync(tensor, 100, 100);

    using var mask = await Image.LoadAsync<Rgba32>(stream);

    for (var y = 0; y < mask.Height; y++)
    {
      for (var x = 0; x < mask.Width; x++)
      {
        // we expect the mask to be greyscale
        // so R, G, B should be equal
        mask[x, y].R.ShouldBe(mask[x, y].G);
        mask[x, y].G.ShouldBe(mask[x, y].B);
      }
    }
  }

  [Fact]
  public async Task RemoveBackgroundAsync_WithValidImageAndMask_ShouldReturnProcessedStream()
  {
    var width = 2;
    var height = 2;

    using var imageStream = new MemoryStream();
    using var image = new Image<Rgba32>(width, height);

    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        image[x, y] = new Rgba32(255, 0, 0, 255); // Red pixels
      }
    }

    await image.SaveAsPngAsync(imageStream);
    imageStream.Position = 0;

    using var maskStream = new MemoryStream();
    using var mask = new Image<Rgba32>(width, height);

    mask[0, 0] = new Rgba32(0, 0, 0, 255);
    mask[0, 1] = new Rgba32(255, 255, 255, 255);
    mask[1, 0] = new Rgba32(255, 255, 255, 255);
    mask[1, 1] = new Rgba32(255, 255, 255, 255);

    await mask.SaveAsPngAsync(maskStream);
    maskStream.Position = 0;

    var result = await _sut.RemoveBackgroundAsync(imageStream, maskStream);

    result.ShouldNotBeNull();
    result.Length.ShouldBeGreaterThan(0);

    result.Position = 0;
    using var resultImage = await Image.LoadAsync<Rgba32>(result);
    resultImage.Width.ShouldBe(width);
    resultImage.Height.ShouldBe(height);

    resultImage[0, 0].A.ShouldBe((byte)0);

    resultImage[0, 1].R.ShouldBe((byte)255);
    resultImage[0, 1].A.ShouldBe((byte)255);
    resultImage[1, 0].R.ShouldBe((byte)255);
    resultImage[1, 0].A.ShouldBe((byte)255);
    resultImage[1, 1].R.ShouldBe((byte)255);
    resultImage[1, 1].A.ShouldBe((byte)255);
  }

  [Fact]
  public async Task SaveImageAsync_WhenCalled_ItShouldSaveImageToDiskAtProvidedPath()
  {
    using var image = new Image<Rgba32>(100, 100);
    var stream = new MemoryStream();
    await image.SaveAsPngAsync(stream);

    var path = $"{Guid.NewGuid()}.png";
    await _sut.SaveImageAsync(stream, path);

    File.Exists(path).ShouldBeTrue();
    File.Delete(path);
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
      File.Delete(TestImagePath);
      _testImageStream.Dispose();
    }

    _isDisposed = true;
  }
}