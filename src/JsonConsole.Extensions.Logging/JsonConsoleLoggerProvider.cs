using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging
{
    internal class JsonConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly JsonConsoleLoggerOptions _options;
        private readonly ConcurrentDictionary<string, JsonConsoleLogger> _loggers = new ConcurrentDictionary<string, JsonConsoleLogger>();

        private IExternalScopeProvider? _scopeProvider;

        public JsonConsoleLoggerProvider(JsonConsoleLoggerOptions options)
        {
            _options = options;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, x => new JsonConsoleLogger(_scopeProvider, _options, x));
        }

        public void Dispose()
        {
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}
