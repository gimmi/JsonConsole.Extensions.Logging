using System;
using System.IO;

namespace JsonConsole.Extensions.Logging
{
    public class JsonConsoleLoggerOptions
    {
        public Stream Stream { get; set; } = Console.OpenStandardOutput();

        public string TimestampFieldName { get; set; } = "t";
        public string MessageFieldName { get; set; } = "m";
        public string LevelFieldName { get; set; } = "l";
        public string CategoryFieldName { get; set; } = "c";
        public string EventIdFieldName { get; set; } = "i";
        public string ExceptionFieldName { get; set; } = "x";

        public string CriticalLevelName { get; set; } = "Critical";
        public string ErrorLevelName { get; set; } = "Error";
        public string WarningLevelName { get; set; } = "Warning";
        public string InformationLevelName { get; set; } = "Information";
        public string DebugLevelName { get; set; } = "Debug";
        public string TraceLevelName { get; set; } = "Trace";
    }
}
