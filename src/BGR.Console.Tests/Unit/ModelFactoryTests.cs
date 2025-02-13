namespace BGR.Console.Tests.Unit;

public class ModelFactoryTests
{
  private readonly Mock<IResourceManager> _resourceManagerMock;
  private readonly ModelFactory _sut;
  private readonly byte[] _sampleModelBytes = [0x01, 0x02, 0x03];

  public ModelFactoryTests()
  {
    _resourceManagerMock = new Mock<IResourceManager>();
    _sut = new ModelFactory(_resourceManagerMock.Object);
  }

  [Theory]
  [InlineData("u2net.onnx", typeof(U2NetModel))]
  [InlineData("rmbg.onnx", typeof(RmbgModel))]
  [InlineData("modnet.onnx", typeof(ModNetModel))]
  public void Create_WhenCalledWithValidModelName_ItShouldReturnCorrectModelType(string resourceName, Type expectedType)
  {
    SetupResourceManagerMock(resourceName);

    var result = _sut.Create(resourceName);

    result.ShouldBeOfType(expectedType);
    VerifyResourceManagerCalled(resourceName);
  }

  [Fact]
  public void Create_WhenCalledWithUnknownModel_ItShouldThrowArgumentException()
  {
    var resourceName = "unknown.onnx";
    SetupResourceManagerMock(resourceName);

    var exception = Should.Throw<ArgumentException>(() => _sut.Create(resourceName));

    exception.Message.ShouldBe($"Unknown model name: {resourceName}");
    VerifyResourceManagerCalled(resourceName);
  }

  [Fact]
  public void Create_WhenCalledWithU2NetModel_ItShouldReadTheResourceStreamToTheEnd()
  {
    var resourceName = $"{U2NetModel.Id}.onnx";
    var memoryStream = new MemoryStream(_sampleModelBytes);

    _resourceManagerMock.Setup(x => x.GetResource(resourceName))
        .Returns(memoryStream);

    _sut.Create(resourceName);

    memoryStream.Position.ShouldBe(memoryStream.Length);
  }

  private void SetupResourceManagerMock(string resourceName)
  {
    var memoryStream = new MemoryStream(_sampleModelBytes);

    _resourceManagerMock.Setup(x => x.GetResource(resourceName))
      .Returns(memoryStream);
  }

  private void VerifyResourceManagerCalled(string resourceName)
  {
    _resourceManagerMock.Verify(x => x.GetResource(resourceName), Times.Once);
  }
}