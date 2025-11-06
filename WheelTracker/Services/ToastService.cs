using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WheelTracker.Services
{
    /// <summary>
    /// Lightweight toast service for WPF alerts (e.g., wheel expiration risks).
    /// Uses animated Popups—no external deps, auto-queues to avoid overlap.
    /// </summary>
    public class ToastService
    {
        public static readonly ToastService Instance = new();

        private readonly ConcurrentQueue<ToastMessage> _queue = new();
        private Popup? _currentPopup;
        private readonly object _lock = new();

        private ToastService() { }  // Singleton

        /// <summary>
        /// Queues and displays a toast. Auto-dismiss after duration; supports queueing.
        /// </summary>
        public void Show(ToastType type, string message, int durationMs = 5000)
        {
            lock (_lock)
            {
                _queue.Enqueue(new ToastMessage { Type = type, Message = message, Duration = durationMs });
                if (_currentPopup == null) ProcessQueue();
            }
        }

        /// <summary>
        /// Convenience: Info (blue, neutral).
        /// </summary>
        public void ShowInformation(string message) => Show(ToastType.Information, message);

        /// <summary>
        /// Convenience: Warning (orange, e.g., near expiry).
        /// </summary>
        public void ShowWarning(string message) => Show(ToastType.Warning, message);

        /// <summary>
        /// Convenience: Error (red, e.g., API fail).
        /// </summary>
        public void ShowError(string message) => Show(ToastType.Error, message);

        private async void ProcessQueue()
        {
            if (_queue.TryDequeue(out var toast)) await ShowToastAsync(toast);
            if (_queue.IsEmpty) _currentPopup = null;  // Ready for next
            else _ = Task.Run(ProcessQueue);  // Recurse for queue
        }

        private async Task ShowToastAsync(ToastMessage toast)
        {
            // Create overlay Popup (bottom-right, 300x80)
            var border = new Border
            {
                Background = GetBrush(toast.Type),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(10),
                Width = 300,
                Child = new TextBlock
                {
                    Text = toast.Message,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            _currentPopup = new Popup
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = Application.Current.MainWindow,
                HorizontalOffset = SystemParameters.PrimaryScreenWidth - 320,
                VerticalOffset = SystemParameters.PrimaryScreenHeight - 100,
                Child = border,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            // Fade-in animation (0.3s ease)
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(UIElement.OpacityProperty, fadeIn);  // Fixed: UIElement.OpacityProperty

            _currentPopup.IsOpen = true;

            // Auto-dismiss after duration + fade-out
            await Task.Delay(toast.Duration);
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (s, e) => _currentPopup!.IsOpen = false;
            border.BeginAnimation(UIElement.OpacityProperty, fadeOut);  // Fixed: UIElement.OpacityProperty
        }

        private static Brush GetBrush(ToastType type) => type switch
        {
            ToastType.Information => new SolidColorBrush(Color.FromRgb(52, 152, 219)),  // Blue
            ToastType.Warning => new SolidColorBrush(Color.FromRgb(243, 156, 18)),    // Orange
            ToastType.Error => new SolidColorBrush(Color.FromRgb(231, 76, 60)),       // Red
            _ => Brushes.Gray
        };

        public enum ToastType  // Fixed: Public for method accessibility
        {
            Information,
            Warning,
            Error
        }

        private class ToastMessage
        {
            public ToastType Type { get; set; } = default;
            public string Message { get; set; } = string.Empty;
            public int Duration { get; set; }
        }
    }
}