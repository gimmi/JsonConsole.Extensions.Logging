using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, JsonConsoleLogger> _loggers = new ConcurrentDictionary<string, JsonConsoleLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, x => new JsonConsoleLogger(x));
        }

        public void Dispose()
        {
        }
    }
}
