using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactorControl.Classes;
using Serilog;

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
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo
                .File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();

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
            catch (Exception ex)
            {
                Log.Fatal(ex, "App crashed");
                Application.Exit();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        //register the classes since were using dependency injection
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<StateStore>();
            services.AddSingleton<ComPortManager>();
            services.AddSingleton<TestManager>();
            services.AddSingleton<MainForm>();


            var config = LoadConfig();
            services.AddSingleton(config);

            services.AddLogging(loggingBuilder => {
                loggingBuilder.ClearProviders();                        // drop Console/WPF defaults
                loggingBuilder.AddSerilog(Log.Logger, dispose: true);   // use our Serilog instance
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
            comPortManager.MessageReceived += testManager.OnMessageReceived;
            comPortManager.AckReceived += testManager.OnAckReceived;
            testManager.CommandRequested += comPortManager.OnCommandRequested;
            testManager.WatchDogTimer.Elapsed += testManager.OnWatchDogElapsed;
        }

        private static void UnsubscribeFromEvents(ComPortManager comPortManager, TestManager testManager) {
            comPortManager.MessageReceived -= testManager.OnMessageReceived;
            comPortManager.AckReceived -= testManager.OnAckReceived;
            testManager.CommandRequested -= comPortManager.OnCommandRequested;
            testManager.WatchDogTimer.Elapsed -= testManager.OnWatchDogElapsed;
        }
    }
}