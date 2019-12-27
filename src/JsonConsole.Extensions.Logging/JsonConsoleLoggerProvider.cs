using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ConcurrentDictionary<string, JsonConsoleLogger> _loggers = new ConcurrentDictionary<string, JsonConsoleLogger>();
        private readonly Func<DateTime> _utcNowFn;
        private readonly Stream _stream;

        private IExternalScopeProvider _scopeProvider;

        public JsonConsoleLoggerProvider(Func<DateTime> utcNowFn, Stream stream)
        {
            _utcNowFn = utcNowFn;
            _stream = stream;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, x => new JsonConsoleLogger(_scopeProvider, _utcNowFn, _stream, x));
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
