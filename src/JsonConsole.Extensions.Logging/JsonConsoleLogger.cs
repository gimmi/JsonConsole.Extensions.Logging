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
        private readonly JsonConsoleLoggerOptions _options;
        private readonly string _categoryName;
        private readonly Utf8JsonWriter _jsonWriter;

        public JsonConsoleLogger(IExternalScopeProvider? scopeProvider, JsonConsoleLoggerOptions options, string categoryName)
        {
            _scopeProvider = scopeProvider;
            _options = options;
            _categoryName = categoryName;
            _jsonWriter = new Utf8JsonWriter(options.Stream);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (_jsonWriter)
            {
                _jsonWriter.WriteStartObject();
                if (!string.IsNullOrWhiteSpace(_options.TimestampFieldName))
                {
                    _jsonWriter.WriteString(_options.TimestampFieldName, DateTime.UtcNow);
                }

                if (!string.IsNullOrWhiteSpace(_options.MessageFieldName))
                {
                    _jsonWriter.WriteString(_options.MessageFieldName, formatter(state, exception));
                }

                if (!string.IsNullOrWhiteSpace(_options.LevelFieldName))
                {
                    _jsonWriter.WriteString(_options.LevelFieldName, GetLogLevelString(logLevel));
                }

                if (!string.IsNullOrWhiteSpace(_options.CategoryFieldName))
                {
                    _jsonWriter.WriteString(_options.CategoryFieldName, _categoryName);
                }

                if (!string.IsNullOrWhiteSpace(_options.EventIdFieldName))
                {
                    // This could really be just eventId.ToString() but this is to save on allocation
                    if (eventId.Name != default)
                    {
                        _jsonWriter.WriteString(_options.EventIdFieldName, eventId.Name);
                    }
                    else if (eventId.Id != default)
                    {
                        _jsonWriter.WriteNumber(_options.EventIdFieldName, eventId.Id);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_options.ExceptionFieldName) && exception != default)
                {
                    _jsonWriter.WriteString("x", exception.ToString());
                }

                WriteFormattedLogValues(state, _jsonWriter);

                _scopeProvider?.ForEachScope(WriteFormattedLogValues, _jsonWriter);

                _jsonWriter.WriteEndObject();
                _jsonWriter.Flush();
                _jsonWriter.Reset();

                _options.Stream.Write(NewLine, 0, NewLine.Length);
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

        private string GetLogLevelString(LogLevel logLevel)
        {
            // This could really be just logLevel.ToString(), but with the case it saves some allocation
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return _options.CriticalLevelName;
                case LogLevel.Error:
                    return _options.ErrorLevelName;
                case LogLevel.Warning:
                    return _options.WarningLevelName;
                case LogLevel.Information:
                    return _options.InformationLevelName;
                case LogLevel.Debug:
                    return _options.DebugLevelName;
                case LogLevel.Trace:
                    return _options.TraceLevelName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private void WriteFormattedLogValues(object? state, Utf8JsonWriter jsonWriter)
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

        private bool IsValidPropertyName(string name)
        {
            return !string.IsNullOrWhiteSpace(name)
                   && name[0] != '{'
                   && name != _options.MessageFieldName
                   && name != _options.LevelFieldName
                   && name != _options.TimestampFieldName
                   && name != _options.CategoryFieldName
                   && name != _options.EventIdFieldName
                   && name != _options.ExceptionFieldName;
        }
    }
}
