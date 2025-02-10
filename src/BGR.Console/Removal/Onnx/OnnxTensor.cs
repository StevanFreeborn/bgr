namespace BGR.Console.Removal.Onnx;

public class OnnxTensor(
  int batchSize,
  int channels,
  int height,
  int width
) : ITensor<float>
{
  private readonly DenseTensor<float> _tensor =
    new([batchSize, channels, height, width]);

  public int Height => _tensor.Dimensions[2];

  public int Width => _tensor.Dimensions[3];

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
}