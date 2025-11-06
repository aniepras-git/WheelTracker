// App.xaml.cs
using System.Windows;
using WheelTracker.Services;

namespace WheelTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // THIS CREATES THE DB + TABLE
            new AppDbContext().EnsureDatabaseCreated();

            new Views.MainWindow().Show();
        }
    }
}