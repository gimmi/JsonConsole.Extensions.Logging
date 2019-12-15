using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Microsoft.Extensions.Logging.Internal;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLogger : ILogger
    {
        private readonly IExternalScopeProvider _scopeProvider;
        private readonly Func<DateTime> _utcNowFn;
        private readonly TextWriter _stdout;
        private readonly string _categoryName;

        public JsonConsoleLogger(IExternalScopeProvider scopeProvider, Func<DateTime> utcNowFn, TextWriter stdout, string categoryName)
        {
            _scopeProvider = scopeProvider;
            _utcNowFn = utcNowFn;
            _stdout = stdout;
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

            var formattedMessage = formatter(state, exception);

            var dict = new Dictionary<string, object>();
            dict["m"] = formattedMessage;
            dict["l"] = logLevel.ToString();
            dict["t"] = _utcNowFn.Invoke();
            dict["c"] = _categoryName;
            if (exception != null)
            {
                dict["x"] = exception.ToString();
            }

            if (eventId != default)
            {
                dict["i"] = eventId.ToString();
            }

            WriteFormattedLogValues(state, dict);

            _scopeProvider?.ForEachScope(WriteFormattedLogValues, dict);

            var json = JsonSerializer.Serialize(dict);
            _stdout.WriteLine(json);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (_scopeProvider == null)
            {
                return NullScope.Instance;
            }

            return _scopeProvider.Push(state);
        }

        private void WriteFormattedLogValues(object formattedLogValues, Dictionary<string, object> dict)
        {
            if (formattedLogValues is IEnumerable<KeyValuePair<string, object>> values)
            {
                foreach (var value in values)
                {
                    var name = value.Key;
                    if (string.IsNullOrWhiteSpace(value.Key) || name[0] == '{')
                    {
                        continue;
                    }

                    dict[name] = value.Value;
                }
            }
        }
    }
}
