using System.Globalization;
using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;

namespace AGRA_EASY_MOBILE;

public partial class ShippingCostFilterView : ContentPage
{
    private bool _isSynchronizingDatePicker;
    private bool _warehousesLoaded;
    private readonly bool _openedWithoutExistingFilter;
    public ShippingCostFilterView(bool openedWithoutExistingFilter = false)
    {
        _openedWithoutExistingFilter = openedWithoutExistingFilter;
        InitializeComponent();
        StartDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        StartDatePicker.Focused += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDatePicker.Focused += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        LoadCurrentFilter(); SynchronizeDatePicker(StartDateEntry, StartDatePicker); SynchronizeDatePicker(EndDateEntry, EndDatePicker);
    }
    protected override async void OnAppearing() { base.OnAppearing(); if (!EasySession.IsCustomerBillingManager) { await Navigation.PopModalAsync(); await Shell.Current.GoToAsync("//home"); return; } await LoadWarehousesAsync(); SelectCurrentWarehouse(); }
    private void LoadCurrentFilter() { var f = GlobalState.CurrentShippingCostFilter; var currentAccount = EasySession.CurrentAccount; if (f != null) { StartDateEntry.Text = FormatFilterDate(f.FirstDate); EndDateEntry.Text = FormatFilterDate(f.LastDate); EntryAccountCode.Text = f.AccountCode; EntryShippingCostDeliveryNumber.Text = f.ShippingCostDeliveryNumber; EntryTurnoverDeliveryNumber.Text = f.TurnoverDeliveryNumber; } if (EasySession.IsClient) { EntryAccountCode.Text = currentAccount?.AccountCode; EntryAccountCode.IsEnabled = false; EntryAccountCode.BackgroundColor = Color.FromArgb("#EFEFEF"); EntryAccountCode.TextColor = Colors.Gray; SelectClientButton.IsVisible = false; } }
    private async Task LoadWarehousesAsync() { if (_warehousesLoaded) return; try { PickerWarehouse.Items.Clear(); PickerWarehouse.Items.Add(string.Empty); var warehouses = await EasySession.GetWarehousesListV2Async() ?? Array.Empty<Warehouse>(); foreach (var code in warehouses.Where(x => !x.IsExternal).Select(x => x.WarehouseCode?.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x)) PickerWarehouse.Items.Add(code); _warehousesLoaded = true; } catch (Exception ex) { await ModernAlertService.ShowWarningAsync(ex.Message); } }
    private void SelectCurrentWarehouse() { var current = GlobalState.CurrentShippingCostFilter?.Warehouse?.Trim() ?? string.Empty; for (int i = 0; i < PickerWarehouse.Items.Count; i++) if (string.Equals(PickerWarehouse.Items[i], current, StringComparison.OrdinalIgnoreCase)) { PickerWarehouse.SelectedIndex = i; return; } PickerWarehouse.SelectedIndex = 0; }
    private async void OnApplyFilterClicked(object sender, EventArgs e) { var accountCode = EasySession.IsClient ? EasySession.CurrentAccount?.AccountCode : EntryAccountCode.Text; if (string.IsNullOrWhiteSpace(accountCode) && string.IsNullOrWhiteSpace(EntryShippingCostDeliveryNumber.Text) && string.IsNullOrWhiteSpace(EntryTurnoverDeliveryNumber.Text)) { await ModernAlertService.ShowWarningAsync("Pour lancer la recherche, renseignez au moins un code client, un BL de frais de port ou un BL marchandise."); return; } var dateRange = await ReadDateRangeAsync(); if (!dateRange.Success) return; var f = GlobalState.CurrentShippingCostFilter ?? new ShippingCostFilter(); f.FirstDate = dateRange.FirstDate; f.LastDate = dateRange.LastDate; f.Warehouse = PickerWarehouse.SelectedIndex > 0 ? PickerWarehouse.SelectedItem?.ToString() : string.Empty; f.AccountCode = accountCode; f.ShippingCostDeliveryNumber = EntryShippingCostDeliveryNumber.Text; f.TurnoverDeliveryNumber = EntryTurnoverDeliveryNumber.Text; GlobalState.CurrentShippingCostFilter = f; await Navigation.PopModalAsync(); }
    private static string FormatFilterDate(DateTime? value) => value.HasValue ? value.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR")) : string.Empty;
    private async Task<(bool Success, DateTime? FirstDate, DateTime? LastDate)> ReadDateRangeAsync() { string st = (StartDateEntry.Text ?? string.Empty).Trim(); string et = (EndDateEntry.Text ?? string.Empty).Trim(); if (string.IsNullOrWhiteSpace(st) && string.IsNullOrWhiteSpace(et)) return (true, null, null); if (string.IsNullOrWhiteSpace(st) || string.IsNullOrWhiteSpace(et)) { await ModernAlertService.ShowWarningAsync("Les deux dates doivent être renseignées, ou les deux dates doivent être laissées vides."); return (false, null, null); } if (!TryParseFilterDate(st, out DateTime fd)) { await ModernAlertService.ShowWarningAsync("La date de début doit être saisie au format jj/mm/aaaa."); return (false, null, null); } if (!TryParseFilterDate(et, out DateTime ld)) { await ModernAlertService.ShowWarningAsync("La date de fin doit être saisie au format jj/mm/aaaa."); return (false, null, null); } if (ld < fd) { await ModernAlertService.ShowWarningAsync("La date de fin ne peut pas être antérieure à la date de début."); return (false, null, null); } return (true, fd, ld); }
    private static bool TryParseFilterDate(string value, out DateTime date) { string[] formats = { "dd/MM/yyyy", "d/M/yyyy" }; return DateTime.TryParseExact(value, formats, CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, out date); }
    private void SynchronizeDatePicker(Entry sourceEntry, DatePicker picker) { string text = (sourceEntry.Text ?? string.Empty).Trim(); if (!TryParseFilterDate(text, out DateTime parsedDate)) return; if (picker.Date == parsedDate.Date) return; _isSynchronizingDatePicker = true; try { picker.Date = parsedDate.Date; } finally { _isSynchronizingDatePicker = false; } }
    private void OnStartDateSelected(object sender, DateChangedEventArgs e) { if (_isSynchronizingDatePicker) return; StartDateEntry.Text = FormatFilterDate(e.NewDate); }
    private void OnEndDateSelected(object sender, DateChangedEventArgs e) { if (_isSynchronizingDatePicker) return; EndDateEntry.Text = FormatFilterDate(e.NewDate); }
    private async void OnSelectClientClicked(object sender, EventArgs e) { if (!EntryAccountCode.IsEnabled) return; var selected = await ClientAccountSelectionPage.ShowAsync(this, "Choisir un client", (EntryAccountCode.Text ?? string.Empty).Trim()); var accountCode = selected?.AccountCode?.Trim(); if (!string.IsNullOrWhiteSpace(accountCode)) EntryAccountCode.Text = accountCode; }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();

        if (_openedWithoutExistingFilter)
            await Shell.Current.GoToAsync("//home");
    }
}
