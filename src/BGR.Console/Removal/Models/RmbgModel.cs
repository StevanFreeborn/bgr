namespace BGR.Console.Removal.Models;

internal class RmbgModel(byte[] modelBytes) : Model(modelBytes)
{
  public const string Id = "rmbg";
  public override int InputWidth => 1024;
  public override int InputHeight => 1024;
  public override float RedNormalizationMean => 0.485f;
  public override float GreenNormalizationMean => 0.456f;
  public override float BlueNormalizationMean => 0.406f;
  public override float RedNormalizationStd => 0.229f;
  public override float GreenNormalizationStd => 0.224f;
  public override float BlueNormalizationStd => 0.225f;
}