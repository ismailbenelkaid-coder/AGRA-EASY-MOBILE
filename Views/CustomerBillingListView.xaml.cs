using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;
using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE;

public partial class CustomerBillingListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string CreationDateSortLabel = "Date facture";
    private const string InvoiceSortLabel = "N° facture";
    private const string DeliveredClientSortLabel = "Client";
    private const string InvoicedClientSortLabel = "Client facturé";
    private const string ArticleSortLabel = "Article";
    private const string ClientOrderSortLabel = "N° commande";
    private const string DeliveryNumberSortLabel = "N° BL / retour";

    private bool _isSingleDetailView;
    private bool _isReadyToLoadData;
    private CustomerBillingLine? _currentDetailHeader;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;
    private bool _isUpdatingSortPicker;

    public bool ShowDetailedView => (GlobalState.CurrentCustomerBillingFilter?.ShowDetails ?? false) || _isSingleDetailView;
    public bool ShowSingleDetailHeader => _isSingleDetailView && _currentDetailHeader != null;
    public bool ShowRepeatedHeader => !_isSingleDetailView;
    public bool ShowDetailsToggle => !_isSingleDetailView;
    public bool ShowBackButton => _isSingleDetailView;
    public bool ShowHeaderButtons => !_isSingleDetailView && !(GlobalState.CurrentCustomerBillingFilter?.ShowDetails ?? false);
    public bool ShowHeaderTotals => !(GlobalState.CurrentCustomerBillingFilter?.ShowDetails ?? false);
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    public CustomerBillingLine? CurrentDetailHeader
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

    public CustomerBillingListView()
    {
        InitializeComponent();
        BindingContext = this;
        InitializeSortPicker();
        UpdateHeaderVisualState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!EasySession.IsCustomerBillingManager)
        {
            await Shell.Current.GoToAsync("//home");
            return;
        }
        _isReadyToLoadData = false;
        _isSingleDetailView = false;
        CurrentDetailHeader = null;

        var currentAccount = EasySession.CurrentAccount;
        var isAdministrator = string.Equals(currentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
        var isClient = string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);

        if (isClient && !string.IsNullOrWhiteSpace(currentAccount?.AccountCode))
            GlobalState.EnsureClientCustomerBillingFilter(currentAccount.AccountCode);

        if (isAdministrator && !GlobalState.HasValidAdminCustomerBillingFilter())
        {
            await Navigation.PushModalAsync(new CustomerBillingFilterView());
            return;
        }

        CheckGlobalDetails.IsChecked = GlobalState.CurrentCustomerBillingFilter.ShowDetails;
        RefreshSortPickerItems(true);
        OnPropertyChanged(nameof(ShowDetailedView));
        OnPropertyChanged(nameof(ShowRepeatedHeader));
        OnPropertyChanged(nameof(ShowDetailsToggle));
        OnPropertyChanged(nameof(ShowBackButton));
        OnPropertyChanged(nameof(ShowHeaderButtons));
        OnPropertyChanged(nameof(ShowHeaderTotals));
        UpdateHeaderVisualState();
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
            CreationDateSortLabel,
            InvoiceSortLabel,
            DeliveredClientSortLabel,
            ArticleSortLabel,
            ClientOrderSortLabel,
            DeliveryNumberSortLabel
        };

        if (EasySession.IsAdministrator)
            availableModes.Add(InvoicedClientSortLabel);

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
        if (!_isReadyToLoadData)
            return;

        if (GlobalState.CurrentCustomerBillingFilter != null)
        {
            GlobalState.CurrentCustomerBillingFilter.ShowDetails = e.Value;
            _isSingleDetailView = false;
            CurrentDetailHeader = null;
            OnPropertyChanged(nameof(ShowDetailedView));
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(ShowHeaderButtons));
            OnPropertyChanged(nameof(ShowHeaderTotals));
            UpdateHeaderVisualState();
            await RefreshDataAsync();
        }
    }

    private async Task RefreshDataAsync()
    {
        if (!_isReadyToLoadData || GlobalState.CurrentCustomerBillingFilter == null)
            return;

        SetLoadingState(true);
        try
        {
            var rawData = await EasySession.GetCustomerBillingsLinesAsync(GlobalState.CurrentCustomerBillingFilter) ?? Array.Empty<CustomerBillingLine>();
            CustomerBillingCollection.ItemsSource = ApplySort(rawData);
            CurrentDetailHeader = null;
            OnPropertyChanged(nameof(ShowDetailedView));
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(ShowHeaderButtons));
            OnPropertyChanged(nameof(ShowHeaderTotals));
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

    private CustomerBillingLine[] ApplySort(CustomerBillingLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data;

        IOrderedEnumerable<CustomerBillingLine> orderedData = _selectedSortMode switch
        {
            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.CustomerBillingId ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.CustomerBillingId ?? string.Empty),

            InvoiceSortLabel => _sortAscending
                ? data.OrderBy(x => x.CustomerBillingId ?? string.Empty).ThenBy(x => x.CreationDate)
                : data.OrderByDescending(x => x.CustomerBillingId ?? string.Empty).ThenByDescending(x => x.CreationDate),

            DeliveredClientSortLabel => _sortAscending
                ? data.OrderBy(x => x.AccountCode ?? string.Empty).ThenBy(x => x.AccountName ?? string.Empty).ThenBy(x => x.CustomerBillingId ?? string.Empty)
                : data.OrderByDescending(x => x.AccountCode ?? string.Empty).ThenByDescending(x => x.AccountName ?? string.Empty).ThenByDescending(x => x.CustomerBillingId ?? string.Empty),

            InvoicedClientSortLabel => _sortAscending
                ? data.OrderBy(x => x.InvoicedAccountCode ?? string.Empty).ThenBy(x => x.InvoicedAccountName ?? string.Empty).ThenBy(x => x.CustomerBillingId ?? string.Empty)
                : data.OrderByDescending(x => x.InvoicedAccountCode ?? string.Empty).ThenByDescending(x => x.InvoicedAccountName ?? string.Empty).ThenByDescending(x => x.CustomerBillingId ?? string.Empty),

            ArticleSortLabel => _sortAscending
                ? data.OrderBy(x => x.ProductCode ?? string.Empty).ThenBy(x => x.CustomerBillingId ?? string.Empty)
                : data.OrderByDescending(x => x.ProductCode ?? string.Empty).ThenByDescending(x => x.CustomerBillingId ?? string.Empty),

            ClientOrderSortLabel => _sortAscending
                ? data.OrderBy(x => x.SorderClientCode ?? string.Empty).ThenBy(x => x.CustomerBillingId ?? string.Empty)
                : data.OrderByDescending(x => x.SorderClientCode ?? string.Empty).ThenByDescending(x => x.CustomerBillingId ?? string.Empty),

            DeliveryNumberSortLabel => _sortAscending
                ? data.OrderBy(x => x.DeliveryNumber ?? string.Empty).ThenBy(x => x.CustomerBillingId ?? string.Empty)
                : data.OrderByDescending(x => x.DeliveryNumber ?? string.Empty).ThenByDescending(x => x.CustomerBillingId ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async Task LoadDetailLinesAsync(CustomerBillingLine item)
    {
        SetLoadingState(true);
        try
        {
            var details = await EasySession.GetCustomerBillingLinesAsync(item.CustomerBillingId, item.Warehouse) ?? Array.Empty<CustomerBillingLine>();
            _isSingleDetailView = true;
            CurrentDetailHeader = item;
            CustomerBillingCollection.ItemsSource = ApplySort(details);
            OnPropertyChanged(nameof(ShowDetailedView));
            OnPropertyChanged(nameof(ShowRepeatedHeader));
            OnPropertyChanged(nameof(ShowDetailsToggle));
            OnPropertyChanged(nameof(ShowBackButton));
            OnPropertyChanged(nameof(ShowHeaderButtons));
            OnPropertyChanged(nameof(ShowHeaderTotals));
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

    private async void OnViewDetailClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is CustomerBillingLine item)
            await LoadDetailLinesAsync(item);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _isSingleDetailView = false;
        CurrentDetailHeader = null;
        OnPropertyChanged(nameof(ShowDetailedView));
        OnPropertyChanged(nameof(ShowRepeatedHeader));
        OnPropertyChanged(nameof(ShowDetailsToggle));
        OnPropertyChanged(nameof(ShowBackButton));
        OnPropertyChanged(nameof(ShowHeaderButtons));
        OnPropertyChanged(nameof(ShowHeaderTotals));
        UpdateHeaderVisualState();
        await RefreshDataAsync();
    }

    private async void OnDownloadPdfClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not CustomerBillingLine item)
            return;

        SetLoadingState(true);
        try
        {
            var pdf = await EasySession.GetCustomerBillingDocumentAsync(item.CustomerBillingId, item.Warehouse);
            if (pdf == null || pdf.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("Le document PDF est vide ou indisponible.");
                return;
            }

            string safeBillingId = SafeFileSegment(item.CustomerBillingId ?? "Facture");
            string fileName = $"Facture_Client_{safeBillingId}.pdf";
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

    private async void OnOpenFilterClicked(object sender, EventArgs e)
        => await Navigation.PushModalAsync(new CustomerBillingFilterView());

    private void OnSorderClientCodeTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Element element)
            return;

        var popup = FindAncestorSiblingByStyleId(element, "SorderClientCodePopup");
        if (popup != null)
            popup.IsVisible = !popup.IsVisible;
    }

    private void OnProductLabelTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Element element)
            return;

        var popup = FindAncestorSiblingByStyleId(element, "ProductLabelPopup");
        if (popup != null)
            popup.IsVisible = !popup.IsVisible;
    }

    private static VisualElement? FindAncestorSiblingByStyleId(Element start, string styleId)
    {
        Element? current = start.Parent;
        while (current != null)
        {
            if (current is Layout layout)
            {
                var found = FindChildByStyleId(layout, styleId);
                if (found != null)
                    return found;
            }

            current = current.Parent;
        }

        return null;
    }

    private static VisualElement? FindChildByStyleId(Element element, string styleId)
    {
        if (element is VisualElement visual && visual.StyleId == styleId)
            return visual;

        if (element is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Element childElement)
                {
                    var found = FindChildByStyleId(childElement, styleId);
                    if (found != null)
                        return found;
                }
            }
        }

        if (element is Border border && border.Content is Element content)
            return FindChildByStyleId(content, styleId);

        if (element is ContentView contentView && contentView.Content is Element contentViewContent)
            return FindChildByStyleId(contentViewContent, styleId);

        return null;
    }

    private static string SafeFileSegment(string value)
    {
        var text = string.IsNullOrWhiteSpace(value) ? DateTime.Now.ToString("yyyyMMddHHmmss") : value.Trim();
        return string.Concat(text.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    }

    private void SetLoadingState(bool isLoading)
    {
        ApiLoadingOverlay.IsVisible = isLoading;
        ApiActivityIndicator.IsVisible = isLoading;
        ApiActivityIndicator.IsRunning = isLoading;
    }
}
