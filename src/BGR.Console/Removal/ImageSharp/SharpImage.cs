namespace BGR.Console.Removal.ImageSharp;

internal class SharpImage : IImage
{
  public int Width { get; }
  public int Height { get; }
  public Stream Data { get; }

  public SharpImage(int width, int height, Stream data)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width), "must be greater than 0");
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height), "must be greater than 0");
    }

    Width = width;
    Height = height;
    Data = data ?? throw new ArgumentNullException(nameof(data));
  }
}