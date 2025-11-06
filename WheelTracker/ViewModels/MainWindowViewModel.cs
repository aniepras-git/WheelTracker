// ViewModels/MainWindowViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WheelTracker.Views;

namespace WheelTracker.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        [RelayCommand]
        private void OpenTradeEntry()
        {
           new TradeEntryWindow().ShowDialog();  // This will now work
           
            // new TradeEntryWindow().ShowDialog();
        }

        [RelayCommand]
        private void OpenPositions()
        {
            new OpenPositionsWindow().Show();
        }
    }
}