using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;
using System.ComponentModel;

namespace AGRA_EASY_MOBILE;

public partial class ShippingCostListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string WarehouseSortLabel = "Plateforme";
    private const string ShippingCostDeliverySortLabel = "N° BL";
    private const string ShippingDateSortLabel = "Date expédition";
    private const string CarrierSortLabel = "Transporteur";
    private const string AccountSortLabel = "Client";
    private bool _isReadyToLoadData;
    private bool _isSingleDetailView;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;
    public bool ShowAccountInfo => EasySession.IsAdministrator;
    public bool ShowDetailedView => (GlobalState.CurrentShippingCostFilter?.ShowDetails ?? false) || _isSingleDetailView;
    public bool ShowLineButton => !ShowDetailedView;
    public bool ShowSortDirection => _selectedSortMode != DefaultSortLabel;
    public string SortDirectionIcon => _sortAscending ? "↑" : "↓";
    public ShippingCostListView() { InitializeComponent(); BindingContext = this; InitializeSortPicker(); }
    protected override async void OnAppearing() { base.OnAppearing(); if (!EasySession.IsCustomerBillingManager) { await Shell.Current.GoToAsync("//home"); return; } _isReadyToLoadData = false; _isSingleDetailView = false; var currentAccount = EasySession.CurrentAccount; if (EasySession.IsClient && !string.IsNullOrWhiteSpace(currentAccount?.AccountCode)) GlobalState.EnsureClientShippingCostFilter(currentAccount.AccountCode); else _ = GlobalState.CurrentShippingCostFilter; if (EasySession.IsAdministrator && !GlobalState.HasValidAdminShippingCostFilter()) { var openedWithoutFilter = GlobalState.PeekShippingCostFilter() == null; await Navigation.PushModalAsync(new ShippingCostFilterView(openedWithoutFilter)); return; } ApplyFilterToHeaderControls(); OnPropertyChanged(nameof(ShowAccountInfo)); OnPropertyChanged(nameof(ShowDetailedView)); OnPropertyChanged(nameof(ShowLineButton)); _isReadyToLoadData = true; await RefreshDataAsync(); }
    private void InitializeSortPicker() { PickerSortMode.Items.Clear(); PickerSortMode.Items.Add(DefaultSortLabel); PickerSortMode.Items.Add(WarehouseSortLabel); PickerSortMode.Items.Add(ShippingCostDeliverySortLabel); PickerSortMode.Items.Add(ShippingDateSortLabel); PickerSortMode.Items.Add(CarrierSortLabel); PickerSortMode.Items.Add(AccountSortLabel); PickerSortMode.SelectedIndex = 0; }
    private void ApplyFilterToHeaderControls() { var f = GlobalState.CurrentShippingCostFilter; CheckGlobalDetails.IsChecked = f.ShowDetails; CheckWaitingLines.IsChecked = f.WithWaitingLines; CheckBilledLines.IsChecked = f.WithBilledLines; CheckExemptLines.IsChecked = f.WithExemptLines; }
    private void SetLoadingState(bool isLoading) { ApiLoadingOverlay.IsVisible = isLoading; ApiActivityIndicator.IsVisible = isLoading; ApiActivityIndicator.IsRunning = isLoading; }
    private async Task RefreshDataAsync() { if (!_isReadyToLoadData || GlobalState.CurrentShippingCostFilter == null) return; SetLoadingState(true); try { var raw = await EasySession.GetShippingCostLinesAsync(GlobalState.CurrentShippingCostFilter) ?? Array.Empty<ShippingCostLine>(); ShippingCostCollection.ItemsSource = ApplySort(raw); } catch (Exception ex) { await ModernAlertService.ShowErrorAsync(ex.Message); } finally { SetLoadingState(false); } }
    private ShippingCostLine[] ApplySort(ShippingCostLine[] data) { if (_selectedSortMode == DefaultSortLabel) return data.OrderByDescending(x => x.ShippingDate).ThenByDescending(x => x.ShippingCostDeliveryNumber ?? string.Empty).ThenByDescending(x => x.ShippingCostCode ?? string.Empty).ToArray(); IOrderedEnumerable<ShippingCostLine> ordered = _selectedSortMode switch { WarehouseSortLabel => _sortAscending ? data.OrderBy(x => x.Warehouse ?? string.Empty).ThenBy(x => x.ShippingDate) : data.OrderByDescending(x => x.Warehouse ?? string.Empty).ThenByDescending(x => x.ShippingDate), ShippingCostDeliverySortLabel => _sortAscending ? data.OrderBy(x => x.ShippingCostDeliveryNumber ?? string.Empty).ThenBy(x => x.ShippingDate) : data.OrderByDescending(x => x.ShippingCostDeliveryNumber ?? string.Empty).ThenByDescending(x => x.ShippingDate), ShippingDateSortLabel => _sortAscending ? data.OrderBy(x => x.ShippingDate).ThenBy(x => x.ShippingCostDeliveryNumber ?? string.Empty) : data.OrderByDescending(x => x.ShippingDate).ThenByDescending(x => x.ShippingCostDeliveryNumber ?? string.Empty), CarrierSortLabel => _sortAscending ? data.OrderBy(x => x.CarrierName ?? string.Empty).ThenBy(x => x.ShippingDate) : data.OrderByDescending(x => x.CarrierName ?? string.Empty).ThenByDescending(x => x.ShippingDate), AccountSortLabel => _sortAscending ? data.OrderBy(x => x.AccountCode ?? string.Empty).ThenBy(x => x.ShippingDate) : data.OrderByDescending(x => x.AccountCode ?? string.Empty).ThenByDescending(x => x.ShippingDate), _ => data.OrderBy(x => 0) }; return ordered.ToArray(); }
    private async void OnGlobalDetailsChanged(object sender, CheckedChangedEventArgs e) { if (!_isReadyToLoadData || GlobalState.CurrentShippingCostFilter == null) return; _isSingleDetailView = false; GlobalState.CurrentShippingCostFilter.ShowDetails = e.Value; OnPropertyChanged(nameof(ShowDetailedView)); OnPropertyChanged(nameof(ShowLineButton)); await RefreshDataAsync(); }
    private async void OnStatusFilterChanged(object sender, CheckedChangedEventArgs e) { if (!_isReadyToLoadData || GlobalState.CurrentShippingCostFilter == null) return; var f = GlobalState.CurrentShippingCostFilter; f.WithWaitingLines = CheckWaitingLines.IsChecked; f.WithBilledLines = CheckBilledLines.IsChecked; f.WithExemptLines = CheckExemptLines.IsChecked; await RefreshDataAsync(); }
    private async void OnSortModeChanged(object sender, EventArgs e) { if (!_isReadyToLoadData || PickerSortMode.SelectedIndex < 0) return; _selectedSortMode = PickerSortMode.SelectedItem?.ToString() ?? DefaultSortLabel; OnPropertyChanged(nameof(ShowSortDirection)); OnPropertyChanged(nameof(SortDirectionIcon)); await RefreshDataAsync(); }
    private async void OnSortDirectionToggleClicked(object sender, EventArgs e) { if (!_isReadyToLoadData || _selectedSortMode == DefaultSortLabel) return; _sortAscending = !_sortAscending; OnPropertyChanged(nameof(SortDirectionIcon)); await RefreshDataAsync(); }
    private async void OnOpenFilterClicked(object sender, EventArgs e) => await Navigation.PushModalAsync(new ShippingCostFilterView(false));
    private async void OnShowLinesClicked(object sender, EventArgs e) { if (GlobalState.CurrentShippingCostFilter == null) return; _isSingleDetailView = true; GlobalState.CurrentShippingCostFilter.ShowDetails = true; CheckGlobalDetails.IsChecked = true; OnPropertyChanged(nameof(ShowDetailedView)); OnPropertyChanged(nameof(ShowLineButton)); await RefreshDataAsync(); }
    private async void OnShowFullDescriptionTapped(object sender, TappedEventArgs e) { if (e.Parameter is ShippingCostLine item && !string.IsNullOrWhiteSpace(item.Description)) await DisplayAlert("Description", item.Description.Trim(), "FERMER"); }
    private void OnAddressTapped(object sender, TappedEventArgs e) { if (sender is not Element element) return; var popup = FindAncestorSiblingByStyleId(element, "AddressPopup"); if (popup != null) popup.IsVisible = !popup.IsVisible; }
    private static VisualElement? FindAncestorSiblingByStyleId(Element start, string styleId) { Element? current = start.Parent; while (current != null) { if (current is Layout layout) { var found = FindChildByStyleId(layout, styleId); if (found != null) return found; } current = current.Parent; } return null; }
    private static VisualElement? FindChildByStyleId(Element element, string styleId) { if (element is VisualElement visual && visual.StyleId == styleId) return visual; if (element is Layout layout) { foreach (var child in layout.Children) if (child is Element childElement) { var found = FindChildByStyleId(childElement, styleId); if (found != null) return found; } } if (element is Border border && border.Content is Element content) return FindChildByStyleId(content, styleId); if (element is ContentView cv && cv.Content is Element ce) return FindChildByStyleId(ce, styleId); return null; }
}
