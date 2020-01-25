using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging
{
    public static class JsonConsoleLoggerBuilderExtensions
    {
        public static ILoggingBuilder AddJsonConsole(this ILoggingBuilder loggingBuilder)
        {
            // For handling IDisposable use something like this:
            // https://github.com/serilog/serilog-extensions-logging/blob/54ff29a/src/Serilog.Extensions.Logging/SerilogLoggingBuilderExtensions.cs#L42

            var options = new JsonConsoleLoggerOptions();
            var provider = new JsonConsoleLoggerProvider(options);
            loggingBuilder.AddProvider(provider);
            return loggingBuilder;
        }
    }
}
