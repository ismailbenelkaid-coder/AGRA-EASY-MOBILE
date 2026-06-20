using AGRA_EASY_MOBILE.Models;
using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Services;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AGRA_EASY_MOBILE
{
    public class CatalogueView : ContentPage
    {
        private enum CatalogueStep
        {
            Home,
            ReferenceList,
            VehicleNavigation,
            ArticleList,
            ProductSheet
        }

        private enum CatalogueSearchMode
        {
            Reference,
            Registration
        }

        private readonly Entry _accountEntry = new BorderlessEntry { Placeholder = "Code client", TextColor = Color.FromArgb("#0F172A"), PlaceholderColor = Color.FromArgb("#94A3B8"), BackgroundColor = Colors.Transparent, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, ReturnType = ReturnType.Done };
        private readonly Label _accountLabel = new() { FontSize = 12, TextColor = Color.FromArgb("#64748B") };
        private readonly Entry _searchEntry = new BorderlessEntry { Placeholder = "Référence", TextColor = Color.FromArgb("#0F172A"), PlaceholderColor = Color.FromArgb("#94A3B8"), BackgroundColor = Colors.Transparent, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, ReturnType = ReturnType.Search };
        private readonly ImageButton _searchModeButton = new()
        {
            Source = "ic_search_product.png",
            BackgroundColor = Color.FromArgb("#E0F2FE"),
            CornerRadius = 12,
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = new Thickness(8),
            Aspect = Aspect.AspectFit,
            AutomationId = "CatalogueSearchMode"
        };
        private enum WarehouseSelectionMode
        {
            All,
            Proximity,
            Selected
        }

        private readonly CheckBox _showAllArticleCheck = new();
        private readonly CheckBox _showImagesCheck = new() { IsChecked = true };
        private readonly RadioButton _allWarehousesRadio = new() { GroupName = "WarehouseSelection", Content = "Toutes les plateformes", IsChecked = true };
        private readonly RadioButton _proximityWarehousesRadio = new() { GroupName = "WarehouseSelection", Content = "Uniquement proximité" };
        private readonly RadioButton _selectedWarehousesRadio = new() { GroupName = "WarehouseSelection", Content = "Plateformes sélectionnées" };
        private readonly Label _messageLabel = new() { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#B45309"), IsVisible = false };
        private readonly VerticalStackLayout _stepHost = new() { Spacing = 12 };
        private readonly ActivityIndicator _apiActivityIndicator = new() { IsRunning = false, Color = Color.FromArgb("#16A34A"), WidthRequest = 42, HeightRequest = 42 };
        private readonly Grid _apiLoadingOverlay = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#66000000") };
        private readonly Grid _modalOverlay = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#99000000") };
        private readonly Label _modalTitle = new() { FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") };
        private readonly VerticalStackLayout _modalBody = new() { Spacing = 10 };
        private readonly Button _modalCloseButton = new() { Text = "Fermer", BackgroundColor = Color.FromArgb("#16A34A"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 46 };
        private readonly Grid _imageOverlay = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#EE000000") };
        private readonly Image _fullScreenImage = new() { Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
        private readonly HorizontalStackLayout _imageThumbnailStrip = new() { Spacing = 8, Padding = new Thickness(10, 6) };
        private readonly Label _imageCounterLabel = new() { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
        private readonly List<string> _imageGallerySources = new();
        private int _imageGalleryIndex;
        private Button? _basketButton;
        private Border? _backButton;
        private Grid? _searchGrid;
        private Button? _searchButton;
        private Border? _searchEntryBox;
        private Border? _accountCard;

        private readonly List<CatalogArticle> _lastReferenceList = new();
        private readonly List<ArticlePriceAndStock> _lastArticleList = new();
        private readonly List<WarehouseOrderInput> _warehouseInputs = new();
        private CatalogueStep _currentStep = CatalogueStep.Home;
        private GenericArticle? _selectedGenericArticle;
        private Vehicule? _selectedVehicle;
        private VehicleSearchResult? _currentVehicleSearchResult;
        private readonly List<SearchStructureNode> _vehicleNavigationPath = new();
        private CatalogueSearchMode _searchMode = CatalogueSearchMode.Reference;
        private ArticlePriceAndStock? _selectedArticle;
        private ArticlePriceAndStock? _selectedArticleDetail;
        private string _selectedProductCode = string.Empty;
        private int _selectedQuantitative = 1;
        private bool _canEditAccount;
        private bool _hasDepotSupplier;
        private bool _initialized;
        private WarehouseSelectionMode _warehouseSelectionMode = WarehouseSelectionMode.All;
        private string _proximityWarehouse = string.Empty;
        private readonly List<Warehouse> _availableWarehouses = new();
        private readonly HashSet<string> _selectedWarehouseCodes = new(StringComparer.OrdinalIgnoreCase);

        public CatalogueView()
        {
            Title = "Catalogue";
            BackgroundColor = Color.FromArgb("#F6F8FB");
            BuildContent();
            UpdateSearchModeVisual();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            UpdateAccountSectionVisibility();

            if (!_initialized)
            {
                _initialized = true;
                await InitializePageAsync();
                return;
            }

            await RunServiceAsync(async () => await RefreshGlobalInformationAsync(), false);
        }

        private void BuildContent()
        {
            var root = new Grid();
            var scroll = new ScrollView();
            var content = new VerticalStackLayout { Padding = new Thickness(14, 12, 14, 24), Spacing = 14 };
            scroll.Content = content;
            root.Add(scroll);

            var backButton = BackButton();
            _backButton = backButton;
            backButton.IsVisible = false;
            backButton.HorizontalOptions = LayoutOptions.Start;

            var basketButton = PrimaryButton("Panier (0 Pcs)");
            _basketButton = basketButton;
            basketButton.Clicked += async (_, __) => await Shell.Current.GoToAsync("//orderBasket");
            var optionsButton = SecondaryButton("Options");
            optionsButton.Clicked += async (_, __) => await RunServiceAsync(async () =>
            {
                await EnsureWarehouseOptionsLoadedAsync();
                ShowOptionsModal();
            }, true);

            var headerActions = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions = Columns("*,Auto,Auto")
            };
            headerActions.Add(backButton, 0, 0);
            headerActions.Add(optionsButton, 1, 0);
            headerActions.Add(basketButton, 2, 0);
            content.Add(Header("", "", headerActions));

            var validateAccountButton = SecondaryButton("Choisir");
            validateAccountButton.Clicked += async (_, __) => await ShowClientSelectionForCatalogueAsync();
            _accountEntry.Completed += async (_, __) => await ShowClientSelectionForCatalogueAsync();
            var accountGrid = new Grid { ColumnSpacing = 8, ColumnDefinitions = Columns("*,Auto") };
            accountGrid.Add(EditBox(_accountEntry), 0, 0);
            accountGrid.Add(validateAccountButton, 1, 0);
            _accountCard = Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    SectionTitle("Client affecté au panier"),
                    accountGrid,
                    _accountLabel
                }
            });
            content.Add(_accountCard);

            var searchButton = PrimaryButton("Chercher");
            _searchButton = searchButton;
            searchButton.Clicked += async (_, __) => await SearchCatalogueAsync();
            _searchEntry.Completed += async (_, __) => await SearchCatalogueAsync();
            _searchModeButton.Clicked += (_, __) => ToggleSearchMode();
            _searchModeButton.IsVisible = false;
            var searchGrid = new Grid { ColumnSpacing = 8, ColumnDefinitions = Columns("*,Auto,Auto") };
            _searchGrid = searchGrid;
            var searchEntryBox = EditBox(_searchEntry);
            _searchEntryBox = searchEntryBox;
            searchGrid.Add(searchEntryBox, 0, 0);
            searchGrid.Add(_searchModeButton, 1, 0);
            searchGrid.Add(searchButton, 2, 0);
            content.Add(Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    SectionTitle("Recherche article"),
                    searchGrid,
                    _messageLabel
                }
            }));

            content.Add(_stepHost);

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

            _modalCloseButton.Clicked += (_, __) => HideModal();
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
                    WidthRequest = 320,
                    Children = { _modalTitle, _modalBody, _modalCloseButton }
                }
            });
            root.Add(_modalOverlay);

            var imageCloseButton = new Button
            {
                Text = "✕",
                BackgroundColor = Color.FromArgb("#E2E8F0"),
                TextColor = Color.FromArgb("#0F172A"),
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 22,
                WidthRequest = 44,
                HeightRequest = 44,
                Padding = 0
            };
            imageCloseButton.Clicked += (_, __) =>
            {
                _imageOverlay.IsVisible = false;
                _imageGallerySources.Clear();
                _imageThumbnailStrip.Children.Clear();
            };

            var previousImageButton = GalleryNavigationButton("‹");
            previousImageButton.Clicked += (_, __) => MoveImageGallery(-1);
            var nextImageButton = GalleryNavigationButton("›");
            nextImageButton.Clicked += (_, __) => MoveImageGallery(1);

            var imageArea = new Grid { ColumnDefinitions = Columns("Auto,*,Auto"), ColumnSpacing = 8 };
            imageArea.Add(previousImageButton, 0, 0);
            imageArea.Add(_fullScreenImage, 1, 0);
            imageArea.Add(nextImageButton, 2, 0);

            var galleryRoot = new Grid
            {
                RowDefinitions = Rows("*,Auto"),
                Padding = new Thickness(10, 18, 10, 18),
                RowSpacing = 10
            };
            galleryRoot.Add(imageArea, 0, 0);

            AddImageGallerySwipeGestures(_fullScreenImage);

            var thumbnailScroll = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                Content = _imageThumbnailStrip,
                HeightRequest = 82
            };
            AddImageGallerySwipeGestures(thumbnailScroll);
            AddImageGallerySwipeGestures(_imageThumbnailStrip);

            var thumbnailFooter = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    _imageCounterLabel,
                    thumbnailScroll
                }
            };
            galleryRoot.Add(thumbnailFooter, 0, 1);
            galleryRoot.Add(imageCloseButton, 0, 0);
            imageCloseButton.HorizontalOptions = LayoutOptions.End;
            imageCloseButton.VerticalOptions = LayoutOptions.Start;
            imageCloseButton.Margin = new Thickness(0, 0, 8, 0);
            _imageOverlay.Add(galleryRoot);
            root.Add(_imageOverlay);

            Content = root;
            ShowHomeStep();
        }

        private async Task InitializePageAsync()
        {
            await RunServiceAsync(async () =>
            {
                if (EasySession.IsAdministrator)
                {
                    if (!await EasySession.IsSalesConditionManagerAsync())
                        throw new Exception("L'utilisateur n'est pas autorisé à consulter les conditions de vente.");

                    _canEditAccount = true;
                }
                else if (EasySession.IsClient)
                {
                    _canEditAccount = false;
                    string login = Preferences.Default.Get("UserLogin", string.Empty);
                    string currentAccount = EasySession.CurrentAccount?.AccountCode ?? string.Empty;

                    if (!string.Equals(login, currentAccount, StringComparison.OrdinalIgnoreCase) && !await EasySession.IsClientOrderManagerAsync())
                        throw new Exception("L'utilisateur n'est pas autorisé à passer des commandes.");
                }

                UpdateSearchModeAvailability();
                UpdateAccountSectionVisibility();

                _accountEntry.IsReadOnly = !_canEditAccount;
                string account = await EasySession.GetOrderBasketAccountCodeAsync();
                if (string.IsNullOrWhiteSpace(account))
                    account = EasySession.CurrentAccount?.AccountCode ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(account))
                {
                    _accountEntry.Text = account;
                    await ValidateAccountAsync(account);
                }
                else
                {
                    _accountLabel.Text = "Aucun client affecté au panier.";
                }

                await RefreshGlobalInformationAsync();
            }, true);
        }

        private void UpdateAccountSectionVisibility()
        {
            if (_accountCard != null)
                _accountCard.IsVisible = !EasySession.IsClient;
        }

        private async Task ShowClientSelectionForCatalogueAsync()
        {
            if (!_canEditAccount)
                return;

            int unitCount = 0;
            await RunServiceAsync(async () =>
            {
                unitCount = await EasySession.GetShoppingCartUnitCountAsync();
            }, true);

            if (unitCount > 0)
            {
                await ShowBlockingMessageAsync(
                    "Changement impossible",
                    "Le panier contient déjà des lignes. Veuillez valider ou vider le panier avant de changer de client.");
                return;
            }

            ClientAccount? selectedAccount = await ClientAccountSelectionPage.ShowAsync(
                this,
                "Choisir un client",
                (_accountEntry.Text ?? string.Empty).Trim());

            if (selectedAccount == null)
                return;

            string accountCode = (selectedAccount.AccountCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(accountCode))
                return;

            await RunServiceAsync(async () =>
            {
                _accountEntry.Text = accountCode;
                await ValidateAccountAsync(accountCode);
                await RefreshGlobalInformationAsync();
                await ModernAlertService.ShowSuccessAsync("Client affecté au panier.");
            }, true);
        }

        private async Task ValidateAccountAsync(string accountCode)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
                throw new Exception("Sélectionner un client d'abord.");

            if (EasySession.IsClient && !string.Equals(EasySession.CurrentAccount?.AccountCode ?? string.Empty, accountCode, StringComparison.OrdinalIgnoreCase))
                throw new Exception("L'utilisateur n'est pas autorisé à changer son compte client.");

            ClientAccount? selectedAccount = null;
            if (EasySession.IsAdministrator)
                selectedAccount = await EasySession.SetOrderBasketAccountCodeAsync(accountCode);

            string assignedAccount = await EasySession.GetOrderBasketAccountCodeAsync();
            if (string.IsNullOrWhiteSpace(assignedAccount))
                assignedAccount = accountCode;

            string depotSupplier = await EasySession.GetDepotSupplierCodeAsync(assignedAccount);
            _hasDepotSupplier = !string.IsNullOrWhiteSpace(depotSupplier);
            _proximityWarehouse = EasySession.CurrentAccount?.Warehouse ?? string.Empty;
            ResetWarehouseSelectionMode();
            await EnsureWarehouseOptionsLoadedAsync();
            _accountEntry.Text = assignedAccount;
            _accountLabel.Text = await BuildAssignedAccountLabelAsync(assignedAccount, depotSupplier, selectedAccount);
        }

        private async Task<string> BuildAssignedAccountLabelAsync(string accountCode, string? depotSupplierCode, ClientAccount? selectedAccount)
        {
            string accountDisplay = await BuildAssignedAccountDisplayAsync(accountCode, selectedAccount);
            return string.IsNullOrWhiteSpace(depotSupplierCode)
                ? $"Client affecté : {accountDisplay}"
                : $"Client affecté : {accountDisplay} / fournisseur dépôt : {depotSupplierCode}";
        }

        private async Task<string> BuildAssignedAccountDisplayAsync(string accountCode, ClientAccount? selectedAccount = null)
        {
            string fallback = accountCode ?? string.Empty;

            if (selectedAccount != null)
            {
                string name = (selectedAccount.AccountName ?? string.Empty).Trim();
                string postalCode = (selectedAccount.Zip ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(name))
                    return string.IsNullOrWhiteSpace(postalCode) ? name : $"{name} ({postalCode})";
            }

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
                // Si les informations de nom/CP ne sont pas disponibles, conserver le code déjà validé.
            }

            return fallback;
        }

        private async Task SearchCatalogueAsync()
        {
            if (_searchMode == CatalogueSearchMode.Registration)
            {
                await SearchRegistrationAsync();
                return;
            }

            await SearchReferenceAsync();
        }

        private async Task SearchReferenceAsync()
        {
            await RunServiceAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(_accountEntry.Text))
                    throw new Exception("Sélectionner un client d'abord.");

                string filter = (_searchEntry.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(filter))
                    throw new Exception("Renseignez une référence à rechercher.");

                _selectedGenericArticle = null;
                _selectedArticle = null;
                _selectedArticleDetail = null;
                _selectedProductCode = string.Empty;
                _selectedQuantitative = 1;
                _lastReferenceList.Clear();
                _lastArticleList.Clear();
                _warehouseInputs.Clear();
                _selectedVehicle = null;
                _currentVehicleSearchResult = null;
                _vehicleNavigationPath.Clear();

                var references = await EasySession.FindArticleListAsync(filter, false) ?? Array.Empty<CatalogArticle>();
                _lastReferenceList.AddRange(references);

                if (references.Length == 0)
                {
                    ShowReferencesStep();
                    await ShowBlockingMessageAsync("Recherche", "Aucune référence trouvée.");
                    return;
                }

                if (references.Length == 1)
                {
                    await LoadArticleListAsync(references[0]);
                    return;
                }

                ShowReferencesStep();
            }, true);
        }

        private void ToggleSearchMode()
        {
            if (!EasySession.IsAdministrator)
                return;

            _searchMode = _searchMode == CatalogueSearchMode.Reference
                ? CatalogueSearchMode.Registration
                : CatalogueSearchMode.Reference;
            UpdateSearchModeVisual();
            ShowHomeStep();
        }

        private void UpdateSearchModeVisual()
        {
            if (_searchMode == CatalogueSearchMode.Registration)
            {
                _searchEntry.Placeholder = "Immatriculation";
                _searchModeButton.Source = "ic_article_vehicles.png";
                return;
            }

            _searchEntry.Placeholder = "Référence";
            _searchModeButton.Source = "ic_search_product.png";
        }

        private void UpdateSearchModeAvailability()
        {
            bool canUseRegistration = EasySession.IsAdministrator;
            if (!canUseRegistration && _searchMode != CatalogueSearchMode.Reference)
                _searchMode = CatalogueSearchMode.Reference;

            _searchModeButton.IsVisible = canUseRegistration;
            UpdateSearchModeVisual();

            if (_searchGrid == null || _searchButton == null || _searchEntryBox == null)
                return;

            _searchGrid.Children.Clear();
            _searchGrid.ColumnDefinitions.Clear();

            if (canUseRegistration)
            {
                _searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                _searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _searchGrid.Add(_searchEntryBox, 0, 0);
                _searchGrid.Add(_searchModeButton, 1, 0);
                _searchGrid.Add(_searchButton, 2, 0);
            }
            else
            {
                _searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                _searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _searchGrid.Add(_searchEntryBox, 0, 0);
                _searchGrid.Add(_searchButton, 1, 0);
            }
        }

        private async Task SearchRegistrationAsync()
        {
            await RunServiceAsync(async () =>
            {
                if (!EasySession.IsAdministrator)
                    throw new Exception("La recherche par immatriculation est réservée aux administrateurs.");

                if (string.IsNullOrWhiteSpace(_accountEntry.Text))
                    throw new Exception("Sélectionner un client d'abord.");

                string registration = (_searchEntry.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(registration))
                    throw new Exception("Renseignez une immatriculation à rechercher.");

                _selectedGenericArticle = null;
                _selectedArticle = null;
                _selectedArticleDetail = null;
                _selectedProductCode = string.Empty;
                _selectedQuantitative = 1;
                _lastReferenceList.Clear();
                _lastArticleList.Clear();
                _warehouseInputs.Clear();
                _currentVehicleSearchResult = null;
                _vehicleNavigationPath.Clear();

                _selectedVehicle = await EasySession.GetVehiculeFromImmatriculationAsync(registration);
                if (_selectedVehicle == null || string.IsNullOrWhiteSpace(_selectedVehicle.k_type))
                    throw new Exception("Véhicule introuvable.");

                if (!string.IsNullOrWhiteSpace(_selectedVehicle.Immatriculation))
                    _searchEntry.Text = _selectedVehicle.Immatriculation;

                await LoadVehicleNodeAsync(_selectedVehicle.k_type, string.Empty);
            }, true);
        }

        private async Task LoadVehicleNodeAsync(string kTypeNo, string treeNodeId, IReadOnlyList<SearchStructureNode>? navigationPath = null)
        {
            if (string.IsNullOrWhiteSpace(kTypeNo))
                throw new Exception("Véhicule introuvable.");

            var vehicleResult = await EasySession.GetKTypeAlternativeAsync(kTypeNo, treeNodeId ?? string.Empty);
            if (vehicleResult == null)
                throw new Exception("Aucune famille de pièces trouvée pour ce véhicule.");

            _currentVehicleSearchResult = vehicleResult;
            if (navigationPath != null)
            {
                _vehicleNavigationPath.Clear();
                _vehicleNavigationPath.AddRange(navigationPath);
            }
            else
            {
                SynchronizeVehicleNavigationPath(vehicleResult);
            }

            var articleList = vehicleResult.ArticleList ?? Array.Empty<ArticlePriceAndStock>();
            if (articleList.Length > 0)
            {
                _lastArticleList.Clear();
                _lastArticleList.AddRange(FilterAndOrderVehicleArticles(articleList));
                ShowArticleListStep();
                return;
            }

            ShowVehicleNavigationStep();
        }

        private List<ArticlePriceAndStock> FilterAndOrderVehicleArticles(ArticlePriceAndStock[] articles)
        {
            var result = new List<ArticlePriceAndStock>();
            var activeWarehouseFilter = GetActiveWarehouseFilter();
            bool applyWarehouseFilter = _warehouseSelectionMode != WarehouseSelectionMode.All;

            foreach (var article in articles)
            {
                if (applyWarehouseFilter)
                    ApplyWarehouseFilterToArticle(article, activeWarehouseFilter);

                result.Add(article);
            }

            return result;
        }

        private void ShowHomeStep()
        {
            _currentStep = CatalogueStep.Home;
            UpdateBackButton();
            _stepHost.Children.Clear();
            _stepHost.Children.Add(Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    SectionTitle(_searchMode == CatalogueSearchMode.Registration ? "Recherche par immatriculation" : "Recherche pièce"),
                    new Label
                    {
                        Text = _searchMode == CatalogueSearchMode.Registration
                            ? "Saisissez une immatriculation pour identifier le véhicule, naviguer dans les familles de pièces, puis afficher les articles compatibles."
                            : "Saisissez une référence puis lancez la recherche. Les étapes s'afficheront l'une après l'autre comme dans EASY.",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#475569")
                    }
                }
            }));
        }

        private void UpdateBackButton()
        {
            if (_backButton != null)
                _backButton.IsVisible = _currentStep != CatalogueStep.Home;
        }

        private async Task GoBackOneStepAsync()
        {
            if (_currentStep == CatalogueStep.ProductSheet)
            {
                ShowArticleListStep();
                return;
            }

            if (_currentStep == CatalogueStep.ArticleList)
            {
                if (_searchMode == CatalogueSearchMode.Registration && _currentVehicleSearchResult != null)
                {
                    await NavigateVehicleBackAsync();
                    return;
                }

                if (_lastReferenceList.Count > 1)
                    ShowReferencesStep();
                else
                    ShowHomeStep();
                return;
            }

            if (_currentStep == CatalogueStep.VehicleNavigation)
            {
                if (_searchMode == CatalogueSearchMode.Registration && _currentVehicleSearchResult != null && !string.IsNullOrWhiteSpace(_currentVehicleSearchResult.TreeNodeId))
                {
                    await NavigateVehicleBackAsync();
                    return;
                }

                ShowHomeStep();
                return;
            }

            if (_currentStep == CatalogueStep.ReferenceList)
                ShowHomeStep();
        }

        private async Task NavigateVehicleBackAsync()
        {
            if (_currentVehicleSearchResult == null)
            {
                ShowHomeStep();
                return;
            }

            string kTypeNo = _currentVehicleSearchResult.KTypNo ?? _selectedVehicle?.k_type ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kTypeNo))
            {
                ShowHomeStep();
                return;
            }

            if (_vehicleNavigationPath.Count == 0)
            {
                ShowHomeStep();
                return;
            }

            var targetPath = _vehicleNavigationPath.Take(Math.Max(0, _vehicleNavigationPath.Count - 1)).ToList();
            string parentNodeId = targetPath.LastOrDefault()?.TreeNodeId ?? string.Empty;
            await RunServiceAsync(async () => await LoadVehicleNodeAsync(kTypeNo, parentNodeId, targetPath), true);
        }

        private async Task NavigateVehicleNodeAsync(SearchStructureNode node, string? kTypeNo, bool isParent)
        {
            string effectiveKType = kTypeNo ?? _selectedVehicle?.k_type ?? string.Empty;
            string targetNodeId = node.TreeNodeId ?? string.Empty;

            var targetPath = BuildNavigationPathForNode(node, isParent);
            await RunServiceAsync(async () => await LoadVehicleNodeAsync(effectiveKType, targetNodeId, targetPath), true);
        }

        private List<SearchStructureNode> BuildNavigationPathForNode(SearchStructureNode node, bool isParent)
        {
            string targetNodeId = node.TreeNodeId ?? string.Empty;
            var targetPath = new List<SearchStructureNode>(_vehicleNavigationPath);

            if (isParent)
            {
                int index = targetPath.FindLastIndex(n => string.Equals(n.TreeNodeId ?? string.Empty, targetNodeId, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                    return targetPath.Take(index + 1).ToList();
            }

            if (!string.IsNullOrWhiteSpace(targetNodeId))
            {
                int existingIndex = targetPath.FindLastIndex(n => string.Equals(n.TreeNodeId ?? string.Empty, targetNodeId, StringComparison.OrdinalIgnoreCase));
                if (existingIndex >= 0)
                    return targetPath.Take(existingIndex + 1).ToList();
            }

            targetPath.Add(node);
            return targetPath;
        }

        private void SynchronizeVehicleNavigationPath(VehicleSearchResult result)
        {
            var parentNodes = (result.ParentNodeList ?? Array.Empty<SearchStructureNode>())
                .Where(n => !string.IsNullOrWhiteSpace(n.TreeNodeId))
                .ToList();

            if (parentNodes.Count > 0)
            {
                _vehicleNavigationPath.Clear();
                _vehicleNavigationPath.AddRange(parentNodes);
            }
            else if (string.IsNullOrWhiteSpace(result.TreeNodeId))
            {
                _vehicleNavigationPath.Clear();
            }
        }

        private void ShowVehicleNavigationStep()
        {
            _currentStep = CatalogueStep.VehicleNavigation;
            UpdateBackButton();
            _stepHost.Children.Clear();
            _stepHost.Children.Add(VehicleNavigationCard());
        }

        private View VehicleNavigationCard()
        {
            return Card(new VerticalStackLayout
            {
                Spacing = 10,
                Children = { StepHeader("Recherche par immatriculation"), BuildVehicleNavigationContent() }
            });
        }

        private View BuildVehicleNavigationContent()
        {
            var root = new VerticalStackLayout { Spacing = 12 };
            root.Children.Add(VehicleSummaryCard());

            var currentResult = _currentVehicleSearchResult;
            if (currentResult == null)
            {
                root.Children.Add(EmptyLabel("Aucune famille de pièces à afficher."));
                return root;
            }

            if (!string.IsNullOrWhiteSpace(currentResult.TreeNodeId) || _vehicleNavigationPath.Count > 0)
            {
                var parentStack = new VerticalStackLayout { Spacing = 8 };
                parentStack.Children.Add(ArticleGroupHeader("Navigation"));
                parentStack.Children.Add(VehicleRootNodeCard(currentResult.KTypNo));
                for (int i = 0; i < _vehicleNavigationPath.Count; i++)
                {
                    var parentNode = _vehicleNavigationPath[i];
                    bool isCurrentNode = i == _vehicleNavigationPath.Count - 1;
                    parentStack.Children.Add(VehicleNodeCard(parentNode, currentResult.KTypNo, true, isCurrentNode));
                }
                root.Children.Add(parentStack);
            }

            var childNodes = currentResult.ChildrenNodeList ?? Array.Empty<SearchStructureNode>();
            if (childNodes.Length > 0)
            {
                var childrenStack = new VerticalStackLayout { Spacing = 8 };
                childrenStack.Children.Add(ArticleGroupHeader("Familles de pièces"));
                foreach (var childNode in childNodes)
                    childrenStack.Children.Add(VehicleNodeCard(childNode, currentResult.KTypNo, false));
                root.Children.Add(childrenStack);
            }
            else if (!string.IsNullOrWhiteSpace(currentResult.TreeNodeId) || _vehicleNavigationPath.Count > 0)
            {
                var endOfPathStack = new VerticalStackLayout { Spacing = 8 };
                endOfPathStack.Children.Add(ArticleGroupHeader("Articles et conditions de vente"));
                endOfPathStack.Children.Add(EmptyLabel("Aucun article à sélectionner."));
                root.Children.Add(endOfPathStack);
            }
            return root;
        }

        private View VehicleSummaryCard()
        {
            var vehicle = _selectedVehicle;
            if (vehicle == null)
                return EmptyLabel("Véhicule non identifié.");

            string title = string.Join(" ", new[] { vehicle.marque, vehicle.modele }.Where(v => !string.IsNullOrWhiteSpace(v))).Trim();
            if (string.IsNullOrWhiteSpace(title))
                title = "Véhicule identifié";

            var details = new VerticalStackLayout { Spacing = 4 };
            details.Children.Add(new Label
            {
                Text = title,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0F172A"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            string version = string.Join(" - ", new[] { vehicle.version, vehicle.energie, vehicle.date1erCir }.Where(v => !string.IsNullOrWhiteSpace(v))).Trim();
            if (!string.IsNullOrWhiteSpace(version))
                details.Children.Add(new Label { Text = version, FontSize = 13, TextColor = Color.FromArgb("#475569"), LineBreakMode = LineBreakMode.WordWrap });

            details.Children.Add(new Label
            {
                Text = $"Immatriculation : {vehicle.Immatriculation ?? (_searchEntry.Text ?? string.Empty)}",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#16A34A"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            if (!string.IsNullOrWhiteSpace(vehicle.k_type))
                details.Children.Add(new Label { Text = $"KType : {vehicle.k_type}", FontSize = 12, TextColor = Color.FromArgb("#64748B") });

            return InnerCard(details);
        }

        private View VehicleRootNodeCard(string? kTypeNo)
        {
            var grid = new Grid { ColumnDefinitions = Columns("Auto,*"), ColumnSpacing = 10 };
            grid.Add(new Label
            {
                Text = "↩",
                FontSize = 20,
                TextColor = Color.FromArgb("#DC2626"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 34
            }, 0, 0);
            grid.Add(new Label
            {
                Text = "Racine",
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#DC2626"),
                VerticalOptions = LayoutOptions.Center
            }, 1, 0);

            var card = CompactInnerCard(grid);
            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await RunServiceAsync(async () => await LoadVehicleNodeAsync(kTypeNo ?? _selectedVehicle?.k_type ?? string.Empty, string.Empty, Array.Empty<SearchStructureNode>()), true))
            });
            return card;
        }

        private View VehicleNodeCard(SearchStructureNode node, string? kTypeNo, bool isParent, bool isCurrentNode = false)
        {
            var grid = new Grid { ColumnDefinitions = Columns("Auto,*"), ColumnSpacing = 10 };
            grid.Add(VehicleNodeIcon(node, isParent, isCurrentNode), 0, 0);
            grid.Add(new Label
            {
                Text = node.Description ?? string.Empty,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = isCurrentNode
                    ? Color.FromArgb("#15803D")
                    : isParent ? Color.FromArgb("#DC2626") : Color.FromArgb("#0F172A"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Center
            }, 1, 0);

            var card = CompactInnerCard(grid);
            if (isCurrentNode)
            {
                card.BackgroundColor = Color.FromArgb("#ECFDF5");
                card.Stroke = Color.FromArgb("#BBF7D0");
            }
            else
            {
                card.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => await NavigateVehicleNodeAsync(node, kTypeNo, isParent))
                });
            }
            return card;
        }

        private View VehicleNodeIcon(SearchStructureNode node, bool isParent, bool isCurrentNode = false)
        {
            string iconPath = FirstNonEmpty(node.IconPath, node.IconName);
            if (!string.IsNullOrWhiteSpace(iconPath))
            {
                return new Border
                {
                    BackgroundColor = Color.FromArgb("#E0F2FE"),
                    StrokeThickness = 0,
                    WidthRequest = 38,
                    HeightRequest = 34,
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Content = new Image
                    {
                        Source = BuildEasyAdminUrl(iconPath, iconPath),
                        WidthRequest = 24,
                        HeightRequest = 24,
                        Aspect = Aspect.AspectFit,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };
            }

            return new Border
            {
                BackgroundColor = isCurrentNode
                    ? Color.FromArgb("#DCFCE7")
                    : isParent ? Color.FromArgb("#FEF2F2") : Color.FromArgb("#EFF6FF"),
                StrokeThickness = 0,
                WidthRequest = 38,
                HeightRequest = 34,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Label
                {
                    Text = isCurrentNode ? "✓" : isParent ? "↩" : "⚙",
                    FontSize = isCurrentNode ? 16 : isParent ? 18 : 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = isCurrentNode
                        ? Color.FromArgb("#16A34A")
                        : isParent ? Color.FromArgb("#DC2626") : Color.FromArgb("#2563EB"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private void ShowReferencesStep()
        {
            _currentStep = CatalogueStep.ReferenceList;
            UpdateBackButton();
            _stepHost.Children.Clear();
            var list = new VerticalStackLayout { Spacing = 8 };

            if (_lastReferenceList.Count == 0)
                list.Children.Add(EmptyLabel("Aucune référence catalogue."));
            else
            {
                foreach (var reference in _lastReferenceList)
                    list.Children.Add(ReferenceCard(reference));
            }

            _stepHost.Children.Add(Card(new VerticalStackLayout
            {
                Spacing = 10,
                Children = { StepHeader("Choix de catalogue"), list }
            }));
        }

        private View ReferenceCard(CatalogArticle reference)
        {
            var grid = new Grid
            {
                ColumnSpacing = 12,
                RowSpacing = 4,
                ColumnDefinitions = Columns("Auto,*")
            };

            var imageStack = new VerticalStackLayout
            {
                Spacing = 4,
                WidthRequest = 76,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            imageStack.Children.Add(ClickableArticleImage(BuildArticleImageUrl(reference.ArticleImage), 72, 52,
                async () => await ShowArticleImageGalleryAsync(reference.BrandNo, reference.ArtNo, BuildArticleImageUrl(reference.ArticleImage))));
            imageStack.Children.Add(new Image
            {
                Source = BuildBrandLogoUrl(reference.BrandLogo),
                Aspect = Aspect.AspectFit,
                WidthRequest = 72,
                HeightRequest = 24
            });
            grid.Add(imageStack, 0, 0);

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb("#E0F2FE"),
                Padding = new Thickness(8, 3),
                HorizontalOptions = LayoutOptions.Start,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Content = new Label
                {
                    Text = DisplayCatalog(reference.Catalog),
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#075985")
                }
            };

            var info = new VerticalStackLayout { Spacing = 4 };
            info.Children.Add(new Label
            {
                Text = reference.ShortProductCode,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                TextColor = Color.FromArgb("#0F172A"),
                LineBreakMode = LineBreakMode.TailTruncation
            });
            info.Children.Add(new Label
            {
                Text = ReferenceLabel(reference),
                FontSize = 13,
                TextColor = Color.FromArgb("#64748B"),
                LineBreakMode = LineBreakMode.WordWrap
            });

            var supplierLine = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions = Columns("Auto,*")
            };
            supplierLine.Add(badge, 0, 0);
            supplierLine.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(reference.SupplierName) ? (reference.BrandName ?? string.Empty) : $"{reference.SupplierCode} - {reference.SupplierName}",
                FontSize = 12,
                TextColor = Color.FromArgb("#475569"),
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 2,
                VerticalOptions = LayoutOptions.Center
            }, 1, 0);
            info.Children.Add(supplierLine);
            info.Children.Add(new Label
            {
                Text = "Toucher la carte pour sélectionner ›",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#16A34A"),
                HorizontalTextAlignment = TextAlignment.End
            });
            grid.Add(info, 1, 0);

            var card = InnerCard(grid);
            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await RunServiceAsync(async () => await LoadArticleListAsync(reference), true))
            });

            return card;
        }

        private async Task LoadArticleListAsync(CatalogArticle reference)
        {
            await LoadArticleListAsync(new GenericArticle
            {
                ShortProductCode = reference.ShortProductCode,
                SupplierCode = reference.SupplierCode,
                Catalog = reference.Catalog
            });
        }

        private void ShowArticleListStep()
        {
            _currentStep = CatalogueStep.ArticleList;
            UpdateBackButton();
            _stepHost.Children.Clear();
            if (_searchMode == CatalogueSearchMode.Registration && _currentVehicleSearchResult != null)
                _stepHost.Children.Add(VehicleNavigationCard());

            var root = new VerticalStackLayout { Spacing = 12 };
            AddArticleGroup(root, "Article stocké ou géré en stock", _lastArticleList.Where(IsStockedGroup).ToList());
            AddArticleGroup(root, "Correspondances gérées/stockées sur autre DROP", _lastArticleList.Where(IsExcludedGroup).ToList());
            AddArticleGroup(root, "Catalogue drop-shipping (Livraison directe fournisseur)", _lastArticleList.Where(IsDropshippingGroup).ToList());

            var otherArticles = _lastArticleList.Where(a => !IsStockedGroup(a) && !IsExcludedGroup(a) && !IsDropshippingGroup(a)).ToList();
            if (_showAllArticleCheck.IsChecked)
                AddArticleGroup(root, "Autres articles", otherArticles);

            if (root.Children.Count == 0)
            {
                bool hasVisibleVehicleFamilies = _searchMode == CatalogueSearchMode.Registration
                    && (_currentVehicleSearchResult?.ChildrenNodeList?.Length ?? 0) > 0;

                if (_searchMode != CatalogueSearchMode.Registration)
                {
                    _stepHost.Children.Add(Card(new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children = { EmptyLabel("Aucun article à afficher.") }
                    }));
                }
                else if (!hasVisibleVehicleFamilies)
                {
                    _stepHost.Children.Add(Card(new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children = { StepHeader("Articles et conditions de vente"), EmptyLabel("Aucun article à afficher.") }
                    }));
                }
                return;
            }

            _stepHost.Children.Add(Card(new VerticalStackLayout
            {
                Spacing = 10,
                Children = { StepHeader("Articles et conditions de vente"), root }
            }));
        }

        private void AddArticleGroup(VerticalStackLayout parent, string title, List<ArticlePriceAndStock> articles)
        {
            if (articles.Count == 0)
                return;

            var group = new VerticalStackLayout { Spacing = 8 };
            group.Children.Add(ArticleGroupHeader(title));
            foreach (var article in articles)
                group.Children.Add(ArticleCard(article));
            parent.Children.Add(group);
        }

        private View ArticleCard(ArticlePriceAndStock article)
        {
            var grid = new Grid
            {
                ColumnSpacing = 12,
                RowSpacing = 6,
                ColumnDefinitions = Columns("Auto,*")
            };

            var imageStack = new VerticalStackLayout
            {
                Spacing = 4,
                WidthRequest = 82,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            imageStack.Children.Add(ClickableArticleImage(BuildArticleImageUrl(article.ArticleImage), 78, 60,
                async () => await ShowArticleImageGalleryAsync(article.BrandNo, article.ArtNo, BuildArticleImageUrl(article.ArticleImage))));
            imageStack.Children.Add(new Image
            {
                Source = BuildBrandLogoUrl(article.BrandLogo),
                Aspect = Aspect.AspectFit,
                WidthRequest = 78,
                HeightRequest = 28
            });
            grid.Add(imageStack, 0, 0);

            var info = new VerticalStackLayout { Spacing = 5 };
            var header = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions = Columns("*,Auto,Auto")
            };
            header.Add(new Label
            {
                Text = ShortProductCode(article.ProductCode),
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = article.SearchedArticle ? Color.FromArgb("#DC2626") : Color.FromArgb("#0F172A"),
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            }, 0, 0);

            int actionColumn = 1;
            if (CanShowSupplierButton(article))
            {
                var supplierIcon = IconActionButton("🚚", "Fournisseur");
                supplierIcon.Clicked += async (_, __) => await RunServiceAsync(async () => await ShowSupplierCommandModalAsync(article), true);
                header.Add(supplierIcon, actionColumn++, 0);
            }

            var basketIcon = IconActionButton("🛒", "Ajouter au panier");
            basketIcon.Clicked += async (_, __) => await RunServiceAsync(async () =>
            {
                await PrepareSelectedArticleForBasketAsync(article, 1);
                ShowAddToBasketModal();
            }, true);
            header.Add(basketIcon, actionColumn, 0);
            info.Children.Add(header);

            info.Children.Add(new Label { Text = article.SupplierName ?? string.Empty, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#475569") });
            info.Children.Add(new Label { Text = article.ProductDescription ?? string.Empty, FontSize = 13, TextColor = Color.FromArgb("#475569"), LineBreakMode = LineBreakMode.WordWrap });
            if (!string.IsNullOrWhiteSpace(article.ReplacementProductCode))
                info.Children.Add(new Label { Text = $"Rempl. : {ShortProductCode(article.ReplacementProductCode)}", FontSize = 12, TextColor = Color.FromArgb("#64748B") });

            AddArticleFeeLabels(info, article);

            var actionLine = new Grid { ColumnSpacing = 10, ColumnDefinitions = Columns("*,*") };
            actionLine.Add(ActionPill($"Stock : {article.Disponibility}", async () =>
            {
                ShowDisponibilityModal(article.WarehouseDisponibilityList ?? Array.Empty<WarehouseDisponibility>());
                await Task.CompletedTask;
            }), 0, 0);
            actionLine.Add(ActionPill($"P.NET : {FormatNetPrice(article)}", async () =>
            {
                ShowQuantitativeModal(article, true);
                await Task.CompletedTask;
            }), 1, 0);
            info.Children.Add(actionLine);
            info.Children.Add(BuildArticleInfoActions(article));

            grid.Add(info, 1, 0);
            return InnerCard(grid);
        }

        private async Task PrepareSelectedArticleForBasketAsync(ArticlePriceAndStock article, int quantitative)
        {
            if (string.IsNullOrWhiteSpace(article.ProductCode) || article.ProductCode.Length < 4)
                throw new Exception("Code article invalide.");

            _selectedArticle = article;
            _selectedProductCode = article.ProductCode;
            _selectedQuantitative = Math.Max(1, quantitative);

            string supplier = article.ProductCode.Substring(0, 3);
            string shortCode = article.ProductCode.Substring(3);
            _selectedArticleDetail = _hasDepotSupplier
                ? await EasySession.GetLocalConditionForArticleAsync(shortCode, supplier, "AGRA", _selectedQuantitative)
                : await EasySession.GetAllConditionForArticleAsync(shortCode, supplier, "AGRA", _selectedQuantitative);

            _warehouseInputs.Clear();
            foreach (var warehouse in _selectedArticleDetail.WarehouseDisponibilityList ?? Array.Empty<WarehouseDisponibility>())
                _warehouseInputs.Add(new WarehouseOrderInput(warehouse));
        }

        private async Task LoadProductSheetAsync(ArticlePriceAndStock article, int quantitative)
        {
            await PrepareSelectedArticleForBasketAsync(article, quantitative);
            ShowAddToBasketModal();
        }

        private void ShowProductSheetStep()
        {
            _currentStep = CatalogueStep.ProductSheet;
            UpdateBackButton();
            _stepHost.Children.Clear();

            if (_selectedArticleDetail == null)
            {
                _stepHost.Children.Add(EmptyLabel("Aucun article sélectionné."));
                return;
            }

            var root = new VerticalStackLayout { Spacing = 12 };
            var top = new Grid { ColumnDefinitions = Columns("Auto,*"), ColumnSpacing = 18 };

            var imageStack = new VerticalStackLayout { Spacing = 7, WidthRequest = 116 };
            imageStack.Children.Add(ClickableArticleImage(BuildArticleImageUrl(_selectedArticleDetail.ArticleImage), 112, 88,
                async () => await ShowArticleImageGalleryAsync(_selectedArticleDetail.BrandNo, _selectedArticleDetail.ArtNo, BuildArticleImageUrl(_selectedArticleDetail.ArticleImage))));
            imageStack.Children.Add(new Image
            {
                Source = BuildBrandLogoUrl(_selectedArticleDetail.BrandLogo),
                Aspect = Aspect.AspectFit,
                WidthRequest = 108,
                HeightRequest = 34
            });
            top.Add(imageStack, 0, 0);

            var productHeader = new VerticalStackLayout { Spacing = 4 };
            productHeader.Children.Add(new Label { Text = _selectedArticleDetail.ProductCode, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") });
            productHeader.Children.Add(new Label { Text = _selectedArticleDetail.ProductDescription ?? string.Empty, FontSize = 14, TextColor = Color.FromArgb("#475569") });

            var quantitativePrices = BuildQuantitativePriceList(_selectedArticleDetail);
            if (quantitativePrices.Count > 1)
                productHeader.Children.Add(QuantitativeSelector(_selectedArticleDetail, quantitativePrices));

            productHeader.Children.Add(InfoLine("Prix", FormatDecimalText(_selectedArticleDetail.ProductPrice)));
            productHeader.Children.Add(InfoLine("Remise", FormatDiscount(_selectedArticleDetail.ProductDiscount)));
            productHeader.Children.Add(InfoLine("Prix net", FormatNetPrice(_selectedArticleDetail)));

            if (_selectedArticleDetail.PurchasePackagingQuantity > 0)
                productHeader.Children.Add(InfoLine("Conditionnement", _selectedArticleDetail.PurchasePackagingQuantity.ToString("0.##", CultureInfo.CurrentCulture)));

            if (!string.IsNullOrWhiteSpace(_selectedArticleDetail.ConsigneProduct))
                productHeader.Children.Add(InfoLine("Consigne", $"{_selectedArticleDetail.ConsigneValue} EURO ({_selectedArticleDetail.ConsigneLabel})"));

            if (!string.IsNullOrWhiteSpace(_selectedArticleDetail.EcotaxeProduct))
                productHeader.Children.Add(InfoLine("Écotaxe", $"{_selectedArticleDetail.EcotaxeValue} EURO ({_selectedArticleDetail.EcotaxeLabel})"));

            top.Add(productHeader, 1, 0);
            root.Children.Add(top);
            root.Children.Add(BuildArticleInfoActions(_selectedArticleDetail));

            string businessMessage = BuildDisponibilityMessage(_selectedArticleDetail);
            if (!string.IsNullOrWhiteSpace(businessMessage))
                root.Children.Add(new Border
                {
                    BackgroundColor = Color.FromArgb("#FEF2F2"),
                    Stroke = Color.FromArgb("#FCA5A5"),
                    StrokeThickness = 1,
                    Padding = 10,
                    StrokeShape = new RoundRectangle { CornerRadius = 14 },
                    Content = new Label { Text = businessMessage, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#B91C1C") }
                });

            var addBasketButton = PrimaryButton("Ajouter au panier");
            addBasketButton.Clicked += (_, __) => ShowAddToBasketModal();
            root.Children.Add(addBasketButton);

            _stepHost.Children.Add(Card(root));
        }

        private View WarehouseInputRow(WarehouseOrderInput input)
        {
            var quantity = new BorderlessEntry
            {
                Text = input.QuantityText,
                Keyboard = Keyboard.Numeric,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#0F172A"),
                PlaceholderColor = Color.FromArgb("#94A3B8"),
                WidthRequest = 54,
                HeightRequest = 28,
                FontSize = 13
            };
            quantity.TextChanged += (_, e) => input.QuantityText = e.NewTextValue;

            var quantityBox = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                Padding = new Thickness(3, 0),
                WidthRequest = 66,
                HeightRequest = 32,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Content = quantity
            };

            var grid = new Grid
            {
                ColumnSpacing = 6,
                Padding = new Thickness(2, 0),
                ColumnDefinitions = Columns("*,*,*")
            };
            grid.Add(new Label { Text = input.Warehouse, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }, 0, 0);
            grid.Add(new Label { Text = input.DisponibilityText, FontSize = 13, TextColor = Color.FromArgb("#475569"), VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }, 1, 0);
            grid.Add(quantityBox, 2, 0);
            return CompactInnerCard(grid);
        }

        private void ShowAddToBasketModal()
        {
            var body = new VerticalStackLayout { Spacing = 8 };
            body.Children.Add(HeaderRow(new[] { "Entrepôt", "Disponibilité", "Quantité" }));

            var rows = new VerticalStackLayout { Spacing = 4 };
            if (_warehouseInputs.Count == 0)
            {
                rows.Children.Add(EmptyLabel("Aucune plateforme disponible."));
            }
            else
            {
                foreach (var input in _warehouseInputs)
                    rows.Children.Add(WarehouseInputRow(input));
            }

            body.Children.Add(new ScrollView
            {
                Content = rows,
                HeightRequest = 444
            });

            var buttons = new Grid { ColumnSpacing = 8, ColumnDefinitions = Columns("*,*") };
            var cancelButton = SecondaryButton("Annuler");
            cancelButton.Clicked += (_, __) => HideModal();
            var addButton = PrimaryButton("Ajouter");
            addButton.Clicked += async (_, __) =>
            {
                HideModal();
                await AddSelectedArticleToBasketAsync();
            };
            buttons.Add(cancelButton, 0, 0);
            buttons.Add(addButton, 1, 0);
            body.Children.Add(buttons);

            ShowModal("Ajouter au panier", body.Children.ToList(), false);
        }

        private async Task AddSelectedArticleToBasketAsync()
        {
            await RunServiceAsync(async () =>
            {
                if (_selectedArticleDetail == null || string.IsNullOrWhiteSpace(_selectedProductCode))
                    throw new Exception("Aucun article sélectionné.");

                var lines = new List<OrderLine>();
                foreach (var input in _warehouseInputs)
                {
                    if (input.Quantity <= 0)
                        continue;

                    if (_selectedQuantitative > input.Quantity)
                        throw new Exception($"La quantité à commander pour {input.Warehouse} n'est pas suffisante pour avoir le prix quantitatif sélectionné.");

                    lines.Add(new OrderLine
                    {
                        ProductCode = _selectedProductCode,
                        Warehouse = input.Warehouse,
                        Quantity = input.Quantity,
                        IsSpecialLine = false,
                        NotRuptureInformation = false,
                        RequestProductCode = _selectedProductCode
                    });
                }

                if (lines.Count == 0)
                    throw new Exception("Saisissez au moins une quantité à ajouter au panier.");

                await EasySession.AddArticleListToWarehouseShoppingCartAsync(lines.ToArray());
                int addedQuantity = lines.Sum(l => l.Quantity);
                string addedProductCode = _selectedProductCode;
                _selectedArticle = null;
                _selectedArticleDetail = null;
                _selectedProductCode = string.Empty;
                _selectedQuantitative = 1;
                _warehouseInputs.Clear();
                _lastReferenceList.Clear();
                _lastArticleList.Clear();
                _selectedGenericArticle = null;
                _searchEntry.Text = string.Empty;
                await RefreshGlobalInformationAsync();
                await ShowBlockingMessageAsync("Panier", $"{addedProductCode} ajouté au panier : {addedQuantity} pièce(s).");
                ShowHomeStep();
            }, true);
        }

        private List<ArticlePriceAndStock> FilterAndOrderArticles(ArticlePriceAndStock[] articles, GenericArticle genericArticle)
        {
            var result = new List<ArticlePriceAndStock>();
            string searchedProduct = Clean(genericArticle.ShortProductCode);
            ArticlePriceAndStock? searchedArticle = null;

            var activeWarehouseFilter = GetActiveWarehouseFilter();
            bool applyWarehouseFilter = _warehouseSelectionMode != WarehouseSelectionMode.All;

            foreach (var article in articles)
            {
                if (applyWarehouseFilter)
                    ApplyWarehouseFilterToArticle(article, activeWarehouseFilter);

                if (article.TecdocGenericArticleList != null && article.TecdocGenericArticleList.Length == 0)
                {
                    // ASPX can hide empty generic articles depending on options. The mobile first version keeps them visible
                    // because the corresponding filter is in the options panel and not yet validated for exclusion.
                }

                if (searchedArticle == null && !string.IsNullOrWhiteSpace(article.CleanProductCode))
                {
                    string cleanProduct = article.CleanProductCode.Length > 3 ? article.CleanProductCode.Substring(3) : article.CleanProductCode;
                    if (string.Equals(Clean(cleanProduct), searchedProduct, StringComparison.OrdinalIgnoreCase))
                    {
                        article.SearchedArticle = true;
                        searchedArticle = article;
                        continue;
                    }
                }

                result.Add(article);
            }

            if (searchedArticle != null)
                result.Insert(0, searchedArticle);

            return result;
        }

        private void ApplyWarehouseFilterToArticle(ArticlePriceAndStock article, HashSet<string> selectedWarehouses)
        {
            if (selectedWarehouses.Count == 0)
            {
                article.WarehouseDisponibilityList = Array.Empty<WarehouseDisponibility>();
                article.Disponibility = "0";
                article.Warehouse = EasySession.CurrentAccount?.Warehouse ?? string.Empty;
                return;
            }

            var filtered = new List<WarehouseDisponibility>();
            decimal totalDisponibility = 0;
            foreach (var warehouseDisponibility in article.WarehouseDisponibilityList ?? Array.Empty<WarehouseDisponibility>())
            {
                if (warehouseDisponibility.Disponibility > 0 && selectedWarehouses.Contains(warehouseDisponibility.Warehouse))
                {
                    filtered.Add(warehouseDisponibility);
                    totalDisponibility += warehouseDisponibility.Disponibility;
                }
            }

            article.WarehouseDisponibilityList = filtered.ToArray();
            article.Disponibility = totalDisponibility.ToString(CultureInfo.InvariantCulture);
            if (filtered.Count == 0)
                article.Warehouse = EasySession.CurrentAccount?.Warehouse ?? string.Empty;
            else if (filtered.Count == 1)
                article.Warehouse = filtered[0].Warehouse;
            else
                article.Warehouse = "M.PLATEFORME";
        }

        private bool IsStockedGroup(ArticlePriceAndStock article)
        {
            if (_warehouseSelectionMode != WarehouseSelectionMode.All)
                return ParseDecimal(article.Disponibility) > 0;

            return ParseDecimal(article.Disponibility) > 0 || (article.Stocked && string.IsNullOrWhiteSpace(article.DropshippingSupplierCode));
        }

        private bool IsExcludedGroup(ArticlePriceAndStock article)
        {
            if (!string.IsNullOrWhiteSpace(article.DropshippingSupplierCode))
                return false;

            if (_warehouseSelectionMode != WarehouseSelectionMode.All && article.Stocked && !IsStockedGroup(article))
                return true;

            return !string.IsNullOrWhiteSpace(article.Warehouse) && !IsStockedGroup(article);
        }

        private bool IsDropshippingGroup(ArticlePriceAndStock article)
        {
            return !string.IsNullOrWhiteSpace(article.DropshippingSupplierCode) && !IsStockedGroup(article);
        }

        private bool CanShowSupplierButton(ArticlePriceAndStock article)
        {
            return !string.IsNullOrWhiteSpace(article.DropshippingSupplierCode)
                && (EasySession.IsAdministrator || ParseDecimal(article.Disponibility) <= 0);
        }

        private void ShowDisponibilityModal(WarehouseDisponibility[] disponibilities)
        {
            var body = new VerticalStackLayout { Spacing = 8 };
            body.Children.Add(HeaderRow(new[] { "Plateforme", "Stock" }));
            var rows = new VerticalStackLayout { Spacing = 4 };
            if (disponibilities.Length == 0)
                rows.Children.Add(EmptyLabel("Aucune disponibilité plateforme."));
            else
            {
                foreach (var disponibility in disponibilities)
                {
                    var row = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8, Padding = new Thickness(2, 0) };
                    row.Add(new Label { Text = disponibility.Warehouse, FontAttributes = FontAttributes.Bold, FontSize = 13, TextColor = Color.FromArgb("#0F172A"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 0, 0);
                    row.Add(new Label { Text = disponibility.Disponibility.ToString("0.##", CultureInfo.CurrentCulture), FontSize = 13, TextColor = Color.FromArgb("#475569"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 1, 0);
                    rows.Children.Add(CompactInnerCard(row));
                }
            }

            body.Children.Add(new ScrollView
            {
                Content = rows,
                HeightRequest = 392
            });

            ShowModal("Disponibilités", body.Children.ToList());
        }

        private void ShowQuantitativeModal(ArticlePriceAndStock article, bool canSelect)
        {
            var body = new VerticalStackLayout { Spacing = 8 };
            body.Children.Add(HeaderRow(new[] { "Quantitative", "Prix", "Remise", "P.Net" }));

            var prices = BuildQuantitativePriceList(article);
            if (prices.Count == 0)
            {
                body.Children.Add(EmptyLabel("Aucun tarif quantitatif."));
            }
            else
            {
                foreach (var price in prices)
                {
                    var row = new Grid { ColumnDefinitions = Columns("*,*,*,*"), ColumnSpacing = 8 };
                    var quantityButton = QuantitativeActionButton(price.Quantitative.ToString("0.##", CultureInfo.CurrentCulture), canSelect);
                    quantityButton.Clicked += async (_, __) =>
                    {
                        HideModal();
                        int quantitative = Convert.ToInt32(price.Quantitative);
                        await RunServiceAsync(async () => await LoadProductSheetAsync(article, quantitative), true);
                    };
                    row.Add(quantityButton, 0, 0);
                    row.Add(new Label { Text = price.Price.ToString("0.00", CultureInfo.CurrentCulture), TextColor = Color.FromArgb("#475569"), VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }, 1, 0);
                    row.Add(new Label { Text = QuantitativeDiscountText(article, price), TextColor = Color.FromArgb("#475569"), VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }, 2, 0);
                    row.Add(new Label { Text = QuantitativeNetPriceText(article, price), TextColor = Color.FromArgb("#B91C1C"), FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center }, 3, 0);
                    body.Children.Add(InnerCard(row));
                }
            }

            ShowModal("Tarifs quantitatifs", body.Children.ToList());
        }

        private Grid BuildArticleInfoActions(ArticlePriceAndStock article)
        {
            var row = new Grid { ColumnDefinitions = Columns("*,*,*,*,*"), ColumnSpacing = 5 };
            var gallery = ArticleIconButton("ic_article_gallery.png", "Images article");
            gallery.Clicked += async (_, __) => await RunServiceAsync(async () =>
                await ShowArticleImageGalleryAsync(article.BrandNo, article.ArtNo, BuildArticleImageUrl(article.ArticleImage)), true);

            var documents = ArticleIconButton("ic_article_documents.png", "Documents article");
            documents.Clicked += async (_, __) => await RunServiceAsync(async () =>
                await ShowArticleDocumentListAsync(article.BrandNo, article.ArtNo), true);

            var oem = ArticleIconButton("ic_article_oem.png", "Références OEM");
            oem.Clicked += async (_, __) => await RunServiceAsync(async () =>
                await ShowArticleReferenceListAsync(article.BrandNo, article.ArtNo), true);

            var vehicles = ArticleIconButton("ic_article_vehicles.png", "Véhicules article");
            vehicles.Clicked += async (_, __) => await RunServiceAsync(async () =>
                await ShowArticleVehicleListAsync(article.BrandNo, article.ArtNo), true);

            var attributes = ArticleIconButton("ic_article_attributes.png", "Propriétés article");
            attributes.Clicked += async (_, __) => await RunServiceAsync(async () =>
                await ShowArticleAttributeListAsync(article.BrandNo, article.ArtNo), true);

            row.Add(gallery, 0, 0);
            row.Add(documents, 1, 0);
            row.Add(oem, 2, 0);
            row.Add(vehicles, 3, 0);
            row.Add(attributes, 4, 0);
            return row;
        }

        private async Task ShowArticleImageGalleryAsync(string? brandNo, string? articleNo, string fallbackSource)
        {
            var files = await GetArticleFilesAsync(brandNo, articleNo);
            var images = files
                .Where(IsImageFile)
                .OrderBy(ArticleFileSort)
                .Select(f => BuildArticleFileUrl(f.Path))
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToList();

            if (images.Count == 0 && !string.IsNullOrWhiteSpace(fallbackSource))
                images.Add(fallbackSource);

            if (images.Count == 0)
                throw new Exception("Aucune image disponible pour cet article.");

            ShowImageGallery(images);
        }

        private async Task ShowArticleDocumentListAsync(string? brandNo, string? articleNo)
        {
            var files = (await GetArticleFilesAsync(brandNo, articleNo))
                .Where(f => !IsImageFile(f))
                .OrderBy(ArticleFileSort)
                .ToList();

            var body = new VerticalStackLayout { Spacing = 8 };
            var rows = new VerticalStackLayout { Spacing = 6 };
            if (files.Count == 0)
            {
                rows.Children.Add(EmptyLabel("Aucun fichier associé à cet article."));
            }
            else
            {
                foreach (var file in files)
                    rows.Children.Add(DocumentFileRow(file));
            }

            body.Children.Add(new ScrollView { Content = rows, HeightRequest = 392 });
            ShowModal("Documents article", body.Children.ToList());
        }

        private View DocumentFileRow(ArticlFile file)
        {
            string label = BuildArticleDocumentLabel(file);

            var link = new Label
            {
                Text = label,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#DC2626"),
                TextDecorations = TextDecorations.Underline,
                LineBreakMode = LineBreakMode.WordWrap
            };
            link.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    HideModal();
                    await RunServiceAsync(async () => await OpenArticleFileAsync(file), true);
                })
            });

            return CompactInnerCard(link);
        }

        private static string BuildArticleDocumentLabel(ArticlFile file)
        {
            if (!string.IsNullOrWhiteSpace(file.Label))
                return file.Label.Trim();

            string path = file.Path ?? string.Empty;
            string name = System.IO.Path.GetFileName(path.Split('?')[0]);
            return string.IsNullOrWhiteSpace(name) ? "Document" : name;
        }

        private async Task OpenArticleFileAsync(ArticlFile file)
        {
            string path = BuildArticleFileUrl(file.Path);
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("Chemin du fichier article vide.");

            string extension = ResolveArticleFileExtension(file);
            if (string.IsNullOrWhiteSpace(extension))
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out var externalUri))
                {
                    await Launcher.Default.OpenAsync(externalUri);
                    return;
                }

                throw new Exception("Lien article invalide.");
            }

            if (!string.Equals(extension, "pdf", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, "zip", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Type de fichier non pris en charge : {extension}.");

            string localPath = await DownloadArticleFileAsync(path, file.Label, extension);
            string fileName = System.IO.Path.GetFileName(localPath);
            if (string.Equals(extension, "pdf", StringComparison.OrdinalIgnoreCase))
                await Navigation.PushModalAsync(new PdfViewerPage(localPath, fileName));
            else
                await Navigation.PushModalAsync(new ZipViewerPage(localPath, fileName));
        }

        private static async Task<string> DownloadArticleFileAsync(string url, string? label, string extension)
        {
            using var http = new HttpClient();
            byte[] bytes = await http.GetByteArrayAsync(url);
            if (bytes.Length == 0)
                throw new Exception("Le fichier téléchargé est vide.");

            string baseName = SanitizeFileName(string.IsNullOrWhiteSpace(label) ? "document_article" : label);
            string fileName = $"{baseName}.{extension.TrimStart('.').ToLowerInvariant()}";
            string localPath = System.IO.Path.Combine(FileSystem.CacheDirectory, fileName);
            System.IO.File.WriteAllBytes(localPath, bytes);
            return localPath;
        }

        private async Task ShowArticleReferenceListAsync(string? brandNo, string? articleNo)
        {
            EnsureArticleKey(brandNo, articleNo);
            var references = await EasySession.GetArticleReferenceListAsync(brandNo!, articleNo!) ?? Array.Empty<ArticleReference>();
            var body = new VerticalStackLayout { Spacing = 8 };
            body.Children.Add(HeaderRow(new[] { "Référence OEM", "Fabricant" }));
            var rows = new VerticalStackLayout { Spacing = 4 };
            if (references.Length == 0)
                rows.Children.Add(EmptyLabel("Aucune référence OEM."));
            else
            {
                foreach (var reference in references.OrderBy(r => r.ManufacturerCode).ThenBy(r => r.ReferenceNo))
                {
                    var row = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8, Padding = new Thickness(2, 0) };
                    row.Add(CenteredCell(reference.ReferenceNo), 0, 0);
                    row.Add(CenteredCell(reference.ManufacturerCode), 1, 0);
                    rows.Children.Add(CompactInnerCard(row));
                }
            }
            body.Children.Add(new ScrollView { Content = rows, HeightRequest = 392 });
            ShowModal("Références OEM", body.Children.ToList());
        }

        private async Task ShowArticleVehicleListAsync(string? brandNo, string? articleNo)
        {
            EnsureArticleKey(brandNo, articleNo);
            var vehicles = await EasySession.GetArticleVehicleListAsync(brandNo!, articleNo!) ?? Array.Empty<ArticlVehicle>();
            var rows = new VerticalStackLayout { Spacing = 6 };
            if (vehicles.Length == 0)
                rows.Children.Add(EmptyLabel("Aucun véhicule associé."));
            else
            {
                foreach (var vehicle in vehicles.OrderBy(v => v.ManufacturerCode).ThenBy(v => v.ModelLabel).ThenBy(v => v.VehicleLabel))
                    rows.Children.Add(VehicleCard(vehicle));
            }
            var body = new VerticalStackLayout { Spacing = 8, Children = { new ScrollView { Content = rows, HeightRequest = 420 } } };
            ShowModal("Véhicules compatibles", body.Children.ToList());
        }

        private View VehicleCard(ArticlVehicle vehicle)
        {
            var stack = new VerticalStackLayout { Spacing = 4 };
            stack.Children.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(vehicle.ManufacturerCode) ? "Constructeur" : vehicle.ManufacturerCode,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0F172A")
            });
            stack.Children.Add(new Label { Text = $"Modèle : {vehicle.ModelLabel}", FontSize = 12, TextColor = Color.FromArgb("#475569"), LineBreakMode = LineBreakMode.WordWrap });
            stack.Children.Add(new Label { Text = $"Véhicule : {vehicle.VehicleLabel}", FontSize = 12, TextColor = Color.FromArgb("#475569"), LineBreakMode = LineBreakMode.WordWrap });
            stack.Children.Add(new Label { Text = $"KTypeNo : {vehicle.KTypeNo}", FontSize = 12, TextColor = Color.FromArgb("#64748B") });
            stack.Children.Add(new Label { Text = $"Période : {vehicle.FromDate} - {vehicle.ToData}", FontSize = 12, TextColor = Color.FromArgb("#64748B") });
            return CompactInnerCard(stack);
        }

        private async Task ShowArticleAttributeListAsync(string? brandNo, string? articleNo)
        {
            EnsureArticleKey(brandNo, articleNo);
            var attributes = await EasySession.GetArticleAttributeListAsync(brandNo!, articleNo!) ?? Array.Empty<ArticleAttribute>();
            var body = new VerticalStackLayout { Spacing = 8 };
            body.Children.Add(HeaderRow(new[] { "Description", "Valeur" }));
            var rows = new VerticalStackLayout { Spacing = 4 };
            if (attributes.Length == 0)
                rows.Children.Add(EmptyLabel("Aucune propriété."));
            else
            {
                foreach (var attribute in attributes.OrderBy(ArticleAttributeSort).ThenBy(a => a.Description))
                {
                    var row = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8, Padding = new Thickness(2, 0) };
                    row.Add(CenteredCell(attribute.Description), 0, 0);
                    row.Add(CenteredCell(string.IsNullOrWhiteSpace(attribute.UnitLabel) ? attribute.Value : $"{attribute.Value} {attribute.UnitLabel}"), 1, 0);
                    rows.Children.Add(CompactInnerCard(row));
                }
            }
            body.Children.Add(new ScrollView { Content = rows, HeightRequest = 392 });
            ShowModal("Propriétés article", body.Children.ToList());
        }

        private async Task<ArticlFile[]> GetArticleFilesAsync(string? brandNo, string? articleNo)
        {
            EnsureArticleKey(brandNo, articleNo);
            return await EasySession.GetArticleFileListAsync(brandNo!, articleNo!) ?? Array.Empty<ArticlFile>();
        }

        private static void EnsureArticleKey(string? brandNo, string? articleNo)
        {
            if (string.IsNullOrWhiteSpace(brandNo) || string.IsNullOrWhiteSpace(articleNo))
                throw new Exception("Référence article TecDoc incomplète : BrandNo ou ArtNo manquant.");
        }

        private static bool IsImageFile(ArticlFile file)
        {
            string extension = ResolveArticleFileExtension(file);
            return extension is "gif" or "bmp" or "jpg" or "jpeg" or "png";
        }

        private static string ResolveArticleFileExtension(ArticlFile file)
        {
            string extension = (file.Extension ?? string.Empty).Trim().TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(extension))
                return extension;

            string pathExtension = System.IO.Path.GetExtension((file.Path ?? string.Empty).Split('?')[0]).TrimStart('.').ToLowerInvariant();
            return pathExtension;
        }

        private static int ArticleFileSort(ArticlFile file)
        {
            return int.TryParse(file.SortNo, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort) ? sort : int.MaxValue;
        }

        private static int ArticleAttributeSort(ArticleAttribute attribute)
        {
            return int.TryParse(attribute.SortNo, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort) ? sort : int.MaxValue;
        }

        private string BuildArticleFileUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string value = path.Trim();
            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return value;

            return BuildEasyAdminUrl(value, value);
        }

        private static Label CenteredCell(string? text)
        {
            return new Label
            {
                Text = text ?? string.Empty,
                FontSize = 12,
                TextColor = Color.FromArgb("#475569"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap
            };
        }

        private static string SanitizeFileName(string value)
        {
            string result = value.Trim();
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');
            return string.IsNullOrWhiteSpace(result) ? "document_article" : result;
        }

        private async Task ShowSupplierCommandModalAsync(ArticlePriceAndStock article)
        {
            if (string.IsNullOrWhiteSpace(article.ProductCode))
                throw new Exception("Code article fournisseur invalide.");

            int supplierStock = await EasySession.GetExternalStockStatusAsync(article.ProductCode);
            if (supplierStock <= 0)
                throw new Exception("Aucune disponibilité chez le fournisseur.");

            var clientOrderEntry = new BorderlessEntry
            {
                Placeholder = "N° commande client (facultatif)",
                TextColor = Color.FromArgb("#0F172A"),
                PlaceholderColor = Color.FromArgb("#94A3B8"),
                BackgroundColor = Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            var quantityEntry = new BorderlessEntry
            {
                Text = "1",
                Keyboard = Keyboard.Numeric,
                TextColor = Color.FromArgb("#0F172A"),
                PlaceholderColor = Color.FromArgb("#94A3B8"),
                BackgroundColor = Colors.Transparent,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            var body = new VerticalStackLayout { Spacing = 12 };
            body.Children.Add(new Border
            {
                BackgroundColor = Color.FromArgb("#ECFDF5"),
                Stroke = Color.FromArgb("#86EFAC"),
                StrokeThickness = 1,
                Padding = 10,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = new Label
                {
                    Text = $"Rupture plateformes, livré par le fournisseur, ni repris ni échangé. Stock fournisseur : {supplierStock}. Frais de port supplémentaire : 25 €.",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#166534"),
                    LineBreakMode = LineBreakMode.WordWrap
                }
            });
            body.Children.Add(InfoLine("Article", article.ProductCode));
            body.Children.Add(InfoLine("Libellé", article.ProductDescription ?? string.Empty));
            body.Children.Add(EditBox(clientOrderEntry));
            body.Children.Add(EditBox(quantityEntry));

            var buttons = new Grid { ColumnSpacing = 8, ColumnDefinitions = Columns("*,*") };
            var cancelButton = SecondaryButton("Annuler");
            cancelButton.Clicked += (_, __) => HideModal();
            var sendButton = PrimaryButton("Envoyer");
            sendButton.Clicked += async (_, __) =>
            {
                HideModal();
                await RunServiceAsync(async () =>
                {
                    if (!int.TryParse(quantityEntry.Text, out int quantity) || quantity <= 0)
                        throw new Exception("Veuillez préciser la quantité voulue.");

                    var result = await EasySession.SendExternalCommandAsync(article.ProductCode, quantity, clientOrderEntry.Text ?? string.Empty);
                    string message = result.Rupture <= 0
                        ? $"Commande fournisseur passée avec succès. Commande : {result.SorderCode}"
                        : $"Commande fournisseur partiellement passée. Livré : {result.ToBeDelivred}, rupture : {result.Rupture}. Commande : {result.SorderCode}";

                    await ShowBlockingMessageAsync("Commande fournisseur", message);
                }, true);
            };
            buttons.Add(cancelButton, 0, 0);
            buttons.Add(sendButton, 1, 0);
            body.Children.Add(buttons);

            ShowModal("Commande fournisseur", body.Children.ToList(), false);
        }

        private Image ClickableArticleImage(string source, double width, double height, Func<Task>? clickAction = null)
        {
            var image = new Image
            {
                Source = source,
                Aspect = Aspect.AspectFit,
                WidthRequest = width,
                HeightRequest = height
            };
            image.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    if (clickAction != null)
                        await clickAction();
                    else
                        ShowFullScreenImage(source);
                })
            });
            return image;
        }

        private void ShowFullScreenImage(string source)
        {
            ShowImageGallery(new List<string> { source });
        }

        private void ShowImageGallery(List<string> sources)
        {
            _imageGallerySources.Clear();
            _imageGallerySources.AddRange(sources.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (_imageGallerySources.Count == 0)
                return;

            _imageGalleryIndex = 0;
            UpdateImageGallery();
            _imageOverlay.IsVisible = true;
        }

        private void MoveImageGallery(int step)
        {
            if (_imageGallerySources.Count == 0)
                return;

            _imageGalleryIndex += step;
            if (_imageGalleryIndex < 0)
                _imageGalleryIndex = _imageGallerySources.Count - 1;
            if (_imageGalleryIndex >= _imageGallerySources.Count)
                _imageGalleryIndex = 0;
            UpdateImageGallery();
        }

        private void AddImageGallerySwipeGestures(View view)
        {
            view.GestureRecognizers.Add(new SwipeGestureRecognizer
            {
                Direction = SwipeDirection.Left,
                Command = new Command(() => MoveImageGallery(1))
            });
            view.GestureRecognizers.Add(new SwipeGestureRecognizer
            {
                Direction = SwipeDirection.Right,
                Command = new Command(() => MoveImageGallery(-1))
            });
        }

        private void UpdateImageGallery()
        {
            if (_imageGallerySources.Count == 0)
                return;

            _fullScreenImage.Source = _imageGallerySources[_imageGalleryIndex];
            _imageCounterLabel.Text = $"Image {_imageGalleryIndex + 1} / {_imageGallerySources.Count}";
            _imageThumbnailStrip.Children.Clear();

            for (int i = 0; i < _imageGallerySources.Count; i++)
            {
                int index = i;
                var thumbnail = new Image
                {
                    Source = _imageGallerySources[i],
                    Aspect = Aspect.AspectFill,
                    WidthRequest = 74,
                    HeightRequest = 58
                };
                var border = new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = index == _imageGalleryIndex ? Color.FromArgb("#16A34A") : Color.FromArgb("#CBD5E1"),
                    StrokeThickness = index == _imageGalleryIndex ? 3 : 1,
                    Padding = 2,
                    StrokeShape = new RoundRectangle { CornerRadius = 10 },
                    Content = thumbnail
                };
                border.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        _imageGalleryIndex = index;
                        UpdateImageGallery();
                    })
                });
                AddImageGallerySwipeGestures(border);
                _imageThumbnailStrip.Children.Add(border);
            }
        }

        private void ShowOptionsModal()
        {
            bool showAllArticleValue = _showAllArticleCheck.IsChecked;
            bool showImagesValue = _showImagesCheck.IsChecked;
            var modalWarehouseSelectionMode = _warehouseSelectionMode;
            var selectedWarehouseStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var radioRefreshers = new List<Action>();

            var content = new VerticalStackLayout { Spacing = 8 };
            content.Children.Add(CompactVisualCheckLine(() => showAllArticleValue, value => showAllArticleValue = value, "Afficher tous les articles"));
            content.Children.Add(CompactVisualCheckLine(() => showImagesValue, value => showImagesValue = value, "Afficher les images"));

            content.Children.Add(OptionGroupTitle("Sélection plateformes"));

            var selectedWarehouseGrid = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 4,
                IsVisible = modalWarehouseSelectionMode == WarehouseSelectionMode.Selected,
                ColumnDefinitions = Columns("*,*")
            };

            void RefreshRadioVisuals()
            {
                foreach (var refresh in radioRefreshers)
                    refresh();
                selectedWarehouseGrid.IsVisible = modalWarehouseSelectionMode == WarehouseSelectionMode.Selected;
            }

            content.Children.Add(CompactVisualRadioLine(
                () => modalWarehouseSelectionMode == WarehouseSelectionMode.All,
                () =>
                {
                    modalWarehouseSelectionMode = WarehouseSelectionMode.All;
                    RefreshRadioVisuals();
                },
                "Toutes les plateformes",
                radioRefreshers));

            content.Children.Add(CompactVisualRadioLine(
                () => modalWarehouseSelectionMode == WarehouseSelectionMode.Proximity,
                () =>
                {
                    modalWarehouseSelectionMode = WarehouseSelectionMode.Proximity;
                    RefreshRadioVisuals();
                },
                "Uniquement proximité",
                radioRefreshers));

            content.Children.Add(CompactVisualRadioLine(
                () => modalWarehouseSelectionMode == WarehouseSelectionMode.Selected,
                () =>
                {
                    modalWarehouseSelectionMode = WarehouseSelectionMode.Selected;
                    RefreshRadioVisuals();
                },
                "Plateformes sélectionnées",
                radioRefreshers));

            int rowIndex = 0;
            int columnIndex = 0;
            foreach (var warehouse in _availableWarehouses.OrderBy(w => w.Priority).ThenBy(w => w.WarehouseCode))
            {
                string code = warehouse.WarehouseCode ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                if (columnIndex == 0)
                    selectedWarehouseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                selectedWarehouseStates[code] = _selectedWarehouseCodes.Contains(code);
                selectedWarehouseGrid.Add(CompactVisualCheckLine(
                    () => selectedWarehouseStates.TryGetValue(code, out bool selected) && selected,
                    value => selectedWarehouseStates[code] = value,
                    code), columnIndex, rowIndex);

                columnIndex++;
                if (columnIndex > 1)
                {
                    columnIndex = 0;
                    rowIndex++;
                }
            }

            if (selectedWarehouseStates.Count == 0)
            {
                selectedWarehouseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var emptyPlatformLabel = EmptyLabel("Aucune plateforme disponible.");
                selectedWarehouseGrid.Add(emptyPlatformLabel, 0, 0);
                Grid.SetColumnSpan(emptyPlatformLabel, 2);
            }

            content.Children.Add(selectedWarehouseGrid);
            RefreshRadioVisuals();

            var buttons = new Grid { ColumnSpacing = 8, ColumnDefinitions = Columns("*,*") };
            var cancelButton = SecondaryButton("Annuler");
            cancelButton.Clicked += (_, __) => HideModal();
            var applyButton = PrimaryButton("Appliquer");
            applyButton.Clicked += async (_, __) =>
            {
                _showAllArticleCheck.IsChecked = showAllArticleValue;
                _showImagesCheck.IsChecked = showImagesValue;
                _warehouseSelectionMode = modalWarehouseSelectionMode;

                _selectedWarehouseCodes.Clear();
                foreach (var item in selectedWarehouseStates)
                {
                    if (item.Value)
                        _selectedWarehouseCodes.Add(item.Key);
                }

                if (_warehouseSelectionMode == WarehouseSelectionMode.Selected && _selectedWarehouseCodes.Count == 0)
                    _warehouseSelectionMode = WarehouseSelectionMode.All;

                HideModal();
                await RunServiceAsync(async () => await RefreshAfterOptionsChangedAsync(), true);
            };
            buttons.Add(cancelButton, 0, 0);
            buttons.Add(applyButton, 1, 0);

            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            double optionsHeight = Math.Min(560, Math.Max(430, screenHeight - 220));

            var root = new Grid
            {
                RowDefinitions = Rows("*,Auto"),
                RowSpacing = 12,
                HeightRequest = optionsHeight
            };
            root.Add(new ScrollView { Content = content }, 0, 0);
            root.Add(buttons, 0, 1);

            ShowModal("Options catalogue", new List<IView> { root }, false);
        }

        private async Task RefreshAfterOptionsChangedAsync()
        {
            if (_searchMode == CatalogueSearchMode.Registration && _currentVehicleSearchResult != null)
            {
                await LoadVehicleNodeAsync(
                    _currentVehicleSearchResult.KTypNo ?? _selectedVehicle?.k_type ?? string.Empty,
                    _vehicleNavigationPath.LastOrDefault()?.TreeNodeId ?? _currentVehicleSearchResult.TreeNodeId ?? string.Empty,
                    _vehicleNavigationPath.ToList());
                return;
            }

            if (_selectedGenericArticle != null && _currentStep == CatalogueStep.ArticleList)
            {
                await LoadArticleListAsync(_selectedGenericArticle);
                return;
            }

            if (_currentStep == CatalogueStep.ReferenceList)
                ShowReferencesStep();
            else
                ShowHomeStep();
        }

        private async Task LoadArticleListAsync(GenericArticle genericArticle)
        {
            _selectedGenericArticle = genericArticle;
            _selectedVehicle = null;
            _currentVehicleSearchResult = null;
            _vehicleNavigationPath.Clear();

            ArticlePriceAndStock[] articles;
            if (_hasDepotSupplier)
            {
                articles = await EasySession.GetLocalConditionForArticleListAsync(new[] { _selectedGenericArticle });
            }
            else
            {
                articles = await EasySession.GetAllConditionForArticleAndAlternativeAsync(
                    _selectedGenericArticle.ShortProductCode,
                    _selectedGenericArticle.SupplierCode,
                    _selectedGenericArticle.Catalog,
                    false);
            }

            _lastArticleList.Clear();
            _lastArticleList.AddRange(FilterAndOrderArticles(articles ?? Array.Empty<ArticlePriceAndStock>(), _selectedGenericArticle));
            ShowArticleListStep();
        }

        private async Task EnsureWarehouseOptionsLoadedAsync()
        {
            if (_availableWarehouses.Count > 0)
                return;

            var warehouses = await EasySession.GetWarehousesListV2Async() ?? Array.Empty<Warehouse>();
            _availableWarehouses.Clear();
            _availableWarehouses.AddRange(warehouses
                .Where(w => w.IsActive && !string.IsNullOrWhiteSpace(w.WarehouseCode))
                .OrderBy(w => w.Priority)
                .ThenBy(w => w.WarehouseCode));
        }

        private void ResetWarehouseSelectionMode()
        {
            _warehouseSelectionMode = WarehouseSelectionMode.All;
            _selectedWarehouseCodes.Clear();
            _allWarehousesRadio.IsChecked = true;
            _proximityWarehousesRadio.IsChecked = false;
            _selectedWarehousesRadio.IsChecked = false;
        }

        private HashSet<string> GetActiveWarehouseFilter()
        {
            var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_warehouseSelectionMode == WarehouseSelectionMode.Proximity)
            {
                if (!string.IsNullOrWhiteSpace(_proximityWarehouse))
                    selected.Add(_proximityWarehouse);
            }
            else if (_warehouseSelectionMode == WarehouseSelectionMode.Selected)
            {
                foreach (var code in _selectedWarehouseCodes)
                    selected.Add(code);
            }

            return selected;
        }

        private void ShowModal(string title, List<IView> views, bool showDefaultCloseButton = true)
        {
            _modalCloseButton.Text = "Fermer";
            _modalCloseButton.IsVisible = showDefaultCloseButton;
            _modalTitle.Text = title;
            _modalBody.Children.Clear();
            foreach (var view in views)
                _modalBody.Children.Add(view);
            _modalOverlay.IsVisible = true;
        }

        private void HideModal()
        {
            _modalOverlay.IsVisible = false;
            _modalBody.Children.Clear();
        }

        private async Task ShowBlockingMessageAsync(string title, string message)
        {
            var completion = new TaskCompletionSource<bool>();
            var oldHandler = default(EventHandler);
            oldHandler = (_, __) =>
            {
                _modalCloseButton.Clicked -= oldHandler;
                HideModal();
                completion.TrySetResult(true);
            };

            _modalCloseButton.Clicked += oldHandler;
            ShowModal(title, new List<IView>
            {
                new Label { Text = CleanServiceMessage(message), FontSize = 14, TextColor = Color.FromArgb("#475569"), LineBreakMode = LineBreakMode.WordWrap }
            });
            _modalCloseButton.Text = "OK";
            await completion.Task;
        }

        private async Task RefreshGlobalInformationAsync()
        {
            int unitCount = await EasySession.GetShoppingCartUnitCountAsync();
            if (_basketButton != null)
                _basketButton.Text = $"Panier ({unitCount} Pcs)";

            var messages = new List<string>();
            if (await EasySession.IsExpressSystemBlockedAsync())
                messages.Add("La préparation des commandes express est suspendue pour aujourd'hui.");
            if (await EasySession.IsMagasinSystemBlockedAsync())
                messages.Add("La préparation des commandes magasin est suspendue pour aujourd'hui.");

            _messageLabel.Text = string.Join("  •  ", messages);
            _messageLabel.IsVisible = messages.Count > 0;
        }

        private async Task RunServiceAsync(Func<Task> action, bool blockingErrors)
        {
            SetLoadingState(true);
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                string message = CleanServiceMessage(ex.Message);
                if (blockingErrors)
                    await ShowBlockingMessageAsync("Information", message);
                else
                    await ModernAlertService.ShowErrorAsync(message);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            _apiLoadingOverlay.IsVisible = isLoading;
            _apiActivityIndicator.IsVisible = isLoading;
            _apiActivityIndicator.IsRunning = isLoading;
        }

        private static string BuildDisponibilityMessage(ArticlePriceAndStock article)
        {
            if (article.SupplierCode == "663" && (article.ProductFamilyCode == "0201" || article.ProductFamilyCode == "0202" || article.ProductFamilyCode == "0206"))
                return "Commande avant 16h, livraison le lendemain avant 13h";
            if (article.ProductCode == "410HFOSOLTM-5K" || article.ProductCode == "410MI-HFOYF-3,1" || article.ProductCode == "410MX-R134A-12K" || article.ProductCode == "410MX-R134A-13K")
                return "Commande jusqu’à 16h, livraison lendemain avant 13h";
            if (article.IsDangerous)
                return $"Le produit '{article.ProductCode}' ne sera pas livré en Express";
            if (article.ReturnNotAllowed)
                return "Article ni repris, ni échangé. Interdit au retour pour les références en déstockage";
            return string.Empty;
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

        private View QuantitativeSelector(ArticlePriceAndStock article, List<QuantitativePrice> prices)
        {
            var value = new Label
            {
                Text = _selectedQuantitative.ToString(CultureInfo.CurrentCulture),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0F172A"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            var selector = new Border
            {
                BackgroundColor = Color.FromArgb("#E0F2FE"),
                Stroke = Color.FromArgb("#BAE6FD"),
                StrokeThickness = 1,
                Padding = new Thickness(16, 7),
                HorizontalOptions = LayoutOptions.Start,
                MinimumWidthRequest = 64,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = value
            };
            selector.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => ShowQuantitativeModal(article, true))
            });

            var grid = new Grid
            {
                ColumnDefinitions = Columns("Auto,Auto"),
                ColumnSpacing = 10
            };
            grid.Add(new Label { Text = "Quantitative :", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#475569"), VerticalOptions = LayoutOptions.Center }, 0, 0);
            grid.Add(selector, 1, 0);
            return grid;
        }

        private Border BackButton()
        {
            var iconCircle = new Border
            {
                WidthRequest = 26,
                HeightRequest = 26,
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 13 },
                Content = new Label
                {
                    Text = "↶",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#2563EB"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            };

            var layout = new HorizontalStackLayout
            {
                Spacing = 7,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    iconCircle,
                    new Label
                    {
                        Text = "Revenir",
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#0F172A"),
                        VerticalTextAlignment = TextAlignment.Center
                    }
                }
            };

            var button = new Border
            {
                BackgroundColor = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 0,
                Padding = new Thickness(8, 5),
                HorizontalOptions = LayoutOptions.Start,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = layout
            };
            button.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await GoBackOneStepAsync())
            });
            return button;
        }

        private static Label OptionGroupTitle(string text)
        {
            return new Label
            {
                Text = text,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0F172A"),
                Margin = new Thickness(0, 6, 0, 0)
            };
        }

        private static View CompactVisualCheckLine(Func<bool> getIsChecked, Action<bool> setIsChecked, string text)
        {
            var checkMark = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Colors.White
            };

            var square = new Border
            {
                WidthRequest = 24,
                HeightRequest = 24,
                StrokeThickness = 2,
                Padding = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Content = checkMark
            };

            void Refresh()
            {
                bool checkedValue = getIsChecked();
                square.Stroke = checkedValue ? Color.FromArgb("#16A34A") : Color.FromArgb("#64748B");
                square.BackgroundColor = checkedValue ? Color.FromArgb("#16A34A") : Colors.White;
                checkMark.Text = checkedValue ? "✓" : string.Empty;
            }

            var row = new Grid
            {
                ColumnSpacing = 8,
                Padding = new Thickness(4, 5),
                ColumnDefinitions = Columns("Auto,*")
            };
            row.Add(square, 0, 0);
            row.Add(new Label
            {
                Text = text,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#334155"),
                LineBreakMode = LineBreakMode.WordWrap
            }, 1, 0);

            var tap = new TapGestureRecognizer();
            tap.Command = new Command(() =>
            {
                setIsChecked(!getIsChecked());
                Refresh();
            });
            row.GestureRecognizers.Add(tap);
            Refresh();
            return row;
        }

        private static View CompactVisualRadioLine(Func<bool> getIsChecked, Action select, string text, ICollection<Action> refreshers)
        {
            var dot = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Colors.White
            };

            var circle = new Border
            {
                WidthRequest = 24,
                HeightRequest = 24,
                StrokeThickness = 2,
                Padding = 0,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = dot
            };

            void Refresh()
            {
                bool checkedValue = getIsChecked();
                circle.Stroke = checkedValue ? Color.FromArgb("#16A34A") : Color.FromArgb("#64748B");
                circle.BackgroundColor = checkedValue ? Color.FromArgb("#16A34A") : Colors.White;
                dot.Text = checkedValue ? "•" : string.Empty;
            }

            var row = new Grid
            {
                ColumnSpacing = 8,
                Padding = new Thickness(4, 5),
                ColumnDefinitions = Columns("Auto,*")
            };
            row.Add(circle, 0, 0);
            row.Add(new Label
            {
                Text = text,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#334155"),
                LineBreakMode = LineBreakMode.WordWrap
            }, 1, 0);

            var tap = new TapGestureRecognizer { Command = new Command(select) };
            row.GestureRecognizers.Add(tap);
            refreshers.Add(Refresh);
            Refresh();
            return row;
        }

        private static void AddArticleFeeLabels(VerticalStackLayout info, ArticlePriceAndStock article)
        {
            if (!string.IsNullOrWhiteSpace(article.ConsigneProduct))
                info.Children.Add(new Label
                {
                    Text = $"Consigne : {article.ConsigneValue} EURO ({article.ConsigneLabel})",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#B45309"),
                    LineBreakMode = LineBreakMode.WordWrap
                });

            if (!string.IsNullOrWhiteSpace(article.EcotaxeProduct))
                info.Children.Add(new Label
                {
                    Text = $"Écotaxe : {article.EcotaxeValue} EURO ({article.EcotaxeLabel})",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#B45309"),
                    LineBreakMode = LineBreakMode.WordWrap
                });
        }

        private static Button GalleryNavigationButton(string text)
        {
            return new Button
            {
                Text = text,
                BackgroundColor = Color.FromArgb("#66000000"),
                TextColor = Colors.White,
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 22,
                WidthRequest = 44,
                HeightRequest = 62,
                Padding = 0,
                VerticalOptions = LayoutOptions.Center
            };
        }

        private static ImageButton ArticleIconButton(string source, string automationName)
        {
            return new ImageButton
            {
                Source = source,
                BackgroundColor = Color.FromArgb("#E0F2FE"),
                CornerRadius = 12,
                WidthRequest = 42,
                HeightRequest = 36,
                Padding = new Thickness(7),
                Aspect = Aspect.AspectFit,
                AutomationId = automationName
            };
        }

        private static Button IconActionButton(string icon, string automationName)
        {
            var button = new Button
            {
                Text = icon,
                BackgroundColor = Color.FromArgb("#E0F2FE"),
                TextColor = Color.FromArgb("#075985"),
                FontSize = 15,
                CornerRadius = 13,
                WidthRequest = 34,
                HeightRequest = 30,
                Padding = new Thickness(0),
                AutomationId = automationName
            };
            return button;
        }

        private static Border ActionPill(string text, Func<Task> action)
        {
            var label = new Label
            {
                Text = text,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#075985"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            var pill = new Border
            {
                BackgroundColor = Color.FromArgb("#E0F2FE"),
                StrokeThickness = 0,
                Padding = new Thickness(8, 5),
                HorizontalOptions = LayoutOptions.Fill,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Content = label
            };
            pill.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await action())
            });
            return pill;
        }

        private static Border EditBox(Entry entry)
        {
            entry.BackgroundColor = Colors.Transparent;
            entry.TextColor = Color.FromArgb("#0F172A");
            entry.PlaceholderColor = Color.FromArgb("#94A3B8");
            entry.HorizontalTextAlignment = TextAlignment.Center;
            entry.VerticalTextAlignment = TextAlignment.Center;
            entry.HeightRequest = 42;
            return new Border
            {
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                Padding = new Thickness(8, 0),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = entry
            };
        }

        private static View InfoLine(string label, string value)
        {
            return new Label
            {
                Text = $"{label} : {value}",
                FontSize = 13,
                TextColor = Color.FromArgb("#475569"),
                FontAttributes = FontAttributes.Bold
            };
        }

        private static string FormatDiscount(string? discount)
        {
            if (decimal.TryParse(discount, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal value) ||
                decimal.TryParse((discount ?? string.Empty).Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return value == 0 ? "NET" : value.ToString("0.##", CultureInfo.CurrentCulture);

            return string.IsNullOrWhiteSpace(discount) ? "NET" : discount;
        }

        private static string FormatDecimalText(string? value)
        {
            // Les pages ASPX affichent directement les chaînes ProductPrice / ProductDiscountPrice
            // fournies par ArticlePriceAndStock. Ne pas appliquer de conversion de culture ici :
            // en environnement FR, "2.90" peut être interprété à tort comme 290.
            return value ?? string.Empty;
        }

        private static List<QuantitativePrice> BuildQuantitativePriceList(ArticlePriceAndStock article)
        {
            var prices = new List<QuantitativePrice>
            {
                new QuantitativePrice
                {
                    ProductCode = article.ProductCode,
                    Quantitative = 1,
                    Price = ParseDecimal(article.ProductPrice),
                    NetPrice = article.IsNetPrice ? "1" : "2"
                }
            };

            foreach (var price in article.QuantitativePriceList ?? Array.Empty<QuantitativePrice>())
            {
                if (price.Quantitative == 1)
                    continue;
                price.ProductCode = article.ProductCode;
                prices.Add(price);
            }

            return prices.OrderBy(p => p.Quantitative).ToList();
        }

        private static string QuantitativeDiscountText(ArticlePriceAndStock article, QuantitativePrice price)
        {
            if (price.NetPrice == "1")
                return "NET";

            decimal discount = ParseDecimal(article.ProductBrutDiscount);
            return discount == 0 ? "NET" : discount.ToString("0.##", CultureInfo.CurrentCulture);
        }

        private static string QuantitativeNetPriceText(ArticlePriceAndStock article, QuantitativePrice price)
        {
            decimal discount = price.NetPrice == "1" ? 0 : ParseDecimal(article.ProductBrutDiscount);
            decimal netPrice = price.Price * (1 - discount / 100);
            return netPrice.ToString("0.00", CultureInfo.CurrentCulture);
        }

        private string BuildArticleImageUrl(string? path)
        {
            return BuildEasyAdminUrl(_showImagesCheck.IsChecked ? path : "css/page/no_photo.png", "css/page/no_photo.png");
        }

        private string BuildBrandLogoUrl(string? path)
        {
            return BuildEasyAdminUrl(_showImagesCheck.IsChecked ? path : "css/page/no_logo.png", "css/page/no_logo.png");
        }

        private static string BuildEasyAdminUrl(string? path, string fallback)
        {
            string relative = string.IsNullOrWhiteSpace(path) ? fallback : path.Trim();
            if (relative.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || relative.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return relative;

            relative = relative.TrimStart('/');
            string currentUrl = EasySession.CurrentUrl ?? string.Empty;
            int easyIndex = currentUrl.IndexOf("/easy/", StringComparison.OrdinalIgnoreCase);
            string host = easyIndex > 0 ? currentUrl.Substring(0, easyIndex) : currentUrl.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(host))
                return relative;

            return $"{host}/EASY/Admin/{relative}";
        }

        private static View CheckLine(CheckBox checkBox, string text)
        {
            return new HorizontalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    checkBox,
                    new Label { Text = text, VerticalOptions = LayoutOptions.Center, TextColor = Color.FromArgb("#334155") }
                }
            };
        }

        private static View StepHeader(string text)
        {
            return new Border
            {
                BackgroundColor = Color.FromArgb("#ECFDF5"),
                Stroke = Color.FromArgb("#BBF7D0"),
                StrokeThickness = 1,
                Padding = new Thickness(12, 9),
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = new Label
                {
                    Text = text,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#15803D"),
                    LineBreakMode = LineBreakMode.WordWrap
                }
            };
        }

        private static View ArticleGroupHeader(string text)
        {
            return new Border
            {
                BackgroundColor = Color.FromArgb("#F0FDF4"),
                Stroke = Color.FromArgb("#DCFCE7"),
                StrokeThickness = 1,
                Padding = new Thickness(10, 8),
                Margin = new Thickness(0, 6, 0, 2),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new Label
                {
                    Text = text,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#16A34A"),
                    LineBreakMode = LineBreakMode.WordWrap
                }
            };
        }

        private static Label SectionTitle(string text)
        {
            return new Label { Text = text, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") };
        }

        private static Label EmptyLabel(string text)
        {
            return new Label { Text = text, FontSize = 13, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center };
        }

        private static View HeaderRow(string[] columns)
        {
            var grid = new Grid { ColumnSpacing = 6, Padding = new Thickness(8, 6), BackgroundColor = Color.FromArgb("#E2E8F0") };
            for (int i = 0; i < columns.Length; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            for (int i = 0; i < columns.Length; i++)
                grid.Add(new Label { Text = columns[i], FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), HorizontalTextAlignment = TextAlignment.Center }, i, 0);

            return grid;
        }

        private static Border Card(View content)
        {
            return new Border
            {
                Content = content,
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = 12,
                StrokeShape = new RoundRectangle { CornerRadius = 18 }
            };
        }


        private static Border CompactInnerCard(View content)
        {
            return new Border
            {
                Content = content,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = new Thickness(7, 5),
                MinimumHeightRequest = 40,
                StrokeShape = new RoundRectangle { CornerRadius = 12 }
            };
        }

        private static Border InnerCard(View content)
        {
            return new Border
            {
                Content = content,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = 10,
                StrokeShape = new RoundRectangle { CornerRadius = 14 }
            };
        }

        private static Border Header(string title, string subtitle, View actionView)
        {
            actionView.HorizontalOptions = LayoutOptions.Fill;
            return new Border
            {
                Content = actionView,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0, 0, 0, 2),
                StrokeThickness = 0,
                HorizontalOptions = LayoutOptions.Fill
            };
        }

        private static Button SmallActionButton(string text)
        {
            return new Button
            {
                Text = text,
                BackgroundColor = Color.FromArgb("#E0F2FE"),
                TextColor = Color.FromArgb("#075985"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 11,
                CornerRadius = 11,
                HeightRequest = 34,
                Padding = new Thickness(6, 0)
            };
        }


        private static Button QuantitativeActionButton(string text, bool isEnabled)
        {
            return new Button
            {
                Text = text,
                IsEnabled = isEnabled,
                BackgroundColor = isEnabled ? Color.FromArgb("#DC2626") : Color.FromArgb("#E2E8F0"),
                TextColor = isEnabled ? Colors.White : Color.FromArgb("#64748B"),
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                CornerRadius = 13,
                HeightRequest = 34,
                WidthRequest = 58,
                Padding = new Thickness(6, 0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        private static Button PrimaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#16A34A"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44, Padding = new Thickness(12, 0) };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#E2E8F0"), TextColor = Color.FromArgb("#0F172A"), FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44, Padding = new Thickness(12, 0) };
        }

        private static ColumnDefinitionCollection Columns(string pattern)
        {
            var collection = new ColumnDefinitionCollection();
            foreach (var part in pattern.Split(','))
            {
                string value = part.Trim();
                collection.Add(new ColumnDefinition { Width = value == "Auto" ? GridLength.Auto : GridLength.Star });
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

        private static string DisplayCatalog(string? catalog)
        {
            if (catalog == "EQUIPEMENTIER")
                return "TECDOC";
            if (catalog == "FABRICANT")
                return "OEM";
            return catalog ?? string.Empty;
        }

        private static string ReferenceLabel(CatalogArticle article)
        {
            return string.IsNullOrWhiteSpace(article.AgraLabel) ? article.Label ?? string.Empty : article.AgraLabel.Trim();
        }

        private static string ShortProductCode(string? productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                return string.Empty;
            return productCode.Length > 3 ? productCode.Substring(3) : productCode;
        }

        private static string FormatNetPrice(ArticlePriceAndStock article)
        {
            // Même règle que Workshop.aspx.cs : afficher ProductDiscountPrice tel que fourni.
            // Aucune multiplication, division ou reformatage culturel.
            return article.ProductDiscountPrice ?? string.Empty;
        }

        private static decimal ParseDecimal(string? value)
        {
            string normalized = (value ?? string.Empty).Replace(',', '.');
            if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
                return result;
            return 0;
        }

        private static string Clean(string value)
        {
            return Regex.Replace((value ?? string.Empty).ToUpperInvariant(), @"[^A-Z0-9]", string.Empty);
        }
    }
}
