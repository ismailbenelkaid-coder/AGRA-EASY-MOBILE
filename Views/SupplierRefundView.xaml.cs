using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using Services;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AGRA_EASY_MOBILE;

public partial class SupplierRefundView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string ArticleSortLabel = "Article";
    private const string ReturnSortLabel = "Code retour";
    private const string ReturnClientSortLabel = "Code retour client";
    private const string SupplierResponseSortLabel = "État";
    private const string CreationDateSortLabel = "Date création";

    private bool _isReadyToLoadData;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;
    private bool _isUpdatingSortPicker;

    public bool ShowAccountInfo => !string.Equals(EasySession.CurrentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);
    public bool ShowAdminStatusCode => string.Equals(EasySession.CurrentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    public SupplierRefundView()
    {
        InitializeComponent();
        BindingContext = this;
        InitializeSortPicker();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isReadyToLoadData = false;

        GlobalState.EnsureReturnFilterWithSlidingWeek();

        var currentAccount = EasySession.CurrentAccount;
        var isAdministrator = string.Equals(currentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
        var isClient = string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);

        if (isClient && !string.IsNullOrWhiteSpace(currentAccount?.AccountCode))
            GlobalState.EnsureClientReturnFilter(currentAccount.AccountCode);

        if (isAdministrator && !GlobalState.HasValidAdminReturnFilter())
        {
            await Navigation.PushModalAsync(new ReturnFilterView(true));
            return;
        }

        CheckTreated.IsChecked = GlobalState.CurrentReturnFilter.WithTreated;
        OnPropertyChanged(nameof(ShowAccountInfo));
        OnPropertyChanged(nameof(ShowAdminStatusCode));
        RefreshSortPickerItems(true);
        _isReadyToLoadData = true;
        await RefreshDataAsync();
    }

    private void InitializeSortPicker()
    {
        RefreshSortPickerItems(false);
    }

    private void RefreshSortPickerItems(bool preserveSelected = true)
    {
        if (PickerSortMode == null)
            return;

        var selected = preserveSelected ? _selectedSortMode : DefaultSortLabel;
        var availableModes = new List<string>
        {
            DefaultSortLabel,
            ArticleSortLabel,
            ReturnSortLabel,
            ReturnClientSortLabel,
            SupplierResponseSortLabel,
            CreationDateSortLabel
        };

        if (!availableModes.Contains(selected))
            selected = DefaultSortLabel;

        _isUpdatingSortPicker = true;
        PickerSortMode.Items.Clear();
        foreach (var mode in availableModes)
            PickerSortMode.Items.Add(mode);

        PickerSortMode.SelectedIndex = availableModes.IndexOf(selected);
        _selectedSortMode = selected;
        _isUpdatingSortPicker = false;

        OnPropertyChanged(nameof(ShowSortDirection));
        OnPropertyChanged(nameof(SortDirectionIcon));
    }

    private async Task RefreshDataAsync()
    {
        if (!_isReadyToLoadData || GlobalState.CurrentReturnFilter == null)
            return;

        SetLoadingState(true);
        try
        {
            var rawData = await EasySession.GetSupplierRefundsLinesAsync(GlobalState.CurrentReturnFilter) ?? Array.Empty<SupplierRefundLine>();
            SupplierRefundCollection.ItemsSource = ApplySort(rawData);
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private SupplierRefundLine[] ApplySort(SupplierRefundLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data
                .OrderByDescending(x => x.CreationDate)
                .ThenByDescending(x => x.SupplierRefundLineCode ?? string.Empty)
                .ToArray();

        IOrderedEnumerable<SupplierRefundLine> orderedData = _selectedSortMode switch
        {
            ArticleSortLabel => _sortAscending
                ? data.OrderBy(x => x.ProductCode ?? string.Empty).ThenBy(x => x.ReturnCode ?? string.Empty)
                : data.OrderByDescending(x => x.ProductCode ?? string.Empty).ThenByDescending(x => x.ReturnCode ?? string.Empty),

            ReturnSortLabel => _sortAscending
                ? data.OrderBy(x => x.ReturnCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.ReturnCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            ReturnClientSortLabel => _sortAscending
                ? data.OrderBy(x => x.ReturnClientCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.ReturnClientCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            SupplierResponseSortLabel => _sortAscending
                ? data.OrderBy(x => x.SupplierResponse ?? string.Empty).ThenBy(x => x.ReturnCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.SupplierResponse ?? string.Empty).ThenByDescending(x => x.ReturnCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.ReturnCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.ReturnCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async Task<SupplierRefundLine?> LoadSupplierRefundDetailAsync(SupplierRefundLine item)
    {
        if (item == null)
            return null;

        var details = await EasySession.GetSupplierRefundLineAsync(item.SupplierRefundLineCode, item.Warehouse) ?? Array.Empty<SupplierRefundLine>();
        return details.FirstOrDefault() ?? item;
    }

    private async void OnTreatedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!_isReadyToLoadData || _isUpdatingSortPicker)
            return;

        GlobalState.CurrentReturnFilter.WithTreated = e.Value;
        await RefreshDataAsync();
    }

    private async void OnSortModeChanged(object sender, EventArgs e)
    {
        if (!_isReadyToLoadData || _isUpdatingSortPicker)
            return;

        if (PickerSortMode.SelectedIndex < 0)
            return;

        _selectedSortMode = PickerSortMode.SelectedItem?.ToString() ?? DefaultSortLabel;
        OnPropertyChanged(nameof(ShowSortDirection));
        OnPropertyChanged(nameof(SortDirectionIcon));
        await RefreshDataAsync();
    }

    private async void OnSortDirectionToggleClicked(object sender, EventArgs e)
    {
        if (!_isReadyToLoadData || _selectedSortMode == DefaultSortLabel)
            return;

        _sortAscending = !_sortAscending;
        OnPropertyChanged(nameof(SortDirectionIcon));
        await RefreshDataAsync();
    }

    private async void OnOpenFilterClicked(object sender, EventArgs e)
        => await Navigation.PushModalAsync(new ReturnFilterView(true));

    private async void OnReturnClientCodeTapped(object sender, TappedEventArgs e)
    {
        var returnClientCode = e.Parameter?.ToString();
        if (string.IsNullOrWhiteSpace(returnClientCode))
            return;

        await DisplayAlert("Code retour client", returnClientCode, "FERMER");
    }

    private async void OnReturnClientCodeButtonClicked(object sender, EventArgs e)
    {
        var returnClientCode = (sender as Button)?.CommandParameter?.ToString();
        if (string.IsNullOrWhiteSpace(returnClientCode))
            return;

        await DisplayAlert("Code retour client", returnClientCode, "FERMER");
    }

    private async void OnShowFullSupplierCommentTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is SupplierRefundLine item)
            await ShowFullSupplierCommentAsync(item);
    }

    private async void OnDownloadSupplierFileTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is SupplierRefundLine item)
            await DownloadSupplierFileAsync(item);
    }

    private async void OnSupplierResponseTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is SupplierRefundLine item && item.CanOpenSupplierRefundDocument)
            await OpenSupplierRefundDocumentAsync(item);
    }

    private async Task ShowFullSupplierCommentAsync(SupplierRefundLine item)
    {
        SetLoadingState(true);
        try
        {
            var detail = await LoadSupplierRefundDetailAsync(item);
            var comment = detail?.SupplierComment?.Trim();
            if (string.IsNullOrWhiteSpace(comment))
                comment = item.SupplierComment?.Trim();

            if (!string.IsNullOrWhiteSpace(comment))
                await DisplayAlert("Commentaire fournisseur", comment, "FERMER");
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task DownloadSupplierFileAsync(SupplierRefundLine item)
    {
        SetLoadingState(true);
        try
        {
            var detail = await LoadSupplierRefundDetailAsync(item);
            var filePath = detail?.FilePath?.Trim();
            if (string.IsNullOrWhiteSpace(filePath))
                filePath = item.FilePath?.Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                await ModernAlertService.ShowWarningAsync("Aucun fichier fournisseur n'est disponible pour ce remboursement.");
                return;
            }

            if (!Uri.TryCreate(filePath, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                await ModernAlertService.ShowWarningAsync("Le chemin du fichier fournisseur n'est pas téléchargeable directement depuis l'application.");
                return;
            }

            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(uri);
            if (bytes == null || bytes.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("Le fichier fournisseur est vide ou indisponible.");
                return;
            }

            var fileName = ExtractFileName(filePath);
            await using var stream = new MemoryStream(bytes);
            var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

            if (result.IsSuccessful)
                await ModernAlertService.ShowSuccessAsync($"Le fichier {fileName} a bien été téléchargé.");
            else
                await ModernAlertService.ShowErrorAsync(result.Exception?.Message ?? "Le téléchargement du fichier a échoué.");
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task OpenSupplierRefundDocumentAsync(SupplierRefundLine item)
    {
        SetLoadingState(true);
        try
        {
            var pdf = await EasySession.GetSupplierRefundDocumentAsync(item.SupplierRefundLineCode, item.Warehouse);
            if (pdf == null || pdf.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("Le bon de remboursement fournisseur n'est pas disponible.");
                return;
            }

            var fileName = $"Bon_Remboursement_Fournisseur_{SafeFileSegment(item.SupplierRefundLineCode)}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            File.WriteAllBytes(filePath, pdf);

            await Navigation.PushModalAsync(new PdfViewerPage(filePath, fileName));
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private static string ExtractFileName(string filePath)
    {
        var normalized = filePath.Replace('\\', '/').Trim();
        var withoutQuery = normalized.Split('?', '#')[0];
        var fileName = Path.GetFileName(withoutQuery);

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "Justificatif_fournisseur";

        return fileName;
    }

    private static string SafeFileSegment(string value)
    {
        var text = string.IsNullOrWhiteSpace(value) ? DateTime.Now.ToString("yyyyMMddHHmmss") : value.Trim();
        return Regex.Replace(text, "[^A-Za-z0-9_-]", "_");
    }

    private void SetLoadingState(bool isLoading)
    {
        ApiLoadingOverlay.IsVisible = isLoading;
        ApiActivityIndicator.IsVisible = isLoading;
        ApiActivityIndicator.IsRunning = isLoading;
    }
}
