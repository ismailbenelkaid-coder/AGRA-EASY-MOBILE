using AGRA_EASY_MOBILE;

namespace AGRA_EASY_MOBILE.Services;

public static class MobileNotificationNavigationService
{
    private static PendingShippingWarningNotification? _pendingShippingWarningNotification;

    public static Task OpenShippingWarningAsync(IDictionary<string, string> data)
    {
        data.TryGetValue("type", out var type);
        if (!string.Equals(type, "shipping-warning", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        data.TryGetValue("shipping_warning_id", out var shippingWarningId);
        data.TryGetValue("original_warehouse", out var originalWarehouse);
        data.TryGetValue("short_description", out var shortDescription);

        return OpenShippingWarningAsync(shippingWarningId, originalWarehouse, shortDescription);
    }

    public static async Task OpenShippingWarningAsync(string? shippingWarningId, string? originalWarehouse, string? shortDescription = null)
    {
        if (string.IsNullOrWhiteSpace(shippingWarningId))
            throw new InvalidOperationException("La notification ne contient pas l'identifiant de l'avertissement livraison.");

        if (string.IsNullOrWhiteSpace(originalWarehouse))
            throw new InvalidOperationException("La notification ne contient pas la plateforme d'origine.");

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var notification = new PendingShippingWarningNotification(shippingWarningId.Trim(), originalWarehouse.Trim(), shortDescription);
            if (!await TryOpenShippingWarningAsync(notification))
                _pendingShippingWarningNotification = notification;
        });
    }

    public static async Task OpenPendingNotificationAsync()
    {
        var notification = _pendingShippingWarningNotification;
        if (notification == null)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (await TryOpenShippingWarningAsync(notification))
                _pendingShippingWarningNotification = null;
        });
    }

    private static async Task<bool> TryOpenShippingWarningAsync(PendingShippingWarningNotification notification)
    {
        if (Shell.Current == null)
            return false;

        var page = new ShippingWarningDetailView(
            notification.ShippingWarningId,
            notification.OriginalWarehouse,
            notification.ShortDescription);

        await Shell.Current.Navigation.PushAsync(page);
        return true;
    }

    private sealed class PendingShippingWarningNotification
    {
        public PendingShippingWarningNotification(string shippingWarningId, string originalWarehouse, string? shortDescription)
        {
            ShippingWarningId = shippingWarningId;
            OriginalWarehouse = originalWarehouse;
            ShortDescription = shortDescription;
        }

        public string ShippingWarningId { get; }
        public string OriginalWarehouse { get; }
        public string? ShortDescription { get; }
    }
}
