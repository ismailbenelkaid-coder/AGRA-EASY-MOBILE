using System.Xml.Linq;

namespace AGRA_EASY_MOBILE.Services;

public static class PlatformEndpointResolver
{
    private const string DefaultUrl = "http://security.groupe-agra.fr/easy/services/ShoppingCartController.asmx";

    public static async Task<string> GetServiceUrlAsync(string warehouseName, bool secondary = false)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("platforms.xml");
            var doc = XDocument.Load(stream);
            var node = doc.Descendants("Warehouse").FirstOrDefault(x => IsAttributeMatch(x, "Name", warehouseName));

            var url = secondary ? node?.Attribute("Secondary")?.Value : node?.Attribute("Primary")?.Value;
            return string.IsNullOrWhiteSpace(url) ? DefaultUrl : url.Trim();
        }
        catch
        {
            return DefaultUrl;
        }
    }

    public static async Task<string> GetRequiredPrimaryServiceUrlAsync(string warehouseName)
    {
        if (string.IsNullOrWhiteSpace(warehouseName))
            throw new InvalidOperationException("La plateforme d'origine de la notification est absente.");

        using var stream = await FileSystem.OpenAppPackageFileAsync("platforms.xml");
        var doc = XDocument.Load(stream);
        var node = doc.Descendants("Warehouse").FirstOrDefault(x => IsAttributeMatch(x, "Code", warehouseName));

        var url = node?.Attribute("Primary")?.Value;
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException($"La plateforme '{warehouseName}' est introuvable dans la configuration des adresses.");

        return url.Trim();
    }

    private static bool IsAttributeMatch(XElement warehouse, string attributeName, string? value)
    {
        value = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return string.Equals(warehouse.Attribute(attributeName)?.Value?.Trim(), value, StringComparison.OrdinalIgnoreCase);
    }
}
