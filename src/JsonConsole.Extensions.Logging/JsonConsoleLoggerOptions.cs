using System;
using System.IO;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLoggerOptions
    {
        public Stream Stream { get; set; } = Console.OpenStandardOutput();
        public string TimestampFieldName { get; set; } = "t";
    }
}
