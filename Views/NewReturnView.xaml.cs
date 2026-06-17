using AGRA_EASY_MOBILE.Services;
using Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AGRA_EASY_MOBILE
{
    public partial class NewReturnView : ContentPage
    {
        private ClientAccount? _selectedAccount;
        private DeliveryLinesStatus? _selectedDeliveryLine;
        private ReturnNoticeKey? _validatedReturnKey;
        private TaskCompletionSource<bool>? _confirmationCompletion;
        private bool _isInitialized;
        private bool _canEditAccount;
        private bool _hasPageError;
        private bool _hasProductMessage;
        private bool _showSaleConditions = true;
        private bool _isGarantie;
        private bool _isConfirmationVisible;
        private bool _isConfirmationCancelVisible;
        private bool _isAutoValidatingAccount;
        private bool _isAutoSearchingProduct;
        private bool _isOpeningClientSelection;
        private string _lastAutoValidatedAccountCode = string.Empty;
        private string _lastAutoSearchedProductCode = string.Empty;
        private string _pageMessage = string.Empty;
        private string _productMessage = string.Empty;
        private string _accountCodeInput = string.Empty;
        private string _productSearchText = string.Empty;
        private string _quantityText = string.Empty;
        private string _motifText = string.Empty;
        private string _returnClientCodeText = string.Empty;
        private string _selectedPrice = string.Empty;
        private string _selectedDiscount = string.Empty;
        private string _selectedAbattement = string.Empty;
        private string _confirmationTitle = string.Empty;
        private string _confirmationMessage = string.Empty;
        private string _confirmationAcceptText = string.Empty;
        private string _confirmationCancelText = string.Empty;

        public ObservableCollection<ReturnableArticle> ReturnableArticles { get; } = new();
        public ObservableCollection<DeliveryLinesStatus> DeliveryLines { get; } = new();
        public ObservableCollection<ReturnBasketLine> BasketLines { get; } = new();

        public string AccountCodeInput
        {
            get => _accountCodeInput;
            set
            {
                _accountCodeInput = value;
                OnPropertyChanged();
            }
        }

        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
            }
        }

        public string QuantityText
        {
            get => _quantityText;
            set
            {
                _quantityText = value;
                OnPropertyChanged();
            }
        }

        public string MotifText
        {
            get => _motifText;
            set
            {
                _motifText = value;
                OnPropertyChanged();
            }
        }

        public string ReturnClientCodeText
        {
            get => _returnClientCodeText;
            set
            {
                _returnClientCodeText = value;
                OnPropertyChanged();
            }
        }

        public string PageMessage
        {
            get => _pageMessage;
            set
            {
                _pageMessage = value;
                OnPropertyChanged();
                HasPageError = !string.IsNullOrWhiteSpace(value);
            }
        }

        public string ProductMessage
        {
            get => _productMessage;
            set
            {
                _productMessage = value;
                OnPropertyChanged();
                HasProductMessage = !string.IsNullOrWhiteSpace(value);
            }
        }

        public bool HasPageError
        {
            get => _hasPageError;
            set
            {
                _hasPageError = value;
                OnPropertyChanged();
            }
        }

        public bool HasProductMessage
        {
            get => _hasProductMessage;
            set
            {
                _hasProductMessage = value;
                OnPropertyChanged();
            }
        }

        public bool CanEditAccount
        {
            get => _canEditAccount;
            set
            {
                _canEditAccount = value;
                OnPropertyChanged();
            }
        }

        public bool ShowSaleConditions
        {
            get => _showSaleConditions;
            set
            {
                _showSaleConditions = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDepotSupplier));
            }
        }

        public bool HasDepotSupplier => !ShowSaleConditions;

        public bool IsGarantie
        {
            get => _isGarantie;
            set
            {
                _isGarantie = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPrice
        {
            get => _selectedPrice;
            set
            {
                _selectedPrice = value;
                OnPropertyChanged();
            }
        }

        public string SelectedDiscount
        {
            get => _selectedDiscount;
            set
            {
                _selectedDiscount = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAbattement
        {
            get => _selectedAbattement;
            set
            {
                _selectedAbattement = value;
                OnPropertyChanged();
            }
        }

        public bool IsConfirmationVisible
        {
            get => _isConfirmationVisible;
            set
            {
                _isConfirmationVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsConfirmationCancelVisible
        {
            get => _isConfirmationCancelVisible;
            set
            {
                _isConfirmationCancelVisible = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmationTitle
        {
            get => _confirmationTitle;
            set
            {
                _confirmationTitle = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmationMessage
        {
            get => _confirmationMessage;
            set
            {
                _confirmationMessage = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmationAcceptText
        {
            get => _confirmationAcceptText;
            set
            {
                _confirmationAcceptText = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmationCancelText
        {
            get => _confirmationCancelText;
            set
            {
                _confirmationCancelText = value;
                OnPropertyChanged();
            }
        }

        public bool HasSelectedAccount => _selectedAccount != null;
        public bool HasSelectedAccountAddress => !string.IsNullOrWhiteSpace(SelectedAccountAddress);
        public bool HasSelectedAccountCity => !string.IsNullOrWhiteSpace(SelectedAccountCity);
        public bool HasArticleChoices => ReturnableArticles.Count > 1 && DeliveryLines.Count == 0 && _selectedDeliveryLine == null;
        public bool HasDeliveryLines => DeliveryLines.Count > 0 && _selectedDeliveryLine == null;
        public bool HasSelectedDeliveryLine => _selectedDeliveryLine != null;
        public bool HasBasketLines => BasketLines.Count > 0;
        public bool IsBasketEmpty => BasketLines.Count == 0;
        public bool HasValidatedReturn => _validatedReturnKey != null;
        public bool HasValidationSection => HasBasketLines || HasValidatedReturn;
        public string BasketButtonText => BasketLines.Count <= 0 ? "Voir le panier" : $"Voir le panier ({BasketLines.Count})";

        public string SelectedAccountTitle
            => _selectedAccount == null ? string.Empty : $"{_selectedAccount.AccountCode} - {_selectedAccount.AccountName}";

        public string SelectedAccountAddress
            => _selectedAccount == null ? string.Empty : ($"{_selectedAccount.AddressLine1} {_selectedAccount.AddressLine2}".Trim());

        public string SelectedAccountCity
            => _selectedAccount == null ? string.Empty : ($"{_selectedAccount.Zip} {_selectedAccount.City}".Trim());

        public string SelectedDeliveryTitle
            => _selectedDeliveryLine == null ? string.Empty : $"{_selectedDeliveryLine.Reference} - BL {_selectedDeliveryLine.DeliveryNumber}";

        public string SelectedDeliveryDetail
            => _selectedDeliveryLine == null ? string.Empty : $"{_selectedDeliveryLine.Warehouse} - {_selectedDeliveryLine.DeliveryDate}";

        public string AvailableQuantityText
            => _selectedDeliveryLine == null ? string.Empty : $"Quantité disponible : {GetAvailableQuantity(_selectedDeliveryLine)}";

        public string BasketCountText
            => BasketLines.Count <= 1 ? $"{BasketLines.Count} ligne" : $"{BasketLines.Count} lignes";

        public string ValidatedReturnMessage
            => _validatedReturnKey == null ? string.Empty : $"Retour validé avec succès. N° retour : {_validatedReturnKey.ReturnNoticeCode}";

        public NewReturnView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ResetPageState();
            await InitializePageAsync();
        }

        private void ResetPageState()
        {
            _isInitialized = false;
            _selectedAccount = null;
            _selectedDeliveryLine = null;
            _validatedReturnKey = null;
            _confirmationCompletion?.TrySetResult(false);
            _confirmationCompletion = null;

            ReturnableArticles.Clear();
            DeliveryLines.Clear();
            BasketLines.Clear();

            AccountCodeInput = string.Empty;
            ProductSearchText = string.Empty;
            QuantityText = string.Empty;
            MotifText = string.Empty;
            ReturnClientCodeText = string.Empty;
            PageMessage = string.Empty;
            ProductMessage = string.Empty;
            SelectedPrice = string.Empty;
            SelectedDiscount = string.Empty;
            SelectedAbattement = string.Empty;
            ConfirmationTitle = string.Empty;
            ConfirmationMessage = string.Empty;
            ConfirmationAcceptText = string.Empty;
            ConfirmationCancelText = string.Empty;
            _lastAutoValidatedAccountCode = string.Empty;
            _lastAutoSearchedProductCode = string.Empty;

            CanEditAccount = false;
            ShowSaleConditions = true;
            IsGarantie = false;
            IsConfirmationVisible = false;
            IsConfirmationCancelVisible = false;

            RefreshAccountProperties();
            RefreshCollectionProperties();
            RefreshSelectionProperties();
            RefreshBasketProperties();
            OnPropertyChanged(nameof(HasValidatedReturn));
            OnPropertyChanged(nameof(HasValidationSection));
            OnPropertyChanged(nameof(ValidatedReturnMessage));
        }

        private async Task InitializePageAsync()
        {
            SetLoadingState(true);

            try
            {
                bool canCreateReturn = await EasySession.IsReturnSystemsManagerAsync();
                if (!canCreateReturn)
                {
                    PageMessage = "Violation des règles de sécurité. Vous n'êtes pas autorisé à faire un retour.";
                    return;
                }

                var currentAccount = EasySession.CurrentAccount;
                bool isAdministrator = string.Equals(currentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
                bool isClient = string.Equals(currentAccount?.Type, "Client", StringComparison.OrdinalIgnoreCase);

                CanEditAccount = isAdministrator;

                string accountCode = string.Empty;
                if (isClient)
                    accountCode = currentAccount?.AccountCode ?? string.Empty;

                if (isAdministrator)
                    accountCode = await EasySession.GetReturnBasketAccountCodeAsync();

                AccountCodeInput = accountCode;

                if (!string.IsNullOrWhiteSpace(accountCode))
                    await ValidateAccountAsync(accountCode);

                await RefreshBasketAsync();
            }
            catch (Exception ex)
            {
                PageMessage = ex.Message;
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void OnValidateAccountClicked(object sender, EventArgs e)
            => await ShowClientSelectionForReturnAsync();

        private async void OnAccountCodeEntryCompleted(object sender, EventArgs e)
            => await ShowClientSelectionForReturnAsync();

        private async void OnAccountCodeEntryUnfocused(object sender, FocusEventArgs e)
            => await ShowClientSelectionForReturnAsync();

        private async Task ShowClientSelectionForReturnAsync()
        {
            if (!CanEditAccount)
                return;

            if (_isAutoValidatingAccount || _isOpeningClientSelection)
                return;

            _isOpeningClientSelection = true;

            try
            {

            ReturnBasketLine[] basketLines = Array.Empty<ReturnBasketLine>();
            SetLoadingState(true);
            try
            {
                basketLines = await EasySession.GetReturnBasketLinesAsync() ?? Array.Empty<ReturnBasketLine>();
            }
            catch (Exception ex)
            {
                await ModernAlertService.ShowErrorAsync(ex.Message);
                return;
            }
            finally
            {
                SetLoadingState(false);
            }

            if (basketLines.Length > 0)
            {
                await ShowBlockingMessageAsync(
                    "Changement impossible",
                    "Le panier de retour contient déjà des lignes. Veuillez valider ou vider le panier avant de changer de client.");
                return;
            }

            ClientAccount? selectedAccount = await ClientAccountSelectionPage.ShowAsync(
                this,
                "Choisir un client",
                (AccountCodeInput ?? string.Empty).Trim());

            if (selectedAccount == null)
                return;

            string accountCode = (selectedAccount.AccountCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(accountCode))
                return;

            _isAutoValidatingAccount = true;
            SetLoadingState(true);

            try
            {
                AccountCodeInput = accountCode;
                await ValidateAccountAsync(accountCode);
                _lastAutoValidatedAccountCode = AccountCodeInput?.Trim() ?? accountCode;
                _lastAutoSearchedProductCode = string.Empty;
                await RefreshBasketAsync();
            }
            catch (Exception ex)
            {
                ClearSelectedAccount();
                await ModernAlertService.ShowErrorAsync(ex.Message);
            }
            finally
            {
                SetLoadingState(false);
                _isAutoValidatingAccount = false;
            }
            }
            finally
            {
                _isOpeningClientSelection = false;
            }
        }

        private async Task ValidateAccountInputAsync(bool showEmptyWarning, bool avoidDuplicate)
        {
            if (!CanEditAccount)
                return;

            string accountCode = (AccountCodeInput ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(accountCode))
            {
                if (showEmptyWarning)
                    await ModernAlertService.ShowWarningAsync("Veuillez renseigner un code client.");
                return;
            }

            if (avoidDuplicate && string.Equals(_lastAutoValidatedAccountCode, accountCode, StringComparison.OrdinalIgnoreCase))
                return;

            if (_isAutoValidatingAccount)
                return;

            _isAutoValidatingAccount = true;
            SetLoadingState(true);

            try
            {
                await ValidateAccountAsync(accountCode);
                _lastAutoValidatedAccountCode = AccountCodeInput?.Trim() ?? accountCode;
                _lastAutoSearchedProductCode = string.Empty;
                await RefreshBasketAsync();
            }
            catch (Exception ex)
            {
                ClearSelectedAccount();
                await ModernAlertService.ShowErrorAsync(ex.Message);
            }
            finally
            {
                SetLoadingState(false);
                _isAutoValidatingAccount = false;
            }
        }

        private async Task ValidateAccountAsync(string accountCode)
        {
            var basketLines = await EasySession.GetReturnBasketLinesAsync() ?? Array.Empty<ReturnBasketLine>();

            if (basketLines.Length > 0)
            {
                string currentAccountCode = await EasySession.GetReturnBasketAccountCodeAsync();

                if (!string.Equals(currentAccountCode?.Trim(), accountCode?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    await ShowBlockingMessageAsync(
                        "Changement impossible",
                        "Le panier contient déjà des lignes pour un autre client. Veuillez valider ou vider le panier avant de changer de client.");
                    return;
                }

                _selectedAccount ??= new ClientAccount
                {
                    AccountCode = currentAccountCode
                };

                AccountCodeInput = currentAccountCode;
            }
            else
            {
                _selectedAccount = await EasySession.SetReturnBasketAccountCodeAsync(accountCode);
                AccountCodeInput = _selectedAccount?.AccountCode ?? accountCode;
            }

            PageMessage = string.Empty;
            await RefreshDepotSupplierStateAsync();
            RefreshAccountProperties();
        }

        private async Task RefreshDepotSupplierStateAsync()
        {
            string accountCode = _selectedAccount?.AccountCode ?? await EasySession.GetReturnBasketAccountCodeAsync();
            string depotSupplierCode = string.IsNullOrWhiteSpace(accountCode)
                ? string.Empty
                : await EasySession.GetDepotSupplierCodeAsync(accountCode);

            ShowSaleConditions = string.IsNullOrWhiteSpace(depotSupplierCode);
        }

        private async void OnOpenBasketClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ReturnBasketView());
        }

        private async void OnSearchProductClicked(object sender, EventArgs e)
            => await SearchProductInputAsync(true, false);

        private async void OnProductEntryCompleted(object sender, EventArgs e)
            => await SearchProductInputAsync(false, true);

        private async void OnProductEntryUnfocused(object sender, FocusEventArgs e)
            => await SearchProductInputAsync(false, true);

        private async Task SearchProductInputAsync(bool showEmptyWarning, bool avoidDuplicate)
        {
            if (_selectedAccount == null)
            {
                if (showEmptyWarning || !string.IsNullOrWhiteSpace(ProductSearchText))
                    ProductMessage = "Sélectionnez un client d'abord.";
                return;
            }

            string productCode = (ProductSearchText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(productCode))
            {
                if (showEmptyWarning)
                    ProductMessage = "Veuillez renseigner une référence article.";
                return;
            }

            if (avoidDuplicate && string.Equals(_lastAutoSearchedProductCode, productCode, StringComparison.OrdinalIgnoreCase))
                return;

            if (_isAutoSearchingProduct)
                return;

            _isAutoSearchingProduct = true;
            SetLoadingState(true);

            try
            {
                ProductMessage = string.Empty;
                ClearProductSelection(false);

                var articles = await EasySession.FindReturnableArticleListAsync(productCode) ?? Array.Empty<ReturnableArticle>();
                if (articles.Length == 0)
                {
                    ProductMessage = "Aucun article trouvé.";
                    return;
                }

                if (articles.Length == 1)
                {
                    ProductSearchText = articles[0].ProductCode;
                    _lastAutoSearchedProductCode = ProductSearchText?.Trim() ?? productCode;
                    await LoadDeliveryLinesAsync(articles[0].ProductCode, true);
                    return;
                }

                ReturnableArticles.Clear();
                foreach (var article in articles)
                    ReturnableArticles.Add(article);

                _lastAutoSearchedProductCode = ProductSearchText?.Trim() ?? productCode;
                RefreshCollectionProperties();
            }
            catch (Exception ex)
            {
                ProductMessage = ex.Message;
            }
            finally
            {
                SetLoadingState(false);
                _isAutoSearchingProduct = false;
            }
        }

        private async void OnArticleSelectedClicked(object sender, EventArgs e)
        {
            var article = ((Button)sender).CommandParameter as ReturnableArticle;
            if (article == null)
                return;

            SetLoadingState(true);

            try
            {
                ProductSearchText = article.ProductCode;
                await LoadDeliveryLinesAsync(article.ProductCode, true);
            }
            catch (Exception ex)
            {
                ProductMessage = ex.Message;
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async Task LoadDeliveryLinesAsync(string productCode, bool requireValidation)
        {
            ReturnableArticles.Clear();
            DeliveryLines.Clear();
            _selectedDeliveryLine = null;
            RefreshSelectionProperties();
            RefreshCollectionProperties();

            var lines = await EasySession.GetArticlesReturnableDeliveryLinesAsync(productCode) ?? Array.Empty<DeliveryLinesStatus>();
            if (lines.Length == 0)
            {
                ProductMessage = $"Aucun bon de livraison pour le produit {productCode}.";
                return;
            }

            if (requireValidation && RequiresSupplementValidation(lines[0]))
            {
                bool accepted = await ShowConfirmationAsync(
                    "Frais de retour",
                    $"L'article '{productCode}' est un produit à forte rotation, 5% de frais de retour sur le prix net de l'article.",
                    "Accepter",
                    "Refuser");

                if (!accepted)
                {
                    ProductSearchText = string.Empty;
                    ProductMessage = string.Empty;
                    return;
                }

                await LoadDeliveryLinesAsync(productCode, false);
                return;
            }

            foreach (var line in lines)
                DeliveryLines.Add(line);

            ProductMessage = string.Empty;
            RefreshCollectionProperties();
        }

        private async void OnDeliveryLineSelectedClicked(object sender, EventArgs e)
        {
            var line = ((Button)sender).CommandParameter as DeliveryLinesStatus;
            if (line == null)
                return;

            SetLoadingState(true);

            try
            {
                decimal availableQuantity = GetAvailableQuantity(line);
                if (availableQuantity <= 0)
                {
                    bool canForceReturn = false;
                    bool isAdministrator = string.Equals(EasySession.CurrentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
                    string currentWarehouse = EasySession.CurrentAccount?.Warehouse ?? string.Empty;

                    if (isAdministrator && string.Equals(line.Warehouse, currentWarehouse, StringComparison.OrdinalIgnoreCase))
                        canForceReturn = await EasySession.IsReturnSystemsSuperManagerAsync();

                    if (!canForceReturn)
                    {
                        ProductMessage = "L’article est déjà retourné, merci de choisir un nouvel article.";
                        return;
                    }

                    QuantityText = "1";
                }
                else
                {
                    QuantityText = availableQuantity.ToString(CultureInfo.InvariantCulture);
                }

                _selectedDeliveryLine = line;
                IsGarantie = false;
                MotifText = string.Empty;

                if (ShowSaleConditions)
                {
                    SelectedPrice = line.Price;
                    SelectedDiscount = NormalizeDiscount(line.Discount);
                    SelectedAbattement = line.DisplayAbattement;
                }
                else
                {
                    SelectedPrice = "0";
                    SelectedDiscount = "0";
                    SelectedAbattement = "sans abattement";
                }

                ProductMessage = string.Empty;
                RefreshSelectionProperties();
                RefreshCollectionProperties();
            }
            catch (Exception ex)
            {
                ProductMessage = ex.Message;
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void OnGarantieChanged(object sender, CheckedChangedEventArgs e)
        {
            IsGarantie = e.Value;
            if (!IsGarantie)
                MotifText = string.Empty;
        }

        private async void OnAddBasketLineClicked(object sender, EventArgs e)
        {
            if (_selectedAccount == null)
            {
                await ModernAlertService.ShowWarningAsync("Veuillez choisir un client d'abord.");
                return;
            }

            if (_selectedDeliveryLine == null)
            {
                await ModernAlertService.ShowWarningAsync("Veuillez choisir une ligne de livraison.");
                return;
            }

            if (!TryParseDecimal(QuantityText, out decimal quantity) || quantity <= 0)
            {
                await ModernAlertService.ShowWarningAsync("Veuillez préciser une quantité valide.");
                return;
            }

            SetLoadingState(true);

            try
            {
                var basketLines = await EasySession.GetReturnBasketLinesAsync() ?? Array.Empty<ReturnBasketLine>();
                var existingWarehouse = basketLines
                    .Select(x => x.Warehouse)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                if (!string.IsNullOrWhiteSpace(existingWarehouse) &&
                    !string.Equals(existingWarehouse.Trim(), _selectedDeliveryLine.Warehouse?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    await ShowBlockingMessageAsync(
                        "Ajout impossible",
                        "Le panier contient déjà des lignes d’une autre plateforme. Veuillez valider ou vider le panier avant d’ajouter cette ligne.");
                    return;
                }

                await EasySession.AddReturnBasketLineAsync(
                    _selectedDeliveryLine.Warehouse,
                    _selectedDeliveryLine.Index,
                    quantity,
                    IsGarantie,
                    MotifText ?? string.Empty,
                    _selectedDeliveryLine.Reference);

                await RefreshBasketAsync();
                ClearProductSelection(true);
                await ModernAlertService.ShowSuccessAsync("Ligne de retour ajoutée au panier avec succès.");
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

        private async Task RefreshBasketAsync()
        {
            var lines = await EasySession.GetReturnBasketLinesAsync() ?? Array.Empty<ReturnBasketLine>();
            BasketLines.Clear();
            foreach (var line in lines)
                BasketLines.Add(line);

            RefreshBasketProperties();
        }

        private async void OnDeleteBasketLineClicked(object sender, EventArgs e)
        {
            var line = ((Button)sender).CommandParameter as ReturnBasketLine;
            if (line == null)
                return;

            bool confirm = await ShowConfirmationAsync(
                "Suppression",
                $"Êtes-vous sûr de vouloir enlever le produit {line.ProductCode} de votre panier ?",
                "Oui",
                "Non");

            if (!confirm)
                return;

            SetLoadingState(true);

            try
            {
                await EasySession.DeleteReturnBasketLineAsync(line.ReturnNoticeLineId);
                await RefreshBasketAsync();
                ClearProductSelection(true);
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

        private async void OnValidateReturnClicked(object sender, EventArgs e)
        {
            if (_selectedAccount == null)
            {
                await ModernAlertService.ShowWarningAsync("Sélectionnez un client d'abord.");
                return;
            }

            if (BasketLines.Count == 0)
            {
                await ModernAlertService.ShowWarningAsync("Ajoutez une ou plusieurs lignes à votre retour d'abord.");
                return;
            }

            SetLoadingState(true);

            try
            {
                _validatedReturnKey = await EasySession.ReturnBasketCheckOutAsync(ReturnClientCodeText ?? string.Empty);
                OnPropertyChanged(nameof(HasValidatedReturn));
                OnPropertyChanged(nameof(HasValidationSection));
                OnPropertyChanged(nameof(ValidatedReturnMessage));
                await RefreshBasketAsync();
                await ModernAlertService.ShowSuccessAsync(ValidatedReturnMessage);
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

        private async void OnOpenValidatedReturnPdfClicked(object sender, EventArgs e)
        {
            if (_validatedReturnKey == null)
                return;

            SetLoadingState(true);

            try
            {
                byte[] pdf = await EasySession.GetReturnDocumentAsync(_validatedReturnKey.ReturnNoticeCode, _validatedReturnKey.WarehouseCode);
                if (pdf == null || pdf.Length == 0)
                {
                    await ModernAlertService.ShowWarningAsync("Le document PDF est vide ou indisponible.");
                    return;
                }

                string safeReturnCode = string.Concat((_validatedReturnKey.ReturnNoticeCode ?? "Retour").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                string fileName = $"Retour_{safeReturnCode}.pdf";
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

        private bool RequiresSupplementValidation(DeliveryLinesStatus line)
        {
            if (line == null)
                return false;

            if (!string.Equals(line.CaClassification, "A", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(line.Classification, "A", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(line.Classification, "B", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!TryParseDecimal(line.Price, out decimal price))
                return false;

            decimal discount = 0;
            string discountString = (line.Discount ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(discountString) && !string.Equals(discountString, "NET", StringComparison.OrdinalIgnoreCase))
            {
                TryParseDecimal(discountString.Replace("%", string.Empty), out decimal discountPercent);
                discount = discountPercent / 100;
            }

            decimal netPrice = price * (1 - discount);
            return netPrice < 100;
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

        private string NormalizeDiscount(string discount)
        {
            if (string.IsNullOrWhiteSpace(discount))
                return string.Empty;

            try
            {
                decimal value = Convert.ToDecimal(discount.Trim(), CultureInfo.GetCultureInfo("fr-FR"));

                if (value == 0)
                    return "NET";

                return (value / 10).ToString("00.0", CultureInfo.GetCultureInfo("fr-FR")) + " %";
            }
            catch
            {
                return discount;
            }
        }

        private bool TryParseDecimal(string value, out decimal result)
        {
            value = (value ?? string.Empty).Replace("%", string.Empty).Trim();
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out result)
                || decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private void ClearSelectedAccount()
        {
            _selectedAccount = null;
            RefreshAccountProperties();
        }

        private void ClearProductSelection(bool clearSearchText)
        {
            ReturnableArticles.Clear();
            DeliveryLines.Clear();
            _selectedDeliveryLine = null;
            IsGarantie = false;
            MotifText = string.Empty;
            QuantityText = string.Empty;
            ProductMessage = string.Empty;
            SelectedPrice = string.Empty;
            SelectedDiscount = string.Empty;
            SelectedAbattement = string.Empty;

            if (clearSearchText)
                ProductSearchText = string.Empty;

            RefreshCollectionProperties();
            RefreshSelectionProperties();
        }

        private void RefreshAccountProperties()
        {
            OnPropertyChanged(nameof(HasSelectedAccount));
            OnPropertyChanged(nameof(SelectedAccountTitle));
            OnPropertyChanged(nameof(SelectedAccountAddress));
            OnPropertyChanged(nameof(SelectedAccountCity));
            OnPropertyChanged(nameof(HasSelectedAccountAddress));
            OnPropertyChanged(nameof(HasSelectedAccountCity));
        }

        private void RefreshCollectionProperties()
        {
            OnPropertyChanged(nameof(HasArticleChoices));
            OnPropertyChanged(nameof(HasDeliveryLines));
        }

        private void RefreshSelectionProperties()
        {
            OnPropertyChanged(nameof(HasSelectedDeliveryLine));
            OnPropertyChanged(nameof(SelectedDeliveryTitle));
            OnPropertyChanged(nameof(SelectedDeliveryDetail));
            OnPropertyChanged(nameof(AvailableQuantityText));
        }

        private void RefreshBasketProperties()
        {
            OnPropertyChanged(nameof(HasBasketLines));
            OnPropertyChanged(nameof(IsBasketEmpty));
            OnPropertyChanged(nameof(HasValidationSection));
            OnPropertyChanged(nameof(BasketCountText));
            OnPropertyChanged(nameof(BasketButtonText));
        }

        private async Task ShowBlockingMessageAsync(string title, string message)
        {
            await ShowConfirmationAsync(title, message, "OK", string.Empty);
        }

        private Task<bool> ShowConfirmationAsync(string title, string message, string acceptText, string cancelText)
        {
            _confirmationCompletion?.TrySetResult(false);

            ConfirmationTitle = title;
            ConfirmationMessage = message;
            ConfirmationAcceptText = acceptText;
            ConfirmationCancelText = cancelText;
            IsConfirmationCancelVisible = !string.IsNullOrWhiteSpace(cancelText);
            IsConfirmationVisible = true;

            _confirmationCompletion = new TaskCompletionSource<bool>();
            return _confirmationCompletion.Task;
        }

        private void OnConfirmationAcceptClicked(object sender, EventArgs e)
            => CloseConfirmation(true);

        private void OnConfirmationCancelClicked(object sender, EventArgs e)
            => CloseConfirmation(false);

        private void CloseConfirmation(bool result)
        {
            IsConfirmationVisible = false;
            _confirmationCompletion?.TrySetResult(result);
            _confirmationCompletion = null;
        }

        private void SetLoadingState(bool isLoading)
        {
            ApiLoadingOverlay.IsVisible = isLoading;
            ApiActivityIndicator.IsVisible = isLoading;
            ApiActivityIndicator.IsRunning = isLoading;
        }
    }
}
