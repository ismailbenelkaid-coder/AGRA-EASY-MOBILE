using Services;

namespace AGRA_EASY_MOBILE.Services;

public static class ProductBarcodeScanService
{
    public static async Task<string?> ScanAndResolveProductCodeAsync(Page owner)
    {
        ReturnableArticle? product = await ScanAndResolveProductAsync(owner);
        return product?.ProductCode?.Trim();
    }

    public static async Task<ReturnableArticle?> ScanAndResolveProductAsync(Page owner)
    {
        string? scannedCode = await BarcodeScannerPage.ShowAsync(owner);
        if (string.IsNullOrWhiteSpace(scannedCode))
            return null;

        string cleanCode = scannedCode.Trim();

        try
        {
            ReturnableArticle[] products = await EasySession.FindProductCodeListAsync(cleanCode, false, true)
                ?? Array.Empty<ReturnableArticle>();

            ReturnableArticle? resolvedProduct = products.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(resolvedProduct?.ProductCode))
                return resolvedProduct;

            return new ReturnableArticle
            {
                ProductCode = cleanCode,
                ProductLabel = string.Empty,
                SupplierName = string.Empty,
                IsActive = false
            };
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowWarningAsync(ex.Message);
            return null;
        }
    }
}
