using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging
{
    public static class JsonConsoleLoggerBuilderExtensions
    {
        public static ILoggingBuilder AddJsonConsole(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider, JsonConsoleLoggerProvider>();
            return loggingBuilder;
        }
    }
}
