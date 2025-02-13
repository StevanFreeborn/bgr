namespace BGR.Console.Tests.Unit;

public class ModNetModelTests
{
  private readonly byte[] _sampleModelBytes = [0x01, 0x02, 0x03];
  private readonly ModNetModel _sut;

  public ModNetModelTests()
  {
    _sut = new ModNetModel(_sampleModelBytes);
  }

  [Fact]
  public void Id_WhenCalled_ItShouldReturnModNetId()
  {
    ModNetModel.Id.ShouldBe("modnet");
  }

  [Fact]
  public void InputWidth_WhenCalled_ItShouldReturn512()
  {
    _sut.InputWidth.ShouldBe(512);
  }

  [Fact]
  public void InputHeight_WhenCalled_ItShouldReturn512()
  {
    _sut.InputHeight.ShouldBe(512);
  }

  [Fact]
  public void RedNormalizationMean_WhenCalled_ItShouldReturnCorrectValue()
  {
    _sut.RedNormalizationMean.ShouldBe(0.485f);
  }

  [Fact]
  public void GreenNormalizationMean_WhenCalled_ItShouldReturnCorrectValue()
  {
    _sut.GreenNormalizationMean.ShouldBe(0.456f);
  }

  [Fact]
  public void BlueNormalizationMean_WhenCalled_ItShouldReturnCorrectValue()
  {
    _sut.BlueNormalizationMean.ShouldBe(0.406f);
  }

  [Fact]
  public void RedNormalizationStd_WhenCalled_ItShouldReturnCorrectValue()
  {
    _sut.RedNormalizationStd.ShouldBe(0.229f);
  }

  [Fact]
  public void GreenNormalizationStd_WhenCalled_ItShouldReturnCorrectValue()
  {
    _sut.GreenNormalizationStd.ShouldBe(0.224f);
  }

  [Fact]
  public void BlueNormalizationStd_WhenCalled_ItShouldReturnCorrectValue()
  {
    _sut.BlueNormalizationStd.ShouldBe(0.225f);
  }
}
