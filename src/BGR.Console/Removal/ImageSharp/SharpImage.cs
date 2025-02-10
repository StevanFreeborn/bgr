namespace BGR.Console.Removal.ImageSharp;

internal class SharpImage(Image<Rgba32> image) : IImage
{
  private readonly Image<Rgba32> _image = image;

  public int Width => _image.Width;
  public int Height => _image.Height;

  public void Resize(int width, int height)
  {
    _image.Mutate(x => x.Resize(width, height));
  }

  public IPixel GetPixel(int x, int y)
  {
    return new SharpPixel(_image[x, y]);
  }
}