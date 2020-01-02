using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace JsonConsole.Extensions.Logging.Tests
{
    public class JsonConsoleLoggerTest
    {
        private Stream? _stream;
        private JsonConsoleLoggerProvider? _sut;

        [SetUp]
        public void SetUp()
        {
            _stream = new MemoryStream();
            _sut = new JsonConsoleLoggerProvider(() => new DateTime(2019, 12, 11, 20, 25, 0, DateTimeKind.Utc), _stream);
            _sut.SetScopeProvider(new LoggerExternalScopeProvider());
        }

        [Test]
        public void Should_log_plain_information()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation("This is an information");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is an information','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory'}"
            }));
        }

        [Test]
        public void Should_log_eventid()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation(123, "Msg");
            logger.LogInformation(new EventId(456, "CustomEvent"), "Msg");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'Msg','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','i':123}",
                "{'m':'Msg','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','i':'CustomEvent'}"
            }));
        }

        [Test]
        public void Should_format_log_message()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation("par1={,10:D4}", 123);

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'par1=      0123','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory'}"
            }));
        }

        [Test]
        public void Should_log_exception()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogError(new ApplicationException("AHHH!!"), "This is an error");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is an error','l':'Error','t':'2019-12-11T20:25:00Z','c':'myCategory','x':'System.ApplicationException: AHHH!!'}"
            }));
        }

        [Test]
        public void Should_log_named_properties()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation("This is a string property: {strProp}", "value");
            logger.LogInformation("This is a numeric property: {numProp}", 456.789);
            logger.LogInformation("This is a bool property: {boolProp}", true);

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is a string property: value','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','strProp':'value'}",
                "{'m':'This is a numeric property: 456.789','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','numProp':456.789}",
                "{'m':'This is a bool property: True','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','boolProp':true}",
            }));
        }

        [Test]
        public void Should_include_scope_properties()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");

            using (logger.BeginScope("{prop1}", "val1"))
            {
                using (logger.BeginScope("{prop2}", 456))
                {
                    logger.LogInformation("The message");
                }
            }

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'The message','l':'Information','t':'2019-12-11T20:25:00Z','c':'myCategory','prop1':'val1','prop2':456}",
            }));
        }

        private string[] Pop()
        {
            _stream!.Seek(0, SeekOrigin.Begin);
            var lines = new StreamReader(_stream)
                .ReadToEnd()
                .Replace("\"", "'")
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return lines;
        }
    }
}
