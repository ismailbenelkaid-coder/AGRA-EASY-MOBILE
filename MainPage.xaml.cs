using AGRA_EASY_MOBILE.Services;

namespace AGRA_EASY_MOBILE
{
    public partial class MainPage : ContentPage
    {
        private bool _alertAnimationRunning;
        public MainPage()
        {
            InitializeComponent();
            ApplyProfileVisibility();
            ShippingWarningNotificationService.NewAlertsStateChanged += OnNewAlertsStateChanged;
            UpdateAlertIcon();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ApplyProfileVisibility();
            UpdateAlertIcon();
            if (EasySession.IsConnected)
                ShippingWarningNotificationService.RequestImmediateCheck();
        }

        private void ApplyProfileVisibility()
        {
            bool showBilling = EasySession.IsCustomerBillingManager;
            if (BillingShippingCard != null)
                BillingShippingCard.IsVisible = showBilling;
        }

        private void OnNewAlertsStateChanged(object? sender, EventArgs e) => UpdateAlertIcon();

        private void UpdateAlertIcon()
        {
            if (AlertButtonHome == null)
                return;

            AlertButtonHome.Source = ShippingWarningNotificationService.HasNewAlerts ? "bell_alert.svg" : "bell.svg";
            if (ShippingWarningNotificationService.HasNewAlerts)
                StartAlertAnimation();
            else
                StopAlertAnimation();
        }

        private void StartAlertAnimation()
        {
            if (_alertAnimationRunning || AlertButtonHome == null)
                return;

            _alertAnimationRunning = true;
            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                while (_alertAnimationRunning && AlertButtonHome != null && ShippingWarningNotificationService.HasNewAlerts)
                {
                    await AlertButtonHome.ScaleTo(1.12, 450, Easing.CubicInOut);
                    await AlertButtonHome.ScaleTo(1.0, 450, Easing.CubicInOut);
                }

                if (AlertButtonHome != null)
                    AlertButtonHome.Scale = 1.0;
            });
        }

        private void StopAlertAnimation()
        {
            _alertAnimationRunning = false;
            if (AlertButtonHome != null)
                AlertButtonHome.Scale = 1.0;
        }

        private static async Task NavigateTo(string route)
        {
            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync(route);
        }

        private async void OnNotificationsClicked(object sender, EventArgs e) => await NavigateTo("//notifications");
        private async void GoToCatalogue(object sender, EventArgs e) => await NavigateTo("//catalogue");
        private async void GoToOrderBasket(object sender, EventArgs e) => await NavigateTo("//orderBasket");
        private async void GoToOrderList(object sender, EventArgs e) => await NavigateTo("//groupe_expedition/orderList");
        private async void GoToTrackReturn(object sender, EventArgs e) => await NavigateTo("//groupe_retour/trackReturn");
        private async void GoToCustomerBilling(object sender, EventArgs e) => await NavigateTo("//groupe_facturation/customerBillingList");
        private async void GoToShippingCost(object sender, EventArgs e) => await NavigateTo("//groupe_facturation/shippingCostList");
    }
}
