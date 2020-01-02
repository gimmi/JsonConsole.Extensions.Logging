using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging.Benchmarks
{
    [ShortRunJob, MemoryDiagnoser]
    public class SingleLogEntry
    {
        private readonly ILogger _logger;

        public SingleLogEntry()
        {
            var dateTime = new DateTime(2019, 12, 11, 20, 25, 0, DateTimeKind.Utc);
            var loggerProvider = new JsonConsoleLoggerProvider(() => dateTime, Stream.Null);
            _logger = loggerProvider.CreateLogger("MyCategory");
        }

        [Benchmark]
        public void Run()
        {
            _logger.Log(LogLevel.Information, 123, "MyMsg");
        }
    }
}
