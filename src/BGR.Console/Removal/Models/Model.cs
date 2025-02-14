namespace BGR.Console.Removal.Models;

internal abstract class Model
{
  private const float PixelMax = 255f;
  public abstract int InputWidth { get; }
  public abstract int InputHeight { get; }
  public abstract float RedNormalizationMean { get; }
  public abstract float GreenNormalizationMean { get; }
  public abstract float BlueNormalizationMean { get; }
  public abstract float RedNormalizationStd { get; }
  public abstract float GreenNormalizationStd { get; }
  public abstract float BlueNormalizationStd { get; }
  public byte[] Bytes { get; } = [];

  internal Model()
  {
  }

  protected Model(byte[] modelBytes)
  {
    Bytes = modelBytes;
  }

  public virtual float NormalizeRed(float value)
  {
    return Normalize(value, RedNormalizationMean, RedNormalizationStd);
  }

  public virtual float NormalizeGreen(float value)
  {
    return Normalize(value, GreenNormalizationMean, GreenNormalizationStd);
  }

  public virtual float NormalizeBlue(float value)
  {
    return Normalize(value, BlueNormalizationMean, BlueNormalizationStd);
  }

  private static float Normalize(float value, float mean, float std)
  {
    return ((value / PixelMax) - mean) / std;
  }
}