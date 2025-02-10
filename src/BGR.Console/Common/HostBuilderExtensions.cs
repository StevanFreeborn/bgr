namespace BGR.Console.Common;

internal static class HostBuilderExtensions
{
  public static CommandApp BuildApp(this IHostBuilder builder)
  {
    var registrar = new TypeRegistrar(builder);
    var app = new CommandApp(registrar);

    return app;
  }
}