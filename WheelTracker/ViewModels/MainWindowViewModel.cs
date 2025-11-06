using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using WheelTracker.Views;

namespace WheelTracker.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        [ObservableProperty] private ISeries[] _allocationSeries = new ISeries[2];  // For pie

        public MainWindowViewModel()
        {
            UpdateAllocation();  // Initial calc (pull from DB in prod)
        }

        [RelayCommand]
        private void OpenTradeEntry() => new TradeEntryWindow().ShowDialog();

        [RelayCommand]
        private void OpenPositions() => new OpenPositionsWindow().Show();

        [RelayCommand]
        private void ExportReport()
        {
            // Stub: Use CsvHelper to export Trades to CSV
            // Implementation in a service; e.g., var records = Trades.Select(t => new { t.Ticker, t.AnnualReturn });
        }

        private void UpdateAllocation()
        {
            // Sample: 60% CSP capital, 40% shares (compute from open Trades)
            AllocationSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] { 60 }, Name = "CSPs", Fill = new SolidColorPaint(SKColors.Green) },
                new PieSeries<double> { Values = new double[] { 40 }, Name = "Shares", Fill = new SolidColorPaint(SKColors.Orange) }
            };
        }
    }
}