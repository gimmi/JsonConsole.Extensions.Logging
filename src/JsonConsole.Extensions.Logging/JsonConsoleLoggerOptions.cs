using System;
using System.IO;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLoggerOptions
    {
        public Stream Stream { get; set; } = Console.OpenStandardOutput();

        public string TimestampFieldName { get; set; } = "time";
        public string MessageFieldName { get; set; } = "log";
        public string LevelFieldName { get; set; } = "lvl";
        public string CategoryFieldName { get; set; } = "cat";
        public string EventIdFieldName { get; set; } = "evt";
        public string ExceptionFieldName { get; set; } = "exc";

        public string CriticalLevelName { get; set; } = "fatal";
        public string ErrorLevelName { get; set; } = "error";
        public string WarningLevelName { get; set; } = "warn";
        public string InformationLevelName { get; set; } = "info";
        public string DebugLevelName { get; set; } = "debug";
        public string TraceLevelName { get; set; } = "trace";
    }
}
