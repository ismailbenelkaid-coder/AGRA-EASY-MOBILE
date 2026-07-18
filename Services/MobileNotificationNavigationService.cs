using AGRA_EASY_MOBILE;

namespace AGRA_EASY_MOBILE.Services;

public static class MobileNotificationNavigationService
{
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
            var page = new ShippingWarningDetailView(shippingWarningId.Trim(), originalWarehouse.Trim(), shortDescription);
            if (Shell.Current != null)
                await Shell.Current.Navigation.PushAsync(page);
            else if (Application.Current?.MainPage?.Navigation != null)
                await Application.Current.MainPage.Navigation.PushAsync(page);
        });
    }
}
