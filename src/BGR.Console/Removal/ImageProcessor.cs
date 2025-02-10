namespace BGR.Console.Removal;

internal abstract class ImageProcessor
{
  public abstract Task<ITensor<float>> CreateTensorInputAsync(Stream image, Model model);

  public abstract Task<Stream> GenerateMaskAsync(OnnxTensor maskTensor, int width, int height);

  public abstract Task<Stream> RemoveBackgroundAsync(Stream image, Stream mask);

  protected static void WalkImage(int height, int width, Action<int, int> action)
  {
    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        action(x, y);
      }
    }
  }

}