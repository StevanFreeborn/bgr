using System.Diagnostics;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BGR.Console.Logging;

internal static class LoggerExtensions
{
  private static readonly Action<ILogger, Exception> RemovalCommandFailedMsg = LoggerMessage.Define(
    LogLevel.Error,
    new EventId(0, nameof(RemovalCommandFailed)),
    "An error occurred while executing the command."
  );

  private static readonly Action<ILogger, string, long, Exception> TimeAndLogActionMsg = LoggerMessage.Define<string, long>(
    LogLevel.Information,
    new EventId(0, nameof(TimeAndLogAction)),
    "{Message} in {ElapsedMilliseconds}ms"
  );

  public static void RemovalCommandFailed(this ILogger logger, Exception ex)
  {
    RemovalCommandFailedMsg(logger, ex);
  }

  public static async Task TimeAndLogActionAsync(this ILogger logger, string message, Func<Task> action)
  {
    var sw = new Stopwatch();
    sw.Start();
    await action();
    sw.Stop();
    TimeAndLogActionMsg(logger, message, sw.ElapsedMilliseconds, default!);
  }

  public static async Task<T> TimeAndLogActionAsync<T>(this ILogger logger, string message, Func<Task<T>> action)
  {
    var sw = new Stopwatch();
    sw.Start();
    var result = await action();
    sw.Stop();
    TimeAndLogActionMsg(logger, message, sw.ElapsedMilliseconds, default!);
    return result;
  }

  public static void TimeAndLogAction(this ILogger logger, string message, Action action)
  {
    var sw = new Stopwatch();
    sw.Start();
    action();
    sw.Stop();
    TimeAndLogActionMsg(logger, message, sw.ElapsedMilliseconds, default!);
  }

  public static T TimeAndLogAction<T>(this ILogger logger, string message, Func<T> action)
  {
    var sw = new Stopwatch();
    sw.Start();
    var result = action();
    sw.Stop();
    TimeAndLogActionMsg(logger, message, sw.ElapsedMilliseconds, default!);
    return result;
  }
}