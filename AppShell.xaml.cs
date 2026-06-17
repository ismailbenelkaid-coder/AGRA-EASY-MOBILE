using AGRA_EASY_MOBILE.Services;

namespace AGRA_EASY_MOBILE
{
    public partial class AppShell : Shell
    {
        private bool _alertAnimationRunning;
        public AppShell()
        {
            InitializeComponent();
            ApplyAuthorizations();
            ShippingWarningNotificationService.NewAlertsStateChanged += OnNewAlertsStateChanged;
            ShippingWarningNotificationService.Start();
            UpdateAlertIcon();
        }

        private void ApplyAuthorizations()
        {
            if (FrameFacturation != null)
                FrameFacturation.IsVisible = EasySession.IsCustomerBillingManager;
        }

        private async Task NavigateToBillingRoute(string route)
        {
            if (!EasySession.IsCustomerBillingManager)
            {
                Shell.Current.FlyoutIsPresented = false;
                await Shell.Current.GoToAsync("//home");
                return;
            }

            await NavigateTo(route);
        }

        private void ToggleExpedition(object sender, EventArgs e)
        {
            SubMenuExpedition.IsVisible = !SubMenuExpedition.IsVisible;
            ArrowExpedition.Text = SubMenuExpedition.IsVisible ? "▲" : "▼";
        }

        private void ToggleRetour(object sender, EventArgs e)
        {
            SubMenuRetour.IsVisible = !SubMenuRetour.IsVisible;
            ArrowRetour.Text = SubMenuRetour.IsVisible ? "▲" : "▼";
        }

        private void ToggleFacturation(object sender, EventArgs e)
        {
            SubMenuFacturation.IsVisible = !SubMenuFacturation.IsVisible;
            ArrowFacturation.Text = SubMenuFacturation.IsVisible ? "▲" : "▼";
        }

        private async void GoToHome(object sender, EventArgs e) => await NavigateTo("//home");
        private async void GoToOrderBasket(object sender, EventArgs e) => await NavigateTo("//orderBasket");
        private async void GoToCatalogue(object sender, EventArgs e) => await NavigateTo("//catalogue");
        private async void GoToOrderList(object sender, EventArgs e) => await NavigateTo("//groupe_expedition/orderList");
        private async void GoToContainerList(object sender, EventArgs e) => await NavigateTo("//groupe_expedition/containerList");
        private async void GoToDeliveryList(object sender, EventArgs e) => await NavigateTo("//groupe_expedition/deliveryList");
        private async void GoToRuptureList(object sender, EventArgs e) => await NavigateTo("//groupe_expedition/ruptureList");
        private async void GoToNewReturn(object sender, EventArgs e) => await NavigateTo("//groupe_retour/newReturn");
        private async void GoToTrackReturn(object sender, EventArgs e) => await NavigateTo("//groupe_retour/trackReturn");
        private async void GoToDeniedReturn(object sender, EventArgs e) => await NavigateTo("//groupe_retour/deniedReturn");
        private async void GoToSupplierRefund(object sender, EventArgs e) => await NavigateTo("//groupe_retour/supplierRefund");
        private async void GoToCustomerBilling(object sender, EventArgs e) => await NavigateToBillingRoute("//groupe_facturation/customerBillingList");
        private async void GoToInvoiceWaiting(object sender, EventArgs e) => await NavigateToBillingRoute("//groupe_facturation/invoiceWaitingList");
        private async void GoToRefundWaiting(object sender, EventArgs e) => await NavigateToBillingRoute("//groupe_facturation/refundWaitingList");
        private async void GoToShippingCost(object sender, EventArgs e) => await NavigateToBillingRoute("//groupe_facturation/shippingCostList");
        private async void OnNotificationsClicked(object sender, EventArgs e) => await NavigateTo("//notifications");
        private async void OnProfileClicked(object sender, EventArgs e) => await NavigateTo("//login");

        private void OnNewAlertsStateChanged(object? sender, EventArgs e) => UpdateAlertIcon();

        private void UpdateAlertIcon()
        {
            if (AlertButtonShell == null)
                return;

            AlertButtonShell.ImageSource = ShippingWarningNotificationService.HasNewAlerts ? "bell_alert.svg" : "bell.svg";
            AlertButtonShell.TextColor = ShippingWarningNotificationService.HasNewAlerts ? Color.FromArgb("#DC2626") : Color.FromArgb("#475569");

            if (ShippingWarningNotificationService.HasNewAlerts)
                StartAlertAnimation();
            else
                StopAlertAnimation();
        }

        private void StartAlertAnimation()
        {
            if (_alertAnimationRunning || AlertButtonShell == null)
                return;

            _alertAnimationRunning = true;
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                while (_alertAnimationRunning && AlertButtonShell != null && ShippingWarningNotificationService.HasNewAlerts)
                {
                    await AlertButtonShell.ScaleTo(1.08, 450, Easing.CubicInOut);
                    await AlertButtonShell.ScaleTo(1.0, 450, Easing.CubicInOut);
                }

                if (AlertButtonShell != null)
                    AlertButtonShell.Scale = 1.0;
            });
        }

        private void StopAlertAnimation()
        {
            _alertAnimationRunning = false;
            if (AlertButtonShell != null)
            {
                AlertButtonShell.Scale = 1.0;
                AlertButtonShell.TextColor = Color.FromArgb("#475569");
            }
        }

        private async Task NavigateTo(string route)
        {
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync(route);
        }
    }
}
