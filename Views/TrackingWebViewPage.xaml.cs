using AGRA_EASY_MOBILE.Services;
namespace AGRA_EASY_MOBILE;

public partial class TrackingWebViewPage : ContentPage
{
    private readonly string _trackingUrl;
    private bool _sourceInitialized;

    public TrackingWebViewPage(string trackingUrl, string? title = null)
    {
        InitializeComponent();
        _trackingUrl = NormalizeUrl(trackingUrl);
        Title = string.IsNullOrWhiteSpace(title) ? "Suivi colis" : title;
        TrackingWebView.HandlerChanged += OnTrackingWebViewHandlerChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_sourceInitialized)
            return;

        _sourceInitialized = true;
        TrackingWebView.Source = _trackingUrl;
    }

    private static string NormalizeUrl(string url)
    {
        var value = (url ?? string.Empty).Trim();

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return $"https://{value}";
    }

    private void OnTrackingWebViewHandlerChanged(object? sender, EventArgs e)
    {
#if ANDROID
        if (TrackingWebView.Handler?.PlatformView is Android.Webkit.WebView nativeWebView)
        {
            var settings = nativeWebView.Settings;
            settings.JavaScriptEnabled = true;
            settings.DomStorageEnabled = true;
            settings.SetSupportZoom(true);
            settings.BuiltInZoomControls = true;
            settings.DisplayZoomControls = false;
            settings.UseWideViewPort = true;
            settings.LoadWithOverviewMode = true;
        }
#endif
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
        => SetLoadingState(true);

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        SetLoadingState(false);

        if (e.Result != WebNavigationResult.Success)
        {
            await ModernAlertService.ShowWarningAsync("La page de suivi n'a pas pu être chargée.");
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingOverlay.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
        => await Navigation.PopModalAsync();
}
