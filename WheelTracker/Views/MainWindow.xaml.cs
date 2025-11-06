using System.Windows;
using WheelTracker.Views;

namespace WheelTracker.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenTradeEntry(object sender, RoutedEventArgs e)
            => new TradeEntryWindow().ShowDialog();

        private void OpenPositions(object sender, RoutedEventArgs e)
            => new OpenPositionsWindow().Show();
    }
}