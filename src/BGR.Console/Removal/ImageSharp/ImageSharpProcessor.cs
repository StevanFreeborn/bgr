namespace BGR.Console.Removal.ImageSharp;

internal class ImageSharpProcessor : ImageProcessor
{
  public override async Task<IImage> LoadImageAsync(string path)
  {
    var image = await Image.LoadAsync<Rgba32>(path);

    if (image.Metadata.DecodedImageFormat is null)
    {
      throw new InvalidOperationException("Image format is not supported.");
    }

    var stream = new MemoryStream();
    await image.SaveAsync(stream, image.Metadata.DecodedImageFormat);
    stream.Position = 0;

    return new SharpImage(image.Width, image.Height, stream);
  }

  public override async Task<ITensor<float>> CreateTensorInputAsync(Stream image, Model model)
  {
    using var resized = await Image.LoadAsync<Rgba32>(image);

    resized.Mutate(x => x.Resize(model.InputWidth, model.InputHeight));

    const int batchSize = 1;
    const int channels = 3;
    var tensor = new OnnxTensor(batchSize, channels, model.InputHeight, model.InputWidth);

    WalkImage(resized.Height, resized.Width, (x, y) =>
    {
      var pixel = resized[x, y];
      tensor.SetValue(0, 0, y, x, model.NormalizeRed(pixel.R));
      tensor.SetValue(0, 1, y, x, model.NormalizeGreen(pixel.G));
      tensor.SetValue(0, 2, y, x, model.NormalizeBlue(pixel.B));
    });

    return tensor;
  }

  public override async Task<Stream> GenerateMaskAsync(ITensor<float> maskTensor, int width, int height)
  {
    using var mask = new Image<Rgba32>(width, height);

    using Image<Rgba32> tempMask = new(maskTensor.Width, maskTensor.Height);

    const byte opaqueAlpha = 255;

    WalkImage(maskTensor.Height, maskTensor.Width, (x, y) =>
    {
      var sigmoidValue = CalculateSigmoid(maskTensor.GetValue(0, 0, y, x));
      var normalizedValue = Normalize(sigmoidValue);
      var intensity = ConvertToGreyscale(normalizedValue);

      tempMask[x, y] = new Rgba32(intensity, intensity, intensity, opaqueAlpha);
    });

    tempMask.Mutate(x => x.Resize(width, height));

    WalkImage(height, width, (x, y) => mask[x, y] = tempMask[x, y]);

    var stream = new MemoryStream();
    await mask.SaveAsync(stream, new PngEncoder());
    stream.Position = 0;

    return stream;
  }

  public override async Task<Stream> RemoveBackgroundAsync(Stream image, Stream mask)
  {
    image.Position = 0;
    mask.Position = 0;

    var imageWithBg = await Image.LoadAsync<Rgba32>(image);
    var maskImage = await Image.LoadAsync<Rgba32>(mask);
    using var imageWithBgRemoved = new Image<Rgba32>(imageWithBg.Width, imageWithBg.Height);

    const byte alphaThreshold = 20;
    var transparentPixel = new Rgba32(0, 0, 0, 0);

    WalkImage(imageWithBg.Height, imageWithBg.Width, (x, y) =>
    {
      var sourcePixel = imageWithBg[x, y];
      var maskPixel = maskImage[x, y];

      var alpha = maskPixel.R;

      imageWithBgRemoved[x, y] = alpha > alphaThreshold
        ? new Rgba32(sourcePixel.R, sourcePixel.G, sourcePixel.B, sourcePixel.A)
        : transparentPixel;
    });

    var result = new MemoryStream();
    await imageWithBgRemoved.SaveAsync(result, new PngEncoder());
    result.Position = 0;

    return result;
  }

  public override async Task SaveImageAsync(Stream image, string path)
  {
    image.Position = 0;
    var img = await Image.LoadAsync<Rgba32>(image);
    await img.SaveAsync(path, new PngEncoder());
  }

  private static float Normalize(float value)
  {
    const float binarizationThreshold = 0.5f;
    const float normalizationFactor = 2f;

    return value > binarizationThreshold
        ? (value - binarizationThreshold) * normalizationFactor
        : 0f;
  }

  private static byte ConvertToGreyscale(float value)
  {
    const float maxIntensity = 255f;
    return (byte)(value * maxIntensity);
  }

  private static float CalculateSigmoid(float x)
  {
    const float sigmoidScale = 1f;
    const float sigmoidShift = 1f;
    const float sigmoidDivisor = -1f;

    return sigmoidScale / (sigmoidShift + MathF.Exp(sigmoidDivisor * x));
  }
}