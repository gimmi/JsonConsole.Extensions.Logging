using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace JsonConsole.Extensions.Logging.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var serviceProvider = new ServiceCollection()
                .AddLogging(logging => {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddFilter(typeof(Program).Namespace, LogLevel.Trace);
                    //logging.AddConsole(console => { console.Format = ConsoleLoggerFormat.Systemd; });
                    logging.AddJsonConsole();
                })
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogTrace("This is Trace {par1,10:D4}; {}", 123, 456);
            logger.LogDebug("This is Debug");
            logger.LogInformation("This is Information");
            logger.LogWarning("This is Warning");
            try
            {
                throw new ApplicationException("AHHH!!");
            }
            catch (Exception e)
            {
                logger.LogError(e, "This is Error");
            }
            logger.LogCritical("This is Critical");
        }
    }
}
