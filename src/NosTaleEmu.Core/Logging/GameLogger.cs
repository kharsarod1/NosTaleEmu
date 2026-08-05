using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace NosTaleEmu.Core.Logging;

public static class GameLogger
{
    private const string TrafficProperty = "IsTraffic";

    public static void Initialize(string appName, bool displayTraffic = true)
    {
        LoggerConfiguration config = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("App", appName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code);

        if (!displayTraffic)
        {
            config = config.Filter.ByExcluding(evt => evt.Properties.ContainsKey(TrafficProperty));
        }

        Log.Logger = config.CreateLogger();
    }

    public static ILogger Traffic => Log.ForContext(TrafficProperty, true);
}
