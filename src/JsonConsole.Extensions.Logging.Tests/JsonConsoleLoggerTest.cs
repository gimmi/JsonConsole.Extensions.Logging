using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace JsonConsole.Extensions.Logging.Tests
{
    public class JsonConsoleLoggerTest
    {
        private StringBuilder _sb;
        private JsonConsoleLoggerProvider _sut;

        [SetUp]
        public void Setup()
        {
            _sb = new StringBuilder();
            _sut = new JsonConsoleLoggerProvider(() => new DateTime(2019, 12, 11, 20, 25, 0, DateTimeKind.Utc), new StringWriter(_sb));
        }

        [Test]
        public void Should_log_plain_information()
        {
            ILogger logger = _sut.CreateLogger("myCategory");
            logger.LogInformation("This is an information");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is an information','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory'}"
            }));
        }

        [Test]
        public void Should_format_log_message()
        {
            ILogger logger = _sut.CreateLogger("myCategory");
            logger.LogInformation("par1={par1,10:D4}", 123);

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'par1=      0123','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory'}"
            }));
        }

        [Test]
        public void Should_log_exception()
        {
            ILogger logger = _sut.CreateLogger("myCategory");
            logger.LogError(new ApplicationException("AHHH!!"), "This is an error");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is an error','l':'Error','t':'2019-12-11T20:25:00Z','c':'myCategory','e':'System.ApplicationException: AHHH!!'}"
            }));
        }

        [Test]
        public void Should_log_named_properties()
        {
            ILogger logger = _sut.CreateLogger("myCategory");
            logger.LogInformation("This is a string property: {strProp}", "value");
            logger.LogInformation("This is a numeric property: {numProp}", 456.789);
            logger.LogInformation("This is a bool property: {boolProp}", true);

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is a string property: value','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','strProp':'value'}",
                "{'m':'This is a numeric property: 456.789','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','numProp':456.789}",
                "{'m':'This is a bool property: True','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','boolProp':true}",
            }));
        }

        private string[] Pop()
        {
            var lines = _sb.ToString()
                .Replace("\"", "'")
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            _sb.Clear();
            return lines;
        }
    }
}
