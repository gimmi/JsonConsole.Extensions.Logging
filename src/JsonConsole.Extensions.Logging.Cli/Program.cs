using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace JsonConsole.Extensions.Logging.Cli
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var workerCount = Convert.ToInt32(args.FirstOrDefault() ?? "1");
            var delayBetweenLogsMs = Convert.ToInt32(args.Skip(1).FirstOrDefault() ?? "1000");

            var serviceProvider = new ServiceCollection()
                .AddLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    // logging.AddFilter(typeof(Program).Namespace, LogLevel.Trace);
                    // logging.AddConsole(console => { console.Format = ConsoleLoggerFormat.Systemd; });
                    // logging.AddConsole(c => c.IncludeScopes = true);
                    logging.AddJsonConsole();
                })
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("workerCount: {} delayBetweenLogsMs: {}", workerCount, delayBetweenLogsMs);

            var ct = BindCtrlC();

            var workerTasks = Enumerable.Range(1, workerCount)
                .Select(workerId => Task.Run(() => WorkerLoop(logger, workerId, ct, delayBetweenLogsMs), ct))
                .ToList();

            await Task.WhenAll(workerTasks);

            await serviceProvider.DisposeAsync();
        }

        public static CancellationToken BindCtrlC()
        {
            var stopCts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                stopCts.Cancel();
            };
            return stopCts.Token;
        }

        public static async Task<bool> WaitNextAsync(int millisecondsDelay, CancellationToken ct)
        {
            try
            {
                await Task.Delay(millisecondsDelay, ct);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public static async Task WorkerLoop(ILogger logger, int workerId, CancellationToken ct, int delayBetweenLogsMs)
        {
            var count = 1;
            using var workerScope = logger.BeginScope("{workerId}", workerId);
            while (await WaitNextAsync(delayBetweenLogsMs, ct))
            {
                using var scope2 = logger.BeginScope("{innerScope}", "Something");
                logger.LogInformation("Log #{}", count++);
            }
        }
    }
}
