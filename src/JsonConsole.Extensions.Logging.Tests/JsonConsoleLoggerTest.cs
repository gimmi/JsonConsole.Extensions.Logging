using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace JsonConsole.Extensions.Logging.Tests
{
    public class JsonConsoleLoggerTest
    {
        private Stream? _stream;
        private JsonConsoleLoggerProvider? _sut;
        private JsonConsoleLoggerOptions? _options;

        [SetUp]
        public void SetUp()
        {
            _stream = new MemoryStream();
            _options = new JsonConsoleLoggerOptions{ TimestampFieldName = "", Stream = _stream };
            _sut = new JsonConsoleLoggerProvider(_options);
            _sut.SetScopeProvider(new LoggerExternalScopeProvider());
        }

        [Test]
        public void Should_log_timestamp()
        {
            _options!.TimestampFieldName = "timestamp";

            ILogger logger = _sut!.CreateLogger("c");
            logger.LogInformation("m");

            Assert.That(Pop().Single(), Does.Match(@"\{'timestamp':'\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(.\d+)?Z','m':'m','l':'Information','c':'c'}"));
        }

        [Test]
        public void Should_log_plain_information()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation("This is an information");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is an information','l':'Information','c':'myCategory'}"
            }));
        }

        [Test]
        public void Should_log_eventid()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation(123, "Msg");
            logger.LogInformation(new EventId(456, "CustomEvent"), "Msg");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'Msg','l':'Information','c':'myCategory','i':123}",
                "{'m':'Msg','l':'Information','c':'myCategory','i':'CustomEvent'}"
            }));
        }

        [Test]
        public void Should_format_log_message()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogInformation("par1={,10:D4}", 123);

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'par1=      0123','l':'Information','c':'myCategory'}"
            }));
        }

        [Test]
        public void Should_log_exception()
        {
            ILogger logger = _sut!.CreateLogger("myCategory");
            logger.LogError(new ApplicationException("AHHH!!"), "This is an error");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'This is an error','l':'Error','c':'myCategory','x':'System.ApplicationException: AHHH!!'}"
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
                "{'m':'This is a string property: value','l':'Information','c':'myCategory','strProp':'value'}",
                "{'m':'This is a numeric property: 456.789','l':'Information','c':'myCategory','numProp':456.789}",
                "{'m':'This is a bool property: True','l':'Information','c':'myCategory','boolProp':true}",
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
                "{'m':'The message','l':'Information','c':'myCategory','prop1':'val1','prop2':456}",
            }));
        }

        [Test]
        public void Should_skip_standard_property_names()
        {
            ILogger logger = _sut!.CreateLogger("cat");
            logger.LogInformation("{m}, {l}, {t}, {c}, {i}, {x}", "m", "l", "t", "c", "i", "x");

            Assert.That(Pop(), Is.EqualTo(new[] {
                "{'m':'m, l, t, c, i, x','l':'Information','c':'cat','t':'t'}"
            }));
        }

        [Test]
        public void Should_write_unicode_chars_as_escape_sequences()
        {
            ILogger logger = _sut!.CreateLogger("cat");
            logger.LogInformation("简 - Д - ∏ - ç - é - è - ñ");

            Assert.That(Pop(), Is.EqualTo(new[] {
                @"{'m':'\u7B80 - \u0414 - \u220F - \u00E7 - \u00E9 - \u00E8 - \u00F1','l':'Information','c':'cat'}"
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
