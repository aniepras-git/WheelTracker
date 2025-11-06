using Serilog;
using System;
using System.Windows;

namespace WheelTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()  // Now works with Serilog.Sinks.Console
                .WriteTo.File("logs/wheeltracker-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Optional: Test toast on startup
            // ToastService.Instance.ShowInformation("WheelTracker loaded—monitoring positions.");

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}