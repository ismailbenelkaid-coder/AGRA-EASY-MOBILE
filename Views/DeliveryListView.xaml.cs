using AGRA_EASY_MOBILE.Services;
using AGRA_EASY_MOBILE.Models;
using Services;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace AGRA_EASY_MOBILE;

public partial class DeliveryListView : ContentPage, INotifyPropertyChanged
{
    private const string DefaultSortLabel = "Par défaut";
    private const string ArticleSortLabel = "Article";
    private const string DeliverySortLabel = "N° BL";
    private const string ClientOrderSortLabel = "Commande";
    private const string CreationDateSortLabel = "Date création";
    private const string ContainerSortLabel = "Conteneur";

    private bool _isSingleDetailView = false;
    private bool _isReadyToLoadData = false;
    private DeliveryLine? _currentDetailHeader;
    private string _selectedSortMode = DefaultSortLabel;
    private bool _sortAscending = true;
    private TaskCompletionSource<bool>? _confirmationCompletion;

    public bool IsConfirmationVisible { get; private set; }
    public string ConfirmationTitle { get; private set; } = string.Empty;
    public string ConfirmationMessage { get; private set; } = string.Empty;

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

    public DeliveryLine? CurrentDetailHeader
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

    public DeliveryListView()
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
        PickerSortMode.Items.Add(DeliverySortLabel);
        PickerSortMode.Items.Add(ClientOrderSortLabel);
        PickerSortMode.Items.Add(ContainerSortLabel);
        PickerSortMode.Items.Add(CreationDateSortLabel);
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
            var rawData = await EasySession.GetDeliveriesLinesAsync(GlobalState.CurrentExpeditionFilter);
            if (rawData != null)
            {
                var orderedData = ApplySort(rawData);
                DeliveryCollection.ItemsSource = orderedData;
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

    private DeliveryLine[] ApplySort(DeliveryLine[] data)
    {
        if (_selectedSortMode == DefaultSortLabel)
            return data;

        IOrderedEnumerable<DeliveryLine> orderedData = _selectedSortMode switch
        {
            ArticleSortLabel => _sortAscending
                ? data.OrderBy(x => x.ProductCode ?? string.Empty).ThenBy(x => x.RptDelivryId ?? string.Empty)
                : data.OrderByDescending(x => x.ProductCode ?? string.Empty).ThenByDescending(x => x.RptDelivryId ?? string.Empty),

            DeliverySortLabel => _sortAscending
                ? data.OrderBy(x => x.RptDelivryId ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.RptDelivryId ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            ClientOrderSortLabel => _sortAscending
                ? data.OrderBy(x => x.SorderClientCode ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.SorderClientCode ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            ContainerSortLabel => _sortAscending
                ? data.OrderBy(x => x.ContainerNo ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.ContainerNo ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            CreationDateSortLabel => _sortAscending
                ? data.OrderBy(x => x.CreationDate).ThenBy(x => x.RptDelivryId ?? string.Empty).ThenBy(x => x.ProductCode ?? string.Empty)
                : data.OrderByDescending(x => x.CreationDate).ThenByDescending(x => x.RptDelivryId ?? string.Empty).ThenByDescending(x => x.ProductCode ?? string.Empty),

            _ => data.OrderBy(x => 0)
        };

        return orderedData.ToArray();
    }

    private async Task LoadDetailLinesAsync(DeliveryLine item)
    {
        SetLoadingState(true);
        try
        {
            var details = await EasySession.GetDeliveryLinesAsync(item.RptDelivryId, item.Warehouse);
            if (details != null)
            {
                var orderedDetails = ApplySort(details);
                _isSingleDetailView = true;
                CurrentDetailHeader = orderedDetails.FirstOrDefault() ?? item;
                DeliveryCollection.ItemsSource = orderedDetails;
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
        var item = (DeliveryLine)((Button)sender).CommandParameter;
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
        var item = (DeliveryLine)((Button)sender).CommandParameter;
        SetLoadingState(true);

        try
        {
            byte[] pdf = await EasySession.GetDeliveryDocumentAsync(item.RptDelivryId, item.Warehouse);
            if (pdf == null || pdf.Length == 0)
            {
                await ModernAlertService.ShowWarningAsync("Le document PDF est vide ou indisponible.");
                return;
            }

            string safeDeliveryId = string.Concat((item.RptDelivryId ?? "BL").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            string fileName = $"BL_{safeDeliveryId}.pdf";
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

    private async void OnReturnBasketShortcutClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new ReturnBasketView());
    }

    private async void OnAddToReturnBasketClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not DeliveryLine item)
            return;

        SetLoadingState(true);
        try
        {
            var basketLines = await EasySession.GetReturnBasketLinesAsync() ?? Array.Empty<ReturnBasketLine>();
            if (basketLines.Any(line => !string.IsNullOrWhiteSpace(line.Warehouse)
                && !SameText(line.Warehouse, item.Warehouse)))
            {
                SetLoadingState(false);
                await ShowBlockingConfirmationAsync(
                    "Ajout impossible",
                    "Le panier contient déjà des lignes d'une autre plateforme. Veuillez valider ou vider le panier avant d'ajouter cette ligne.");
                return;
            }

            bool isAdministrator = string.Equals(EasySession.CurrentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
            if (isAdministrator)
            {
                if (string.IsNullOrWhiteSpace(item.AccountCode))
                {
                    SetLoadingState(false);
                    await ShowBlockingConfirmationAsync(
                        "Ajout impossible",
                        "Le client livré de cette ligne de BL est introuvable. Impossible d'affecter le panier retour.");
                    return;
                }

                if (basketLines.Length == 0)
                {
                    await EasySession.SetReturnBasketAccountCodeAsync(item.AccountCode);
                }
                else
                {
                    var basketAccountCode = await EasySession.GetReturnBasketAccountCodeAsync();
                    if (!SameText(basketAccountCode, item.AccountCode))
                    {
                        SetLoadingState(false);
                        await ShowBlockingConfirmationAsync(
                            "Ajout impossible",
                            "Le panier retour contient déjà des lignes pour un autre client. Veuillez valider ou vider le panier avant d'ajouter cette ligne.");
                        return;
                    }
                }
            }

            var match = await FindReturnableLineForDeliveryAsync(item);
            if (match == null)
            {
                SetLoadingState(false);
                await ShowBlockingConfirmationAsync(
                    "Ajout impossible",
                    "Aucune ligne retournable n'a été trouvée pour cette ligne de BL.");
                return;
            }

            decimal availableQuantity = GetAvailableQuantity(match);
            if (availableQuantity <= 0)
            {
                SetLoadingState(false);
                await ShowBlockingConfirmationAsync(
                    "Article déjà retourné",
                    "Cet article est déjà retourné ou ne dispose plus de quantité retournable.");
                return;
            }

            await EasySession.AddReturnBasketLineAsync(
                match.Warehouse,
                match.Index,
                availableQuantity,
                false,
                string.Empty,
                match.Reference);

            SetLoadingState(false);
            bool goBasket = await ShowConfirmationAsync(
                "Ligne ajoutée",
                $"La quantité retournable ({availableQuantity:0.####}) a été ajoutée au panier de retour. Pensez à valider le panier retour pour finaliser la demande.");

            if (goBasket)
                await Navigation.PushModalAsync(new ReturnBasketView());
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

    private async Task<DeliveryLinesStatus?> FindReturnableLineForDeliveryAsync(DeliveryLine item)
    {
        if (string.IsNullOrWhiteSpace(item.ProductCode))
            return null;

        var lines = await EasySession.GetArticlesReturnableDeliveryLinesAsync(item.ProductCode) ?? Array.Empty<DeliveryLinesStatus>();
        return lines
            .Where(line => SameText(line.Warehouse, item.Warehouse))
            .Where(line => SameText(line.DeliveryNumber, item.RptDelivryId))
            .Where(line => SameText(line.Reference, item.ProductCode))
            .Where(line => MatchContainer(line.ContainerNo, item.ContainerNo))
            .OrderByDescending(GetAvailableQuantity)
            .FirstOrDefault();
    }

    private static bool SameText(string? left, string? right)
        => string.Equals((left ?? string.Empty).Trim(), (right ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchContainer(string? returnableContainer, string? deliveryContainer)
    {
        if (IsEmptyOrZeroContainer(returnableContainer) || IsEmptyOrZeroContainer(deliveryContainer))
            return true;

        return SameText(returnableContainer, deliveryContainer);
    }

    private static bool IsEmptyOrZeroContainer(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(text) || text == new string('0', 18);
    }

    private decimal GetAvailableQuantity(DeliveryLinesStatus line)
    {
        try
        {
            return line.DeliveredQuantity - line.ReturnedQuantity;
        }
        catch
        {
            return 0;
        }
    }

    private async Task ShowBlockingConfirmationAsync(string title, string message)
    {
        await ShowConfirmationAsync(title, message);
    }

    private Task<bool> ShowConfirmationAsync(string title, string message)
    {
        _confirmationCompletion?.TrySetResult(false);
        ConfirmationTitle = title;
        ConfirmationMessage = message;
        IsConfirmationVisible = true;
        OnPropertyChanged(nameof(ConfirmationTitle));
        OnPropertyChanged(nameof(ConfirmationMessage));
        OnPropertyChanged(nameof(IsConfirmationVisible));
        _confirmationCompletion = new TaskCompletionSource<bool>();
        return _confirmationCompletion.Task;
    }

    private void OnConfirmationOkClicked(object sender, EventArgs e)
        => CloseConfirmation(false);

    private void OnConfirmationReturnBasketClicked(object sender, EventArgs e)
        => CloseConfirmation(true);

    private void CloseConfirmation(bool goBasket)
    {
        IsConfirmationVisible = false;
        OnPropertyChanged(nameof(IsConfirmationVisible));
        _confirmationCompletion?.TrySetResult(goBasket);
        _confirmationCompletion = null;
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
