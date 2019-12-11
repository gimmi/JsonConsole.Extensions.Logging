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
        public static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddFilter(typeof(Program).Namespace, LogLevel.Trace);
                    //logging.AddConsole(console => { console.Format = ConsoleLoggerFormat.Systemd; });
                    logging.AddJsonConsole();
                })
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            var count = 1;
            var ct = BindCtrlC();
            while (await WaitNextAsync(1_000, ct))
            {
                logger.LogInformation("Log #{}", count++);
            }

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

    }
}
