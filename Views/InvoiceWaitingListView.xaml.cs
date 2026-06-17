using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE;

public partial class InvoiceWaitingListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string CreationDateSortLabel = "Date création";
    private const string DeliveryDateSortLabel = "Date livraison";
    private const string InvoiceWaitingSortLabel = "N° attente";
    private const string DeliveredClientSortLabel = "Client";
    private const string ArticleSortLabel = "Article";
    private const string ClientOrderSortLabel = "N° commande";
    private const string DeliveryNumberSortLabel = "N° BL";

    private bool _isReadyToLoadData;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;

    public bool ShowAccountInfo => EasySession.IsAdministrator;
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";

    public InvoiceWaitingListView()
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

        var currentAccount = EasySession.CurrentAccount;
        var isAdministrator = EasySession.IsAdministrator;
        var isClient = EasySession.IsClient;

        if (isClient && !string.IsNullOrWhiteSpace(currentAccount?.AccountCode))
            GlobalState.EnsureClientExpeditionFilter(currentAccount.AccountCode);

        if (isAdministrator && !GlobalState.HasValidAdminInvoiceWaitingFilter())
        {
            await Navigation.PushModalAsync(new ExpeditionFilterView(ExpeditionFilterMode.InvoiceWaiting));
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
        PickerSortMode.Items.Add(InvoiceWaitingSortLabel);
        PickerSortMode.Items.Add(DeliveredClientSortLabel);
        PickerSortMode.Items.Add(ArticleSortLabel);
        PickerSortMode.Items.Add(ClientOrderSortLabel);
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
        if (!_isReadyToLoadData || GlobalState.CurrentExpeditionFilter == null)
            return;

        SetLoadingState(true);
        try
        {
            var rawData = await EasySession.GetInvoiceWaitingLinesAsync(GlobalState.CurrentExpeditionFilter) ?? Array.Empty<InvoiceWaitingLine>();
            InvoiceWaitingCollection.ItemsSource = ApplySort(rawData);
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

    private InvoiceWaitingLine[] ApplySort(InvoiceWaitingLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data
                .OrderByDescending(x => x.CreationDate)
                .ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty)
                .ToArray();

        IOrderedEnumerable<InvoiceWaitingLine> orderedData = _selectedSortMode switch
        {
            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.InvoiceWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty),

            DeliveryDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.DeliveryDateForSort).ThenBy(x => x.InvoiceWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.DeliveryDateForSort).ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty),

            InvoiceWaitingSortLabel => _sortAscending
                ? data.OrderBy(x => x.InvoiceWaitingCode ?? string.Empty).ThenBy(x => x.CreationDate)
                : data.OrderByDescending(x => x.InvoiceWaitingCode ?? string.Empty).ThenByDescending(x => x.CreationDate),

            DeliveredClientSortLabel => _sortAscending
                ? data.OrderBy(x => x.AccountCode ?? string.Empty).ThenBy(x => x.InvoiceWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.AccountCode ?? string.Empty).ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty),

            ArticleSortLabel => _sortAscending
                ? data.OrderBy(x => x.ProductCode ?? string.Empty).ThenBy(x => x.InvoiceWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.ProductCode ?? string.Empty).ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty),

            ClientOrderSortLabel => _sortAscending
                ? data.OrderBy(x => x.SorderClientCode ?? string.Empty).ThenBy(x => x.InvoiceWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.SorderClientCode ?? string.Empty).ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty),

            DeliveryNumberSortLabel => _sortAscending
                ? data.OrderBy(x => x.DeliveryNumber ?? string.Empty).ThenBy(x => x.InvoiceWaitingCode ?? string.Empty)
                : data.OrderByDescending(x => x.DeliveryNumber ?? string.Empty).ThenByDescending(x => x.InvoiceWaitingCode ?? string.Empty),

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
        => await Navigation.PushModalAsync(new ExpeditionFilterView(ExpeditionFilterMode.InvoiceWaiting));

    private void OnSorderClientCodeTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Element element)
            return;

        var popup = FindAncestorSiblingByStyleId(element, "SorderClientCodePopup");
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
