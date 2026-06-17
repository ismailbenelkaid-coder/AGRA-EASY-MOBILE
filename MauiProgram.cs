using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using BarcodeScanning;
#if ANDROID || IOS || MACCATALYST || WINDOWS
using Microsoft.Maui.Handlers;
#endif
#if ANDROID || IOS
using MauiNativePdfView;
#endif

namespace AGRA_EASY_MOBILE
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            var appBuilder = builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseBarcodeScanning();

#if ANDROID || IOS
            appBuilder.UseMauiNativePdfView();
#endif


#if ANDROID
            EntryHandler.Mapper.AppendToMapping(nameof(BorderlessEntry), (handler, view) =>
            {
                if (view is BorderlessEntry)
                {
                    handler.PlatformView.Background = null;
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    handler.PlatformView.SetPadding(0, 0, 0, 0);
                    handler.PlatformView.Gravity = Android.Views.GravityFlags.Center;
                }
            });

            PickerHandler.Mapper.AppendToMapping(nameof(BorderlessPicker), (handler, view) =>
            {
                if (view is BorderlessPicker)
                {
                    handler.PlatformView.Background = null;
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    handler.PlatformView.SetPadding(0, 0, 0, 0);
                    handler.PlatformView.Gravity = Android.Views.GravityFlags.Center;
                }
            });

            DatePickerHandler.Mapper.AppendToMapping(nameof(BorderlessDatePicker), (handler, view) =>
            {
                if (view is BorderlessDatePicker)
                {
                    handler.PlatformView.Background = null;
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    handler.PlatformView.SetPadding(0, 0, 0, 0);
                    handler.PlatformView.Gravity = Android.Views.GravityFlags.Center;
                }
            });
#endif


#if IOS || MACCATALYST
            PickerHandler.Mapper.AppendToMapping(nameof(BorderlessPicker), (handler, view) =>
            {
                if (view is BorderlessPicker)
                {
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                    handler.PlatformView.TextAlignment = UIKit.UITextAlignment.Center;
                    handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                }
            });
#endif

#if WINDOWS
            PickerHandler.Mapper.AppendToMapping(nameof(BorderlessPicker), (handler, view) =>
            {
                if (view is BorderlessPicker)
                {
                    handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                    handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
                    handler.PlatformView.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
                    handler.PlatformView.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                }
            });
#endif

            appBuilder.ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
