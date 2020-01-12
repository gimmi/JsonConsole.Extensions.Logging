using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging.Cli
{
    [Verb("log", HelpText = "Just keep log.")]
    public class LogOptions : CommonOptions
    {
        [Option("workers", Default = 1, HelpText = "Number of parallel workers")]
        public int WorkerCount { get; set; }

        [Option("delay", Default = 1_000, HelpText = "Delay between logs, in milliseconds")]
        public int DelayBetweenLogsMs { get; set; }

        public async Task<int> RunAsync()
        {
            var serviceProvider = BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<LogOptions>>();

            logger.LogInformation("workerCount: {} delayBetweenLogsMs: {}", WorkerCount, DelayBetweenLogsMs);

            var ct = BindCtrlC();

            var workerTasks = Enumerable.Range(1, WorkerCount)
                .Select(workerId => Task.Run(() => WorkerLoop(logger, workerId, ct, DelayBetweenLogsMs), ct))
                .ToList();

            await Task.WhenAll(workerTasks);

            await serviceProvider.DisposeAsync();

            return 0;
        }
    }
}
