namespace BGR.Console.Removal;

internal interface IImage
{
  int Width { get; }
  int Height { get; }
  void Resize(int width, int height);
  IPixel GetPixel(int x, int y);
}