using System.Globalization;
using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AGRA_EASY_MOBILE;

public partial class ReturnFilterView : ContentPage
{
    private readonly bool _hideReturnStatusFilters;
    private bool _isResolvingProductCode;
    private bool _isSynchronizingDatePicker;
    private string _lastResolvedProductText = string.Empty;
    private string _lastUserProductFilter = string.Empty;
    private readonly KeyboardScanInputTracker _productScanInputTracker = new();

    public ReturnFilterView(bool hideReturnStatusFilters = false)
    {
        _hideReturnStatusFilters = hideReturnStatusFilters;
        InitializeComponent();
        EntryProduct.TextChanged += OnProductTextChanged;
        EntryProduct.Completed += async (_, __) => await OnProductEntryCompletedAsync();
        EntryProduct.Unfocused += async (_, __) => await ResolveProductCodeAsync(true);
        StartDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDateEntry.TextChanged += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        StartDatePicker.Focused += (_, __) => SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        EndDatePicker.Focused += (_, __) => SynchronizeDatePicker(EndDateEntry, EndDatePicker);
        LoadCurrentFilter();
        ApplyContextualVisibility();
        SynchronizeDatePicker(StartDateEntry, StartDatePicker);
        SynchronizeDatePicker(EndDateEntry, EndDatePicker);
    }


    private void ApplyContextualVisibility()
    {
        if (!_hideReturnStatusFilters)
            return;

        ReturnStatusContainer.IsVisible = false;
        ProductStatusContainer.IsVisible = false;
        PickerReturnStatus.SelectedItem = string.Empty;
        PickerProductStatus.SelectedItem = string.Empty;
    }

    private void LoadCurrentFilter()
    {
        var f = GlobalState.CurrentReturnFilter;
        var currentAccount = EasySession.CurrentAccount;

        if (f != null)
        {
            StartDateEntry.Text = FormatFilterDate(f.FirstDate);
            EndDateEntry.Text = FormatFilterDate(f.LastDate);
            EntryProduct.Text = f.ProductCode;
            EntryAccountCode.Text = f.AccountCode;
            PickerReturnStatus.SelectedItem = ReturnStatusLabelFromCode(f.ReturnStatus);
            PickerProductStatus.SelectedItem = ProductStatusLabelFromCode(f.ProductStatus);
            EntryReturnCode.Text = f.ReturnCode;
            EntryReturnClientCode.Text = f.ReturnClientCode;
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

        if (isAdministrator
            && string.IsNullOrWhiteSpace(EntryAccountCode.Text)
            && string.IsNullOrWhiteSpace(EntryProduct.Text)
            && string.IsNullOrWhiteSpace(EntryReturnCode.Text)
            && string.IsNullOrWhiteSpace(EntryReturnClientCode.Text))
        {
            await ModernAlertService.ShowWarningAsync("Pour lancer la recherche, renseignez au moins un critère autre que les dates.");
            return;
        }

        var dateRange = await ReadDateRangeAsync();
        if (!dateRange.Success)
            return;

        var f = GlobalState.CurrentReturnFilter ?? new ReturnFilter();
        f.FirstDate = dateRange.FirstDate;
        f.LastDate = dateRange.LastDate;
        f.ProductCode = EntryProduct.Text;
        f.AccountCode = string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase)
            ? currentAccount.AccountCode
            : EntryAccountCode.Text;
        f.ReturnStatus = _hideReturnStatusFilters ? null : ReturnStatusCodeFromLabel(PickerReturnStatus.SelectedItem?.ToString());
        f.ProductStatus = _hideReturnStatusFilters ? null : ProductStatusCodeFromLabel(PickerProductStatus.SelectedItem?.ToString());
        f.ReturnCode = EntryReturnCode.Text;
        f.ReturnClientCode = EntryReturnClientCode.Text;

        GlobalState.CurrentReturnFilter = f;

        await Navigation.PopModalAsync();
    }

    private static string ReturnStatusLabelFromCode(string code) => code switch
    {
        "treated" => "Traité",
        "waiting" => "En cours",
        "external" => "Externe",
        _ => string.Empty
    };

    private static string ReturnStatusCodeFromLabel(string label) => label switch
    {
        "Traité" => "treated",
        "En cours" => "waiting",
        "Externe" => "external",
        _ => null
    };

    private static string ProductStatusLabelFromCode(string code) => code switch
    {
        "accepted" => "Accepté",
        "warranty" => "Accepté en garantie",
        "refunded" => "Remboursé",
        "supplier" => "Retour au fournisseur",
        "refused" => "Rejeté",
        "missing" => "Manquant",
        _ => string.Empty
    };

    private static string ProductStatusCodeFromLabel(string label) => label switch
    {
        "Accepté" => "accepted",
        "Accepté en garantie" => "warranty",
        "Remboursé" => "refunded",
        "Retour au fournisseur" => "supplier",
        "Rejeté" => "refused",
        "Manquant" => "missing",
        _ => null
    };

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
        if (isAdministrator && !GlobalState.HasValidAdminReturnFilter())
            await Shell.Current.GoToAsync("//home");
    }
}
