
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BGR.Console.Tests.Unit;

public class OnnxInferenceRunnerTests
{
  private const string TestModel = "u2net.onnx";
  private readonly ResourceManager _resourceManager = new();
  private readonly OnnxInferenceRunner _sut = new();
  private readonly byte[] _sampleModelBytes;
  private readonly Mock<ITensor<float>> _mockInputTensor = new();

  public OnnxInferenceRunnerTests()
  {
    var modelStream = _resourceManager.GetResource(TestModel);
    var modelBytes = new byte[modelStream.Length];
    modelStream.ReadExactly(modelBytes);
    _sampleModelBytes = modelBytes;

    _mockInputTensor
      .Setup(static x => x.ToTensor())
      .Returns(new DenseTensor<float>([1, 3, 320, 320]));
  }

  [Fact]
  public void Run_WhenCalledWithValidInput_ItShouldReturnOutput()
  {
    var result = _sut.Run(_sampleModelBytes, _mockInputTensor.Object);

    result.ShouldBeOfType<OnnxTensor>();
    result.ShouldNotBeNull();
  }
}