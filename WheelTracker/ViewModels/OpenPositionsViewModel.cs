using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WheelTracker.Models;
using WheelTracker.Services;

namespace WheelTracker.ViewModels
{
    /// <summary>
    /// ViewModel for open wheel positions: Loads, filters, refreshes quotes, and alerts on risks.
    /// Binds to OpenPositionsWindow.xaml for DataGrid display and command actions.
    /// Handles CSP/CC wheel metrics: Breakeven = Strike - Premium/100, Moneyness = (Current/Strike)*100.
    /// </summary>
    public partial class OpenPositionsViewModel : BaseViewModel
    {
        private readonly AppDbContext _db = new();
        private readonly IQuoteService _quoteService = new YahooQuoteService();
        private readonly CancellationTokenSource _alertCts = new();

        public ObservableCollection<Trade> Trades { get; } = new();
        private readonly ICollectionView _tradesView;

        [ObservableProperty]
        private string _filterTicker = "";

        [ObservableProperty]
        private bool _showOnlyOpen = true;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        public OpenPositionsViewModel()
        {
            _tradesView = CollectionViewSource.GetDefaultView(Trades);
            _tradesView.Filter = FilterTrades;

            // Fixed: Call via generated command (no direct StartAlertTimer)
            _ = Task.Run(async () => await StartAlertTimer(_alertCts.Token));
        }

        partial void OnFilterTickerChanged(string value) => _tradesView.Refresh();

        partial void OnShowOnlyOpenChanged(bool value) => _tradesView.Refresh();

        private bool FilterTrades(object obj)
        {
            if (obj is not Trade trade) return false;

            if (ShowOnlyOpen && trade.Status != "Open") return false;
            if (!string.IsNullOrWhiteSpace(FilterTicker) && !trade.Ticker.Contains(FilterTicker, StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        [RelayCommand]  // Generates LoadAsyncCommand
        private async Task LoadAsync()
        {
            try
            {
                StatusMessage = "Loading...";
                var data = await _db.Trades.OrderBy(t => t.Ticker).ThenBy(t => t.OpenDate).ToListAsync();
                var calcTasks = data.Select(async t => await CalculateTradeAsync(t));  // Fixed: Await each
                await Task.WhenAll(calcTasks);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Trades.Clear();
                    foreach (var t in data) Trades.Add(t);
                });

                StatusMessage = $"{Trades.Count} trades loaded";
                Log.Information("Loaded {Count} trades", Trades.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Load error: {ex.Message}";
                Log.Error(ex, "Load failed");
                ToastService.Instance.ShowError($"Failed to load positions: {ex.Message}");
            }
        }

        [RelayCommand(IncludeCancelCommand = true)]
        private async Task RefreshQuotesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                StatusMessage = "Refreshing quotes...";
                var openTrades = Trades.Where(t => t.Status == "Open").ToList();
                var updateTasks = openTrades.Select(async t =>
                {
                    t.CurrentSharePrice = await _quoteService.GetPriceAsync(t.Ticker);
                    await CalculateTradeAsync(t);
                });

                await Task.WhenAll(updateTasks).ConfigureAwait(false);
                _tradesView.Refresh();
                StatusMessage = "Quotes updated";
                Log.Information("Refreshed quotes for {Count} positions", openTrades.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = "Refresh failed";
                Log.Error(ex, "Quote refresh failed");
                ToastService.Instance.ShowError($"Quote update error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SaveChanges()
        {
            try
            {
                foreach (var trade in Trades)
                {
                    if (trade.Status == "Closed" && trade.CloseDate == null) trade.CloseDate = DateTime.Today;
                    CalculateTrade(trade);
                }
                _db.UpdateRange(Trades);
                _db.SaveChanges();
                StatusMessage = "Saved!";
                Log.Information("Saved {Count} changes", Trades.Count);
                ToastService.Instance.ShowInformation("Changes saved successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Log.Error(ex, "Save failed");
                ToastService.Instance.ShowError($"Save failed: {ex.Message}");
            }
        }

        private async Task CalculateTradeAsync(Trade trade)
        {
            CalculateTrade(trade);

            if (trade.Status == "Open" && trade.Strategy == OptionStrategy.CSP &&
                trade.CurrentSharePrice.HasValue && trade.Strike.HasValue && trade.Qty > 0)
            {
                var premiumPerShare = trade.Premium / 100.0;
                trade.Breakeven = (decimal)(trade.Strike.Value - premiumPerShare);  // Fixed: Cast double to decimal
                trade.UnrealizedGainLoss = (trade.CurrentSharePrice.Value - (double)trade.Breakeven) * trade.Qty * 100;  // Cast back if needed
                trade.Moneyness = (trade.CurrentSharePrice.Value / trade.Strike.Value) * 100;
            }
        }

        private void CalculateTrade(Trade trade)
        {
            trade.InitialDTE = trade.Expiration.HasValue ? (int)(trade.Expiration.Value - trade.OpenDate).TotalDays : 0;
            trade.Total = trade.CreditDebit == "Credit" ? trade.Premium * trade.Qty * 100 - trade.Fees : -(trade.Premium * trade.Qty * 100 + trade.Fees);
            double capital = trade.Strike.HasValue ? trade.Strike.Value * trade.Qty * 100 : 1;
            trade.CalculatedReturn = capital > 0 ? (trade.Total ?? 0) / capital : 0;
            trade.AnnualReturn = trade.InitialDTE > 0 ? trade.CalculatedReturn * (365.0 / trade.InitialDTE) : 0;
            trade.DTE = trade.Expiration.HasValue ? (int)(trade.Expiration.Value - DateTime.Today).TotalDays : 0;

            if (trade.Status == "Closed" && trade.CloseTransType.HasValue)
            {
                switch (trade.CloseTransType.Value)
                {
                    case CloseType.EXP:
                        trade.RealizedGainLoss = trade.Total ?? 0;
                        break;
                    case CloseType.BTC when trade.ClosePrice.HasValue:
                        double closeValue = trade.Action == ActionType.STO
                            ? -(trade.ClosePrice.Value * trade.Qty * 100) - (trade.CloseFee ?? 0)
                            : (trade.ClosePrice.Value * trade.Qty * 100) - (trade.CloseFee ?? 0);
                        trade.RealizedGainLoss = closeValue + (trade.Total ?? 0);
                        break;
                    case CloseType.ASS:
                        trade.RealizedGainLoss = trade.Total ?? 0;  // Premium retained; shares at cost basis
                        trade.DaysHeld ??= trade.CloseDate.HasValue ? (int)(trade.CloseDate.Value - trade.OpenDate).TotalDays : 0;
                        break;
                    case CloseType.ROLL when trade.ClosePrice.HasValue && trade.CloseQty.HasValue:
                        trade.RealizedGainLoss = (trade.Total ?? 0) + (trade.ClosePrice.Value * trade.CloseQty.Value * 100) - (trade.CloseFee ?? 0);
                        break;
                }
            }
        }

        private async Task StartAlertTimer(CancellationToken cancellationToken)  // Fixed: Awaitable, private
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(300000, cancellationToken);  // 5min check
                foreach (var trade in Trades.Where(t => t.Status == "Open"))
                {
                    if (trade.DTE < 7)
                        ToastService.Instance.ShowWarning($"CSP on {trade.Ticker} expires soon (DTE: {trade.DTE})");  // Fixed: Delimiter
                    if (trade.Moneyness.HasValue && trade.Moneyness < 95)
                        ToastService.Instance.ShowInformation($"{trade.Ticker} at risk: {trade.Moneyness:F1}% to strike");  // Fixed: Delimiter
                }
            }
        }

        // Cleanup: Call from Window.Closing event if needed
        public void Dispose() => _alertCts.Cancel();
    }
}