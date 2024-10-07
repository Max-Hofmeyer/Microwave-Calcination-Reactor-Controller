using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactorControl.Classes;

namespace ReactorControl
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            try
            {
                ApplicationConfiguration.Initialize();


                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var comPortManager = serviceProvider.GetRequiredService<ComPortManager>();
                var testManager = serviceProvider.GetRequiredService<TestManager>();
                var mainForm = serviceProvider.GetRequiredService<MainForm>();

                SubscribeToEvents(comPortManager, testManager);

                Application.Run(mainForm);
                UnsubscribeFromEvents(comPortManager, testManager);
            }
            catch
            {
                Application.Exit();
            }
        }

        //register the classes since were using dependency injection
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ComPortManager>();
            services.AddSingleton<TestManager>();

            var config = LoadConfig();
            services.AddSingleton(config);

            services.AddTransient<MainForm>();
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddDebug();
            });
        }

        private static Config LoadConfig()
        {
            try
            {
                var file = File.ReadAllText("config.json");
                var config = JsonSerializer.Deserialize<Config>(file) ?? new Config();
                return config;
            }
            catch
            {
                return new Config()
                {
                    MaxTargetTemperature = 2000,
                    MinTargetTemperature = 21,
                    MaxDeltaTemperature = 100,
                    MinDeltaTemperature = 5,
                    MaxTargetHoldTime = 3600,
                    MinTargetHoldTime = 10
                };
            }
        }

        private static void SubscribeToEvents(ComPortManager comPortManager, TestManager testManager)
        {
            comPortManager.CommandReceived += testManager.OnCommandReceived;
            testManager.CommandRequested += comPortManager.OnCommandRequested;
            testManager.WatchDogTimer.Elapsed += testManager.OnWatchDogElapsed;
        }

        private static void UnsubscribeFromEvents(ComPortManager comPortManager, TestManager testManager) {
            comPortManager.CommandReceived -= testManager.OnCommandReceived;
            testManager.CommandRequested -= comPortManager.OnCommandRequested;
            testManager.WatchDogTimer.Elapsed -= testManager.OnWatchDogElapsed;
        }
    }
}