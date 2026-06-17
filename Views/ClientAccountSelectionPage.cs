using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.ApplicationModel;
using Services;
using System.Collections.ObjectModel;

namespace AGRA_EASY_MOBILE
{
    public class ClientAccountSelectionPage : ContentPage
    {
        private readonly Entry _filterEntry = new BorderlessEntry
        {
            Placeholder = "Filtre client",
            TextColor = Color.FromArgb("#0F172A"),
            PlaceholderColor = Color.FromArgb("#94A3B8"),
            BackgroundColor = Colors.Transparent,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            HeightRequest = 48,
            ReturnType = ReturnType.Search
        };

        private readonly VerticalStackLayout _clientList = new() { Spacing = 10 };
        private readonly Label _messageLabel = new()
        {
            FontSize = 13,
            TextColor = Color.FromArgb("#64748B"),
            HorizontalTextAlignment = TextAlignment.Center,
            IsVisible = false
        };
        private readonly ActivityIndicator _activityIndicator = new()
        {
            Color = Color.FromArgb("#16A34A"),
            IsRunning = false,
            IsVisible = false,
            HorizontalOptions = LayoutOptions.Center
        };
        private readonly Button _validateButton = new()
        {
            Text = "Valider",
            BackgroundColor = Color.FromArgb("#16A34A"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 46,
            IsVisible = false
        };
        private readonly Button _cancelButton = new()
        {
            Text = "Annuler",
            BackgroundColor = Color.FromArgb("#E2E8F0"),
            TextColor = Color.FromArgb("#0F172A"),
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 46
        };

        private readonly ObservableCollection<ClientAccount> _accounts = new();
        private readonly TaskCompletionSource<ClientAccount?> _completion = new();
        private readonly string _initialFilter;
        private ClientAccount? _selectedAccount;
        private string _lastSearchKeyword = string.Empty;
        private bool _isSearching;
        private bool _initialSearchDone;

        public ClientAccountSelectionPage(string title, string initialFilter)
        {
            Title = title;
            _initialFilter = initialFilter ?? string.Empty;
            BackgroundColor = Color.FromArgb("#F6F8FB");
            BuildContent(title);
        }

        public Task<ClientAccount?> Completion => _completion.Task;

        public static async Task<ClientAccount?> ShowAsync(Page owner, string title, string initialFilter)
        {
            var page = new ClientAccountSelectionPage(title, initialFilter);
            await owner.Navigation.PushModalAsync(page);
            return await page.Completion;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_initialSearchDone)
                return;

            _initialSearchDone = true;
            if (!string.IsNullOrWhiteSpace(_initialFilter))
            {
                _filterEntry.Text = _initialFilter;
                await SearchClientsAsync(false);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () => await CancelAsync());
            return true;
        }

        private void BuildContent(string title)
        {
            _filterEntry.Completed += async (_, __) => await SearchClientsAsync(true);
            _filterEntry.Unfocused += async (_, __) => await SearchClientsAsync(true);
            _validateButton.Clicked += async (_, __) => await ValidateSelectionAsync();
            _cancelButton.Clicked += async (_, __) => await CancelAsync();

            var header = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                Padding = 14,
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label
                        {
                            Text = title,
                            FontSize = 20,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A")
                        },
                        new Label
                        {
                            Text = "Saisissez un filtre puis validez la saisie pour lancer la recherche.",
                            FontSize = 13,
                            TextColor = Color.FromArgb("#64748B")
                        },
                        EditBox(_filterEntry),
                        _activityIndicator,
                        _messageLabel
                    }
                }
            };

            var scroll = new ScrollView
            {
                Content = _clientList
            };

            var footer = new Grid
            {
                ColumnSpacing = 10,
                Padding = new Thickness(0, 10, 0, 0),
                ColumnDefinitions = Columns("*,*")
            };
            footer.Add(_cancelButton, 0, 0);
            footer.Add(_validateButton, 1, 0);

            var root = new Grid
            {
                Padding = new Thickness(14, 12, 14, 12),
                RowSpacing = 12,
                RowDefinitions = Rows("Auto,*,Auto")
            };
            root.Add(header, 0, 0);
            root.Add(scroll, 0, 1);
            root.Add(footer, 0, 2);

            Content = root;
            RebuildClientList();
        }

        private async Task SearchClientsAsync(bool avoidDuplicate)
        {
            string keyword = (_filterEntry.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _accounts.Clear();
                _selectedAccount = null;
                _lastSearchKeyword = string.Empty;
                _messageLabel.Text = "Renseignez un filtre client.";
                _messageLabel.IsVisible = true;
                RebuildClientList();
                return;
            }

            if (avoidDuplicate && string.Equals(_lastSearchKeyword, keyword, StringComparison.OrdinalIgnoreCase))
                return;

            if (_isSearching)
                return;

            _isSearching = true;
            _activityIndicator.IsVisible = true;
            _activityIndicator.IsRunning = true;
            _messageLabel.IsVisible = false;

            try
            {
                var result = await EasySession.FindClientAccountAsync(keyword) ?? Array.Empty<ClientAccount>();
                _accounts.Clear();
                foreach (var account in result)
                    _accounts.Add(account);

                _selectedAccount = null;
                _lastSearchKeyword = keyword;
                _messageLabel.Text = result.Length == 0 ? "Aucun client trouvé." : string.Empty;
                _messageLabel.IsVisible = result.Length == 0;
                RebuildClientList();
            }
            catch (Exception ex)
            {
                _accounts.Clear();
                _selectedAccount = null;
                _messageLabel.Text = ex.Message;
                _messageLabel.IsVisible = true;
                RebuildClientList();
            }
            finally
            {
                _activityIndicator.IsRunning = false;
                _activityIndicator.IsVisible = false;
                _isSearching = false;
            }
        }

        private void RebuildClientList()
        {
            _clientList.Children.Clear();

            foreach (var account in _accounts)
                _clientList.Children.Add(ClientCard(account));

            _validateButton.IsVisible = _accounts.Count > 0 && _selectedAccount != null;
        }

        private View ClientCard(ClientAccount account)
        {
            bool selected = ReferenceEquals(_selectedAccount, account)
                || string.Equals(_selectedAccount?.AccountCode, account.AccountCode, StringComparison.OrdinalIgnoreCase);

            var cityLine = BuildZipCity(account);
            var stack = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label
                    {
                        Text = account.AccountCode ?? string.Empty,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = Color.FromArgb("#0F172A")
                    },
                    new Label
                    {
                        Text = account.AccountName ?? string.Empty,
                        FontSize = 14,
                        TextColor = Color.FromArgb("#334155"),
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    new Label
                    {
                        Text = cityLine,
                        FontSize = 13,
                        TextColor = Color.FromArgb("#64748B"),
                        IsVisible = !string.IsNullOrWhiteSpace(cityLine)
                    }
                }
            };

            var card = new Border
            {
                Content = stack,
                BackgroundColor = selected ? Color.FromArgb("#ECFDF5") : Colors.White,
                Stroke = selected ? Color.FromArgb("#16A34A") : Color.FromArgb("#E2E8F0"),
                StrokeThickness = selected ? 2 : 1,
                Padding = 12,
                StrokeShape = new RoundRectangle { CornerRadius = 16 }
            };

            card.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    _selectedAccount = account;
                    RebuildClientList();
                })
            });

            return card;
        }

        private async Task ValidateSelectionAsync()
        {
            if (_selectedAccount == null)
                return;

            _completion.TrySetResult(_selectedAccount);
            await CloseAsync();
        }

        private async Task CancelAsync()
        {
            _completion.TrySetResult(null);
            await CloseAsync();
        }

        private async Task CloseAsync()
        {
            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
        }

        private static string BuildZipCity(ClientAccount account)
        {
            string zip = (account.Zip ?? string.Empty).Trim();
            string city = (account.City ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(zip))
                return city;
            if (string.IsNullOrWhiteSpace(city))
                return zip;
            return $"{zip} - {city}";
        }

        private static Border EditBox(Entry entry)
        {
            entry.HorizontalTextAlignment = TextAlignment.Center;
            entry.VerticalTextAlignment = TextAlignment.Center;
            entry.HorizontalOptions = LayoutOptions.Fill;
            entry.VerticalOptions = LayoutOptions.Center;
            entry.HeightRequest = 48;

            var grid = new Grid
            {
                HeightRequest = 48,
                VerticalOptions = LayoutOptions.Center,
                Children = { entry }
            };

            return new Border
            {
                Content = grid,
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Padding = new Thickness(12, 0),
                HeightRequest = 52,
                StrokeShape = new RoundRectangle { CornerRadius = 14 }
            };
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

        private static RowDefinitionCollection Rows(string pattern)
        {
            var collection = new RowDefinitionCollection();
            foreach (var part in pattern.Split(','))
            {
                string value = part.Trim();
                collection.Add(new RowDefinition { Height = value == "Auto" ? GridLength.Auto : GridLength.Star });
            }
            return collection;
        }
    }
}
