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
      services.AddSingleton(AnsiConsole.Console);
      services.AddSingleton<IResourceManager, ResourceManager>();
      services.AddSingleton<ImageProcessor, ImageSharpProcessor>();
      services.AddSingleton<IInferenceRunner, OnnxInferenceRunner>();
      services.AddSingleton<IModelFactory, ModelFactory>();
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