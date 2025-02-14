namespace BGR.Console.Common;

internal static class HostBuilderExtensions
{
  public static CommandApp<RemovalCommand> BuildApp(this IHostBuilder builder)
  {
    var registrar = new TypeRegistrar(builder);
    var app = new CommandApp<RemovalCommand>(registrar);

    app.Configure(static c =>
      c.SetExceptionHandler(static (ex, resolver) =>
      {
        var logger = resolver?.Resolve(typeof(ILogger<RemovalCommand>)) as ILogger<RemovalCommand>;
        logger?.RemovalCommandFailed(ex);

        var console = resolver?.Resolve(typeof(IAnsiConsole)) as IAnsiConsole;
        console?.WriteLine($"[red]An error occurred while executing the command:[/]");
        console?.WriteException(ex, ExceptionFormats.ShortenEverything);
      })
    );

    return app;
  }
}