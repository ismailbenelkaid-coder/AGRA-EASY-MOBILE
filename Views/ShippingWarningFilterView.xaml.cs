using System.Globalization;
using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;

namespace AGRA_EASY_MOBILE;

public partial class ShippingWarningFilterView : ContentPage
{
    private readonly bool _openedWithoutExistingFilter;
    private bool _isSynchronizingDatePicker;
    private bool _warehousesLoaded;

    public ShippingWarningFilterView(bool openedWithoutExistingFilter = false)
    {
        InitializeComponent();
        _openedWithoutExistingFilter = openedWithoutExistingFilter;
        StartDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        LoadCurrentFilter();
        SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        SynchronizeDatePicker(EndDateEntry, EndDatePicker);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWarehousesAsync();
        SelectCurrentWarehouse();
    }

    private void LoadCurrentFilter()
    {
        var f = GlobalState.CurrentShippingWarningFilter;
        StartDateEntry.Text = FormatFilterDate(f.FirstDate);
        EndDateEntry.Text = FormatFilterDate(f.LastDate);
        EntryContainerNo.Text = f.ContainerNo;
        EntryAccountCode.Text = EasySession.IsClient ? EasySession.CurrentAccount?.AccountCode : f.AccountCode;

        if (EasySession.IsClient)
        {
            EntryAccountCode.IsEnabled = false;
            EntryAccountCode.BackgroundColor = Color.FromArgb("#EFEFEF");
            EntryAccountCode.TextColor = Colors.Gray;
            SelectClientButton.IsVisible = false;
        }
    }

    private async Task LoadWarehousesAsync()
    {
        if (_warehousesLoaded)
            return;

        _warehousesLoaded = true;
        try
        {
            PickerWarehouse.Items.Clear();
            PickerWarehouse.Items.Add(string.Empty);
            var warehouses = await EasySession.GetWarehousesListV2Async() ?? Array.Empty<Warehouse>();
            foreach (var code in warehouses
                         .Where(x => !x.IsExternal)
                         .Select(x => x.WarehouseCode?.Trim())
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(x => x))
                PickerWarehouse.Items.Add(code!);
        }
        catch
        {
            PickerWarehouse.Items.Clear();
            PickerWarehouse.Items.Add(string.Empty);
        }
    }

    private void SelectCurrentWarehouse()
    {
        var current = GlobalState.CurrentShippingWarningFilter.Warehouse?.Trim() ?? string.Empty;
        for (var i = 0; i < PickerWarehouse.Items.Count; i++)
        {
            if (string.Equals(PickerWarehouse.Items[i], current, StringComparison.OrdinalIgnoreCase))
            {
                PickerWarehouse.SelectedIndex = i;
                return;
            }
        }

        PickerWarehouse.SelectedIndex = 0;
    }

    private async void OnApplyFilterClicked(object sender, EventArgs e)
    {
        var dateRange = await ReadDateRangeAsync();
        if (!dateRange.Success)
            return;

        var f = GlobalState.CurrentShippingWarningFilter;
        f.FirstDate = dateRange.FirstDate;
        f.LastDate = dateRange.LastDate;
        f.ContainerNo = EntryContainerNo.Text ?? string.Empty;
        f.Warehouse = PickerWarehouse.SelectedIndex > 0 ? PickerWarehouse.SelectedItem?.ToString() ?? string.Empty : string.Empty;
        f.AccountCode = EasySession.IsClient ? EasySession.CurrentAccount?.AccountCode ?? string.Empty : EntryAccountCode.Text ?? string.Empty;
        GlobalState.CurrentShippingWarningFilter = f;
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_openedWithoutExistingFilter)
        {
            await Navigation.PopModalAsync();
            await Shell.Current.GoToAsync("//home");
            return;
        }

        await Navigation.PopModalAsync();
    }

    private async void OnSelectClientClicked(object sender, EventArgs e)
    {
        if (!EntryAccountCode.IsEnabled) return;
        var selected = await ClientAccountSelectionPage.ShowAsync(this, "Choisir un client", (EntryAccountCode.Text ?? string.Empty).Trim());
        var accountCode = selected?.AccountCode?.Trim();
        if (!string.IsNullOrWhiteSpace(accountCode))
            EntryAccountCode.Text = accountCode;
    }

    private async Task<(bool Success, DateTime? FirstDate, DateTime? LastDate)> ReadDateRangeAsync()
    {
        var firstOk = TryReadDate(StartDateEntry.Text, out var first);
        var lastOk = TryReadDate(EndDateEntry.Text, out var last);
        if (!firstOk || !lastOk)
        {
            await ModernAlertService.ShowWarningAsync("Merci de saisir des dates valides au format jj/mm/aaaa, ou de laisser les champs vides.");
            return (false, null, null);
        }

        if (first.HasValue && last.HasValue && first.Value.Date > last.Value.Date)
        {
            await ModernAlertService.ShowWarningAsync("La date de début doit être inférieure ou égale à la date de fin.");
            return (false, null, null);
        }

        return (true, first, last);
    }

    private static bool TryReadDate(string? text, out DateTime? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (DateTime.TryParseExact(text.Trim(), "dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"), DateTimeStyles.None, out var parsed))
        {
            value = parsed.Date;
            return true;
        }

        return false;
    }

    private void SynchronizeDatePicker(Entry entry, DatePicker picker)
    {
        if (_isSynchronizingDatePicker)
            return;

        if (TryReadDate(entry.Text, out var value) && value.HasValue)
        {
            _isSynchronizingDatePicker = true;
            picker.Date = value.Value;
            _isSynchronizingDatePicker = false;
        }
    }

    private void OnStartDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_isSynchronizingDatePicker) return;
        StartDateEntry.Text = FormatFilterDate(e.NewDate);
    }

    private void OnEndDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_isSynchronizingDatePicker) return;
        EndDateEntry.Text = FormatFilterDate(e.NewDate);
    }

    private static string FormatFilterDate(DateTime? date) => date.HasValue ? date.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR")) : string.Empty;
}
