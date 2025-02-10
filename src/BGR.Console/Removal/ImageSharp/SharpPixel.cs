namespace BGR.Console.Removal.ImageSharp;

internal class SharpPixel(Rgba32 pixel) : IPixel
{
  private readonly Rgba32 _pixel = pixel;

  public float R => _pixel.R;
  public float G => _pixel.G;
  public float B => _pixel.B;
}