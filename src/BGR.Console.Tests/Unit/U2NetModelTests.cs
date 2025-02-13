namespace BGR.Console.Tests.Unit;

public class U2NetModelTests
{
  private readonly byte[] _sampleModelBytes = [0x01, 0x02, 0x03];
  private readonly U2NetModel _sut;

  public U2NetModelTests()
  {
    _sut = new U2NetModel(_sampleModelBytes);
  }

  [Fact]
  public void Id_WhenCalled_ItShouldReturnU2NetId()
  {
    U2NetModel.Id.ShouldBe("u2net");
  }

  [Fact]
  public void InputWidth_WhenCalled_ItShouldReturn320()
  {
    _sut.InputWidth.ShouldBe(320);
  }

  [Fact]
  public void InputHeight_WhenCalled_ItShouldReturn320()
  {
    _sut.InputHeight.ShouldBe(320);
  }

  [Fact]
  public void RedNormalizationMean_WhenCalled_ItShouldHaveCorrectValue()
  {
    _sut.RedNormalizationMean.ShouldBe(0.485f);
  }

  [Fact]
  public void GreenNormalizationMean_WhenCalled_ItShouldHaveCorrectValue()
  {
    _sut.GreenNormalizationMean.ShouldBe(0.456f);
  }

  [Fact]
  public void BlueNormalizationMean_WhenCalled_ItShouldHaveCorrectValue()
  {
    _sut.BlueNormalizationMean.ShouldBe(0.406f);
  }

  [Fact]
  public void RedNormalizationStd_WhenCalled_ItShouldHaveCorrectValue()
  {
    _sut.RedNormalizationStd.ShouldBe(0.229f);
  }

  [Fact]
  public void GreenNormalizationStd_WhenCalled_ItShouldHaveCorrectValue()
  {
    _sut.GreenNormalizationStd.ShouldBe(0.224f);
  }

  [Fact]
  public void BlueNormalizationStd_WhenCalled_ItShouldHaveCorrectValue()
  {
    _sut.BlueNormalizationStd.ShouldBe(0.225f);
  }
}