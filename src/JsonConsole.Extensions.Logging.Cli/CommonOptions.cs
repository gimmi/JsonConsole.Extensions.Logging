using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging.Cli
{
    public class CommonOptions
    {
        public ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    // logging.AddFilter(typeof(Program).Namespace, LogLevel.Trace);
                    // logging.AddConsole(console => { console.Format = ConsoleLoggerFormat.Systemd; });
                    // logging.AddConsole(c => c.IncludeScopes = true);
                    logging.AddJsonConsole();
                })
                .BuildServiceProvider();
        }

        public CancellationToken BindCtrlC()
        {
            var stopCts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                stopCts.Cancel();
            };
            return stopCts.Token;
        }

        public async Task<bool> WaitNextAsync(int millisecondsDelay, CancellationToken ct)
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

        public async Task WorkerLoop(ILogger logger, int workerId, CancellationToken ct, int delayBetweenLogsMs)
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
