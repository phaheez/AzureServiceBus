using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleShared.Services;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            //IServiceCollection serviceDescriptors = new ServiceCollection();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var appSettingsConfig = hostContext.Configuration.GetSection(nameof(AppSettings));

                    services.AddOptions();
                    services.Configure<AppSettings>(appSettingsConfig);
                    services.AddSingleton(appSettingsConfig);
                    services.AddSingleton<IQueueService, AzureBusService>();
                    services.AddHostedService<SendMessageHandler>();
                });
    }
}
