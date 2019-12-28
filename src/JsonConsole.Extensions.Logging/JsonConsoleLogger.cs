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
        private static readonly byte[] Newline = Encoding.UTF8.GetBytes(Environment.NewLine);

        private readonly IExternalScopeProvider? _scopeProvider;
        private readonly Func<DateTime> _utcNowFn;
        private readonly Stream _stream;
        private readonly string _categoryName;

        public JsonConsoleLogger(IExternalScopeProvider? scopeProvider, Func<DateTime> utcNowFn, Stream stream, string categoryName)
        {
            _scopeProvider = scopeProvider;
            _utcNowFn = utcNowFn;
            _stream = stream;
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formattedMessage = formatter(state, exception);

            using (var writer = new Utf8JsonWriter(_stream))
            {
                writer.WriteStartObject();
                writer.WriteString("m", formattedMessage);
                writer.WriteString("l", logLevel.ToString());
                writer.WriteString("t", _utcNowFn.Invoke());
                writer.WriteString("c", _categoryName);

                if (eventId != default)
                {
                    writer.WriteString("i", eventId.ToString());
                }

                if (exception != null)
                {
                    writer.WriteString("x", exception.ToString());
                }

                WriteFormattedLogValues(state, writer);

                _scopeProvider?.ForEachScope(WriteFormattedLogValues, writer);

                writer.WriteEndObject();
            }

            _stream.Write(Newline, 0, Newline.Length);
        }

        // See https://github.com/aspnet/Extensions/blob/dc9a65e/src/Logging/Logging.Console/src/ConsoleLogger.cs#L225
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable BeginScope<TState>(TState state)
        {
            if (_scopeProvider == null)
            {
                return NullScope.Instance;
            }

            return _scopeProvider.Push(state);
        }

        private void WriteFormattedLogValues(object? formattedLogValues, Utf8JsonWriter writer)
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
