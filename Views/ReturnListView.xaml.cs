using AGRA_EASY_MOBILE.Services;
using AGRA_EASY_MOBILE.Models;
using Services;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace AGRA_EASY_MOBILE;

public partial class ReturnListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string ArticleSortLabel = "Article";
    private const string ReturnSortLabel = "N° de retour";
    private const string ReturnClientSortLabel = "Code retour client";
    private const string CreationDateSortLabel = "Date création";
    private const string LineStatusSortLabel = "Statut ligne";
    private const string ReturnStateSortLabel = "État retour";
    private const string ArticleStateSortLabel = "État article";

    private bool _isSingleDetailView = false;
    private bool _isReadyToLoadData = false;
    private ReturnLine? _currentDetailHeader;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;
    private bool _isUpdatingSortPicker = false;

    public bool ShowDetailedView => (GlobalState.CurrentReturnFilter?.ShowDetails ?? false) || _isSingleDetailView;
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

    public ReturnLine? CurrentDetailHeader
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

    public ReturnListView()
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
            GlobalState.EnsureClientReturnFilter(currentAccount.AccountCode);
        }

        if (isAdministrator && !GlobalState.HasValidAdminReturnFilter())
        {
            await Navigation.PushModalAsync(new ReturnFilterView());
            return;
        }

        CheckGlobalDetails.IsChecked = GlobalState.CurrentReturnFilter.ShowDetails;
        OnPropertyChanged(nameof(ShowAccountInfo));
        OnPropertyChanged(nameof(ShowDetailsToggle));
        OnPropertyChanged(nameof(ShowBackButton));
        UpdateHeaderVisualState();
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
            DefaultSortLabel
        };

        if (ShowDetailedView)
            availableModes.Add(ArticleSortLabel);

        availableModes.Add(ReturnSortLabel);
        availableModes.Add(ReturnClientSortLabel);
        availableModes.Add(CreationDateSortLabel);

        if (ShowDetailedView)
            availableModes.Add(LineStatusSortLabel);

        availableModes.Add(ReturnStateSortLabel);

        if (ShowDetailedView)
            availableModes.Add(ArticleStateSortLabel);

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
        if (!_isReadyToLoadData || _isUpdatingSortPicker)
            return;

        if (GlobalState.CurrentReturnFilter != null)
        {
            GlobalState.CurrentReturnFilter.ShowDetails = e.Value;
            _isSingleDetailView = false;
            CurrentDetailHeader = null;
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            UpdateHeaderVisualState();
        RefreshSortPickerItems(true);
            await RefreshDataAsync();
        }
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

        if (GlobalState.CurrentReturnFilter == null)
            return;

        SetLoadingState(true);
        try
        {
            var rawData = await EasySession.GetReturnsLinesAsync(GlobalState.CurrentReturnFilter);
            if (rawData != null)
            {
                var orderedData = ApplySort(rawData);
                ReturnCollection.ItemsSource = orderedData;
            }

            CurrentDetailHeader = null;
            OnPropertyChanged(nameof(ShowDetailedView));
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            UpdateHeaderVisualState();
        RefreshSortPickerItems(true);
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

    private ReturnLine[] ApplySort(ReturnLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data;

        IOrderedEnumerable<ReturnLine> orderedData = _selectedSortMode switch
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


            LineStatusSortLabel => _sortAscending
                ? data.OrderBy(x => x.Treated ?? string.Empty).ThenBy(x => x.ReturnCode ?? string.Empty)
                : data.OrderByDescending(x => x.Treated ?? string.Empty).ThenByDescending(x => x.ReturnCode ?? string.Empty),

            ReturnStateSortLabel => _sortAscending
                ? data.OrderBy(x => x.Archived ?? string.Empty).ThenBy(x => x.ReturnCode ?? string.Empty)
                : data.OrderByDescending(x => x.Archived ?? string.Empty).ThenByDescending(x => x.ReturnCode ?? string.Empty),

            ArticleStateSortLabel => _sortAscending
                ? data.OrderBy(x => x.StatusLabel ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.StatusLabel ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async Task LoadDetailLinesAsync(ReturnLine item)
    {
        SetLoadingState(true);
        try
        {
            var details = await EasySession.GetReturnLinesAsync(item.ReturnCode, item.Warehouse);
            if (details != null)
            {
                var orderedDetails = ApplySort(details);
                _isSingleDetailView = true;
                CurrentDetailHeader = orderedDetails.FirstOrDefault() ?? item;
                ReturnCollection.ItemsSource = orderedDetails;
                OnPropertyChanged(nameof(ShowDetailedView));
                OnPropertyChanged(nameof(ShowRepeatedHeader));
                OnPropertyChanged(nameof(ShowDetailsToggle));
                OnPropertyChanged(nameof(ShowBackButton));
                UpdateHeaderVisualState();
                RefreshSortPickerItems(true);
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
        var item = (ReturnLine)((Button)sender).CommandParameter;
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
        RefreshSortPickerItems(true);
        await RefreshDataAsync();
    }

    private async void OnDownloadPdfClicked(object sender, EventArgs e)
    {
        var item = (ReturnLine)((Button)sender).CommandParameter;
        SetLoadingState(true);

        try
        {
            byte[] pdf = await EasySession.GetReturnDocumentAsync(item.ReturnCode, item.Warehouse);
            if (pdf == null || pdf.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("Le document PDF est vide ou indisponible.");
                return;
            }

            string safeReturnCode = string.Concat((item.ReturnCode ?? "Retour").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            string fileName = $"Retour_{safeReturnCode}.pdf";
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

    private async void OnReturnClientCodeTapped(object sender, TappedEventArgs e)
    {
        var returnClientCode = e.Parameter?.ToString();

        if (string.IsNullOrWhiteSpace(returnClientCode))
            return;

        await ModernAlertService.ShowInfoAsync(returnClientCode);
    }

    private async void OnOpenFilterClicked(object sender, EventArgs e)
        => await Navigation.PushModalAsync(new ReturnFilterView());
}
