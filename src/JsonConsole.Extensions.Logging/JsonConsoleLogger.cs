using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace JsonConsole.Extensions.Logging
{
    internal class JsonConsoleLogger : ILogger
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
            var m = formatter(state, exception);
            var x = exception?.ToString();
            lock (_jsonWriter)
            {
                _jsonWriter.WriteStartObject();
                _jsonWriter.WriteString("m", m);
                _jsonWriter.WriteString("l", GetLogLevelString(logLevel));
                _jsonWriter.WriteString("t", _utcNowFn.Invoke());
                _jsonWriter.WriteString("c", _categoryName);

                // This could really be just eventId.ToString() but this is to save on allocation
                if (eventId.Name != default)
                {
                    _jsonWriter.WriteString("i", eventId.Name);
                }
                else if (eventId.Id != default)
                {
                    _jsonWriter.WriteNumber("i", eventId.Id);
                }

                if (x != default)
                {
                    _jsonWriter.WriteString("x", x);
                }

                WriteFormattedLogValues(state, _jsonWriter);

                _scopeProvider?.ForEachScope(WriteFormattedLogValues, _jsonWriter);

                _jsonWriter.WriteEndObject();
                _jsonWriter.Flush();
                _jsonWriter.Reset();

                _stream.Write(NewLine, 0, NewLine.Length);
            }
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

        private static void WriteFormattedLogValues(object? state, Utf8JsonWriter jsonWriter)
        {
            if (state is IReadOnlyList<KeyValuePair<string, object>> values)
            {
                // foreach cause allocation, so avoid it
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < values.Count; i++)
                {
                    var name = values[i].Key;
                    if (IsValidPropertyName(name))
                    {
                        jsonWriter.WritePropertyName(name);
                        JsonSerializer.Serialize(jsonWriter, values[i].Value);
                    }
                }
            }
        }

        private static bool IsValidPropertyName(string name)
        {
            return name?.Length > 0
                   && name[0] != '{'
                   && name != "m"
                   && name != "l"
                   && name != "t"
                   && name != "c"
                   && name != "i"
                   && name != "x";
        }
    }
}
