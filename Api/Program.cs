using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] Application is starting...");

            var host = CreateHostBuilder(args).Build();
            
            using var factory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = factory.CreateLogger("Program");
            
            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"[Main Error] Application failed to become ready: {ex.Message}");
            }
        }


        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}