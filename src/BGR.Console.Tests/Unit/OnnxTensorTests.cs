using BGR.Console.Removal.Onnx;

using Microsoft.ML.OnnxRuntime.Tensors;

namespace BGR.Console.Tests.Unit;

public class OnnxTensorTests
{
  private const int DefaultBatchSize = 1;
  private const int DefaultChannels = 3;
  private const int DefaultHeight = 4;
  private const int DefaultWidth = 5;

  [Fact]
  public void Constructor_WhenCalledWithDimensions_ItShouldCreateTensorWithCorrectDimensions()
  {
    var tensor = new OnnxTensor(DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth);

    tensor.Height.ShouldBe(DefaultHeight);
    tensor.Width.ShouldBe(DefaultWidth);
  }

  [Fact]
  public void Constructor_WhenCalledWithExistingTensor_ItShouldCreateTensorWithSameDimensions()
  {
    var existingTensor = new DenseTensor<float>([DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth]);

    var tensor = new OnnxTensor(existingTensor);

    tensor.Height.ShouldBe(DefaultHeight);
    tensor.Width.ShouldBe(DefaultWidth);
  }

  [Theory]
  [InlineData(0, 0, 0, 0, 1.0f)]
  [InlineData(0, 1, 2, 3, 2.5f)]
  [InlineData(0, 2, 3, 4, -1.0f)]
  public void SetValue_WhenCalledWithValidValues_ItShouldSetCorrectValueAtPosition(int batch, int channel, int y, int x, float expectedValue)
  {
    var tensor = new OnnxTensor(DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth);

    tensor.SetValue(batch, channel, y, x, expectedValue);
    var actualValue = tensor.GetValue(batch, channel, y, x);

    actualValue.ShouldBe(expectedValue);
  }

  [Theory]
  [InlineData(0, 0, 0, 0, 1.0f)]
  [InlineData(0, 1, 2, 3, 2.5f)]
  [InlineData(0, 2, 3, 4, -1.0f)]
  public void GetValue_WhenCalledWithValidValues_ItShouldReturnCorrectValue(int batch, int channel, int y, int x, float value)
  {
    var tensor = new OnnxTensor(DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth);
    tensor.SetValue(batch, channel, y, x, value);

    var result = tensor.GetValue(batch, channel, y, x);

    result.ShouldBe(value);
  }

  [Fact]
  public void ToTensor_WhenCalled_ItShouldReturnUnderlyingTensor()
  {
    var tensor = new OnnxTensor(DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth);
    const float testValue = 42.0f;
    tensor.SetValue(0, 0, 0, 0, testValue);

    var result = tensor.ToTensor();

    result.ShouldBeOfType<DenseTensor<float>>();
    result[0, 0, 0, 0].ShouldBe(testValue);
  }

  [Theory]
  [InlineData(-1, 0, 0, 0)]
  [InlineData(1, 3, 0, 0)]
  [InlineData(0, 0, 4, 0)]
  [InlineData(0, 0, 0, 5)]
  public void SetValue_WhenCalledWithInvalidIndices_ItShouldThrowIndexOutOfRangeException(int batch, int channel, int y, int x)
  {
    var tensor = new OnnxTensor(DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth);

    try
    {
      tensor.SetValue(batch, channel, y, x, 0);
    }
    catch (IndexOutOfRangeException ex)
    {
      ex.Message.ShouldBe("Index was outside the bounds of the array.");
    }
  }

  [Theory]
  [InlineData(-1, 0, 0, 0)]
  [InlineData(1, 3, 0, 0)]
  [InlineData(0, 0, 4, 0)]
  [InlineData(0, 0, 0, 5)]
  public void GetValue_WhenCalledWithInvalidIndices_ItShouldThrowIndexOutOfRangeException(int batch, int channel, int y, int x)
  {
    var tensor = new OnnxTensor(DefaultBatchSize, DefaultChannels, DefaultHeight, DefaultWidth);

    try
    {
      tensor.GetValue(batch, channel, y, x);
    }
    catch (IndexOutOfRangeException ex)
    {
      ex.Message.ShouldBe("Index was outside the bounds of the array.");
    }
  }

  [Fact]
  public void Constructor_WhenCalledWithZeroDimension_ItShouldCreateEmptyTensor()
  {
    var tensor = new OnnxTensor(0, 0, 0, 0);

    tensor.Height.ShouldBe(0);
    tensor.Width.ShouldBe(0);
  }
}