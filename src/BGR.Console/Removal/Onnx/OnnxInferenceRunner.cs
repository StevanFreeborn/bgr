namespace BGR.Console.Removal.Onnx;

internal class OnnxInferenceRunner : IInferenceRunner
{
  public ITensor<float> Run(byte[] model, ITensor<float> inputTensor)
  {
    using var options = new SessionOptions() { LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR };
    using var session = new InferenceSession(model, options);
    var inputs = new List<NamedOnnxValue>()
    {
      NamedOnnxValue.CreateFromTensor(session.InputNames[0], inputTensor.ToTensor()),
    };

    var results = session.Run(inputs);
    var outputTensor = results[0].AsTensor<float>();
    return new OnnxTensor(outputTensor);
  }
}