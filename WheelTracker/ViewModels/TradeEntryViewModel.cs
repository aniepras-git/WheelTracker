// ViewModels/TradeEntryViewModel.cs - Set required fields in initializer
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

            _db.Trades.Add(NewTrade);
            _db.SaveChanges();

            MessageBox.Show($"Trade saved: {NewTrade.Ticker} {NewTrade.Action}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reset form
            NewTrade = new Trade
            {
                Ticker = string.Empty,
                OpenDate = DateTime.Today,
                Action = ActionType.STO,
                CreditDebit = "Credit",
                Qty = 1
            };
        }
    }
}