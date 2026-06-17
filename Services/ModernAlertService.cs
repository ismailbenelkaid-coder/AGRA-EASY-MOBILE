using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Graphics;

namespace AGRA_EASY_MOBILE.Services
{
    public static class ModernAlertService
    {
        public static Task ShowInfoAsync(string message)
            => ShowAsync(message, Color.FromArgb("#2563EB"));

        public static Task ShowSuccessAsync(string message)
            => ShowAsync(message, Color.FromArgb("#16A34A"));

        public static Task ShowWarningAsync(string message)
            => ShowAsync(message, Color.FromArgb("#F59E0B"));

        public static Task ShowErrorAsync(string message)
            => ShowAsync(message, Color.FromArgb("#DC2626"));

        private static async Task ShowAsync(string message, Color backgroundColor)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = backgroundColor,
                TextColor = Colors.White,
                CornerRadius = new CornerRadius(10)
            };

            var snackbar = Snackbar.Make(
                message,
                duration: TimeSpan.FromSeconds(5),
                visualOptions: snackbarOptions);

            await snackbar.Show();
        }
    }
}
