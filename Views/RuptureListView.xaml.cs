using AGRA_EASY_MOBILE.Services;
using AGRA_EASY_MOBILE.Models;
using Services;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace AGRA_EASY_MOBILE;

public partial class RuptureListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string ArticleSortLabel = "Article";
    private const string RuptureSortLabel = "Rupture";
    private const string ClientOrderSortLabel = "Commande";

    private bool _isSingleDetailView = false;
    private bool _isReadyToLoadData = false;
    private RuptureLine? _currentDetailHeader;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;

    public bool ShowDetailedView => (GlobalState.CurrentExpeditionFilter?.ShowDetails ?? false) || _isSingleDetailView;
    public bool ShowAccountInfo => !string.Equals(EasySession.CurrentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);
    public bool ShowSingleDetailHeader => _isSingleDetailView && _currentDetailHeader != null;
    public bool ShowRepeatedHeader => !_isSingleDetailView;
    public bool ShowDetailsToggle => !_isSingleDetailView;
    public bool ShowBackButton => _isSingleDetailView;
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    private void SetLoadingState(bool isLoading)
    {
        ApiLoadingOverlay.IsVisible = isLoading;
        ApiActivityIndicator.IsVisible = isLoading;
        ApiActivityIndicator.IsRunning = isLoading;
    }

    public RuptureLine? CurrentDetailHeader
    {
        get => _currentDetailHeader;
        private set
        {
            _currentDetailHeader = value;
            OnPropertyChanged(nameof(CurrentDetailHeader));
            OnPropertyChanged(nameof(ShowSingleDetailHeader));
            UpdateHeaderVisualState();
        }
    }

    public RuptureListView()
    {
        InitializeComponent();
        BindingContext = this;
        InitializeSortPicker();
        UpdateHeaderVisualState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isReadyToLoadData = false;
        _isSingleDetailView = false;
        CurrentDetailHeader = null;

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

        CheckGlobalDetails.IsChecked = GlobalState.CurrentExpeditionFilter.ShowDetails;
        OnPropertyChanged(nameof(ShowAccountInfo));
        OnPropertyChanged(nameof(ShowDetailsToggle));
        OnPropertyChanged(nameof(ShowBackButton));
        UpdateHeaderVisualState();
        _isReadyToLoadData = true;
        await RefreshDataAsync();
    }

    private void InitializeSortPicker()
    {
        PickerSortMode.Items.Clear();
        PickerSortMode.Items.Add(DefaultSortLabel);
        PickerSortMode.Items.Add(ArticleSortLabel);
        PickerSortMode.Items.Add(RuptureSortLabel);
        PickerSortMode.Items.Add(ClientOrderSortLabel);
        PickerSortMode.SelectedIndex = 0;
    }

    private void UpdateHeaderVisualState()
    {
        if (SingleDetailHeaderBorder == null || BodyGrid == null)
            return;

        if (ShowSingleDetailHeader)
        {
            SingleDetailHeaderBorder.IsVisible = true;
            SingleDetailHeaderBorder.Margin = new Thickness(12, 10, 12, 8);
            SingleDetailHeaderBorder.HeightRequest = -1;
            SingleDetailHeaderBorder.MinimumHeightRequest = -1;
            BodyGrid.RowDefinitions[0].Height = GridLength.Auto;
        }
        else
        {
            SingleDetailHeaderBorder.IsVisible = false;
            SingleDetailHeaderBorder.Margin = new Thickness(0);
            SingleDetailHeaderBorder.HeightRequest = 0;
            SingleDetailHeaderBorder.MinimumHeightRequest = 0;
            BodyGrid.RowDefinitions[0].Height = new GridLength(0);
        }
    }

    private async void OnGlobalDetailsChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!_isReadyToLoadData)
            return;

        if (GlobalState.CurrentExpeditionFilter != null)
        {
            GlobalState.CurrentExpeditionFilter.ShowDetails = e.Value;
            _isSingleDetailView = false;
            CurrentDetailHeader = null;
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            UpdateHeaderVisualState();
            await RefreshDataAsync();
        }
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

        if (_isSingleDetailView && CurrentDetailHeader != null)
        {
            await LoadDetailLinesAsync(CurrentDetailHeader);
            return;
        }

        await RefreshDataAsync();
    }

    private async void OnSortDirectionToggleClicked(object sender, EventArgs e)
    {
        if (!_isReadyToLoadData || _selectedSortMode == DefaultSortLabel)
            return;

        _sortAscending = !_sortAscending;
        OnPropertyChanged(nameof(SortDirectionIcon));

        if (_isSingleDetailView && CurrentDetailHeader != null)
        {
            await LoadDetailLinesAsync(CurrentDetailHeader);
            return;
        }

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
            var rawData = await EasySession.GetRupturesLinesAsync(GlobalState.CurrentExpeditionFilter);
            if (rawData != null)
            {
                var orderedData = ApplySort(rawData);
                RuptureCollection.ItemsSource = orderedData;
            }

            CurrentDetailHeader = null;
            OnPropertyChanged(nameof(ShowDetailedView));
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            UpdateHeaderVisualState();
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

    private RuptureLine[] ApplySort(RuptureLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data;

        IOrderedEnumerable<RuptureLine> orderedData = _selectedSortMode switch
        {
            ArticleSortLabel => _sortAscending
                ? data.OrderBy(x => x.ProductCode ?? string.Empty).ThenBy(x => x.RuptureStkId ?? string.Empty)
                : data.OrderByDescending(x => x.ProductCode ?? string.Empty).ThenByDescending(x => x.RuptureStkId ?? string.Empty),

            RuptureSortLabel => _sortAscending
                ? data.OrderBy(x => x.RuptureStkId ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.RuptureStkId ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            ClientOrderSortLabel => _sortAscending
                ? data.OrderBy(x => x.SorderClientCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.SorderClientCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async Task LoadDetailLinesAsync(RuptureLine item)
    {
        SetLoadingState(true);
        try
        {
            var details = await EasySession.GetRuptureLinesAsync(item.RuptureStkId, item.Warehouse);
            if (details != null)
            {
                var orderedDetails = ApplySort(details);
                _isSingleDetailView = true;
                CurrentDetailHeader = orderedDetails.FirstOrDefault() ?? item;
                RuptureCollection.ItemsSource = orderedDetails;
                OnPropertyChanged(nameof(ShowDetailedView));
                OnPropertyChanged(nameof(ShowRepeatedHeader));
                OnPropertyChanged(nameof(ShowDetailsToggle));
                OnPropertyChanged(nameof(ShowBackButton));
                UpdateHeaderVisualState();
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

    private async void OnViewDetailClicked(object sender, EventArgs e)
    {
        var item = (RuptureLine)((Button)sender).CommandParameter;
        await LoadDetailLinesAsync(item);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _isSingleDetailView = false;
        CurrentDetailHeader = null;
        OnPropertyChanged(nameof(ShowRepeatedHeader));
        OnPropertyChanged(nameof(ShowDetailsToggle));
        OnPropertyChanged(nameof(ShowBackButton));
        UpdateHeaderVisualState();
        await RefreshDataAsync();
    }

    private async void OnDownloadPdfClicked(object sender, EventArgs e)
    {
        var item = (RuptureLine)((Button)sender).CommandParameter;
        SetLoadingState(true);

        try
        {
            byte[] pdf = await EasySession.GetRuptureDocumentAsync(item.RuptureStkId, item.Warehouse);
            if (pdf == null || pdf.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("Le document PDF est vide ou indisponible.");
                return;
            }

            string safeRuptureNumber = string.Concat((item.RuptureStkId ?? "Rupture").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            string fileName = $"Rupture_{safeRuptureNumber}.pdf";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            await File.WriteAllBytesAsync(filePath, pdf);
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


    private async void OnClientOrderCodeTapped(object sender, TappedEventArgs e)
    {
        var clientOrderCode = e.Parameter?.ToString();

        if (string.IsNullOrWhiteSpace(clientOrderCode))
            return;

        await ModernAlertService.ShowInfoAsync(clientOrderCode);
    }

    private async void OnOpenFilterClicked(object sender, EventArgs e)
        => await Navigation.PushModalAsync(new ExpeditionFilterView());
}
