using Plugin.FirebasePushNotifications;

namespace AGRA_EASY_MOBILE.Services;

public static class FirebasePushNotificationService
{
    private static bool _initialized;
    private static bool _registrationStarted;

    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        if (!CrossFirebasePushNotification.IsSupported)
            return;

        var push = IFirebasePushNotification.Current;
        push.TokenRefreshed += OnTokenRefreshed;
        push.NotificationOpened += OnNotificationOpened;

        var token = push.Token;
        if (!string.IsNullOrWhiteSpace(token))
            MobileNotificationRegistrationService.StorePushToken(token);

        _ = EnsureRegisteredAsync();
    }

    public static async Task EnsureRegisteredAsync()
    {
        if (_registrationStarted || !CrossFirebasePushNotification.IsSupported)
            return;

        _registrationStarted = true;
        try
        {
            await INotificationPermissions.Current.RequestPermissionAsync();
            await IFirebasePushNotification.Current.RegisterForPushNotificationsAsync();

            var token = IFirebasePushNotification.Current.Token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                MobileNotificationRegistrationService.StorePushToken(token);
                await MobileNotificationRegistrationService.TryRegisterCurrentDeviceAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Initialisation Firebase Push ignoree : {ex.Message}");
        }
        finally
        {
            _registrationStarted = false;
        }
    }

    private static void OnTokenRefreshed(object? sender, EventArgs e)
    {
        var token = TryReadStringProperty(e, "Token") ?? IFirebasePushNotification.Current.Token;
        if (string.IsNullOrWhiteSpace(token))
            return;

        MobileNotificationRegistrationService.StorePushToken(token);
        _ = MobileNotificationRegistrationService.TryRegisterCurrentDeviceAsync();
    }

    private static void OnNotificationOpened(object? sender, EventArgs e)
    {
        var data = TryReadData(e);
        if (data.Count == 0)
            return;

        _ = MobileNotificationNavigationService.OpenShippingWarningAsync(data);
    }

    private static string? TryReadStringProperty(object source, string propertyName)
    {
        try
        {
            return source.GetType().GetProperty(propertyName)?.GetValue(source)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, string> TryReadData(object source)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var raw = TryReadProperty(source, "Data")
            ?? TryReadProperty(TryReadProperty(source, "Notification"), "Data");

        if (raw is IEnumerable<KeyValuePair<string, string>> stringPairs)
        {
            foreach (var pair in stringPairs)
                result[pair.Key] = pair.Value;
        }
        else if (raw is IEnumerable<KeyValuePair<string, object>> objectPairs)
        {
            foreach (var pair in objectPairs)
                result[pair.Key] = pair.Value?.ToString() ?? string.Empty;
        }
        else if (raw is IEnumerable<(string Key, string Value)> tuplePairs)
        {
            foreach (var pair in tuplePairs)
                result[pair.Key] = pair.Value;
        }

        return result;
    }

    private static object? TryReadProperty(object? source, string propertyName)
    {
        if (source == null)
            return null;

        try
        {
            return source.GetType().GetProperty(propertyName)?.GetValue(source);
        }
        catch
        {
            return null;
        }
    }
}
