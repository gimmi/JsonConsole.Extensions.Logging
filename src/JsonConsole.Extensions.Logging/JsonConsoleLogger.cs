using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLogger : ILogger
    {
        private readonly Func<DateTime> _utcNowFn;
        private readonly TextWriter _stdout;
        private readonly string _categoryName;

        public JsonConsoleLogger(Func<DateTime> utcNowFn, TextWriter stdout, string categoryName)
        {
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
            return NullScope.Instance;
        }
    }
}
