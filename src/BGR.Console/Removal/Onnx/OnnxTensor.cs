namespace BGR.Console.Removal.Onnx;

public class OnnxTensor : ITensor<float>
{
  private readonly Tensor<float> _tensor;

  public int Height => _tensor.Dimensions[2];

  public int Width => _tensor.Dimensions[3];

  public OnnxTensor(
    int batchSize,
    int channels,
    int height,
    int width
  )
  {
    _tensor = new DenseTensor<float>([batchSize, channels, height, width]);
  }

  public OnnxTensor(Tensor<float> tensor)
  {
    _tensor = tensor;
  }

  public void SetValue(
      int batch,
      int channel,
      int y,
      int x,
      float value
    )
  {
    _tensor[batch, channel, y, x] = value;
  }

  public float GetValue(
      int batch,
      int channel,
      int y,
      int x
    )
  {
    return _tensor[batch, channel, y, x];
  }

  public Tensor<float> ToTensor()
  {
    return _tensor;
  }
}