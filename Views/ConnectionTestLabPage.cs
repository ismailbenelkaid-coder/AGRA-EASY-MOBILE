using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using Microsoft.Maui.Controls.Shapes;
using Services;

namespace AGRA_EASY_MOBILE;

public class ConnectionTestLabPage : ContentPage
{
    private const string SoftwareName = "MAUI_AGRA_V10";

    private readonly string _userName;
    private readonly string _password;
    private readonly string _warehouse;
    private readonly bool _useSecondaryLink;
    private readonly Editor _logEditor;
    private readonly Label _statusLabel;
    private readonly Button _runButton;
    private readonly Button _copyButton;
    private readonly Button _closeButton;
    private string _lastLog = string.Empty;

    public ConnectionTestLabPage(string userName, string password, string warehouse, bool useSecondaryLink)
    {
        _userName = userName ?? string.Empty;
        _password = password ?? string.Empty;
        _warehouse = string.IsNullOrWhiteSpace(warehouse) ? "Meyzieu" : warehouse;
        _useSecondaryLink = useSecondaryLink;

        Title = "Diagnostic iOS";
        BackgroundColor = Color.FromArgb("#F6F8FB");

        var titleLabel = new Label
        {
            Text = "Tests connexion SOAP",
            FontAttributes = FontAttributes.Bold,
            FontSize = 20,
            TextColor = Color.FromArgb("#0F172A")
        };

        var subtitleLabel = new Label
        {
            Text = "Lancez les tests puis copiez le journal complet.",
            FontSize = 13,
            TextColor = Color.FromArgb("#64748B")
        };

        _statusLabel = new Label
        {
            Text = "Pret.",
            FontSize = 12,
            TextColor = Color.FromArgb("#64748B")
        };

        _logEditor = new Editor
        {
            IsReadOnly = true,
            AutoSize = EditorAutoSizeOption.Disabled,
            FontFamily = "monospace",
            FontSize = 12,
            TextColor = Color.FromArgb("#111827"),
            BackgroundColor = Colors.White
        };

        _runButton = new Button
        {
            Text = "LANCER LES TESTS",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 48
        };
        _runButton.Clicked += OnRunTestsClicked;

        _copyButton = new Button
        {
            Text = "COPIER",
            BackgroundColor = Color.FromArgb("#0F766E"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 48,
            IsEnabled = false
        };
        _copyButton.Clicked += OnCopyClicked;

        _closeButton = new Button
        {
            Text = "FERMER",
            BackgroundColor = Color.FromArgb("#16A34A"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 48
        };
        _closeButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

        var header = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { titleLabel, subtitleLabel, _statusLabel }
        };

        var logBorder = new Border
        {
            Margin = new Thickness(0, 14, 0, 14),
            Stroke = Color.FromArgb("#CBD5E1"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Content = _logEditor
        };

        var buttonGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8,
            Children = { _runButton, _copyButton, _closeButton }
        };
        Grid.SetColumn(_copyButton, 1);
        Grid.SetColumn(_closeButton, 2);

        var root = new Grid
        {
            Padding = new Thickness(18, 18, 18, 14),
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children = { header, logBorder, buttonGrid }
        };
        Grid.SetRow(logBorder, 1);
        Grid.SetRow(buttonGrid, 2);

        Content = root;
    }

    public static async Task ShowAsync(Page owner, string userName, string password, string warehouse, bool useSecondaryLink)
    {
        var currentPage = Application.Current?.Windows.FirstOrDefault()?.Page ?? owner;
        await currentPage.Navigation.PushModalAsync(new ConnectionTestLabPage(userName, password, warehouse, useSecondaryLink));
    }

    private async void OnRunTestsClicked(object? sender, EventArgs e)
    {
        _runButton.IsEnabled = false;
        _copyButton.IsEnabled = false;
        _statusLabel.Text = "Tests en cours...";
        _lastLog = "Execution des tests en cours...";
        _logEditor.Text = _lastLog;

        try
        {
            _lastLog = await RunAllTestsAsync();
            _logEditor.Text = _lastLog;
            _copyButton.IsEnabled = true;
            _statusLabel.Text = "Tests termines. Copiez le journal complet.";
        }
        catch (Exception ex)
        {
            _lastLog = "Erreur inattendue du laboratoire de test." + Environment.NewLine + ex;
            _logEditor.Text = _lastLog;
            _copyButton.IsEnabled = true;
            _statusLabel.Text = "Tests interrompus par une erreur.";
        }
        finally
        {
            _runButton.IsEnabled = true;
        }
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(_lastLog);
        _statusLabel.Text = "Journal copie dans le presse-papiers.";
    }

    private async Task<string> RunAllTestsAsync()
    {
        var log = new StringBuilder();
        var url = await ResolveConfiguredUrlAsync();
        var uri = new Uri(url);

        AppendHeader(log, "Contexte appareil et application");
        AppendLine(log, "Date locale", DateTime.Now.ToString("O"));
        AppendLine(log, "Plateforme MAUI", DeviceInfo.Current.Platform.ToString());
        AppendLine(log, "Version OS", DeviceInfo.Current.VersionString);
        AppendLine(log, "Fabricant", DeviceInfo.Current.Manufacturer);
        AppendLine(log, "Modele", DeviceInfo.Current.Model);
        AppendLine(log, "Application", $"{AppInfo.Current.Name} {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})");
        AppendLine(log, "NetworkAccess", Connectivity.Current.NetworkAccess.ToString());
        AppendLine(log, "ConnectionProfiles", string.Join(", ", Connectivity.Current.ConnectionProfiles));

        AppendHeader(log, "Configuration testee");
        AppendLine(log, "Entrepot", _warehouse);
        AppendLine(log, "Lien secondaire", _useSecondaryLink.ToString());
        AppendLine(log, "URL", url);
        AppendLine(log, "Scheme", uri.Scheme);
        AppendLine(log, "Host", uri.Host);
        AppendLine(log, "Port", uri.Port.ToString());
        AppendLine(log, "Login", MaskUser(_userName));
        AppendLine(log, "Mot de passe fourni", string.IsNullOrWhiteSpace(_password) ? "Non" : "Oui");

        await RunStepAsync(log, "DNS", () => TestDnsAsync(uri.Host));
        await RunStepAsync(log, "TCP + TLS brut", () => TestTlsAsync(uri.Host, uri.Port));
        await RunStepAsync(log, "HTTP GET ASMX", () => TestHttpGetAsync(url));
        await RunStepAsync(log, "HTTP GET WSDL", () => TestHttpGetAsync(url + "?WSDL"));
        await RunStepAsync(log, "SOAP manuel Connexion", () => TestManualSoapConnectionAsync(url));
        await RunStepAsync(log, "Client SOAP genere Connexion", () => TestGeneratedSoapConnectionAsync(url));

        AppendHeader(log, "Fin");
        log.AppendLine("Copiez tout ce journal et renvoyez-le pour analyse.");
        return log.ToString();
    }

    private async Task RunStepAsync(StringBuilder log, string title, Func<Task<string>> action)
    {
        AppendHeader(log, title);
        var sw = Stopwatch.StartNew();

        try
        {
            var details = await action();
            sw.Stop();
            AppendLine(log, "Resultat", "OK");
            AppendLine(log, "Duree", $"{sw.ElapsedMilliseconds} ms");
            if (!string.IsNullOrWhiteSpace(details))
            {
                log.AppendLine("Details :");
                log.AppendLine(details.TrimEnd());
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            AppendLine(log, "Resultat", "ECHEC");
            AppendLine(log, "Duree", $"{sw.ElapsedMilliseconds} ms");
            log.AppendLine("Exception :");
            AppendException(log, ex, 0);
        }
    }

    private async Task<string> TestDnsAsync(string host)
    {
        var addresses = await Dns.GetHostAddressesAsync(host);
        if (addresses.Length == 0)
            throw new InvalidOperationException("Aucune adresse IP retournee.");

        var details = addresses
            .Select(x => $"{x} ({x.AddressFamily})");
        return "DNS " + host + " => " + string.Join(", ", details);
    }

    private async Task<string> TestTlsAsync(string host, int port)
    {
        var sb = new StringBuilder();
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(host, port);

        SslPolicyErrors sslErrors = SslPolicyErrors.None;
        X509Chain? callbackChain = null;
        X509Certificate? callbackCertificate = null;

        using var ssl = new SslStream(
            tcp.GetStream(),
            false,
            (_, certificate, chain, errors) =>
            {
                callbackCertificate = certificate;
                callbackChain = chain;
                sslErrors = errors;
                return true;
            });

        await ssl.AuthenticateAsClientAsync(host);

        var cert = callbackCertificate == null ? null : new X509Certificate2(callbackCertificate);
        AppendLine(sb, "Protocol", ssl.SslProtocol.ToString());
        AppendLine(sb, "SslPolicyErrors", sslErrors.ToString());
        AppendLine(sb, "CipherAlgorithm", ssl.CipherAlgorithm.ToString());
        AppendLine(sb, "CipherStrength", ssl.CipherStrength.ToString());
        AppendLine(sb, "HashAlgorithm", ssl.HashAlgorithm.ToString());
        AppendLine(sb, "KeyExchangeAlgorithm", ssl.KeyExchangeAlgorithm.ToString());
        AppendLine(sb, "KeyExchangeStrength", ssl.KeyExchangeStrength.ToString());
        AppendLine(sb, "Certificat subject", cert?.Subject ?? "(absent)");
        AppendLine(sb, "Certificat issuer", cert?.Issuer ?? "(absent)");
        AppendLine(sb, "Certificat debut", cert?.NotBefore.ToString("O") ?? "(absent)");
        AppendLine(sb, "Certificat fin", cert?.NotAfter.ToString("O") ?? "(absent)");
        AppendLine(sb, "Certificat thumbprint", cert?.Thumbprint ?? "(absent)");

        if (callbackChain != null)
        {
            sb.AppendLine("Chaine :");
            foreach (var element in callbackChain.ChainElements)
            {
                var statuses = element.ChainElementStatus.Length == 0
                    ? "OK"
                    : string.Join("; ", element.ChainElementStatus.Select(x => x.Status + " " + x.StatusInformation?.Trim()));
                sb.AppendLine("- " + element.Certificate.Subject);
                sb.AppendLine("  issuer: " + element.Certificate.Issuer);
                sb.AppendLine("  fin: " + element.Certificate.NotAfter.ToString("O"));
                sb.AppendLine("  status: " + statuses);
            }
        }

        return sb.ToString();
    }

    private async Task<string> TestHttpGetAsync(string url)
    {
        using var handler = new HttpClientHandler();
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        using var response = await http.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var preview = body.Length <= 350 ? body : body.Substring(0, 350);
        return $"Status: {(int)response.StatusCode} {response.ReasonPhrase}{Environment.NewLine}"
            + $"Content-Type: {response.Content.Headers.ContentType}{Environment.NewLine}"
            + $"Longueur: {body.Length}{Environment.NewLine}"
            + $"Debut reponse: {preview.Replace(Environment.NewLine, " ")}";
    }

    private async Task<string> TestManualSoapConnectionAsync(string url)
    {
        EnsureCredentials();

        var soapBody = BuildSoapConnectionBody(_userName, _password);
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(soapBody, Encoding.UTF8, "text/xml");
        request.Headers.TryAddWithoutValidation("SOAPAction", "\"http://groupe-agra/Connexion\"");

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var response = await http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        if (!body.Contains("ConnexionResponse", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Reponse SOAP recue, mais ConnexionResponse absent.");

        return ExtractSoapSummary(body, "SOAP manuel", response.StatusCode, response.ReasonPhrase);
    }

    private async Task<string> TestGeneratedSoapConnectionAsync(string url)
    {
        EnsureCredentials();

        var binding = url.StartsWith("https", StringComparison.OrdinalIgnoreCase)
            ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            : new BasicHttpBinding(BasicHttpSecurityMode.None);

        binding.MaxReceivedMessageSize = int.MaxValue;
        binding.AllowCookies = true;

        var client = new ShoppingCartControllerSoapClient(binding, new EndpointAddress(url));

        try
        {
            var account = await client.ConnexionAsync(_userName, _password, SoftwareName);
            if (account == null)
                throw new InvalidOperationException("ConnexionAsync a retourne null.");

            return "Type: " + (account.Type ?? string.Empty) + Environment.NewLine
                + "AccountCode: " + (account.AccountCode ?? string.Empty) + Environment.NewLine
                + "Warehouse: " + (account.Warehouse ?? string.Empty);
        }
        finally
        {
            try
            {
                await client.CloseAsync();
            }
            catch
            {
                try
                {
                    client.Abort();
                }
                catch
                {
                }
            }
        }
    }

    private static string ExtractSoapSummary(string body, string label, HttpStatusCode statusCode, string? reasonPhrase)
    {
        var sb = new StringBuilder();
        AppendLine(sb, "Status", $"{(int)statusCode} {reasonPhrase}");
        AppendLine(sb, "Longueur", body.Length.ToString());
        AppendLine(sb, "Contient ConnexionResponse", body.Contains("ConnexionResponse", StringComparison.OrdinalIgnoreCase).ToString());
        AppendLine(sb, "Contient ConnexionResult", body.Contains("ConnexionResult", StringComparison.OrdinalIgnoreCase).ToString());

        try
        {
            var doc = XDocument.Parse(body);
            XNamespace ns = "http://groupe-agra/";
            var result = doc.Descendants(ns + "ConnexionResult").FirstOrDefault();
            if (result != null)
            {
                AppendLine(sb, label + " Type", result.Element(ns + "Type")?.Value ?? string.Empty);
                AppendLine(sb, label + " AccountCode", result.Element(ns + "AccountCode")?.Value ?? string.Empty);
                AppendLine(sb, label + " Warehouse", result.Element(ns + "Warehouse")?.Value ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            AppendLine(sb, "Analyse XML", ex.Message);
        }

        var preview = body.Length <= 500 ? body : body.Substring(0, 500);
        AppendLine(sb, "Debut reponse", preview.Replace(Environment.NewLine, " "));
        return sb.ToString();
    }

    private async Task<string> ResolveConfiguredUrlAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("platforms.xml");
            var doc = XDocument.Load(stream);
            var node = doc.Descendants("Warehouse").FirstOrDefault(x => x.Attribute("Name")?.Value == _warehouse);
            var resolved = (_useSecondaryLink ? node?.Attribute("Secondary")?.Value : node?.Attribute("Primary")?.Value)
                ?? "https://security.groupe-agra.fr/easy/services/ShoppingCartController.asmx";

            if (resolved.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && resolved.Contains("security.groupe-agra.fr", StringComparison.OrdinalIgnoreCase))
                resolved = "https://" + resolved.Substring("http://".Length);

            return resolved;
        }
        catch
        {
            return "https://security.groupe-agra.fr/easy/services/ShoppingCartController.asmx";
        }
    }

    private void EnsureCredentials()
    {
        if (string.IsNullOrWhiteSpace(_userName) || string.IsNullOrWhiteSpace(_password))
            throw new InvalidOperationException("Login ou mot de passe absent. Renseignez-les sur la page de parametres avant de lancer le test.");
    }

    private static string BuildSoapConnectionBody(string userName, string password)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <Connexion xmlns=""http://groupe-agra/"">
      <UserName>{SecurityElementEscape(userName)}</UserName>
      <Password>{SecurityElementEscape(password)}</Password>
      <SoftwareName>{SoftwareName}</SoftwareName>
    </Connexion>
  </soap:Body>
</soap:Envelope>";
    }

    private static string SecurityElementEscape(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static void AppendHeader(StringBuilder log, string title)
    {
        log.AppendLine();
        log.AppendLine("==== " + title + " ====");
    }

    private static void AppendLine(StringBuilder log, string label, string value)
    {
        log.AppendLine(label + " : " + value);
    }

    private static void AppendException(StringBuilder log, Exception exception, int level)
    {
        var prefix = level == 0 ? string.Empty : $"Inner {level} - ";
        log.AppendLine(prefix + "Type : " + exception.GetType().FullName);
        log.AppendLine(prefix + "Message : " + exception.Message);
        log.AppendLine(prefix + "StackTrace :");
        log.AppendLine(exception.StackTrace ?? "(aucune stack trace)");

        if (exception.InnerException != null)
        {
            log.AppendLine();
            AppendException(log, exception.InnerException, level + 1);
        }
    }

    private static string MaskUser(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return "(vide)";

        if (userName.Length <= 2)
            return "**";

        return userName[0] + new string('*', Math.Max(1, userName.Length - 2)) + userName[^1];
    }
}
