using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLogger : ILogger
    {
        private static readonly byte[] NewLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        private readonly IExternalScopeProvider? _scopeProvider;
        private readonly Func<DateTime> _utcNowFn;
        private readonly Stream _stream;
        private readonly string _categoryName;
        private readonly Utf8JsonWriter _jsonWriter;

        public JsonConsoleLogger(IExternalScopeProvider? scopeProvider, Func<DateTime> utcNowFn, Stream stream, string categoryName)
        {
            _scopeProvider = scopeProvider;
            _utcNowFn = utcNowFn;
            _stream = stream;
            _categoryName = categoryName;
            _jsonWriter = new Utf8JsonWriter(_stream);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _jsonWriter.WriteStartObject();
            _jsonWriter.WriteString("m", formatter(state, exception));
            _jsonWriter.WriteString("l", GetLogLevelString(logLevel));
            _jsonWriter.WriteString("t", _utcNowFn.Invoke());
            _jsonWriter.WriteString("c", _categoryName);

            if (eventId != default)
            {
                _jsonWriter.WriteString("i", eventId.ToString());
            }

            if (exception != null)
            {
                _jsonWriter.WriteString("x", exception.ToString());
            }

            WriteFormattedLogValues(state, _jsonWriter);

            _scopeProvider?.ForEachScope(WriteFormattedLogValues, _jsonWriter);

            _jsonWriter.WriteEndObject();
            _jsonWriter.Flush();
            _jsonWriter.Reset();

            _stream.Write(NewLine, 0, NewLine.Length);
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

        private static string GetLogLevelString(LogLevel logLevel)
        {
            // This could really be just logLevel.ToString(), but with the case it saves some allocation
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return "Critical";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Warning:
                    return "Warning";
                case LogLevel.Information:
                    return "Information";
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Trace:
                    return "Trace";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private void WriteFormattedLogValues(object? formattedLogValues, Utf8JsonWriter writer)
        {
            if (formattedLogValues is IEnumerable<KeyValuePair<string, object>> values)
            {
                foreach (var value in values)
                {
                    var name = value.Key;
                    if (name.Length > 0 && name[0] != '{')
                    {
                        writer.WritePropertyName(name);
                        JsonSerializer.Serialize(writer, value.Value);
                    }
                }
            }
        }
    }
}
