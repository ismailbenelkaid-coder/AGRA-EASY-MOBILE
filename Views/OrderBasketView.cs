using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls.Shapes;
using Services;
using System.Globalization;

namespace AGRA_EASY_MOBILE
{
    public class OrderBasketView : ContentPage
    {
        private readonly Entry _sorderEntry = CreateTextEntry("N° commande client");
        private readonly Entry _lotEntry = CreateTextEntry("N° de lot");
        private readonly DatePicker _deliveryDatePicker = new BorderlessDatePicker
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Format = "dd MMMM yyyy"
        };
        private bool _showAllPlatforms;
        private Border? _showAllPlatformsBox;
        private Label? _showAllPlatformsMark;
        private readonly Label _messageLabel = new() { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#B45309") };
        private readonly Label _totalLabel = new() { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#16A34A"), IsVisible = false };
        private readonly Label _assignedAccountLabel = new() { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#475569"), IsVisible = false };
        private readonly Label _filterLabel = new() { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") };
        private readonly VerticalStackLayout _dashboardList = new() { Spacing = 8 };
        private readonly VerticalStackLayout _basketList = new() { Spacing = 8 };
        private readonly VerticalStackLayout _responseList = new() { Spacing = 8 };
        private readonly List<ShoppingCartResponse> _shoppingCartResponses = new();
        private readonly VerticalStackLayout _checkoutActions = new() { Spacing = 8 };
        private readonly ActivityIndicator _apiActivityIndicator = new() { IsRunning = false, Color = Color.FromArgb("#16A34A"), WidthRequest = 42, HeightRequest = 42 };
        private readonly Grid _apiLoadingOverlay = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#66000000") };
        private readonly Grid _modalOverlay = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#99000000") };
        private readonly Label _modalTitle = new() { FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") };
        private readonly VerticalStackLayout _modalBody = new() { Spacing = 10 };
        private readonly Grid _modalButtons = new() { ColumnSpacing = 10, ColumnDefinitions = Columns("*,*") };
        private Border? _responseCard;
        private decimal _dashboardBasketTotalNetCost;
        private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

        private readonly List<CutoffDisplay> _cutoffDisplays = new();
        private string _warehouseCode = string.Empty;
        private bool _isAdmin;
        private bool _canChangeDeliveryAddress;
        private bool _isPromotionActive;
        private bool _initialized;
        private bool _cutoffTimerStarted;
        private readonly Dictionary<string, string> _lastSubmittedQuantityByLine = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _quantityUpdateKeysInProgress = new(StringComparer.OrdinalIgnoreCase);
        private bool _preserveResponsesOnNextDisappearing;
        private bool _resetResponsesOnNextAppearing;
        private TaskCompletionSource<bool>? _confirmationCompletion;

        public OrderBasketView()
        {
            Title = "Panier";
            BackgroundColor = Color.FromArgb("#F6F8FB");
            _sorderEntry.Completed += async (_, __) => await RefreshBasketAsync();
            BuildContent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized)
            {
                _initialized = true;
                await InitializeAsync();
            }
            else
            {
                if (_resetResponsesOnNextAppearing)
                {
                    _resetResponsesOnNextAppearing = false;
                    _shoppingCartResponses.Clear();
                    RebindResponses();
                }

                await RefreshBasketAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _cutoffDisplays.Clear();

            if (_preserveResponsesOnNextDisappearing)
            {
                _preserveResponsesOnNextDisappearing = false;
                return;
            }

            _resetResponsesOnNextAppearing = true;
        }

        private void BuildContent()
        {
            var root = new Grid();
            var scroll = new ScrollView();
            var content = new VerticalStackLayout { Padding = new Thickness(14, 12, 14, 24), Spacing = 14 };
            scroll.Content = content;
            root.Add(scroll);

            var catalogueButton = SecondaryButton("Catalogue");
            catalogueButton.Clicked += async (_, __) => await Shell.Current.GoToAsync("//catalogue");

            var deliveryOptionsButton = PrimaryButton("Option de livraison");
            deliveryOptionsButton.FontSize = 13;
            deliveryOptionsButton.Clicked += async (_, __) => await RunActionAsync(async () =>
            {
                _canChangeDeliveryAddress = await EasySession.AllowChangeDeliveryAddressAsync();
                ShowDeliveryOptionsModal();
            });

            var headerActions = new Grid { ColumnDefinitions = Columns("0.85*,1.4*"), ColumnSpacing = 8 };
            headerActions.Add(catalogueButton, 0, 0);
            headerActions.Add(deliveryOptionsButton, 1, 0);
            content.Add(headerActions);

            var showAllRow = BuildShowAllPlatformsRow();

            content.Add(Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    SectionTitle("Synthèse plateformes"),
                    showAllRow,
                    _filterLabel,
                    _dashboardList
                }
            }));

            _responseCard = Card(new VerticalStackLayout { Spacing = 10, Children = { SectionTitle("Statut des commandes"), _responseList } });
            _responseCard.IsVisible = false;
            content.Add(_responseCard);

            content.Add(Card(new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    SectionTitle("Mon panier"),
                    _assignedAccountLabel,
                    _messageLabel,
                    BuildSorderBlock(),
                    _checkoutActions
                }
            }));

            content.Add(Card(new VerticalStackLayout { Spacing = 10, Children = { SectionTitle("Lignes du panier"), _basketList } }));

            _apiLoadingOverlay.Add(new Border
            {
                BackgroundColor = Colors.White,
                Padding = 22,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        _apiActivityIndicator,
                        new Label { Text = "Chargement...", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), HorizontalTextAlignment = TextAlignment.Center }
                    }
                }
            });
            root.Add(_apiLoadingOverlay);

            _modalOverlay.Add(new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = new Thickness(20, 18),
                Margin = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 22 },
                Content = new VerticalStackLayout
                {
                    Spacing = 14,
                    WidthRequest = 340,
                    Children = { _modalTitle, _modalBody, _modalButtons }
                }
            });
            root.Add(_modalOverlay);

            Content = root;
        }

        private View BuildShowAllPlatformsRow()
        {
            _showAllPlatformsMark = new Label
            {
                Text = "✓",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                IsVisible = _showAllPlatforms
            };

            _showAllPlatformsBox = new Border
            {
                WidthRequest = 22,
                HeightRequest = 22,
                Padding = 0,
                BackgroundColor = _showAllPlatforms ? Color.FromArgb("#16A34A") : Colors.White,
                Stroke = _showAllPlatforms ? Color.FromArgb("#16A34A") : Color.FromArgb("#4F46E5"),
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Content = _showAllPlatformsMark
            };

            var label = new Label
            {
                Text = "Afficher toutes les plateformes",
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#334155"),
                FontSize = 13
            };

            var row = new HorizontalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(2, 6),
                Children = { _showAllPlatformsBox, label }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) => await ToggleShowAllPlatformsAsync();
            row.GestureRecognizers.Add(tap);

            return row;
        }

        private async Task ToggleShowAllPlatformsAsync()
        {
            _showAllPlatforms = !_showAllPlatforms;
            UpdateShowAllPlatformsVisual();
            await RefreshBasketAsync();
        }

        private void UpdateShowAllPlatformsVisual()
        {
            if (_showAllPlatformsBox == null || _showAllPlatformsMark == null)
                return;

            _showAllPlatformsBox.BackgroundColor = _showAllPlatforms ? Color.FromArgb("#16A34A") : Colors.White;
            _showAllPlatformsBox.Stroke = _showAllPlatforms ? Color.FromArgb("#16A34A") : Color.FromArgb("#4F46E5");
            _showAllPlatformsMark.IsVisible = _showAllPlatforms;
        }

        private View BuildSorderBlock()
        {
            var grid = new Grid { ColumnDefinitions = Columns("*"), ColumnSpacing = 8 };
            grid.Add(FramedEntry(_sorderEntry), 0, 0);
            return grid;
        }

        private void RebuildCheckoutActions()
        {
            _checkoutActions.Children.Clear();
            var row = new Grid { ColumnDefinitions = Columns("*,*,*,*"), ColumnSpacing = 6 };

            var express = CompactPrimaryButton("EXPRESS");
            express.Clicked += async (_, __) => await CheckOutAsync("EXPRESS");
            var magasin = CompactPrimaryButton("MAGASIN");
            magasin.Clicked += async (_, __) => await CheckOutAsync("MAGASIN");
            var stock = CompactPrimaryButton("STOCK");
            stock.Clicked += async (_, __) => await CheckOutAsync("STOCK");
            var promo = _isPromotionActive ? CompactSecondaryButton("Annul. promo") : CompactSecondaryButton("Promo");
            promo.Clicked += async (_, __) =>
            {
                if (_isPromotionActive)
                    await CancelPromotionAsync();
                else
                    await ApplyPromotionsAsync();
            };

            row.Add(express, 0, 0);
            row.Add(magasin, 1, 0);
            row.Add(stock, 2, 0);
            row.Add(promo, 3, 0);
            _checkoutActions.Children.Add(row);
        }

        private async Task InitializeAsync()
        {
            await RunActionAsync(async () =>
            {
                _shoppingCartResponses.Clear();
                RebindResponses();

                _isAdmin = EasySession.IsAdministrator;
                if (_isAdmin && !await EasySession.IsOrderManagerAsync())
                    throw new Exception("L'utilisateur n'est pas autorisé à passer des commandes.");

                if (EasySession.IsClient)
                {
                    string login = Preferences.Default.Get("UserLogin", string.Empty);
                    string currentAccount = EasySession.CurrentAccount?.AccountCode ?? string.Empty;
                    if (!string.Equals(login, currentAccount, StringComparison.OrdinalIgnoreCase) && !await EasySession.IsClientOrderManagerAsync())
                        throw new Exception("L'utilisateur n'est pas autorisé à passer des commandes.");
                }

                _canChangeDeliveryAddress = await EasySession.AllowChangeDeliveryAddressAsync();
                await RefreshBasketCoreAsync();
            });
        }

        private async Task RefreshBasketAsync()
        {
            await RunActionAsync(RefreshBasketCoreAsync);
        }

        private async Task RefreshAssignedAccountDisplayAsync()
        {
            if (EasySession.IsClient)
            {
                _assignedAccountLabel.Text = string.Empty;
                _assignedAccountLabel.IsVisible = false;
                return;
            }

            string accountCode = await EasySession.GetOrderBasketAccountCodeAsync();
            if (string.IsNullOrWhiteSpace(accountCode))
            {
                _assignedAccountLabel.Text = string.Empty;
                _assignedAccountLabel.IsVisible = false;
                return;
            }

            string display = await BuildAssignedAccountDisplayAsync(accountCode);
            _assignedAccountLabel.Text = $"Client : {display}";
            _assignedAccountLabel.IsVisible = !string.IsNullOrWhiteSpace(display);
        }

        private async Task<string> BuildAssignedAccountDisplayAsync(string accountCode)
        {
            string fallback = accountCode ?? string.Empty;
            try
            {
                DelivryAddress address = await EasySession.GetDelivryAddressAsync();
                string name = (address?.ContactName ?? string.Empty).Trim();
                string postalCode = (address?.CP ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(name))
                    return string.IsNullOrWhiteSpace(postalCode) ? name : $"{name} ({postalCode})";
            }
            catch
            {
                // Si le service d'adresse ne retourne pas le nom/CP, conserver le code affecté.
            }

            return fallback;
        }

        private async Task RefreshBasketCoreAsync()
        {
            _dashboardList.Children.Clear();
            _basketList.Children.Clear();
            _totalLabel.Text = string.Empty;
            _cutoffDisplays.Clear();

            await RefreshAssignedAccountDisplayAsync();

            _isPromotionActive = await EasySession.IsPromotionActiveAsync();
            RebuildCheckoutActions();

            int count = await EasySession.GetShoppingCartUnitCountAsync();
            decimal total = await EasySession.GetTotalSorderCoastAsync();
            _messageLabel.Text = $"T.panier : {total:0.00} € - {count} Pcs";
            _messageLabel.TextColor = Color.FromArgb("#16A34A");

            bool expressBlocked = await EasySession.IsExpressSystemBlockedAsync();
            bool magasinBlocked = await EasySession.IsMagasinSystemBlockedAsync();
            var status = new List<string>();
            if (expressBlocked)
                status.Add("La préparation des commandes express est suspendue pour aujourd'hui.");
            if (magasinBlocked)
                status.Add("La préparation des commandes magasin est suspendue pour aujourd'hui.");
            _filterLabel.Text = string.IsNullOrWhiteSpace(_warehouseCode) ? string.Join(" ", status) : $"Plateforme sélectionnée : {_warehouseCode}";

            var dashboard = _showAllPlatforms
                ? await EasySession.GetWarehouseDashboardAsync(true)
                : await EasySession.GetShoppingCartDashboardAsync();
            dashboard ??= Array.Empty<ShoppingCartWarehouseInformation>();
            _dashboardBasketTotalNetCost = total;

            if (dashboard.Length == 0)
                _dashboardList.Children.Add(EmptyLabel("Aucune plateforme dans le panier."));
            else
            {
                foreach (var warehouse in dashboard)
                    _dashboardList.Children.Add(DashboardCard(warehouse));
            }
            StartCutoffTimerIfNeeded();

            var lines = await EasySession.GetOrderBasketLinesAsync(true, true, _warehouseCode ?? string.Empty) ?? Array.Empty<SorderBasketLine>();
            if (lines.Length == 0)
                _basketList.Children.Add(EmptyLabel("Aucune ligne dans le panier."));
            else
            {
                foreach (var line in lines)
                    _basketList.Children.Add(BasketLineCard(new OrderBasketLineSelection(line)));
            }

            _totalLabel.Text = string.Empty;
        }

        private View DashboardCard(ShoppingCartWarehouseInformation warehouse)
        {
            string warehouseName = (warehouse.WarehouseName ?? string.Empty).Trim();
            bool isTotal = string.IsNullOrWhiteSpace(warehouseName);
            string title = isTotal ? "TOTAL PLATEFORMES" : warehouseName;
            bool selected = !isTotal && string.Equals(_warehouseCode, warehouseName, StringComparison.OrdinalIgnoreCase);

            var titleLabel = new Label
            {
                Text = title,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15,
                TextColor = selected ? Colors.White : Color.FromArgb("#0F172A"),
                VerticalOptions = LayoutOptions.Center
            };

            var cutoffLabel = new Label
            {
                Text = isTotal ? string.Empty : BuildInitialCutoffText(warehouse),
                FontSize = 12,
                TextColor = Color.FromArgb("#DC2626"),
                HorizontalTextAlignment = TextAlignment.End,
                IsVisible = !isTotal
            };
            if (!isTotal)
                RegisterCutoffDisplay(warehouse, cutoffLabel);

            var grid = new Grid { RowDefinitions = Rows("Auto,Auto,Auto"), ColumnDefinitions = Columns("*,Auto"), ColumnSpacing = 8, RowSpacing = 5 };
            grid.Add(titleLabel, 0, 0);
            grid.Add(cutoffLabel, 1, 0);
            if (!string.IsNullOrWhiteSpace(warehouse.WarehouseStatus))
            {
                var messageLabel = new Label
                {
                    Text = $"Message : {warehouse.WarehouseStatus}",
                    FontSize = 12,
                    TextColor = selected ? Colors.White : Color.FromArgb("#64748B"),
                    LineBreakMode = LineBreakMode.WordWrap
                };
                grid.Add(messageLabel, 0, 1);
                Grid.SetColumnSpan(messageLabel, 2);
            }

            decimal basketNetCost = GetDashboardBasketNetCost(warehouse, isTotal);
            decimal preparingNetCost = GetDashboardPreparingNetCost(warehouse, isTotal);

            var totals = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8 };
            totals.Add(SmallInfoPill($"T.panier : {basketNetCost:0.00}", selected), 0, 0);
            totals.Add(SmallInfoPill($"En préparation : {preparingNetCost:0.00}", selected), 1, 0);
            grid.Add(totals, 0, 2);
            Grid.SetColumnSpan(totals, 2);

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) =>
            {
                _warehouseCode = isTotal || selected ? string.Empty : warehouseName;
                await RefreshBasketAsync();
            };

            var card = new Border
            {
                Content = grid,
                BackgroundColor = selected ? Color.FromArgb("#16A34A") : (isTotal ? Color.FromArgb("#EFF6FF") : Color.FromArgb("#F8FAFC")),
                Stroke = selected ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = 10,
                StrokeShape = new RoundRectangle { CornerRadius = 14 }
            };
            card.GestureRecognizers.Add(tap);
            return card;
        }

        private decimal GetDashboardBasketNetCost(ShoppingCartWarehouseInformation warehouse, bool isTotal)
        {
            // Alignement exact ShoppingCartCheckout.aspx : la colonne T.panier affiche Eval("LocalCost").
            // Pour TOTAL PLATEFORMES, la ligne total retournée par le dashboard doit donc être utilisée
            // telle quelle ; fallback uniquement si le service ne renseigne pas LocalCost sur cette ligne.
            if (isTotal && warehouse.LocalCost == 0m && _dashboardBasketTotalNetCost != 0m)
                return _dashboardBasketTotalNetCost;

            return warehouse.LocalCost;
        }

        private static decimal GetDashboardPreparingNetCost(ShoppingCartWarehouseInformation warehouse, bool isTotal)
        {
            // Alignement exact ShoppingCartCheckout.aspx : la colonne En.préparation affiche
            // Eval("CurrentDisponibleNetCost"), y compris pour TOTAL PLATEFORMES.
            return warehouse.CurrentDisponibleNetCost;
        }

        private View SmallInfoPill(string text, bool selected)
        {
            return new Border
            {
                BackgroundColor = selected ? Color.FromArgb("#22C55E") : Color.FromArgb("#EAF6FF"),
                Padding = new Thickness(8, 4),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Label
                {
                    Text = text,
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = selected ? Colors.White : Color.FromArgb("#075985"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            };
        }

        private View BasketLineCard(OrderBasketLineSelection selection)
        {
            var line = selection.Line;
            var grid = new Grid
            {
                RowDefinitions = Rows("Auto,Auto,Auto,Auto"),
                ColumnDefinitions = Columns("*,Auto,Auto"),
                ColumnSpacing = 8,
                RowSpacing = 6
            };

            var productCodeLabel = new Label
            {
                Text = line.ProductCode,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.FromArgb("#0F172A"),
                LineBreakMode = LineBreakMode.CharacterWrap,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Add(productCodeLabel, 0, 0);
            Grid.SetColumnSpan(productCodeLabel, 2);

            var productLabel = new Label
            {
                Text = line.ProductLabel,
                FontSize = 13,
                TextColor = Color.FromArgb("#475569"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Add(productLabel, 0, 1);
            Grid.SetColumnSpan(productLabel, 2);

            var lineStatusGrid = new Grid
            {
                ColumnDefinitions = Columns("*,Auto,Auto"),
                ColumnSpacing = 4,
                VerticalOptions = LayoutOptions.Center
            };

            var localLabel = new Label
            {
                Text = line.WarehouseName ?? line.Warehouse,
                FontSize = 12,
                TextColor = Color.FromArgb("#64748B"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center
            };
            lineStatusGrid.Add(localLabel, 0, 0);
            lineStatusGrid.Add(StatusPill($"Dispo. : {line.Disponibility}", DisponibilityColor(line), "#0F172A", 92), 1, 0);
            lineStatusGrid.Add(StatusPill($"Retour : {ReturnText(line.ReturnAllowed)}", IsNo(line.ReturnAllowed) ? "#FEF3C7" : "#F8FAFC", "#92400E", 78), 2, 0);
            grid.Add(lineStatusGrid, 0, 2);
            Grid.SetColumnSpan(lineStatusGrid, 2);

            var price = StatusPill($"Prix : {DisplayValue(line.ProductPrice)}", "#EAF6FF", "#075985", 110);
            price.VerticalOptions = LayoutOptions.Center;
            grid.Add(price, 0, 3);

            var discount = StatusPill($"Remise : {DisplayValue(line.ProductDiscount)}", "#F8FAFC", "#475569", 110);
            discount.VerticalOptions = LayoutOptions.Center;
            grid.Add(discount, 1, 3);

            var quantity = CreateTextEntry("Qté");
            quantity.Text = selection.QuantityText;
            quantity.Keyboard = Keyboard.Numeric;
            quantity.FontSize = 14;
            quantity.TextChanged += (_, e) =>
            {
                selection.QuantityText = e.NewTextValue ?? string.Empty;
            };
            quantity.Completed += async (_, __) => await UpdateLineIfChangedAsync(selection, false);
            quantity.Unfocused += async (_, __) => await UpdateLineIfChangedAsync(selection, false);
            var quantityFrame = FramedEntry(quantity, 56, 42);
            quantityFrame.VerticalOptions = LayoutOptions.Fill;
            grid.Add(quantityFrame, 2, 0);
            Grid.SetRowSpan(quantityFrame, 2);

            var delete = IconDangerButton("ic_delete.png");
            delete.VerticalOptions = LayoutOptions.Fill;
            delete.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await DeleteLineAsync(selection)) });
            grid.Add(delete, 2, 2);
            Grid.SetRowSpan(delete, 2);

            return InnerCard(grid);
        }

        private async Task UpdateLineIfChangedAsync(OrderBasketLineSelection selection, bool showMessage = true)
        {
            string newQuantity = (selection.QuantityText ?? string.Empty).Trim();
            string oldQuantity = (selection.Line.Quantity ?? string.Empty).Trim();

            if (string.Equals(newQuantity, oldQuantity, StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(newQuantity))
            {
                await ShowBlockingMessageAsync("Panier", "Veuillez saisir une quantité.");
                return;
            }

            string lineKey = $"{selection.Line.ProductCode}|{selection.Line.Warehouse}";
            if (_lastSubmittedQuantityByLine.TryGetValue(lineKey, out string? lastSubmitted)
                && string.Equals(lastSubmitted, newQuantity, StringComparison.OrdinalIgnoreCase))
                return;

            string requestKey = $"{lineKey}|{newQuantity}";
            if (!_quantityUpdateKeysInProgress.Add(requestKey))
                return;

            try
            {
                await UpdateLineAsync(selection, showMessage);
                _lastSubmittedQuantityByLine[lineKey] = newQuantity;
            }
            finally
            {
                _quantityUpdateKeysInProgress.Remove(requestKey);
            }
        }

        private async Task UpdateLineAsync(OrderBasketLineSelection selection, bool showMessage = true)
        {
            await RunActionAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(selection.QuantityText))
                    throw new Exception("Veuillez saisir une quantité.");

                await EasySession.UpdateShoppingCartLineQuantityAsync(selection.Line.ProductCode, selection.Line.Warehouse, selection.QuantityText);
                await RefreshBasketCoreAsync();
                if (showMessage)
                    await ShowBlockingMessageAsync("Panier", "Quantité modifiée avec succès.");
            });
        }

        private async Task DeleteLineAsync(OrderBasketLineSelection selection)
        {
            bool confirm = await ShowConfirmationAsync("Suppression", "Supprimer cette ligne du panier ?", "Supprimer", "Annuler");
            if (!confirm)
                return;

            await RunActionAsync(async () =>
            {
                await EasySession.DeleteShoppingCartLineAsync(selection.Line.ShoppingCartLineId);
                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Panier", "Produit retiré du panier avec succès.");
            });
        }

        private void ShowDeliveryOptionsModal()
        {
            var body = new List<View>();
            var openAddress = PrimaryButton("Adresse de livraison");
            openAddress.IsEnabled = _canChangeDeliveryAddress;
            openAddress.Clicked += async (_, __) =>
            {
                HideModalOnly();
                await RunActionAsync(OpenDeliveryAddressAsync);
            };
            body.Add(openAddress);

            if (!_canChangeDeliveryAddress)
                body.Add(new Label { Text = "Adresse de livraison non modifiable pour ce compte.", FontSize = 12, TextColor = Color.FromArgb("#64748B") });

            body.Add(new Label { Text = "Numéro de lot", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") });
            DetachFromParent(_lotEntry);
            body.Add(FramedEntry(_lotEntry, null, 44));
            var lotRow = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8 };
            var applyLot = SecondaryButton("Appliquer lot");
            applyLot.Clicked += async (_, __) => await ApplyLotAsync();
            var clearLot = SecondaryButton("Effacer lot");
            clearLot.Clicked += async (_, __) => await ClearLotAsync();
            lotRow.Add(clearLot, 0, 0);
            lotRow.Add(applyLot, 1, 0);
            body.Add(lotRow);

            body.Add(new Label { Text = "Date de livraison", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") });
            DetachFromParent(_deliveryDatePicker);
            body.Add(FramedDatePicker(_deliveryDatePicker, null, 44));
            var dateRow = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8 };
            var clearDate = SecondaryButton("Effacer date");
            clearDate.Clicked += async (_, __) => await ClearDateAsync();
            var applyDate = SecondaryButton("Appliquer date");
            applyDate.Clicked += async (_, __) => await ApplyDateAsync();
            dateRow.Add(clearDate, 0, 0);
            dateRow.Add(applyDate, 1, 0);
            body.Add(dateRow);

            var buttons = new List<Button>();
            var cancel = SecondaryButton("Fermer");
            cancel.Clicked += (_, __) => HideModalOnly();
            buttons.Add(cancel);
            ShowCustomModal("Option de livraison", body, buttons);
        }


        private static void DetachFromParent(View view)
        {
            if (view.Parent is Border border && ReferenceEquals(border.Content, view))
                border.Content = null;
            else if (view.Parent is Layout layout)
                layout.Children.Remove(view);
            else if (view.Parent is ContentView contentView && ReferenceEquals(contentView.Content, view))
                contentView.Content = null;
        }

        private async Task OpenDeliveryAddressAsync()
        {
            _canChangeDeliveryAddress = await EasySession.AllowChangeDeliveryAddressAsync();
            if (!_canChangeDeliveryAddress)
            {
                await ShowBlockingMessageAsync("Adresse livraison", "Vous n'êtes pas autorisé à changer l'adresse de livraison.");
                return;
            }

            _preserveResponsesOnNextDisappearing = true;
            await Navigation.PushModalAsync(new NavigationPage(new OrderDeliveryAddressView()));
        }

        private async Task ApplyLotAsync()
        {
            if (!_isAdmin)
            {
                await ShowBlockingMessageAsync("Lot", "Le numéro de lot est réservé à un administrateur.");
                return;
            }

            await RunActionAsync(async () =>
            {
                await EasySession.SetLotsNumberAsync(_lotEntry.Text ?? string.Empty);
                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Lot", "Numéro de lot appliqué.");
            });
        }

        private async Task ClearLotAsync()
        {
            if (!_isAdmin)
                return;

            await RunActionAsync(async () =>
            {
                await EasySession.ClearLotsNumberAsync();
                _lotEntry.Text = string.Empty;
                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Lot", "Numéro de lot effacé.");
            });
        }

        private async Task ApplyDateAsync()
        {
            if (!_isAdmin)
            {
                await ShowBlockingMessageAsync("Date livraison", "La date de livraison est réservée à un administrateur.");
                return;
            }

            await RunActionAsync(async () =>
            {
                var picked = _deliveryDatePicker.Date;
                DateTime deliveryDate = picked is DateTime value ? value : DateTime.Today;
                await EasySession.SetDelivryDateAsync(deliveryDate);
                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Date livraison", "Date de livraison appliquée.");
            });
        }

        private async Task ClearDateAsync()
        {
            if (!_isAdmin)
                return;

            await RunActionAsync(async () =>
            {
                await EasySession.ClearDelivryDateAsync();
                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Date livraison", "Date de livraison effacée.");
            });
        }

        private async Task ApplyPromotionsAsync()
        {
            await RunActionAsync(async () =>
            {
                while (true)
                {
                    var bonusList = await EasySession.TreatePromotionWithPromotionalCodeAsync(_sorderEntry.Text ?? string.Empty);
                    if (bonusList == null || bonusList.Length == 0)
                        break;

                    string? selectedProductCode = await PromotionBonusView.SelectBonusAsync(Navigation, bonusList);
                    if (string.IsNullOrWhiteSpace(selectedProductCode))
                        break;

                    await EasySession.ApplicatePromotionAsync(selectedProductCode);
                }

                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Promotions", "Traitement des promotions terminé.");
            });
        }

        private async Task CancelPromotionAsync()
        {
            await RunActionAsync(async () =>
            {
                await EasySession.CancelPromotionAsync();
                await RefreshBasketCoreAsync();
                await ShowBlockingMessageAsync("Promotions", "Promotion annulée.");
            });
        }

        private async Task CheckOutAsync(string type)
        {
            bool confirm = await ShowConfirmationAsync("Validation", $"Valider le panier en commande {type} ?", "Valider", "Annuler");
            if (!confirm)
                return;

            await RunActionAsync(async () =>
            {
                var currentLines = await EasySession.GetOrderBasketLinesAsync(true, false, _warehouseCode ?? string.Empty);
                if (currentLines == null || currentLines.Length == 0)
                {
                    await ShowBlockingMessageAsync("Validation", "Ajoutez un ou plusieurs articles au panier d'abord.");
                    return;
                }

                ShoppingCartResponse[] responses = string.IsNullOrWhiteSpace(_warehouseCode)
                    ? await EasySession.WarehouseShoppingCartCheckOutAsync(type, _sorderEntry.Text ?? string.Empty)
                    : await EasySession.SelectedWarehouseShoppingCartCheckOutAsync(type, _sorderEntry.Text ?? string.Empty, _warehouseCode);

                AddResponses(responses ?? Array.Empty<ShoppingCartResponse>());
                if ((responses ?? Array.Empty<ShoppingCartResponse>()).Any(r => string.Equals(r.Status, "Accepted", StringComparison.OrdinalIgnoreCase) || string.Equals(r.Status, "OK", StringComparison.OrdinalIgnoreCase)))
                    _warehouseCode = string.Empty;
                await RefreshBasketCoreAsync();
            });
        }

        private void AddResponses(ShoppingCartResponse[] responses)
        {
            if (responses.Length > 0)
                _shoppingCartResponses.AddRange(responses);

            RebindResponses();
        }

        private void RebindResponses()
        {
            _responseList.Children.Clear();
            if (_shoppingCartResponses.Count == 0)
            {
                if (_responseCard != null)
                    _responseCard.IsVisible = false;
                return;
            }

            if (_responseCard != null)
                _responseCard.IsVisible = true;
            foreach (var response in _shoppingCartResponses)
                _responseList.Children.Add(ResponseCard(response));
        }

        private View ResponseCard(ShoppingCartResponse response)
        {
            var stack = new VerticalStackLayout { Spacing = 6 };
            stack.Children.Add(new Label { Text = $"{response.WarehouseCode} - {response.Status}", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") });
            if (!string.IsNullOrWhiteSpace(response.ErrorText))
                stack.Children.Add(new Label { Text = CleanServiceMessage(response.ErrorText ?? string.Empty), FontSize = 12, TextColor = Color.FromArgb("#475569") });
            if (!string.IsNullOrWhiteSpace(response.SorderCode))
                stack.Children.Add(new Label { Text = $"N° commande : {response.SorderCode}", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#16A34A") });
            if (!string.IsNullOrWhiteSpace(response.SorderCode))
            {
                var pdf = SecondaryButton("PDF");
                pdf.Clicked += async (_, __) => await OpenSorderPdfAsync(response);
                stack.Children.Add(pdf);
            }
            return InnerCard(stack);
        }

        private async Task OpenSorderPdfAsync(ShoppingCartResponse response)
        {
            await RunActionAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(response.SorderCode) || string.IsNullOrWhiteSpace(response.WarehouseCode))
                    throw new Exception("Impossible d'ouvrir le PDF : commande ou plateforme manquante.");

                byte[] bytes = await EasySession.GetSorderDocumentAsync(response.SorderCode, response.WarehouseCode, response.WarehouseCode);
                if (bytes == null || bytes.Length == 0)
                    throw new Exception("Le PDF de commande est vide.");

                string safeSorderCode = string.Concat((response.SorderCode ?? "Commande").Select(c => System.IO.Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                string fileName = $"Commande_{safeSorderCode}.pdf";
                string path = System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);
                System.IO.Directory.CreateDirectory(FileSystem.CacheDirectory);
                await System.IO.File.WriteAllBytesAsync(path, bytes);
                _preserveResponsesOnNextDisappearing = true;
                await Navigation.PushModalAsync(new PdfViewerPage(path, fileName));
            });
        }

        private async Task RunActionAsync(Func<Task> action)
        {
            SetLoadingState(true);
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                await ShowBlockingMessageAsync("Information", CleanServiceMessage(ex.Message));
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText)
        {
            _confirmationCompletion?.TrySetResult(false);
            _confirmationCompletion = new TaskCompletionSource<bool>();
            ShowModal(title, message, acceptText, cancelText);
            return _confirmationCompletion.Task;
        }

        private async Task ShowBlockingMessageAsync(string title, string message)
        {
            await ShowConfirmationAsync(title, message, "OK", string.Empty);
        }

        private void ShowModal(string title, string message, string acceptText, string cancelText)
        {
            _modalTitle.Text = title;
            _modalBody.Children.Clear();
            _modalBody.Children.Add(new Label { Text = message, FontSize = 14, TextColor = Color.FromArgb("#475569"), LineBreakMode = LineBreakMode.WordWrap });
            _modalButtons.Children.Clear();

            if (!string.IsNullOrWhiteSpace(cancelText))
            {
                var cancel = SecondaryButton(cancelText);
                cancel.Clicked += (_, __) => CloseModal(false);
                _modalButtons.Add(cancel, 0, 0);
            }

            var accept = PrimaryButton(acceptText);
            accept.Clicked += (_, __) => CloseModal(true);
            _modalButtons.Add(accept, string.IsNullOrWhiteSpace(cancelText) ? 0 : 1, 0);
            Grid.SetColumnSpan(accept, string.IsNullOrWhiteSpace(cancelText) ? 2 : 1);
            _modalOverlay.IsVisible = true;
        }

        private void ShowCustomModal(string title, List<View> bodyViews, List<Button> buttons)
        {
            _confirmationCompletion?.TrySetResult(false);
            _confirmationCompletion = null;
            _modalTitle.Text = title;
            _modalBody.Children.Clear();
            foreach (var view in bodyViews)
                _modalBody.Children.Add(view);
            _modalButtons.Children.Clear();

            if (buttons.Count == 0)
            {
                var cancel = SecondaryButton("Fermer");
                cancel.Clicked += (_, __) => HideModalOnly();
                buttons.Add(cancel);
            }

            for (int i = 0; i < buttons.Count && i < 2; i++)
                _modalButtons.Add(buttons[i], i, 0);
            if (buttons.Count == 1)
                Grid.SetColumnSpan(buttons[0], 2);
            _modalOverlay.IsVisible = true;
        }

        private void CloseModal(bool result)
        {
            _modalOverlay.IsVisible = false;
            _confirmationCompletion?.TrySetResult(result);
            _confirmationCompletion = null;
        }

        private void HideModalOnly()
        {
            _modalOverlay.IsVisible = false;
            _confirmationCompletion?.TrySetResult(false);
            _confirmationCompletion = null;
        }

        private void SetLoadingState(bool isLoading)
        {
            _apiLoadingOverlay.IsVisible = isLoading;
            _apiActivityIndicator.IsVisible = isLoading;
            _apiActivityIndicator.IsRunning = isLoading;
        }

        private void RegisterCutoffDisplay(ShoppingCartWarehouseInformation warehouse, Label label)
        {
            TimeSpan? value = ParseCutoff(warehouse.OrderCutoff);
            if (!value.HasValue)
                value = ParseCutoff(warehouse.DeliveryCutoff);
            if (!value.HasValue)
                return;
            _cutoffDisplays.Add(new CutoffDisplay(label, value.Value, DateTime.UtcNow));
        }

        private void StartCutoffTimerIfNeeded()
        {
            if (_cutoffTimerStarted)
                return;
            _cutoffTimerStarted = true;
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (_cutoffDisplays.Count == 0)
                {
                    _cutoffTimerStarted = false;
                    return false;
                }
                foreach (var item in _cutoffDisplays)
                {
                    var remaining = item.InitialValue - (DateTime.UtcNow - item.LoadedAt);
                    if (remaining < TimeSpan.Zero)
                        remaining = TimeSpan.Zero;
                    item.Label.Text = $"Cutoff : {FormatRemaining(remaining)}";
                }
                return true;
            });
        }

        private static string BuildInitialCutoffText(ShoppingCartWarehouseInformation warehouse)
        {
            TimeSpan? value = ParseCutoff(warehouse.OrderCutoff) ?? ParseCutoff(warehouse.DeliveryCutoff);
            return value.HasValue ? $"Cutoff : {FormatRemaining(value.Value)}" : "Cutoff : -";
        }

        private static TimeSpan? ParseCutoff(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            text = text.Trim();
            if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var span))
                return span;
            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var date))
            {
                var diff = date - DateTime.Now;
                return diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
            }
            return null;
        }

        private static string FormatRemaining(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
            return $"00:{time.Minutes:00}:{time.Seconds:00}";
        }

        private static string CleanServiceMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            string cleaned = message.Replace("System.Web.Services.Protocols.SoapException:", string.Empty).Trim();
            int index = cleaned.IndexOf(" à ", StringComparison.Ordinal);
            if (index > 0)
                cleaned = cleaned.Substring(0, index).Trim();
            index = cleaned.IndexOf("System.Exception:", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                cleaned = cleaned.Substring(index + "System.Exception:".Length).Trim();
            index = cleaned.IndexOf("--->", StringComparison.Ordinal);
            if (index >= 0)
                cleaned = cleaned.Substring(index + 4).Trim();
            return cleaned;
        }

        private static Label SectionTitle(string text)
        {
            return new Label { Text = text, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") };
        }

        private static Label EmptyLabel(string text)
        {
            return new Label { Text = text, FontSize = 13, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center };
        }

        private static Border Card(View content)
        {
            return new Border { Content = content, BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2E8F0"), StrokeThickness = 1, Padding = 12, StrokeShape = new RoundRectangle { CornerRadius = 18 } };
        }

        private static Border InnerCard(View content)
        {
            return new Border { Content = content, BackgroundColor = Color.FromArgb("#F8FAFC"), Stroke = Color.FromArgb("#E2E8F0"), StrokeThickness = 1, Padding = 10, StrokeShape = new RoundRectangle { CornerRadius = 14 } };
        }

        private static Border StatusPill(string text, string background, string foreground, double? width = null)
        {
            var border = new Border
            {
                BackgroundColor = Color.FromArgb(background),
                StrokeThickness = 0,
                Padding = new Thickness(8, 4),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Label
                {
                    Text = text,
                    FontSize = 10.5,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb(foreground),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.NoWrap,
                    MaxLines = 1
                }
            };
            if (width.HasValue)
                border.WidthRequest = width.Value;
            return border;
        }

        private static string DisponibilityColor(SorderBasketLine line)
        {
            decimal disponibility = ParseDecimal(line.Disponibility);
            decimal quantity = ParseDecimal(line.Quantity);
            if (disponibility <= 0)
                return "#FEE2E2";
            if (quantity > disponibility)
                return "#FED7AA";
            return "#DCFCE7";
        }

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            string text = value.Trim().Replace(" ", string.Empty).Replace("%", string.Empty);
            if (text.Contains(',') && !text.Contains('.'))
                text = text.Replace(',', '.');
            else if (text.Contains(',') && text.Contains('.'))
            {
                int comma = text.LastIndexOf(',');
                int dot = text.LastIndexOf('.');
                if (comma > dot)
                    text = text.Replace(".", string.Empty).Replace(',', '.');
                else
                    text = text.Replace(",", string.Empty);
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariant))
                return invariant;
            if (decimal.TryParse(value, NumberStyles.Any, FrenchCulture, out var french))
                return french;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var current))
                return current;
            return 0m;
        }

        private static string DisplayValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private static string ReturnText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private static bool IsYes(string? value)
        {
            return string.Equals(value, "Oui", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNo(string? value)
        {
            return string.Equals(value, "Non", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "False", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase);
        }

        private static Entry CreateTextEntry(string placeholder)
        {
            return new BorderlessEntry
            {
                Placeholder = placeholder,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.Transparent,
                HeightRequest = 38,
                FontSize = 15,
                TextColor = Color.FromArgb("#0F172A"),
                PlaceholderColor = Color.FromArgb("#94A3B8")
            };
        }

        private static Border FramedEntry(Entry entry, double? width = null, double height = 44)
        {
            entry.BackgroundColor = Colors.Transparent;
            entry.HeightRequest = height - 6;
            var border = new Border
            {
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                Padding = new Thickness(8, 2),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = height,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = entry
            };
            if (width.HasValue)
            {
                border.WidthRequest = width.Value;
                border.HorizontalOptions = LayoutOptions.Center;
            }
            return border;
        }

        private static Border FramedDatePicker(DatePicker picker, double? width = null, double height = 44)
        {
            picker.BackgroundColor = Colors.Transparent;
            picker.HeightRequest = height - 6;
            picker.VerticalOptions = LayoutOptions.Center;
            var border = new Border
            {
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                Padding = new Thickness(8, 2),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = height,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = picker
            };
            if (width.HasValue)
            {
                border.WidthRequest = width.Value;
                border.HorizontalOptions = LayoutOptions.Center;
            }
            return border;
        }

        private static Button PrimaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#16A34A"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44, Padding = new Thickness(10, 0) };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#E2E8F0"), TextColor = Color.FromArgb("#0F172A"), FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44, Padding = new Thickness(10, 0) };
        }

        private static Button CompactPrimaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#16A34A"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, FontSize = 11, CornerRadius = 12, HeightRequest = 38, Padding = new Thickness(4, 0) };
        }

        private static Button CompactSecondaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#E2E8F0"), TextColor = Color.FromArgb("#0F172A"), FontAttributes = FontAttributes.Bold, FontSize = 11, CornerRadius = 12, HeightRequest = 38, Padding = new Thickness(4, 0) };
        }

        private static Border IconDangerButton(string iconSource)
        {
            return new Border
            {
                BackgroundColor = Color.FromArgb("#DC2626"),
                StrokeThickness = 0,
                HeightRequest = 42,
                WidthRequest = 42,
                Padding = 10,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Image
                {
                    Source = iconSource,
                    Aspect = Aspect.AspectFit,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
        }

        private static Button DangerButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#DC2626"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 40, Padding = new Thickness(8, 0) };
        }

        private static ColumnDefinitionCollection Columns(string pattern)
        {
            var collection = new ColumnDefinitionCollection();
            foreach (var part in pattern.Split(','))
            {
                string value = part.Trim();
                if (value == "Auto")
                {
                    collection.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                else if (value.EndsWith("*", StringComparison.Ordinal)
                    && double.TryParse(value.TrimEnd('*'), NumberStyles.Any, CultureInfo.InvariantCulture, out double star)
                    && star > 0)
                {
                    collection.Add(new ColumnDefinition { Width = new GridLength(star, GridUnitType.Star) });
                }
                else
                {
                    collection.Add(new ColumnDefinition { Width = GridLength.Star });
                }
            }
            return collection;
        }

        private static RowDefinitionCollection Rows(string pattern)
        {
            var collection = new RowDefinitionCollection();
            foreach (var part in pattern.Split(','))
            {
                string value = part.Trim();
                collection.Add(new RowDefinition { Height = value == "Auto" ? GridLength.Auto : GridLength.Star });
            }
            return collection;
        }

        private sealed class CutoffDisplay
        {
            public CutoffDisplay(Label label, TimeSpan initialValue, DateTime loadedAt)
            {
                Label = label;
                InitialValue = initialValue;
                LoadedAt = loadedAt;
            }

            public Label Label { get; }
            public TimeSpan InitialValue { get; }
            public DateTime LoadedAt { get; }
        }
    }
}
