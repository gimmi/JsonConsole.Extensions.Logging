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
        private readonly TextWriter _stdout;

        private IExternalScopeProvider _scopeProvider;

        public JsonConsoleLoggerProvider(Func<DateTime> utcNowFn, TextWriter stdout)
        {
            _utcNowFn = utcNowFn;
            _stdout = stdout;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, x => new JsonConsoleLogger(_scopeProvider, _utcNowFn, _stdout, x));
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
