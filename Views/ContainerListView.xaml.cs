using AGRA_EASY_MOBILE.Services;
using AGRA_EASY_MOBILE.Models;
using Services;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE;

public partial class ContainerListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string ContainerSortLabel = "Conteneur";
    private const string DeliverySortLabel = "BL";
    private const string CreationDateSortLabel = "Date création";

    private bool _isReadyToLoadData = false;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;

    public bool ShowAccountInfo => !string.Equals(EasySession.CurrentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    private void SetLoadingState(bool isLoading)
    {
        ApiLoadingOverlay.IsVisible = isLoading;
        ApiActivityIndicator.IsVisible = isLoading;
        ApiActivityIndicator.IsRunning = isLoading;
    }

    public ContainerListView()
    {
        InitializeComponent();
        BindingContext = this;
        InitializeSortPicker();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isReadyToLoadData = false;

        var currentAccount = EasySession.CurrentAccount;
        var isAdministrator = string.Equals(currentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
        var isClient = string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);

        if (isClient && !string.IsNullOrWhiteSpace(currentAccount?.AccountCode))
        {
            GlobalState.EnsureClientExpeditionFilter(currentAccount.AccountCode);
        }

        if (isAdministrator && !GlobalState.HasValidAdminExpeditionFilter())
        {
            await Navigation.PushModalAsync(new ExpeditionFilterView());
            return;
        }

        OnPropertyChanged(nameof(ShowAccountInfo));
        _isReadyToLoadData = true;
        await RefreshDataAsync();
    }

    private void InitializeSortPicker()
    {
        PickerSortMode.Items.Clear();
        PickerSortMode.Items.Add(DefaultSortLabel);
        PickerSortMode.Items.Add(ContainerSortLabel);
        PickerSortMode.Items.Add(DeliverySortLabel);
        PickerSortMode.Items.Add(CreationDateSortLabel);
        PickerSortMode.SelectedIndex = 0;
    }

    private async void OnSortModeChanged(object sender, EventArgs e)
    {
        if (!_isReadyToLoadData)
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

    private async Task RefreshDataAsync()
    {
        if (!_isReadyToLoadData)
            return;

        if (GlobalState.CurrentExpeditionFilter == null)
            return;

        SetLoadingState(true);
        try
        {
            var rawData = await EasySession.GetShippingNoticeListAsync(GlobalState.CurrentExpeditionFilter);
            if (rawData != null)
            {
                var orderedData = ApplySort(rawData);
                ContainerCollection.ItemsSource = orderedData;
            }
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

    private ShippingNotice[] ApplySort(ShippingNotice[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data;

        IOrderedEnumerable<ShippingNotice> orderedData = _selectedSortMode switch
        {
            ContainerSortLabel => _sortAscending
                ? data.OrderBy(x => x.ContainerNo ?? string.Empty).ThenBy(x => x.DelivryId ?? string.Empty)
                : data.OrderByDescending(x => x.ContainerNo ?? string.Empty).ThenByDescending(x => x.DelivryId ?? string.Empty),

            DeliverySortLabel => _sortAscending
                ? data.OrderBy(x => x.DelivryId ?? string.Empty).ThenBy(x => x.ContainerNo ?? string.Empty)
                : data.OrderByDescending(x => x.DelivryId ?? string.Empty).ThenByDescending(x => x.ContainerNo ?? string.Empty),

            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.ContainerNo ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.ContainerNo ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async void OnTrackingTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not ShippingNotice item)
            return;

        if (string.IsNullOrWhiteSpace(item.TrackingLink))
            return;

        await Navigation.PushModalAsync(new TrackingWebViewPage(item.TrackingLink, item.CarrierLabel));
    }

    private async void OnOpenFilterClicked(object sender, EventArgs e)
        => await Navigation.PushModalAsync(new ExpeditionFilterView());
}
