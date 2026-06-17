using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Services;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE;

public partial class RefusedReturnListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string ArticleSortLabel = "Article";
    private const string ReturnSortLabel = "Code retour";
    private const string ReturnClientSortLabel = "Code retour client";
    private const string CreationDateSortLabel = "Date création";

    private bool _isReadyToLoadData;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;
    private bool _isUpdatingSortPicker;

    public bool ShowAccountInfo => !string.Equals(EasySession.CurrentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);
    public bool CanUploadRefusedReturnPicture => EasySession.IsAdministrator;
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    public RefusedReturnListView()
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

        OnPropertyChanged(nameof(ShowAccountInfo));
        OnPropertyChanged(nameof(CanUploadRefusedReturnPicture));
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
            var rawData = await EasySession.GetRefusedReturnsLinesAsync(GlobalState.CurrentReturnFilter) ?? Array.Empty<RefusedReturnLine>();
            RefusedReturnCollection.ItemsSource = ApplySort(rawData);
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

    private RefusedReturnLine[] ApplySort(RefusedReturnLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data
                .OrderByDescending(x => x.CreationDate)
                .ThenByDescending(x => x.RefusedReturnLineCode ?? string.Empty)
                .ToArray();

        IOrderedEnumerable<RefusedReturnLine> orderedData = _selectedSortMode switch
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

            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.ReturnCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.ReturnCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async Task<RefusedReturnLine?> LoadRefusedReturnDetailAsync(RefusedReturnLine item)
    {
        if (item == null)
            return null;

        var details = await EasySession.GetRefusedReturnLineAsync(item.RefusedReturnLineCode, item.Warehouse) ?? Array.Empty<RefusedReturnLine>();
        return details.FirstOrDefault() ?? item;
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

    private async void OnShowFullReasonTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is RefusedReturnLine item)
            await ShowFullReasonAsync(item);
    }

    private async void OnShowImageTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is RefusedReturnLine item)
            await ShowImageAsync(item);
    }

    private async Task ShowFullReasonAsync(RefusedReturnLine item)
    {
        SetLoadingState(true);
        try
        {
            var detail = await LoadRefusedReturnDetailAsync(item);
            var reason = detail?.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                reason = item.Reason?.Trim();

            if (!string.IsNullOrWhiteSpace(reason))
                await DisplayAlert("Motif du refus", reason, "FERMER");
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

    private async Task ShowImageAsync(RefusedReturnLine item)
    {
        SetLoadingState(true);
        try
        {
            var detail = await LoadRefusedReturnDetailAsync(item);
            var picturePath = detail?.PicturePath?.Trim();
            if (string.IsNullOrWhiteSpace(picturePath))
                picturePath = item.PicturePath?.Trim();

            if (string.IsNullOrWhiteSpace(picturePath))
            {
                await ModernAlertService.ShowWarningAsync("Aucune image n'est disponible pour ce retour refusé.");
                return;
            }

            await Navigation.PushModalAsync(new RefusedReturnImageView(picturePath));
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

    private async void OnUploadPictureTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not RefusedReturnLine item)
            return;

        if (!CanUploadPictureForCurrentWarehouse(item))
            return;

        var receptionTracingId = item.RefusedReturnLineCode?.Trim();
        if (string.IsNullOrWhiteSpace(receptionTracingId))
        {
            await ModernAlertService.ShowWarningAsync("Impossible d'identifier le retour refusé concerné.");
            return;
        }

        try
        {
            var jpegBytes = await RefusedReturnPictureService.CaptureOrPickJpegBytesAsync($"retour_refuse_{receptionTracingId}.jpg");
            if (jpegBytes == null)
                return;

            if (jpegBytes.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("La photo sélectionnée est vide.");
                return;
            }

            SetLoadingState(true);
            await EasySession.UploadRefusedReturnPictureAsync(jpegBytes, receptionTracingId);
            await ModernAlertService.ShowInfoAsync("Photo envoyée au serveur.");
            await RefreshDataAsync();
        }
        catch (PermissionException)
        {
            await ModernAlertService.ShowWarningAsync("L'autorisation caméra n'a pas été accordée.");
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

    private static bool CanUploadPictureForCurrentWarehouse(RefusedReturnLine item)
    {
        if (!EasySession.IsAdministrator)
            return false;

        var lineWarehouse = (item.Warehouse ?? string.Empty).Trim();
        var currentWarehouse = (EasySession.CurrentAccount?.Warehouse ?? string.Empty).Trim();

        return !string.IsNullOrWhiteSpace(lineWarehouse)
            && string.Equals(lineWarehouse, currentWarehouse, StringComparison.OrdinalIgnoreCase);
    }

    private void SetLoadingState(bool isLoading)
    {
        ApiLoadingOverlay.IsVisible = isLoading;
        ApiActivityIndicator.IsVisible = isLoading;
        ApiActivityIndicator.IsRunning = isLoading;
    }
}
