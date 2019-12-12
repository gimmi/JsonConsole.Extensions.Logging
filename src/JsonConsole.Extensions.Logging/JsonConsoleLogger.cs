using System;
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

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteString("m", formattedMessage);
                    writer.WriteString("l", logLevel.ToString());
                    writer.WriteString("t", _utcNowFn.Invoke());
                    writer.WriteString("c", _categoryName);
                    if (exception != null)
                    {
                        writer.WriteString("e", exception.ToString());
                    }

                    WriteFormattedLogValues(state, writer);

                    _scopeProvider?.ForEachScope(WriteFormattedLogValues, writer);

                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
                _stdout.WriteLine(json);
            }
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

        private void WriteFormattedLogValues(object formattedLogValues, Utf8JsonWriter writer)
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

                    writer.WritePropertyName(name);
                    JsonSerializer.Serialize(writer, value.Value);
                }
            }
        }
    }
}
