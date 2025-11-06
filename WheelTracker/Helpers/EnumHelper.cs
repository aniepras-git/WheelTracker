// ViewModels/EnumHelper.cs - Ensure public
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WheelTracker.Models;

namespace WheelTracker.Helpers
{
    public static class EnumHelper
    {
        public static ActionType[] ActionTypes => Enum.GetValues(typeof(ActionType)).Cast<ActionType>().ToArray();
        public static OptionStrategy[] Strategies => Enum.GetValues(typeof(OptionStrategy)).Cast<OptionStrategy>().ToArray();
        
        public static CloseType[] CloseTypes => Enum.GetValues(typeof(CloseType)).Cast<CloseType>().ToArray();

        public static string[] StatusTypes => new[] { "Open", "Closed" };

       // public static string[] CloseTypes => new[] { "BTC", "STC" , "EXP" , "ASS" , "ROLL" };
    }

    public class IsOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is ActionType a && (a == ActionType.STO || a == ActionType.BTC);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ClosedToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string s && s == "Closed";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}