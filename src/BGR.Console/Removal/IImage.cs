namespace BGR.Console.Removal;

internal interface IImage
{
  int Width { get; }
  int Height { get; }
  Stream Data { get; }
}