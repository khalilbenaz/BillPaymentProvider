using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using BillPaymentProvider.Configuration;

namespace BillPaymentProvider.Extensions
{
    /// <summary>
    /// Extensions pour la configuration du logging
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configure Serilog pour l'application
        /// </summary>
        public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
        {
            var loggingSettings = builder.Configuration.GetSection(LoggingSettings.Section).Get<LoggingSettings>() ?? new LoggingSettings();

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(ParseLogLevel(loggingSettings.DefaultLevel))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "BillPaymentProvider")
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

            // Console logging
            if (loggingSettings.EnableConsoleLogging)
            {
                if (loggingSettings.EnableStructuredLogging)
                {
                    loggerConfig.WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
                }
                else
                {
                    loggerConfig.WriteTo.Console();
                }
            }

            // File logging
            if (loggingSettings.EnableFileLogging)
            {
                // Ensure log directory exists
                Directory.CreateDirectory(loggingSettings.LogPath);

                // General application logs
                loggerConfig.WriteTo.File(
                    path: Path.Combine(loggingSettings.LogPath, "app-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingSettings.RetainDays,
                    fileSizeLimitBytes: loggingSettings.MaxFileSizeMB * 1024 * 1024,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

                // Error logs
                loggerConfig.WriteTo.File(
                    path: Path.Combine(loggingSettings.LogPath, "errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: loggingSettings.RetainDays,
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

                // Security logs
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("LogType") && 
                                               e.Properties["LogType"].ToString().Contains("Security"))
                    .WriteTo.File(
                        path: Path.Combine(loggingSettings.LogPath, "security-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: loggingSettings.RetainDays,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [SECURITY] {Message:lj} {Properties:j}{NewLine}"));

                // Audit logs
                loggerConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("LogType") && 
                                               e.Properties["LogType"].ToString().Contains("Audit"))
                    .WriteTo.File(
                        path: Path.Combine(loggingSettings.LogPath, "audit-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: loggingSettings.RetainDays,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [AUDIT] {Message:lj} {Properties:j}{NewLine}"));
            }

            Log.Logger = loggerConfig.CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }

        private static LogEventLevel ParseLogLevel(string level)
        {
            return level.ToLowerInvariant() switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}
