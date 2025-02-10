Log.Logger = new LoggerConfiguration()
  .WriteTo.File(
    formatter: new CompactJsonFormatter(),
    path: Path.Combine(AppContext.BaseDirectory, "logs", "log.jsonl"),
    rollingInterval: RollingInterval.Day
  )
  .Enrich.FromLogContext()
  .MinimumLevel.Verbose()
  .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
  .CreateLogger();

try
{
  var appName = Assembly.GetExecutingAssembly().GetName().Name;
  Log.Information("Starting {AppName}", appName);

  await Host.CreateDefaultBuilder()
    .ConfigureLogging(static logging => logging.ClearProviders())
    .ConfigureServices(static (_, services) =>
    {
      services.AddSerilog();
      services.AddSingleton<IResourceManager, ResourceManager>();
    })
    .BuildApp()
    .RunAsync(args);

  Log.Information("Stopping {AppName}", appName);
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
  throw;
}
finally
{
  await Log.CloseAndFlushAsync();
}

if (args.Length < 1)
{
  Console.WriteLine("Usage: BackgroundRemover <input_image_path>");
  return;
}

var inputImagePath = args[0];
var maskImagePath = Path.ChangeExtension(inputImagePath, null) + "_mask.png";
var outputImagePath = Path.ChangeExtension(inputImagePath, null) + "_no_bg.png";

try
{
  var assembly = Assembly.GetExecutingAssembly();
  var resourceName = "BGR.Console.Resources.Files.rmbg.onnx";

  using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException("Model not found in embedded resources.");
  var modelBytes = new byte[stream.Length];
  stream.ReadExactly(modelBytes);

  using var image = await Image.LoadAsync<Rgba32>(inputImagePath);
  var inputTensor = CreateTensorInput(image);

  using var options = new SessionOptions() { LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR };
  using InferenceSession session = new(modelBytes, options);
  var inputs = new List<NamedOnnxValue>()
  {
    NamedOnnxValue.CreateFromTensor(session.InputNames[0], inputTensor),
  };

  using var results = session.Run(inputs);
  var outputTensor = results[0].AsTensor<float>();

  using var mask = GenerateMask(outputTensor, image.Width, image.Height);

  using var bgRemoved = GetImageWithBackgroundRemoved(image, mask);

  var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression };

  await mask.SaveAsync(maskImagePath, encoder);
  await bgRemoved.SaveAsync(outputImagePath, encoder);

  Console.WriteLine($"Background removed and saved to {outputImagePath}");
}
catch (Exception ex)
{
  Console.WriteLine($"Error: {ex.Message}");
  throw;
}

static Tensor<float> CreateTensorInput(Image<Rgba32> image)
{
  // U2Net expects input images to be 320x320. This is dependent on the model.
  const int targetWidth = 1024;
  const int targetHeight = 1024;

  // ImageNet normalization parameters
  // source:
  // - https://www.image-net.org/
  // - https://pytorch.org calculated these values from the ImageNet dataset
  // and they are commonly used for models trained on ImageNet so we use them here
  // to normalize the input image to better match the distribution of the data the model was trained on
  // NOTE: These values are not universal and may vary for different models
  const float rMean = 0.485f;   // Mean value for Red channel
  const float gMean = 0.456f;   // Mean value for Green channel
  const float bMean = 0.406f;   // Mean value for Blue channel
  const float rStd = 0.229f;    // Standard deviation for Red channel
  const float gStd = 0.224f;    // Standard deviation for Green channel
  const float bStd = 0.225f;    // Standard deviation for Blue channel
  const float pixelMax = 255f;  // Maximum pixel intensity for normalization

  // Create a temporary image for preprocessing
  using var resized = image.Clone();
  resized.Mutate(x => x.Resize(targetWidth, targetHeight));

  // Create tensor of shape (1, 3, 320, 320)
  // 1 for batch size, 3 for RGB channels, 320x320 for image dimensions
  DenseTensor<float> tensor = new([1, 3, targetHeight, targetWidth]);

  // Normalize pixel values and copy to tensor
  WalkImage(resized.Height, resized.Width, (x, y) =>
  {
    var pixel = resized[x, y];

    // u2net expects expect input images to be normalized using ImageNet mean and std
    // to better match the distribution of the data the model was trained on
    // Normalize to range [0, 1] and standardize using ImageNet mean/std
    // The tensor is filled with normalized pixel values
    tensor[0, 0, y, x] = ((pixel.R / pixelMax) - rMean) / rStd; // Red channel
    tensor[0, 1, y, x] = ((pixel.G / pixelMax) - gMean) / gStd; // Green channel
    tensor[0, 2, y, x] = ((pixel.B / pixelMax) - bMean) / bStd; // Blue channel
  });

  return tensor;
}

static Image<Rgba32> GenerateMask(Tensor<float> maskTensor, int width, int height)
{
  var mask = new Image<Rgba32>(width, height);

  var sourceHeight = maskTensor.Dimensions[2]; // Height of the original tensor mask
  var sourceWidth = maskTensor.Dimensions[3];  // Width of the original tensor mask

  using Image<Rgba32> tempMask = new(sourceWidth, sourceHeight);

  // Sigmoid function parameters
  const float sigmoidScale = 1f;    // Scaling factor for sigmoid activation
  const float sigmoidShift = 1f;    // Shift factor in the denominator of the sigmoid function
  const float sigmoidDivisor = -1f; // Multiplier for the exponent in the sigmoid function

  static float CalculateSigmoid(float x)
  {
    return sigmoidScale / (sigmoidShift + MathF.Exp(sigmoidDivisor * x));
  }

  const float binarizationThreshold = 0.5f; // Threshold to determine foreground vs. background
  const float normalizationFactor = 2f;     // Scales the thresholded value to enhance contrast

  // Pixel intensity values
  const byte maxIntensity = 255;  // Maximum grayscale intensity
  const byte opaqueAlpha = 255;   // Fully opaque alpha value


  WalkImage(sourceHeight, sourceWidth, (x, y) =>
  {
    // a sigmoid function is a function that produces an S-shaped curve
    // it is often used in machine learning and statistics to model probabilities
    // the sigmoid function is defined as:
    // f(x) = 1 / (1 + e^(-x))
    // where e is the base of the natural logarithm and x is the input value

    // the raw tensor values for our mask are going to be real unbounded numbers
    // i.e. -1.5, 0.5, 2.0, etc.
    // the sigmoid function will map these values to a range between 0 and 1
    // this allows us to say that value closer to 0 is background and value
    // closer to 1 is foreground
    var sigmoidValue = CalculateSigmoid(maskTensor[0, 0, y, x]);

    // now we want to threshold the sigmoid value to determine if it is foreground or background
    // we are arbitrarily choosing 0.5 as the threshold. so if the sigmoid value is greater than
    // 0.5 we will consider it foreground and if it is less than 0.5 we will consider it background

    // when a sigmoid value is greater than 0.5 we will subtract the threshold from it
    // and multiply it by 2 this way the intensity value will be larger for values closer to 1
    // and create more contrast in the mask
    var normalizedValue = sigmoidValue > binarizationThreshold
        ? (sigmoidValue - binarizationThreshold) * normalizationFactor
        : 0f;

    // Convert to an 8-bit grayscale intensity
    var intensity = (byte)(normalizedValue * maxIntensity);

    // Store the pixel with full opacity
    tempMask[x, y] = new Rgba32(intensity, intensity, intensity, opaqueAlpha);
  });

  // Resize the mask to match the target dimensions
  tempMask.Mutate(x => x.Resize(width, height));

  // Copy the resized mask to the final output image
  WalkImage(height, width, (x, y) => mask[x, y] = tempMask[x, y]);

  return mask;
}

static Image<Rgba32> GetImageWithBackgroundRemoved(Image<Rgba32> image, Image<Rgba32> mask)
{
  Image<Rgba32> result = new(image.Width, image.Height);

  const byte alphaThreshold = 20;
  Rgba32 transparentPixel = new(0, 0, 0, 0);

  WalkImage(image.Height, image.Width, (x, y) =>
  {
    var sourcePixel = image[x, y];
    var maskPixel = mask[x, y];

    var alpha = maskPixel.R;

    result[x, y] = alpha > alphaThreshold
      ? new Rgba32(sourcePixel.R, sourcePixel.G, sourcePixel.B, sourcePixel.A)
      : transparentPixel;
  });

  return result;
}

static void WalkImage(int height, int width, Action<int, int> action)
{
  for (var y = 0; y < height; y++)
  {
    for (var x = 0; x < width; x++)
    {
      action(x, y);
    }
  }
}