using AGRA_EASY_MOBILE.Services;
using Services;

namespace AGRA_EASY_MOBILE;

public partial class ShippingWarningDetailView : ContentPage
{
    private readonly ShippingWarning _source;
    private bool _loaded;

    public ShippingWarningDetailView(ShippingWarning source)
    {
        InitializeComponent();
        _source = source;
        ApplyWarning(source);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
            return;

        _loaded = true;
        await LoadDetailAsync();
    }

    private async Task LoadDetailAsync()
    {
        if (string.IsNullOrWhiteSpace(_source.ShippingWarningId))
            return;

        SetLoading(true);
        try
        {
            var details = await EasySession.GetShippingWarningAsync(_source.ShippingWarningId, _source.OriginalWarehouse ?? string.Empty) ?? Array.Empty<ShippingWarning>();
            var detail = details.FirstOrDefault();
            if (detail != null)
                ApplyWarning(detail);
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ApplyWarning(ShippingWarning warning)
    {
        SubjectLabel.Text = string.IsNullOrWhiteSpace(warning.ShortDescription) ? "Alerte" : warning.ShortDescription;
        DateLabel.Text = warning.CreationDate == default ? string.Empty : warning.CreationDate.ToString("dd/MM/yyyy HH:mm");
        var code = warning.AccountCode?.Trim() ?? string.Empty;
        var name = warning.AccountName?.Trim() ?? string.Empty;
        var clientText = !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name)
            ? $"{code} - {name}"
            : (code + name).Trim();
        var showClient = !EasySession.IsClient && !string.IsNullOrWhiteSpace(clientText);
        ClientLineLabel.Text = clientText;
        ClientLineLabel.IsVisible = showClient;
        ClientSeparator.IsVisible = showClient;
        MessageLabel.Text = string.IsNullOrWhiteSpace(warning.WarningMessage) ? warning.ShortDescription : warning.WarningMessage;
    }

    private void SetLoading(bool loading)
    {
        LoadingOverlay.IsVisible = loading;
        LoadingIndicator.IsRunning = loading;
    }
}
