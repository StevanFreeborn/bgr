namespace BGR.Console.Removal;

internal class RemovalCommand(
  IModelFactory modelFactory,
  ImageProcessor imageProcessor,
  IInferenceRunner inferenceRunner,
  IAnsiConsole console,
  ILogger<RemovalCommand> logger
) : AsyncCommand<RemovalCommand.Settings>
{
  private readonly IModelFactory _modelFactory = modelFactory;
  private readonly ImageProcessor _imageProcessor = imageProcessor;
  private readonly IInferenceRunner _inferenceRunner = inferenceRunner;
  private readonly IAnsiConsole _console = console;
  private readonly ILogger<RemovalCommand> _logger = logger;

  public async Task<int> ExecuteAsync(
    CommandContext context,
    Settings settings
  )
  {
    return await ExecuteAsync(context, settings, CancellationToken.None);
  }

  protected override async Task<int> ExecuteAsync(
    CommandContext context,
    Settings settings,
    CancellationToken cancellationToken
  )
  {
    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .SpinnerStyle(Style.Parse("green"))
      .StartAsync("Removing background...", async ctx =>
      {
        ctx.Status("Loading model...");
        var model = _logger.TimeAndLogAction(
          "Loading model",
          () => _modelFactory.Create(settings.ResourceName)
        );

        ctx.Status("Loading image...");
        var image = await _logger.TimeAndLogActionAsync(
          "Loading image",
          async () => await _imageProcessor.LoadImageAsync(settings.Image)
        );

        ctx.Status("Creating tensor input...");
        var inputTensor = await _logger.TimeAndLogActionAsync(
          "Creating tensor input",
          async () => await _imageProcessor.CreateTensorInputAsync(image.Data, model)
        );

        ctx.Status("Running inference...");
        var outputTensor = _logger.TimeAndLogAction(
          "Running inference",
          () => _inferenceRunner.Run(model.Bytes, inputTensor)
        );

        ctx.Status("Generating mask...");
        var mask = await _logger.TimeAndLogActionAsync(
          "Generating mask",
          async () => await _imageProcessor.GenerateMaskAsync(outputTensor, image.Width, image.Height)
        );

        ctx.Status("Removing background...");
        var output = await _logger.TimeAndLogActionAsync(
          "Removing background",
          async () => await _imageProcessor.RemoveBackgroundAsync(image.Data, mask, settings.FeatherMin, settings.FeatherMax)
        );

        if (settings.IncludeMask)
        {
          await _logger.TimeAndLogActionAsync(
            "Saving mask",
            async () => await _imageProcessor.SaveImageAsync(mask, settings.MaskPath)
          );

          _console.MarkupLine($"[bold]Mask saved to:[/] [blue]{settings.OutputPath}[/]");
        }

        await _logger.TimeAndLogActionAsync(
          "Saving output",
          async () => await _imageProcessor.SaveImageAsync(output, settings.OutputPath)
        );

        _console.MarkupLine($"[bold]Output saved to:[/] [green]{settings.OutputPath}[/]");
      });

    return 0;
  }

  internal class Settings : CommandSettings
  {
    private static readonly Dictionary<string, string> Models = new()
    {
      { RmbgModel.Id, "rmbg.onnx" },
      { ModNetModel.Id, "modnet.onnx" },
      { U2NetModel.Id, "u2net.onnx" },
    };

    [CommandArgument(0, "<image>")]
    [Description("Path to the image file whose background you want to remove")]
    public string Image { get; init; } = string.Empty;

    [CommandOption("--model|-m")]
    [Description("The model to use for background removal")]
    public string Model { get; init; } = "rmbg";

    [CommandOption("--include-mask|-i")]
    [Description("Generate and output the mask used for background removal")]
    public bool IncludeMask { get; init; } = false;

    [CommandOption("--output|-o")]
    [Description("Path to output image without background to. File extension will always be .png")]
    public string Output { get; init; } = string.Empty;

    [CommandOption("--feather-min")]
    [Description("Minimum mask value below which pixels become fully transparent (default: 70)")]
    public byte FeatherMin { get; init; } = 70;

    [CommandOption("--feather-max")]
    [Description("Maximum mask value above which pixels become fully opaque (default: 117)")]
    public byte FeatherMax { get; init; } = 117;

    public string ResourceName => Models[Model];

    public string MaskPath => GetOutputPath("_mask");

    public string OutputPath => GetOutputPath("_no_bg");

    public override ValidationResult Validate()
    {
      if (File.Exists(Image) is false)
      {
        return ValidationResult.Error($"The image file '{Image}' does not exist.");
      }

      if (Models.ContainsKey(Model) is false)
      {
        return ValidationResult.Error($"The model '{Model}' is not supported.");
      }

      return ValidationResult.Success();
    }

    private string GetOutputPath(string modifier)
    {
      if (string.IsNullOrWhiteSpace(Output))
      {
        return Path.ChangeExtension(Image, null) + modifier + ".png";
      }

      return Path.ChangeExtension(Output, ".png");
    }
  }
}