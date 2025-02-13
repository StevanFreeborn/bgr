namespace BGR.Console.Tests.Unit;

public class SharpImageTests : IDisposable
{
  private bool _isDisposed;
  private const int DefaultWidth = 100;
  private const int DefaultHeight = 200;
  private readonly MemoryStream _sampleStream;

  public SharpImageTests()
  {
    _sampleStream = new MemoryStream([0x01, 0x02, 0x03]);
  }

  [Fact]
  public void Constructor_WhenCalled_ItShouldSetProperties()
  {
    var image = new SharpImage(DefaultWidth, DefaultHeight, _sampleStream);

    image.Width.ShouldBe(DefaultWidth);
    image.Height.ShouldBe(DefaultHeight);
    image.Data.ShouldBe(_sampleStream);
  }

  [Theory]
  [InlineData(1, 1)]
  [InlineData(1920, 1080)]
  [InlineData(int.MaxValue, int.MaxValue)]
  public void Constructor_WhenCalledWithDifferentDimensions_ItShouldSetCorrectValues(int width, int height)
  {
    var image = new SharpImage(width, height, _sampleStream);

    image.Width.ShouldBe(width);
    image.Height.ShouldBe(height);
  }

  [Fact]
  public void Constructor_WhenCalledWithNullStream_ItShouldThrowException()
  {
    var action = static () => new SharpImage(DefaultWidth, DefaultHeight, null!);

    action.ShouldThrow<ArgumentNullException>();
  }

  [Theory]
  [InlineData(0, 0)]
  [InlineData(-1, -1)]
  [InlineData(1, -1)]
  [InlineData(-1, 1)]
  public void Constructor_WhenCalledWithInvalidDimensions_ItShouldThrowException(int width, int height)
  {
    var action = () => new SharpImage(width, height, _sampleStream);

    action.ShouldThrow<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Data_WhenCalled_ItShouldReturnSameStreamInstance()
  {
    var stream = new MemoryStream();
    var image = new SharpImage(DefaultWidth, DefaultHeight, stream);

    image.Data.ShouldBeSameAs(stream);
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
      _sampleStream.Dispose();
    }

    _isDisposed = true;
  }
}