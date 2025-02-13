namespace BGR.Console.Tests.Unit;

public class ModelTests
{
  internal sealed class TestModel(byte[] modelBytes) : Model(modelBytes)
  {
    public override int InputWidth => 100;
    public override int InputHeight => 100;
    public override float RedNormalizationMean => 0.5f;
    public override float GreenNormalizationMean => 0.5f;
    public override float BlueNormalizationMean => 0.5f;
    public override float RedNormalizationStd => 0.25f;
    public override float GreenNormalizationStd => 0.25f;
    public override float BlueNormalizationStd => 0.25f;
  }

  private readonly byte[] _sampleBytes = [0x01, 0x02, 0x03];
  private readonly TestModel _sut;
  private const float Delta = 0.00001f;

  public ModelTests()
  {
    _sut = new TestModel(_sampleBytes);
  }

  [Theory]
  [InlineData(0f)]
  [InlineData(127.5f)]
  [InlineData(255f)]
  public void NormalizeRed_WhenCalled_ItShouldNormalizeCorrectly(float value)
  {
    var result = _sut.NormalizeRed(value);

    var expected = ((value / 255f) - _sut.RedNormalizationMean) / _sut.RedNormalizationStd;
    result.ShouldBe(expected, Delta);
  }

  [Theory]
  [InlineData(0f)]
  [InlineData(127.5f)]
  [InlineData(255f)]
  public void NormalizeGreen_WhenCalled_ItShouldNormalizeCorrectly(float value)
  {
    var result = _sut.NormalizeGreen(value);

    var expected = ((value / 255f) - _sut.GreenNormalizationMean) / _sut.GreenNormalizationStd;
    result.ShouldBe(expected, Delta);
  }

  [Theory]
  [InlineData(0f)]
  [InlineData(127.5f)]
  [InlineData(255f)]
  public void NormalizeBlue_WhenCalled_ItShouldNormalizeCorrectly(float value)
  {
    var result = _sut.NormalizeBlue(value);

    var expected = ((value / 255f) - _sut.BlueNormalizationMean) / _sut.BlueNormalizationStd;
    result.ShouldBe(expected, Delta);
  }

  [Theory]
  [InlineData(-1f)]
  [InlineData(256f)]
  public void NormalizeRed_WhenCalledWithOutOfRangeValues_ItShouldStillNormalize(float value)
  {
    var result = _sut.NormalizeRed(value);

    var expected = ((value / 255f) - _sut.RedNormalizationMean) / _sut.RedNormalizationStd;
    result.ShouldBe(expected, Delta);
  }

  [Theory]
  [InlineData(-1f)]
  [InlineData(256f)]
  public void NormalizeGreen_WhenCalledWithOutOfRangeValues_ItShouldStillNormalize(float value)
  {
    var result = _sut.NormalizeGreen(value);

    var expected = ((value / 255f) - _sut.GreenNormalizationMean) / _sut.GreenNormalizationStd;
    result.ShouldBe(expected, Delta);
  }

  [Theory]
  [InlineData(-1f)]
  [InlineData(256f)]
  public void NormalizeBlue_WhenCalledWithOutOfRangeValues_ItShouldStillNormalize(float value)
  {
    var result = _sut.NormalizeBlue(value);

    var expected = ((value / 255f) - _sut.BlueNormalizationMean) / _sut.BlueNormalizationStd;
    result.ShouldBe(expected, Delta);
  }
}