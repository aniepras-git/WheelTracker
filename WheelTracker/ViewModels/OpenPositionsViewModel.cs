using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;  // For ICollectionView
using WheelTracker.Models;
using WheelTracker.Services;

namespace WheelTracker.ViewModels
{
    public partial class OpenPositionsViewModel : BaseViewModel
    {
        private readonly AppDbContext _db = new();
        public ObservableCollection<Trade> Trades { get; } = new();
        private readonly ICollectionView _tradesView;

        [ObservableProperty] private string _filterTicker = "";
        [ObservableProperty] private bool _showOnlyOpen = true;
        [ObservableProperty] private string _statusMessage = "Ready";

        public OpenPositionsViewModel()
        {
            _tradesView = CollectionViewSource.GetDefaultView(Trades);
            _tradesView.Filter = FilterTrades;

            LoadCommand.Execute(null); // Load initial data
        }

        partial void OnFilterTickerChanged(string value) => _tradesView.Refresh();
        partial void OnShowOnlyOpenChanged(bool value) => _tradesView.Refresh();

        private bool FilterTrades(object obj)
        {
            if (obj is not Trade trade) return false;

            if (ShowOnlyOpen && trade.Status != "Open")
                return false;

            if (!string.IsNullOrWhiteSpace(FilterTicker) &&
                !trade.Ticker.Contains(FilterTicker, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        [RelayCommand]
        private void Load()
        {
            var data = _db.Trades
                          .OrderBy(t => t.Ticker)
                          .ThenBy(t => t.OpenDate)
                          .ToList();

            foreach (var trade in data) CalculateTrade(trade);

            Trades.Clear();
            foreach (var t in data) Trades.Add(t);

            StatusMessage = $"{Trades.Count} trades loaded";
        }

        [RelayCommand]
        private void SaveChanges()
        {
            try
            {
                foreach (var trade in Trades)
                {
                    CalculateTrade(trade);
                    if (trade.Status == "Closed" && trade.CloseDate == null)
                        trade.CloseDate = DateTime.Today;
                }

                _db.UpdateRange(Trades);
                _db.SaveChanges();
                StatusMessage = "All changes saved!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void CalculateTrade(Trade trade)
        {
            // Initial DTE
            trade.InitialDTE = trade.Expiration.HasValue ? (trade.Expiration.Value - trade.OpenDate).Days : 0;

            // Total
            trade.Total = trade.CreditDebit == "Credit"
                ? trade.Premium * trade.Qty * 100 - trade.Fees
                : -(trade.Premium * trade.Qty * 100 + trade.Fees);

            // Capital at Risk
            double capital = trade.Strike.HasValue ? trade.Strike.Value * trade.Qty * 100 : 1;

            // Return %
            trade.CalculatedReturn = capital > 0 ? (trade.Total ?? 0) / capital : 0;

            // Annual %
            trade.AnnualReturn = trade.InitialDTE > 0 ? trade.CalculatedReturn * (365.0 / trade.InitialDTE) : 0;

            // Realized G/L (only when closed)
            if (trade.Status == "Closed" && trade.CloseTransType == CloseType.EXP)
            {
                trade.RealizedGainLoss = (trade.Total ?? 0);
            }
            else if (trade.Status == "Closed" && trade.CloseTransType == CloseType.BTC && trade.ClosePrice.HasValue)
            {
                double closeValue = trade.Action == ActionType.STO
                    ? -(trade.ClosePrice.Value * trade.Qty * 100) - (trade.CloseFee ?? 0)
                    : (trade.ClosePrice.Value * trade.Qty * 100) - (trade.CloseFee ?? 0);
                trade.RealizedGainLoss = closeValue + (trade.Total ?? 0);
            }
            else
            {
                trade.RealizedGainLoss = null;
            }
        }

        private void ApplyFilter()
        {
            var query = _db.Trades.AsQueryable();

            if (ShowOnlyOpen)
                query = query.Where(t => t.Status == "Open");

            if (!string.IsNullOrWhiteSpace(FilterTicker))
                query = query.Where(t => t.Ticker.Contains(FilterTicker, StringComparison.OrdinalIgnoreCase));

            var list = query.OrderBy(t => t.Ticker).ToList();
            foreach (var t in list) CalculateTrade(t);

            Trades.Clear();
            foreach (var t in list) Trades.Add(t);
        }
    }
}