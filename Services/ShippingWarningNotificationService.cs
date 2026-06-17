using AGRA_EASY_MOBILE;
using AGRA_EASY_MOBILE.Models;
using Microsoft.Maui.Storage;
using Services;

namespace AGRA_EASY_MOBILE.Services
{
    public static class ShippingWarningNotificationService
    {
        private const decimal BackgroundCheckCount = 20;
        private static bool _isStarted;
        private static bool _isChecking;
        private static CancellationTokenSource? _cts;

        public static bool HasNewAlerts { get; private set; }
        public static event EventHandler? NewAlertsStateChanged;

        public static void Start()
        {
            if (_isStarted)
                return;

            _isStarted = true;
            _cts = new CancellationTokenSource();
            _ = CheckNowAsync();
            _ = RunLoopAsync(_cts.Token);
        }


        public static void RequestImmediateCheck()
        {
            if (!_isStarted)
                Start();

            _ = CheckNowAsync();
        }

        public static void Stop()
        {
            _cts?.Cancel();
            _cts = null;
            _isStarted = false;
        }

        public static string LastLaunchPreferenceKey()
        {
            var user = string.IsNullOrWhiteSpace(EasySession.CurrentLogin)
                ? Preferences.Default.Get("UserLogin", string.Empty)
                : EasySession.CurrentLogin;
            var warehouse = EasySession.CurrentAccount?.Warehouse ?? Preferences.Default.Get("SelectedWarehouseName", string.Empty);
            return $"ShippingWarning.LastLaunchDate.{SanitizeKeyPart(user)}.{SanitizeKeyPart(warehouse)}";
        }

        public static DateTime GetLastLaunchDate()
        {
            return TryGetLastLaunchDate(out var result) ? result : DateTime.MinValue;
        }

        public static bool TryGetLastLaunchDate(out DateTime value)
        {
            value = DateTime.MinValue;
            var raw = Preferences.Default.Get(LastLaunchPreferenceKey(), string.Empty);
            return !string.IsNullOrWhiteSpace(raw)
                && DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out value);
        }

        public static DateTime ReadAndUpdateLastLaunchDate()
        {
            var previous = GetLastLaunchDate();
            Preferences.Default.Set(LastLaunchPreferenceKey(), DateTime.Now.ToString("O"));
            SetHasNewAlerts(false);
            return previous;
        }

        public static async Task CheckNowAsync()
        {
            if (_isChecking || !EasySession.IsConnected)
                return;

            _isChecking = true;
            try
            {
                var currentPage = Shell.Current?.CurrentPage;
                if (currentPage is ShippingWarningListView || currentPage is ShippingWarningDetailView)
                {
                    SetHasNewAlerts(false);
                    return;
                }

                var filter = GlobalState.PeekShippingWarningFilter() ?? GlobalState.CurrentShippingWarningFilter;
                if (EasySession.IsClient && !string.IsNullOrWhiteSpace(EasySession.CurrentAccount?.AccountCode))
                    filter.AccountCode = EasySession.CurrentAccount.AccountCode;

                var list = await EasySession.GetShippingWarningListAsync(filter, 0, BackgroundCheckCount) ?? Array.Empty<ShippingWarning>();
                var hasKnownLastLaunch = TryGetLastLaunchDate(out var lastLaunch);
                var hasNew = hasKnownLastLaunch
                    ? list.Any(x => x != null && x.CreationDate > lastLaunch)
                    : list.Any(x => x != null);
                SetHasNewAlerts(hasNew);
            }
            catch
            {
                // Surveillance non bloquante : ne pas perturber l'utilisateur si le contrôle périodique échoue.
            }
            finally
            {
                _isChecking = false;
            }
        }

        public static void SetHasNewAlerts(bool value)
        {
            if (HasNewAlerts == value)
                return;

            HasNewAlerts = value;
            MainThread.BeginInvokeOnMainThread(() => NewAlertsStateChanged?.Invoke(null, EventArgs.Empty));
        }

        private static async Task RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await CheckNowAsync();
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        private static string SanitizeKeyPart(string? value)
        {
            var text = value ?? string.Empty;
            foreach (var ch in Path.GetInvalidFileNameChars())
                text = text.Replace(ch, '_');
            return string.IsNullOrWhiteSpace(text) ? "default" : text.Trim();
        }
    }
}
