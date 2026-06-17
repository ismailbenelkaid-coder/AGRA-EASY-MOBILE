using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE;

public partial class RefundInvoiceListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string CreationDateSortLabel = "Date création";
    private const string DeliveryDateSortLabel = "Date livraison";
    private const string RefundWaitingSortLabel = "N° attente";
    private const string DeliveredClientSortLabel = "Client";
    private const string ArticleSortLabel = "Article";
    private const string ReturnClientSortLabel = "N° commande";
    private const string DeliveryNumberSortLabel = "N° BL";

    private bool _isReadyToLoadData;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;

    public bool ShowAccountInfo => EasySession.IsAdministrator;
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    public RefundInvoiceListView()
    {
        InitializeComponent();
        BindingContext = this;
        InitializeSortPicker();
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
        GlobalState.EnsureReturnFilterWithSlidingWeek();

        var currentAccount = EasySession.CurrentAccount;
        var isAdministrator = EasySession.IsAdministrator;
        var isClient = EasySession.IsClient;

        if (isClient && !string.IsNullOrWhiteSpace(currentAccount?.AccountCode))
            GlobalState.EnsureClientReturnFilter(currentAccount.AccountCode);

        if (isAdministrator && !GlobalState.HasValidAdminReturnFilter())
        {
            await Navigation.PushModalAsync(new ReturnFilterView(true));
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
        PickerSortMode.Items.Add(CreationDateSortLabel);
        PickerSortMode.Items.Add(DeliveryDateSortLabel);
        PickerSortMode.Items.Add(RefundWaitingSortLabel);
        PickerSortMode.Items.Add(DeliveredClientSortLabel);
        PickerSortMode.Items.Add(ArticleSortLabel);
        PickerSortMode.Items.Add(ReturnClientSortLabel);
        PickerSortMode.Items.Add(DeliveryNumberSortLabel);
        PickerSortMode.SelectedIndex = 0;
    }

    private void SetLoadingState(bool isLoading)
    {
        ApiLoadingOverlay.IsVisible = isLoading;
        ApiActivityIndicator.IsVisible = isLoading;
        ApiActivityIndicator.IsRunning = isLoading;
    }

    private async Task RefreshDataAsync()
    {
        if (!_isReadyToLoadData || GlobalState.CurrentReturnFilter == null)
            return;

        SetLoadingState(true);
        try
        {
            var rawData = await EasySession.GetRefundWaitingLinesAsync(GlobalState.CurrentReturnFilter) ?? Array.Empty<RefundWatingLine>();
            RefundWaitingCollection.ItemsSource = ApplySort(rawData);
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

    private RefundWatingLine[] ApplySort(RefundWatingLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data
                .OrderByDescending(x => x.CreationDate)
                .ThenByDescending(x => x.RefundWaitingCode ?? string.Empty)
                .ToArray();

        IOrderedEnumerable<RefundWatingLine> orderedData = _selectedSortMode switch
        {
            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.RefundWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.RefundWaitingCode ?? string.Empty),

            DeliveryDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.DeliveryDateForSort).ThenBy(x => x.RefundWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.DeliveryDateForSort).ThenByDescending(x => x.RefundWaitingCode ?? string.Empty),

            RefundWaitingSortLabel => _sortAscending
                ? data.OrderBy(x => x.RefundWaitingCode ?? string.Empty).ThenBy(x => x.CreationDate)
                : data.OrderByDescending(x => x.RefundWaitingCode ?? string.Empty).ThenByDescending(x => x.CreationDate),

            DeliveredClientSortLabel => _sortAscending
                ? data.OrderBy(x => x.AccountCode ?? string.Empty).ThenBy(x => x.RefundWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.AccountCode ?? string.Empty).ThenByDescending(x => x.RefundWaitingCode ?? string.Empty),

            ArticleSortLabel => _sortAscending
                ? data.OrderBy(x => x.ProductCode ?? string.Empty).ThenBy(x => x.RefundWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.ProductCode ?? string.Empty).ThenByDescending(x => x.RefundWaitingCode ?? string.Empty),

            ReturnClientSortLabel => _sortAscending
                ? data.OrderBy(x => x.ReturnClientCode ?? string.Empty).ThenBy(x => x.RefundWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.ReturnClientCode ?? string.Empty).ThenByDescending(x => x.RefundWaitingCode ?? string.Empty),

            DeliveryNumberSortLabel => _sortAscending
                ? data.OrderBy(x => x.DeliveryNumber ?? string.Empty).ThenBy(x => x.RefundWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.DeliveryNumber ?? string.Empty).ThenByDescending(x => x.RefundWaitingCode ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
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

    private async void OnOpenFilterClicked(object sender, EventArgs e)
        => await Navigation.PushModalAsync(new ReturnFilterView(true));

    private void OnReturnClientCodeTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Element element)
            return;

        var popup = FindAncestorSiblingByStyleId(element, "ReturnClientCodePopup");
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
}
