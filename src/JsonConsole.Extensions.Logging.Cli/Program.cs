using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace JsonConsole.Extensions.Logging.Cli
{
    public class Program
    {
        private const int DelayBetweenLogsMs = 100;

        public static async Task Main(string[] args)
        {
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

            var ct = BindCtrlC();

            await Task.WhenAll(
                Task.Run(() => Run(logger, 1, ct), ct),
                Task.Run(() => Run(logger, 2, ct), ct)
            );

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

        public static async Task Run(ILogger logger, int workerId, CancellationToken ct)
        {
            var count = 1;
            using var workerScope = logger.BeginScope("{workerId}", workerId);
            while (await WaitNextAsync(DelayBetweenLogsMs, ct))
            {
                using var scope2 = logger.BeginScope("{innerScope}", "Something");
                logger.LogInformation("Log #{}", count++);
            }
        }
    }
}
