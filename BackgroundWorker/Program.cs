using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BackgroundWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv); // loading environment variable
            // Console.WriteLine(dotenv);
            // var testEnv = Environment.GetEnvironmentVariable("test");
            // var testUserName = Environment.GetEnvironmentVariable("RabbitMQ_UserName");
            // var testPassword = Environment.GetEnvironmentVariable("RabbitMQ_Password");
            // var testVirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost");
            // var testHostName = Environment.GetEnvironmentVariable("RabbitMQ_HostName");
            // Console.WriteLine(testUserName);
            // Console.WriteLine(testPassword);
            // Console.WriteLine(testVirtualHost);
            // Console.WriteLine(testHostName);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // services.AddHostedService<Worker>();
                    services.AddHostedService<RabbitMQSubscriber>();
                });
    }
}
