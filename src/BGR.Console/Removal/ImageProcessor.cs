namespace BGR.Console.Removal;

internal abstract class ImageProcessor
{
  public abstract Task<IImage> LoadImageAsync(string path);

  public abstract Task<ITensor<float>> CreateTensorInputAsync(Stream image, Model model);

  public abstract Task<Stream> GenerateMaskAsync(ITensor<float> maskTensor, int width, int height);

  public abstract Task<Stream> RemoveBackgroundAsync(Stream image, Stream mask);

  public abstract Task SaveImageAsync(Stream image, string path);

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