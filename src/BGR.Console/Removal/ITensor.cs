namespace BGR.Console.Removal;

internal interface ITensor<T>
{
  int Height { get; }
  int Width { get; }
  void SetValue(int batch, int channel, int y, int x, T value);
  float GetValue(int batch, int channel, int y, int x);
  Tensor<T> ToTensor();
}
