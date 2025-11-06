// ViewModels/TradeEntryViewModel.cs - Set required fields in initializer
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Windows;
using WheelTracker.Models;
using WheelTracker.Services;

namespace WheelTracker.ViewModels
{
    public partial class TradeEntryViewModel : BaseViewModel
    {
        private readonly AppDbContext _db = new();

        [ObservableProperty]
        private Trade _newTrade = new()
        {
            Ticker = string.Empty,  // Explicitly set
            OpenDate = DateTime.Today,
            Action = ActionType.STO,  // Default action
            CreditDebit = "Credit",
            Qty = 1
        };

        
        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(NewTrade.Ticker))
            {
                MessageBox.Show("Ticker is required!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _db.Trades.Add(NewTrade);
                _db.SaveChanges();
                Log.Information("Saved new trade: {Ticker} {Action}", NewTrade.Ticker, NewTrade.Action);

                MessageBox.Show($"Trade saved: {NewTrade.Ticker} {NewTrade.Action}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset...
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Save trade failed");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}