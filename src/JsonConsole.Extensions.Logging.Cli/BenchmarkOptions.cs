using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging.Cli
{
    [Verb("bench", HelpText = "Do a benchmark.")]
    public class BenchmarkOptions : CommonOptions
    {
        [Option("resultpath", Default = "./result.txt", HelpText = "Where to write benchmark result")]
        public string ResultPath { get; set; } = "";

        [Option("count", Default = 100_000, HelpText = "Number of logs to write")]
        public int Count { get; set; }

        public async Task<int> RunAsync()
        {
            var serviceProvider = BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<LogOptions>>();

            var sw = Stopwatch.StartNew();

            for (int i = 1; i <= Count; i++)
            {
                logger.LogInformation("This is log #{} of {} at {}", i, Count, sw.Elapsed);
            }

            await File.AppendAllLinesAsync(ResultPath, new[]{$"{DateTime.Now:O} - Written {Count} logs in {sw.Elapsed}"});

            return 0;
        }
    }
}
