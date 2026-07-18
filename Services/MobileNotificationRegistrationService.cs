using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AGRA_EASY_MOBILE.Services;

public static class MobileNotificationRegistrationService
{
    private const string PushTokenKey = "Firebase.PushToken";
    private const string PushTokenPlatformKey = "Firebase.PushTokenPlatform";
    private const string PushTokenLastRegistrationUtcKey = "Firebase.PushTokenLastRegistrationUtc";
    private const string InstallationIdKey = "MobileNotification.InstallationId";
    private static bool _isRegistering;

    public static string? CurrentPushToken
    {
        get => Preferences.Default.Get(PushTokenKey, string.Empty);
        set
        {
            var token = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
                Preferences.Default.Remove(PushTokenKey);
            else
                Preferences.Default.Set(PushTokenKey, token);

            Preferences.Default.Set(PushTokenPlatformKey, GetPlatformName());
        }
    }

    public static void StorePushToken(string token)
    {
        CurrentPushToken = token;
    }

    public static async Task TryRegisterCurrentDeviceAsync()
    {
        if (_isRegistering || !EasySession.IsConnected)
            return;

        var token = CurrentPushToken;
        if (string.IsNullOrWhiteSpace(token))
            return;

        _isRegistering = true;
        try
        {
            var xml = BuildRegistrationXml(token);
            var resultXml = await EasySession.RegisterMobileNotificationDeviceAsync(xml);
            if (IsRegistrationSuccess(resultXml))
                Preferences.Default.Set(PushTokenLastRegistrationUtcKey, DateTime.UtcNow.ToString("O"));
        }
        catch (MissingMethodException ex)
        {
            System.Diagnostics.Debug.WriteLine($"RegisterMobileNotificationDevice indisponible dans le contrat genere : {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Enregistrement notification mobile ignore : {ex.Message}");
        }
        finally
        {
            _isRegistering = false;
        }
    }

    private static string BuildRegistrationXml(string token)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = true,
            Indent = false
        };

        var builder = new StringBuilder();
        using var writer = XmlWriter.Create(builder, settings);
        writer.WriteStartElement("mobile_device_config");
        WriteElement(writer, "platform", GetPlatformName());
        WriteElement(writer, "notification_provider", "firebase");
        WriteElement(writer, "push_token", token);
        WriteElement(writer, "device_id", GetInstallationId());
        WriteElement(writer, "application_id", AppInfo.Current.PackageName);
        WriteElement(writer, "application_version", AppInfo.Current.VersionString);
        WriteElement(writer, "device_model", DeviceInfo.Current.Model);
        WriteElement(writer, "manufacturer", DeviceInfo.Current.Manufacturer);
        WriteElement(writer, "operating_system_version", DeviceInfo.Current.VersionString);
        WriteElement(writer, "registration_date_utc", DateTime.UtcNow.ToString("O"));
        writer.WriteEndElement();
        writer.Flush();
        return builder.ToString();
    }

    private static void WriteElement(XmlWriter writer, string name, string? value)
    {
        writer.WriteStartElement(name);
        writer.WriteString(value ?? string.Empty);
        writer.WriteEndElement();
    }

    private static string GetPlatformName()
    {
        if (OperatingSystem.IsAndroid())
            return "android";
        if (OperatingSystem.IsIOS())
            return "ios";
        if (OperatingSystem.IsWindows())
            return "windows";
        if (OperatingSystem.IsMacCatalyst())
            return "maccatalyst";

        return DeviceInfo.Current.Platform.ToString().ToLowerInvariant();
    }

    private static string GetInstallationId()
    {
        var id = Preferences.Default.Get(InstallationIdKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        id = Guid.NewGuid().ToString("N");
        Preferences.Default.Set(InstallationIdKey, id);
        return id;
    }

    private static bool IsRegistrationSuccess(string? resultXml)
    {
        if (string.IsNullOrWhiteSpace(resultXml))
            return true;

        try
        {
            var doc = XDocument.Parse(resultXml);
            var success = doc.Descendants().FirstOrDefault(x =>
                string.Equals(x.Name.LocalName, "success", StringComparison.OrdinalIgnoreCase))?.Value;

            return string.IsNullOrWhiteSpace(success)
                || success == "1"
                || success.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true;
        }
    }
}
