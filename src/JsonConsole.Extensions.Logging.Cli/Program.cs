using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace JsonConsole.Extensions.Logging.Cli
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                return await Parser.Default.ParseArguments<LogOptions, BenchmarkOptions>(args).MapResult(
                    (LogOptions o) => o.RunAsync(),
                    (BenchmarkOptions o) => o.RunAsync(),
                    error => Task.FromResult(1)
                );
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
                return 1;
            }
        }

    }
}
