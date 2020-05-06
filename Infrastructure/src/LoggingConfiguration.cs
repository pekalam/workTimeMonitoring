using System;
using System.Diagnostics;
using Serilog;

namespace Infrastructure
{
    internal static class LoggingConfiguration
    {
        private const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}";

        [Conditional("RELEASE")]
        private static void ConfigureRelease(LoggerConfiguration c)
        {
            c.Enrich.WithThreadId().Enrich.WithMemoryUsage()
                .MinimumLevel.Information()
                .WriteTo.File("log.txt", fileSizeLimitBytes: 1024*1024*512, rollOnFileSizeLimit:true, outputTemplate: OutputTemplate)
                .WriteTo.Console(outputTemplate: OutputTemplate);
        }

        private static void ConfigureDebug(LoggerConfiguration c)
        {
#if RELEASE
#else
            c.Enrich.WithThreadId().Enrich.WithMemoryUsage()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: OutputTemplate);
#endif
        }

        public static void Configure()
        {
            var config = new LoggerConfiguration();
            ConfigureRelease(config);
            ConfigureDebug(config);


            Log.Logger = config.CreateLogger();
        }
    }
}