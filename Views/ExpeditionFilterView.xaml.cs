using System.Globalization;
using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AGRA_EASY_MOBILE;

public partial class ExpeditionFilterView : ContentPage
{
    private readonly ExpeditionFilterMode _mode;
    private bool _isResolvingProductCode;
    private bool _isSynchronizingDatePicker;
    private string _lastResolvedProductText = string.Empty;
    private string _lastUserProductFilter = string.Empty;
    private readonly KeyboardScanInputTracker _productScanInputTracker = new();

    public ExpeditionFilterView()
        : this(ExpeditionFilterMode.Expedition)
    {
    }

    public ExpeditionFilterView(ExpeditionFilterMode mode)
    {
        _mode = mode;
        InitializeComponent();
        ConfigureFilterMode();
        EntryProduct.TextChanged += OnProductTextChanged;
        EntryProduct.Completed += async (_, __) => await OnProductEntryCompletedAsync();
        EntryProduct.Unfocused += async (_, __) => await ResolveProductCodeAsync(true);
        StartDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        StartDatePicker.Focused += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDatePicker.Focused += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        LoadCurrentFilter();
        SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        SynchronizeDatePicker(EndDateEntry, EndDatePicker);
    }

    private void ConfigureFilterMode()
    {
        bool isInvoiceWaitingMode = _mode == ExpeditionFilterMode.InvoiceWaiting;

        PickerSorderType.IsVisible = !isInvoiceWaitingMode;
        PickerSorderType.IsEnabled = !isInvoiceWaitingMode;
        PickerDeliveryType.IsVisible = isInvoiceWaitingMode;
        PickerDeliveryType.IsEnabled = isInvoiceWaitingMode;

        if (isInvoiceWaitingMode)
        {
            FilterTitleLabel.Text = "Filtrer la facturation en attente";
            FilterSubtitleLabel.Text = "Saisissez les critères utiles au suivi de la facturation en attente.";
            TypeFilterLabel.Text = "Type de livraison";
        }
        else
        {
            FilterTitleLabel.Text = "Filtrer les expéditions";
            FilterSubtitleLabel.Text = "Saisissez uniquement les critères utiles à la recherche.";
            TypeFilterLabel.Text = "Type de commande";
        }
    }

    private static readonly (string Value, string Label)[] InvoiceWaitingDeliveryTypes =
    {
        ("DEPOT", "DEPOT"),
        ("EXPRESS", "EXPRESS"),
        ("MAGASIN", "MAGASIN"),
        ("STOCK", "STOCK"),
        ("IMPLANTATION", "IMPLANTATION"),
        ("REFACTURATION", "REFACTURATION"),
        ("DROP-SHIPPING", "DROP-SHIPPING"),
        ("PORT", "FORFAIT DE FRAIS DE PORT"),
        ("MANUEL", "MANUEL"),
        ("REGULARISATION", "REGULARISATION"),
        ("PSD", "PSD"),
        ("PERIODIC", "PÉRIODIQUE")
    };

    private void LoadCurrentFilter()
    {
        var f = GlobalState.CurrentExpeditionFilter;
        var currentAccount = EasySession.CurrentAccount;

        if (f != null)
        {
            StartDateEntry.Text = FormatFilterDate(f.FirstDate);
            EndDateEntry.Text = FormatFilterDate(f.LastDate);
            EntryClientSorder.Text = f.ClientSorderCode;
            EntryBL.Text = f.DeliveryNumber;
            EntryProduct.Text = f.ProductCode;
            EntryContainer.Text = f.ContainerNo;
            if (_mode == ExpeditionFilterMode.InvoiceWaiting)
            {
                PickerDeliveryType.SelectedItem = DeliveryTypeDisplayValue(f.DeliveryType);
                PickerSorderType.SelectedItem = null;
            }
            else
            {
                PickerSorderType.SelectedItem = f.SorderType;
                PickerDeliveryType.SelectedItem = null;
            }
            EntryAccountCode.Text = f.AccountCode;
        }

        if (string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase))
        {
            EntryAccountCode.Text = currentAccount.AccountCode;
            EntryAccountCode.IsEnabled = false;
            EntryAccountCode.BackgroundColor = Color.FromArgb("#EFEFEF");
            EntryAccountCode.TextColor = Colors.Gray;
            EntryAccountCode.IsVisible = false;
            AccountCodeContainer.IsVisible = false;
        }
        else
        {
            EntryAccountCode.IsEnabled = true;
            EntryAccountCode.BackgroundColor = Colors.Transparent;
            EntryAccountCode.TextColor = Color.FromArgb("#0F172A");
            EntryAccountCode.IsVisible = true;
            AccountCodeContainer.IsVisible = true;
        }
    }

    private async void OnApplyFilterClicked(object sender, EventArgs e)
    {
        var currentAccount = EasySession.CurrentAccount;
        var isAdministrator = string.Equals(currentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);

        string selectedType = SelectedTypeValue();
        bool selectedTypeIsSearchCriterion = _mode == ExpeditionFilterMode.InvoiceWaiting && !string.IsNullOrWhiteSpace(selectedType);

        if (isAdministrator
            && string.IsNullOrWhiteSpace(EntryAccountCode.Text)
            && string.IsNullOrWhiteSpace(EntryProduct.Text)
            && string.IsNullOrWhiteSpace(EntryClientSorder.Text)
            && string.IsNullOrWhiteSpace(EntryBL.Text)
            && string.IsNullOrWhiteSpace(EntryContainer.Text)
            && !selectedTypeIsSearchCriterion)
        {
            await ModernAlertService.ShowWarningAsync("Pour lancer la recherche, renseignez au moins un critère autre que les dates.");
            return;
        }

        var dateRange = await ReadDateRangeAsync();
        if (!dateRange.Success)
            return;

        var f = GlobalState.CurrentExpeditionFilter ?? new ExpeditionFilter();
        f.FirstDate = dateRange.FirstDate;
        f.LastDate = dateRange.LastDate;
        f.ClientSorderCode = EntryClientSorder.Text;
        f.DeliveryNumber = EntryBL.Text;
        f.ProductCode = EntryProduct.Text;
        f.ContainerNo = NormalizeContainerNo(EntryContainer.Text);
        if (_mode == ExpeditionFilterMode.InvoiceWaiting)
        {
            f.DeliveryType = selectedType;
            f.SorderType = null;
        }
        else
        {
            f.SorderType = selectedType;
            f.DeliveryType = null;
        }
        f.AccountCode = string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase)
            ? currentAccount.AccountCode
            : EntryAccountCode.Text;

        GlobalState.CurrentExpeditionFilter = f;

        await Navigation.PopModalAsync();
    }

    private void OnProductTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isResolvingProductCode)
            return;

        _productScanInputTracker.ObserveTextChanged(e.OldTextValue, e.NewTextValue);
        _lastUserProductFilter = (e.NewTextValue ?? string.Empty).Trim();
    }

    private string ProductSelectionInitialFilter()
    {
        return !string.IsNullOrWhiteSpace(_lastUserProductFilter)
            ? _lastUserProductFilter
            : (EntryProduct.Text ?? string.Empty).Trim();
    }

    private static string FormatFilterDate(DateTime? value)
    {
        return value.HasValue
            ? value.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("fr-FR"))
            : string.Empty;
    }

    private async Task<(bool Success, DateTime? FirstDate, DateTime? LastDate)> ReadDateRangeAsync()
    {
        string startText = (StartDateEntry.Text ?? string.Empty).Trim();
        string endText = (EndDateEntry.Text ?? string.Empty).Trim();
        bool startEmpty = string.IsNullOrWhiteSpace(startText);
        bool endEmpty = string.IsNullOrWhiteSpace(endText);

        if (startEmpty && endEmpty)
            return (true, null, null);

        if (startEmpty || endEmpty)
        {
            await ModernAlertService.ShowWarningAsync("Les deux dates doivent être renseignées, ou les deux dates doivent être laissées vides.");
            return (false, null, null);
        }

        if (!TryParseFilterDate(startText, out DateTime firstDate))
        {
            await ModernAlertService.ShowWarningAsync("La date de début doit être saisie au format jj/mm/aaaa.");
            return (false, null, null);
        }

        if (!TryParseFilterDate(endText, out DateTime lastDate))
        {
            await ModernAlertService.ShowWarningAsync("La date de fin doit être saisie au format jj/mm/aaaa.");
            return (false, null, null);
        }

        if (lastDate < firstDate)
        {
            await ModernAlertService.ShowWarningAsync("La date de fin ne peut pas être antérieure à la date de début.");
            return (false, null, null);
        }

        return (true, firstDate, lastDate);
    }

    private static bool TryParseFilterDate(string value, out DateTime date)
    {
        string[] formats = { "dd/MM/yyyy", "d/M/yyyy" };
        return DateTime.TryParseExact(
            value,
            formats,
            CultureInfo.GetCultureInfo("fr-FR"),
            DateTimeStyles.None,
            out date);
    }

    private string SelectedTypeValue()
    {
        if (_mode != ExpeditionFilterMode.InvoiceWaiting)
        {
            var selectedSorderType = PickerSorderType.SelectedItem?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(selectedSorderType) ? null : selectedSorderType;
        }

        var selectedDeliveryType = PickerDeliveryType.SelectedItem?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(selectedDeliveryType))
            return null;

        return InvoiceWaitingDeliveryTypes.FirstOrDefault(x => string.Equals(x.Label, selectedDeliveryType, StringComparison.OrdinalIgnoreCase)).Value
            ?? selectedDeliveryType;
    }

    private static string DeliveryTypeDisplayValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return InvoiceWaitingDeliveryTypes.FirstOrDefault(x => string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase)).Label
            ?? value;
    }

    private static string NormalizeContainerNo(string value)
    {
        var text = value?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Length <= 18 ? text.PadLeft(18, '0') : text;
    }

    private void SynchronizeDatePicker(Entry sourceEntry, DatePicker picker)
    {
        string text = (sourceEntry.Text ?? string.Empty).Trim();
        if (!TryParseFilterDate(text, out DateTime parsedDate))
            return;

        if (picker.Date == parsedDate.Date)
            return;

        _isSynchronizingDatePicker = true;
        try
        {
            picker.Date = parsedDate.Date;
        }
        finally
        {
            _isSynchronizingDatePicker = false;
        }
    }

    private void OnStartDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_isSynchronizingDatePicker)
            return;

        StartDateEntry.Text = FormatFilterDate(e.NewDate);
    }

    private void OnEndDateSelected(object sender, DateChangedEventArgs e)
    {
        if (_isSynchronizingDatePicker)
            return;

        EndDateEntry.Text = FormatFilterDate(e.NewDate);
    }


    private async Task ResolveProductCodeAsync(bool avoidDuplicate)
    {
        string keyword = (EntryProduct.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            _lastUserProductFilter = string.Empty;
            _lastResolvedProductText = string.Empty;
            return;
        }

        _lastUserProductFilter = keyword;

        if (avoidDuplicate && string.Equals(_lastResolvedProductText, keyword, StringComparison.OrdinalIgnoreCase))
            return;

        if (_isResolvingProductCode)
            return;

        _isResolvingProductCode = true;
        try
        {
            var products = await EasySession.FindProductCodeListAsync(keyword, false, false) ?? Array.Empty<global::Services.ReturnableArticle>();
            var firstProductCode = products.FirstOrDefault()?.ProductCode?.Trim();
            if (!string.IsNullOrWhiteSpace(firstProductCode))
                EntryProduct.Text = firstProductCode;

            _lastResolvedProductText = (EntryProduct.Text ?? keyword).Trim();
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowWarningAsync(ex.Message);
        }
        finally
        {
            _isResolvingProductCode = false;
        }
    }

    private async Task OnProductEntryCompletedAsync()
    {
        if (_productScanInputTracker.ConsumeCompletedAsScan(EntryProduct.Text))
        {
            string? productCode = await ProductBarcodeScanService.ResolveScannedProductCodeAsync(this, EntryProduct.Text);
            ApplyScannedProductCode(productCode);
            return;
        }

        await ResolveProductCodeAsync(true);
    }

    private async void OnSelectClientClicked(object sender, EventArgs e)
    {
        if (!EntryAccountCode.IsVisible || !EntryAccountCode.IsEnabled)
            return;

        var selectedAccount = await ClientAccountSelectionPage.ShowAsync(
            this,
            "Choisir un client",
            (EntryAccountCode.Text ?? string.Empty).Trim());

        string accountCode = selectedAccount?.AccountCode?.Trim();
        if (string.IsNullOrWhiteSpace(accountCode))
            return;

        EntryAccountCode.Text = accountCode;
    }



    private async void OnScanProductBarcodeClicked(object sender, EventArgs e)
    {
        string? productCode = await ProductBarcodeScanService.ScanAndResolveProductCodeAsync(this);
        ApplyScannedProductCode(productCode);
    }

    private void ApplyScannedProductCode(string? productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return;

        EntryProduct.Text = productCode;
        _lastUserProductFilter = productCode;
        _lastResolvedProductText = productCode;
        _productScanInputTracker.Reset();
    }

    private async void OnSelectProductClicked(object sender, EventArgs e)
    {
        var selectedProduct = await ProductCodeSelectionPage.ShowAsync(
            this,
            "Choisir un article",
            ProductSelectionInitialFilter());

        string productCode = selectedProduct?.ProductCode?.Trim();
        if (string.IsNullOrWhiteSpace(productCode))
            return;

        EntryProduct.Text = productCode;
        _lastUserProductFilter = productCode;
        _lastResolvedProductText = productCode;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();

        var currentAccount = EasySession.CurrentAccount;
        bool isAdministrator = string.Equals(currentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
        bool hasValidFilter = _mode == ExpeditionFilterMode.InvoiceWaiting
            ? GlobalState.HasValidAdminInvoiceWaitingFilter()
            : GlobalState.HasValidAdminExpeditionFilter();

        if (isAdministrator && !hasValidFilter)
            await Shell.Current.GoToAsync("//home");
    }
}

public enum ExpeditionFilterMode
{
    Expedition,
    InvoiceWaiting
}

public class BorderlessEntry : Entry
{
}

public class BorderlessPicker : Picker
{
    private readonly TapGestureRecognizer _openPickerGesture;
    private View? _openPickerGestureTarget;

    public BorderlessPicker()
    {
        _openPickerGesture = new TapGestureRecognizer();
        _openPickerGesture.Tapped += (_, __) => OpenPicker();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        UpdateOpenPickerGestureTarget();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Opacity))
            UpdateOpenPickerGestureTarget();
    }

    private void UpdateOpenPickerGestureTarget()
    {
        if (_openPickerGestureTarget != null)
        {
            _openPickerGestureTarget.GestureRecognizers.Remove(_openPickerGesture);
            _openPickerGestureTarget = null;
        }

        if (Opacity > 0.05 || Parent is not View parent)
            return;

        parent.GestureRecognizers.Add(_openPickerGesture);
        _openPickerGestureTarget = parent;
    }

    private void OpenPicker()
    {
        if (IsEnabled && IsVisible)
            Focus();
    }
}

public class BorderlessDatePicker : DatePicker
{
    private readonly TapGestureRecognizer _openPickerGesture;
    private View? _openPickerGestureTarget;

    public BorderlessDatePicker()
    {
        _openPickerGesture = new TapGestureRecognizer();
        _openPickerGesture.Tapped += (_, __) => OpenPicker();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        UpdateOpenPickerGestureTarget();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Opacity))
            UpdateOpenPickerGestureTarget();
    }

    private void UpdateOpenPickerGestureTarget()
    {
        if (_openPickerGestureTarget != null)
        {
            _openPickerGestureTarget.GestureRecognizers.Remove(_openPickerGesture);
            _openPickerGestureTarget = null;
        }

        if (Opacity > 0.05 || Parent is not View parent)
            return;

        parent.GestureRecognizers.Add(_openPickerGesture);
        _openPickerGestureTarget = parent;
    }

    private void OpenPicker()
    {
        if (IsEnabled && IsVisible)
            Focus();
    }
}
