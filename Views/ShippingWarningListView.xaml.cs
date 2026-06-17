using System.Collections.ObjectModel;
using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Services;

namespace AGRA_EASY_MOBILE;

public partial class ShippingWarningListView : ContentPage
{
    private const decimal PageSize = 20;
    private readonly Dictionary<string, ShippingWarningListItem> _itemsById = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLoading;
    private bool _firstLoadDone;
    private bool _reloadOnNextAppearing;
    private bool _hasMoreWarnings = true;
    private DateTime _lastLaunchDate;

    public ObservableCollection<ShippingWarningListItem> Warnings { get; } = new();

    public ShippingWarningListView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_firstLoadDone)
        {
            _firstLoadDone = true;
            _lastLaunchDate = ShippingWarningNotificationService.ReadAndUpdateLastLaunchDate();
            await LoadNextPageAsync(false);
            return;
        }

        if (_reloadOnNextAppearing)
        {
            _reloadOnNextAppearing = false;
            ResetList();
            await LoadNextPageAsync(false);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Shell.Current?.CurrentPage is ShippingWarningDetailView)
            return;
    }

    private async Task LoadNextPageAsync(bool append)
    {
        if (_isLoading)
            return;

        if (append && !_hasMoreWarnings)
            return;

        _isLoading = true;
        SetLoading(true);
        try
        {
            var f = GlobalState.CurrentShippingWarningFilter;
            if (EasySession.IsClient && !string.IsNullOrWhiteSpace(EasySession.CurrentAccount?.AccountCode))
                f.AccountCode = EasySession.CurrentAccount.AccountCode;

            if (!append)
                _hasMoreWarnings = true;

            var oldNewestDate = Warnings.Count == 0
                ? DateTime.MinValue
                : Warnings.Max(x => x.Warning.CreationDate);
            var offset = append ? (decimal)Warnings.Count : 0m;
            var returned = await EasySession.GetShippingWarningListAsync(f, offset, PageSize) ?? Array.Empty<ShippingWarning>();

            // Ne pas arrêter la pagination sur un retour inférieur à PageSize.
            // Le service peut retourner légèrement moins de lignes selon sa requête de pagination
            // (ex. borne SQL stricte). L'arrêt définitif est déclenché uniquement quand le service
            // ne retourne plus aucune ligne.
            if (returned.Length == 0)
            {
                _hasMoreWarnings = false;
                return;
            }

            var addedHasNewerThanOldTop = AddAndRefresh(returned, oldNewestDate);
            if (addedHasNewerThanOldTop && Warnings.Count > 0)
                WarningsCollection.ScrollTo(Warnings[0], position: ScrollToPosition.Start, animate: true);
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            SetLoading(false);
            _isLoading = false;
        }
    }

    private bool AddAndRefresh(IEnumerable<ShippingWarning> items, DateTime oldNewestDate)
    {
        var hasNewerThanOldTop = false;

        foreach (var warning in items.Where(x => x != null))
        {
            var id = warning.ID?.Trim();
            if (string.IsNullOrWhiteSpace(id))
                id = $"{warning.ShippingWarningId}|{warning.OriginalWarehouse}|{warning.CreationDate:O}";

            if (_itemsById.ContainsKey(id))
                continue;

            var item = new ShippingWarningListItem(warning, warning.CreationDate > _lastLaunchDate);
            _itemsById[id] = item;
            if (oldNewestDate != DateTime.MinValue && warning.CreationDate > oldNewestDate)
                hasNewerThanOldTop = true;

            InsertWarningInDateOrder(item);
        }

        return hasNewerThanOldTop;
    }

    private void InsertWarningInDateOrder(ShippingWarningListItem item)
    {
        var insertIndex = 0;
        while (insertIndex < Warnings.Count && Warnings[insertIndex].Warning.CreationDate >= item.Warning.CreationDate)
            insertIndex++;

        if (insertIndex >= Warnings.Count)
            Warnings.Add(item);
        else
            Warnings.Insert(insertIndex, item);
    }

    private void SetLoading(bool loading)
    {
        LoadingOverlay.IsVisible = loading;
        LoadingIndicator.IsRunning = loading;
    }

    private async void OnWarningsCollectionScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        // Le chargement suivant ne doit être déclenché que lors d'un défilement vers le bas,
        // lorsque l'utilisateur arrive près de la fin de la liste affichée.
        // Un défilement vers le haut ne doit jamais provoquer de récupération.
        if (e.VerticalDelta <= 0)
            return;

        if (_isLoading || !_hasMoreWarnings || Warnings.Count == 0)
            return;

        if (e.LastVisibleItemIndex >= Warnings.Count - 4)
            await LoadNextPageAsync(true);
    }

    private async void OnOpenFilterClicked(object sender, EventArgs e)
    {
        var openedWithoutFilter = GlobalState.PeekShippingWarningFilter() == null;
        _reloadOnNextAppearing = true;
        await Navigation.PushModalAsync(new ShippingWarningFilterView(openedWithoutFilter));
    }

    private async void OnWarningTapped(object sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is ShippingWarningListItem item)
            await Navigation.PushAsync(new ShippingWarningDetailView(item.Warning));
    }

    private void ResetList()
    {
        _itemsById.Clear();
        Warnings.Clear();
        _hasMoreWarnings = true;
    }
}
