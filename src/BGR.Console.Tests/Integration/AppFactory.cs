namespace BGR.Console.Tests.Integration;

internal static class AppFactory
{
  public static CommandApp<RemovalCommand> Create()
  {
    return Host.CreateDefaultBuilder()
    .ConfigureLogging(static logging => logging.ClearProviders())
    .ConfigureServices(static (_, services) =>
    {
      services.AddSingleton<IAnsiConsole>(new TestConsole());
      services.AddSingleton<IResourceManager, ResourceManager>();
      services.AddSingleton<ImageProcessor, ImageSharpProcessor>();
      services.AddSingleton<IInferenceRunner, OnnxInferenceRunner>();
      services.AddSingleton<IModelFactory, ModelFactory>();
    })
    .BuildApp();
  }
}