using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls.Shapes;
using Services;

namespace AGRA_EASY_MOBILE
{
    public class OrderDeliveryAddressView : ContentPage
    {
        private readonly Entry _contactNameEntry = EntryField("Contact");
        private readonly Entry _line1Entry = EntryField("Adresse 1");
        private readonly Entry _line2Entry = EntryField("Adresse 2");
        private readonly Entry _zipEntry = EntryField("Code postal");
        private readonly Entry _cityEntry = EntryField("Ville");
        private readonly Entry _countryEntry = EntryField("Pays");
        private readonly Entry _telephoneEntry = EntryField("Téléphone");
        private readonly Entry _faxEntry = EntryField("Fax");
        private readonly Entry _mailEntry = EntryField("Mail");
        private readonly ActivityIndicator _apiActivityIndicator = new() { IsRunning = false, Color = Color.FromArgb("#16A34A"), WidthRequest = 42, HeightRequest = 42 };
        private readonly Grid _apiLoadingOverlay = new() { IsVisible = false, BackgroundColor = Color.FromArgb("#66000000") };
        private readonly Label _messageLabel = new() { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") };

        public OrderDeliveryAddressView()
        {
            Title = "Adresse livraison";
            BackgroundColor = Color.FromArgb("#F6F8FB");
            BuildContent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAddressAsync();
        }

        private void BuildContent()
        {
            var root = new Grid();
            var scroll = new ScrollView();
            var content = new VerticalStackLayout { Padding = new Thickness(14, 12, 14, 24), Spacing = 14 };
            scroll.Content = content;
            root.Add(scroll);

            var save = PrimaryButton("Appliquer au panier");
            save.Clicked += async (_, __) => await SaveAddressAsync();
            var defaultAddress = SecondaryButton("Adresse par défaut");
            defaultAddress.Clicked += async (_, __) => await SetDefaultAddressAsync();
            var close = SecondaryButton("Fermer");
            close.Clicked += async (_, __) => await Navigation.PopModalAsync();

            var buttons = new Grid { ColumnDefinitions = Columns("*,*"), ColumnSpacing = 8 };
            buttons.Add(save, 0, 0);
            buttons.Add(defaultAddress, 1, 0);

            content.Add(Header("Adresse de livraison", "Adresse affectée au panier commande en cours."));
            content.Add(Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    FramedEntry(_contactNameEntry),
                    FramedEntry(_line1Entry),
                    FramedEntry(_line2Entry),
                    ZipCityGrid(),
                    FramedEntry(_countryEntry),
                    FramedEntry(_telephoneEntry),
                    FramedEntry(_faxEntry),
                    FramedEntry(_mailEntry),
                    buttons,
                    close,
                    _messageLabel
                }
            }));

            _apiLoadingOverlay.Add(new Border
            {
                BackgroundColor = Colors.White,
                Padding = 22,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        _apiActivityIndicator,
                        new Label { Text = "Chargement...", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), HorizontalTextAlignment = TextAlignment.Center }
                    }
                }
            });
            root.Add(_apiLoadingOverlay);

            Content = root;
        }


        private Grid ZipCityGrid()
        {
            var grid = new Grid { ColumnDefinitions = Columns("0.55*,1.45*"), ColumnSpacing = 8 };
            grid.Add(FramedEntry(_zipEntry), 0, 0);
            grid.Add(FramedEntry(_cityEntry), 1, 0);
            return grid;
        }

        private async Task LoadAddressAsync()
        {
            await RunActionAsync(async () =>
            {
                DelivryAddress address = await EasySession.GetDelivryAddressAsync();
                if (address == null)
                    return;

                _contactNameEntry.Text = address.ContactName;
                _line1Entry.Text = address.Line1;
                _line2Entry.Text = address.Line2;
                _zipEntry.Text = address.CP;
                _cityEntry.Text = address.City;
                _countryEntry.Text = address.Country;
                _telephoneEntry.Text = address.Telephone;
                _faxEntry.Text = address.Fax;
                _mailEntry.Text = address.Mail;
            });
        }

        private async Task SaveAddressAsync()
        {
            await RunActionAsync(async () =>
            {
                await EasySession.SetDelivryAddressAsync(
                    _line1Entry.Text ?? string.Empty,
                    _line2Entry.Text ?? string.Empty,
                    _zipEntry.Text ?? string.Empty,
                    _cityEntry.Text ?? string.Empty,
                    _countryEntry.Text ?? string.Empty,
                    _contactNameEntry.Text ?? string.Empty,
                    _telephoneEntry.Text ?? string.Empty,
                    _faxEntry.Text ?? string.Empty,
                    _mailEntry.Text ?? string.Empty);

                _messageLabel.Text = "Adresse de livraison appliquée au panier.";
                _messageLabel.TextColor = Color.FromArgb("#16A34A");
            });
        }

        private async Task SetDefaultAddressAsync()
        {
            await RunActionAsync(async () =>
            {
                await EasySession.SetDefaultDelivryAddressAsync();
                _messageLabel.Text = "Adresse par défaut appliquée au panier.";
                _messageLabel.TextColor = Color.FromArgb("#16A34A");
                await LoadAddressAsync();
            });
        }

        private async Task RunActionAsync(Func<Task> action)
        {
            SetLoadingState(true);
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _messageLabel.Text = CleanServiceMessage(ex.Message);
                _messageLabel.TextColor = Color.FromArgb("#B91C1C");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            _apiLoadingOverlay.IsVisible = isLoading;
            _apiActivityIndicator.IsVisible = isLoading;
            _apiActivityIndicator.IsRunning = isLoading;
        }

        private static string CleanServiceMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            string cleaned = message.Replace("System.Web.Services.Protocols.SoapException:", string.Empty).Trim();
            int index = cleaned.IndexOf(" à ", StringComparison.Ordinal);
            if (index > 0)
                cleaned = cleaned.Substring(0, index).Trim();
            index = cleaned.IndexOf("System.Exception:", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                cleaned = cleaned.Substring(index + "System.Exception:".Length).Trim();
            index = cleaned.IndexOf("--->", StringComparison.Ordinal);
            if (index >= 0)
                cleaned = cleaned.Substring(index + 4).Trim();
            return cleaned;
        }

        private static Entry EntryField(string placeholder)
        {
            return new BorderlessEntry
            {
                Placeholder = placeholder,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                BackgroundColor = Colors.Transparent,
                HeightRequest = 38,
                FontSize = 15,
                TextColor = Color.FromArgb("#0F172A"),
                PlaceholderColor = Color.FromArgb("#94A3B8")
            };
        }

        private static Border FramedEntry(Entry entry)
        {
            entry.BackgroundColor = Colors.Transparent;
            entry.HeightRequest = 38;
            entry.HorizontalTextAlignment = TextAlignment.Center;
            entry.VerticalTextAlignment = TextAlignment.Center;
            return new Border
            {
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                Padding = new Thickness(8, 2),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 46,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = entry
            };
        }

        private static Border Header(string title, string subtitle)
        {
            return new Border
            {
                BackgroundColor = Color.FromArgb("#EAF6FF"),
                Padding = 16,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 22 },
                Content = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        new Label { Text = title, FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") },
                        new Label { Text = subtitle, FontSize = 13, TextColor = Color.FromArgb("#64748B") }
                    }
                }
            };
        }

        private static Border Card(View content)
        {
            return new Border { Content = content, BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2E8F0"), StrokeThickness = 1, Padding = 12, StrokeShape = new RoundRectangle { CornerRadius = 18 } };
        }

        private static Button PrimaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#16A34A"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44 };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#E2E8F0"), TextColor = Color.FromArgb("#0F172A"), FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44 };
        }

        private static ColumnDefinitionCollection Columns(string pattern)
        {
            var collection = new ColumnDefinitionCollection();
            foreach (var part in pattern.Split(','))
            {
                string value = part.Trim();
                collection.Add(new ColumnDefinition { Width = value == "Auto" ? GridLength.Auto : GridLength.Star });
            }
            return collection;
        }
    }
}
