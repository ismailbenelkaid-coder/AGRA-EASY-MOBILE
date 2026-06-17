using AGRA_EASY_MOBILE.Services;
using Services;
using System.Collections.ObjectModel;

namespace AGRA_EASY_MOBILE
{
    public partial class ReturnBasketView : ContentPage
    {
        private ReturnNoticeKey? _validatedReturnKey;
        private TaskCompletionSource<bool>? _confirmationCompletion;
        private bool _showSaleConditions = true;
        private bool _canEditAccount;
        private bool _isOpeningClientSelection;
        private bool _isAutoValidatingAccount;
        private string _accountCodeInput = string.Empty;
        private ClientAccount? _selectedAccount;
        private bool _isConfirmationVisible;
        private bool _isConfirmationCancelVisible;
        private string _returnClientCodeText = string.Empty;
        private string _confirmationTitle = string.Empty;
        private string _confirmationMessage = string.Empty;
        private string _confirmationAcceptText = string.Empty;
        private string _confirmationCancelText = string.Empty;

        public ObservableCollection<ReturnBasketLine> BasketLines { get; } = new();

        public string ReturnClientCodeText
        {
            get => _returnClientCodeText;
            set
            {
                _returnClientCodeText = value;
                OnPropertyChanged();
            }
        }

        public string AccountCodeInput
        {
            get => _accountCodeInput;
            set
            {
                _accountCodeInput = value;
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
                RefreshAccountProperties();
            }
        }

        public bool IsAccountSelectionEditable => CanEditAccount && BasketLines.Count == 0;

        public bool IsAccountSelectionLocked => CanEditAccount && BasketLines.Count > 0 && HasAssignedAccount;

        public bool HasAssignedAccount
            => !string.IsNullOrWhiteSpace(_selectedAccount?.AccountCode) || !string.IsNullOrWhiteSpace(AccountCodeInput);

        public bool HasNoAssignedAccount => CanEditAccount && !HasAssignedAccount;

        public string SelectedAccountTitle
        {
            get
            {
                string code = (_selectedAccount?.AccountCode ?? AccountCodeInput ?? string.Empty).Trim();
                string name = (_selectedAccount?.AccountName ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(code))
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                    return $"Client : {code}";

                return $"Client : {code} - {name}";
            }
        }

        public string AccountSelectionLockedText
            => "Le panier contient déjà des lignes : le client ne peut plus être modifié.";

        public bool ShowSaleConditions
        {
            get => _showSaleConditions;
            set
            {
                _showSaleConditions = value;
                OnPropertyChanged();
            }
        }

        public bool HasBasketLines => BasketLines.Count > 0;
        public bool IsBasketEmpty => BasketLines.Count == 0;
        public bool HasValidatedReturn => _validatedReturnKey != null;
        public bool HasValidationSection => HasBasketLines || HasValidatedReturn;

        public string BasketCountText
            => BasketLines.Count <= 1 ? $"{BasketLines.Count} ligne" : $"{BasketLines.Count} lignes";

        public string ValidatedReturnMessage
            => _validatedReturnKey == null ? string.Empty : $"Retour validé avec succès. N° retour : {_validatedReturnKey.ReturnNoticeCode}";

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

        public ReturnBasketView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializePageAsync();
        }

        private async Task InitializePageAsync()
        {
            SetLoadingState(true);

            try
            {
                await RefreshBasketAsync();
                await InitializeAccountSectionAsync();
                await RefreshSaleConditionsAsync();
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

        private async Task RefreshSaleConditionsAsync()
        {
            string accountCode = await EasySession.GetReturnBasketAccountCodeAsync();
            string depotSupplierCode = string.IsNullOrWhiteSpace(accountCode)
                ? string.Empty
                : await EasySession.GetDepotSupplierCodeAsync(accountCode);

            ShowSaleConditions = string.IsNullOrWhiteSpace(depotSupplierCode);
        }

        private async Task RefreshBasketAsync()
        {
            var lines = await EasySession.GetReturnBasketLinesAsync() ?? Array.Empty<ReturnBasketLine>();
            BasketLines.Clear();

            foreach (var line in lines)
                BasketLines.Add(line);

            RefreshBasketProperties();
        }

        private async Task InitializeAccountSectionAsync()
        {
            bool isAdministrator = string.Equals(EasySession.CurrentAccount?.Type, "Administrateur", StringComparison.OrdinalIgnoreCase);
            CanEditAccount = isAdministrator;

            if (!CanEditAccount)
            {
                _selectedAccount = null;
                AccountCodeInput = string.Empty;
                RefreshAccountProperties();
                return;
            }

            string accountCode = await EasySession.GetReturnBasketAccountCodeAsync();
            AccountCodeInput = accountCode ?? string.Empty;
            _selectedAccount = await FindAccountForDisplayAsync(AccountCodeInput);

            RefreshAccountProperties();
        }

        private async Task<ClientAccount?> FindAccountForDisplayAsync(string accountCode)
        {
            accountCode = (accountCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(accountCode))
                return null;

            try
            {
                var accounts = await EasySession.FindClientAccountAsync(accountCode) ?? Array.Empty<ClientAccount>();
                return accounts.FirstOrDefault(x => string.Equals(x.AccountCode?.Trim(), accountCode, StringComparison.OrdinalIgnoreCase))
                    ?? accounts.FirstOrDefault()
                    ?? new ClientAccount { AccountCode = accountCode };
            }
            catch
            {
                return new ClientAccount { AccountCode = accountCode };
            }
        }

        private async void OnValidateAccountClicked(object sender, EventArgs e)
            => await ShowClientSelectionForReturnBasketAsync();

        private async void OnAccountCodeEntryCompleted(object sender, EventArgs e)
            => await ValidateAccountInputAsync(true);

        private async void OnAccountCodeEntryUnfocused(object sender, FocusEventArgs e)
            => await ValidateAccountInputAsync(false);

        private async Task ShowClientSelectionForReturnBasketAsync()
        {
            if (!IsAccountSelectionEditable)
            {
                if (CanEditAccount && BasketLines.Count > 0)
                    await ModernAlertService.ShowWarningAsync("Le panier contient déjà des lignes. Veuillez valider ou vider le panier avant de changer de client.");
                return;
            }

            if (_isOpeningClientSelection)
                return;

            _isOpeningClientSelection = true;

            try
            {
                ClientAccount? selectedAccount = await ClientAccountSelectionPage.ShowAsync(
                    this,
                    "Choisir un client",
                    (AccountCodeInput ?? string.Empty).Trim());

                if (selectedAccount == null)
                    return;

                string accountCode = (selectedAccount.AccountCode ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(accountCode))
                    return;

                await AssignReturnBasketAccountAsync(accountCode, selectedAccount);
            }
            finally
            {
                _isOpeningClientSelection = false;
            }
        }

        private async Task ValidateAccountInputAsync(bool showEmptyWarning)
        {
            if (!IsAccountSelectionEditable)
                return;

            string accountCode = (AccountCodeInput ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(accountCode))
            {
                if (showEmptyWarning)
                    await ModernAlertService.ShowWarningAsync("Veuillez renseigner un code client.");
                return;
            }

            if (_isAutoValidatingAccount)
                return;

            _isAutoValidatingAccount = true;
            SetLoadingState(true);

            try
            {
                await AssignReturnBasketAccountAsync(accountCode, null);
            }
            catch (Exception ex)
            {
                _selectedAccount = null;
                RefreshAccountProperties();
                await ModernAlertService.ShowErrorAsync(ex.Message);
            }
            finally
            {
                SetLoadingState(false);
                _isAutoValidatingAccount = false;
            }
        }

        private async Task AssignReturnBasketAccountAsync(string accountCode, ClientAccount? selectedAccount)
        {
            if (BasketLines.Count > 0)
            {
                await ModernAlertService.ShowWarningAsync("Le panier contient déjà des lignes. Veuillez valider ou vider le panier avant de changer de client.");
                return;
            }

            ClientAccount assignedAccount = await EasySession.SetReturnBasketAccountCodeAsync(accountCode);
            _selectedAccount = assignedAccount ?? selectedAccount ?? await FindAccountForDisplayAsync(accountCode);
            AccountCodeInput = _selectedAccount?.AccountCode ?? accountCode;

            await RefreshSaleConditionsAsync();
            RefreshAccountProperties();
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
                await ModernAlertService.ShowSuccessAsync("Ligne supprimée du panier.");
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
            string accountCode = await EasySession.GetReturnBasketAccountCodeAsync();
            if (string.IsNullOrWhiteSpace(accountCode))
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

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void RefreshBasketProperties()
        {
            OnPropertyChanged(nameof(HasBasketLines));
            OnPropertyChanged(nameof(IsBasketEmpty));
            OnPropertyChanged(nameof(HasValidationSection));
            OnPropertyChanged(nameof(BasketCountText));
            RefreshAccountProperties();
        }

        private void RefreshAccountProperties()
        {
            OnPropertyChanged(nameof(IsAccountSelectionEditable));
            OnPropertyChanged(nameof(IsAccountSelectionLocked));
            OnPropertyChanged(nameof(HasAssignedAccount));
            OnPropertyChanged(nameof(HasNoAssignedAccount));
            OnPropertyChanged(nameof(SelectedAccountTitle));
            OnPropertyChanged(nameof(AccountSelectionLockedText));
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
