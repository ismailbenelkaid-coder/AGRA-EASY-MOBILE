using BarcodeScanning;
using AGRA_EASY_MOBILE.Services;

namespace AGRA_EASY_MOBILE;

public partial class BarcodeScannerPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _completion = new();
    private bool _isClosing;

    public BarcodeScannerPage()
    {
        InitializeComponent();
    }

    public Task<string?> Completion => _completion.Task;

    public static async Task<string?> ShowAsync(Page owner)
    {
        var page = new BarcodeScannerPage();
        await owner.Navigation.PushModalAsync(page);
        return await page.Completion;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await Methods.AskForRequiredPermissionAsync();
            BarcodeCamera.CameraEnabled = true;
            _ = AnimateScanLineAsync();
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowWarningAsync($"Le scanner code-barres n'est pas disponible : {ex.Message}");
            await CompleteAsync(null);
        }
    }

    protected override void OnDisappearing()
    {
        BarcodeCamera.CameraEnabled = false;
        base.OnDisappearing();
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () => await CompleteAsync(null));
        return true;
    }

    private async Task AnimateScanLineAsync()
    {
        while (!_isClosing && BarcodeCamera.CameraEnabled)
        {
            await ScanLine.TranslateTo(0, 174, 900, Easing.SinInOut);
            if (_isClosing || !BarcodeCamera.CameraEnabled)
                break;

            await ScanLine.TranslateTo(0, 0, 900, Easing.SinInOut);
        }
    }

    private async void BarcodeCamera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        if (_isClosing || e.BarcodeResults == null || e.BarcodeResults.Count == 0)
            return;

        string? code = e.BarcodeResults
            .Cast<object>()
            .Select(ExtractBarcodeValue)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        if (string.IsNullOrWhiteSpace(code))
            return;

        await CompleteAsync(code.Trim());
    }

    private static string? ExtractBarcodeValue(object barcodeResult)
    {
        var type = barcodeResult.GetType();
        foreach (string propertyName in new[] { "DisplayValue", "RawValue", "Value", "Text" })
        {
            var property = type.GetProperty(propertyName);
            var value = property?.GetValue(barcodeResult)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        string? fallback = barcodeResult.ToString();
        return string.IsNullOrWhiteSpace(fallback) ? null : fallback;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CompleteAsync(null);
    }

    private async Task CompleteAsync(string? code)
    {
        if (_isClosing)
            return;

        _isClosing = true;
        BarcodeCamera.CameraEnabled = false;
        _completion.TrySetResult(code);
        await Navigation.PopModalAsync();
    }
}
