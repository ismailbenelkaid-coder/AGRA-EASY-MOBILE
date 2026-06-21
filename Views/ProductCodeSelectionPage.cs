using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using Services;
using System.Collections.ObjectModel;

namespace AGRA_EASY_MOBILE
{
    public class ProductCodeSelectionPage : ContentPage
    {
        private readonly Entry _filterEntry = new BorderlessEntry
        {
            Placeholder = "Filtre article",
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

        private readonly VerticalStackLayout _productList = new() { Spacing = 10 };
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

        private readonly ImageButton _scanButton = new()
        {
            Source = "ic_barcode_scan.png",
            BackgroundColor = Color.FromArgb("#DCFCE7"),
            WidthRequest = 38,
            HeightRequest = 34,
            MinimumWidthRequest = 38,
            MinimumHeightRequest = 34,
            CornerRadius = 12,
            Padding = 9,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center
        };

        private readonly ObservableCollection<ReturnableArticle> _products = new();
        private readonly TaskCompletionSource<ReturnableArticle?> _completion = new();
        private readonly string _initialFilter;
        private ReturnableArticle? _selectedProduct;
        private string _lastSearchKeyword = string.Empty;
        private bool _isSearching;
        private bool _initialSearchDone;
        private readonly KeyboardScanInputTracker _scanInputTracker = new();

        public ProductCodeSelectionPage(string title, string initialFilter)
        {
            Title = title;
            _initialFilter = initialFilter ?? string.Empty;
            BackgroundColor = Color.FromArgb("#F6F8FB");
            BuildContent(title);
        }

        public Task<ReturnableArticle?> Completion => _completion.Task;

        public static async Task<ReturnableArticle?> ShowAsync(Page owner, string title, string initialFilter)
        {
            var page = new ProductCodeSelectionPage(title, initialFilter);
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
                await SearchProductsAsync(false);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () => await CancelAsync());
            return true;
        }

        private void BuildContent(string title)
        {
            _filterEntry.TextChanged += (_, e) => _scanInputTracker.ObserveTextChanged(e.OldTextValue, e.NewTextValue);
            _filterEntry.Completed += async (_, __) => await OnFilterEntryCompletedAsync();
            _filterEntry.Unfocused += async (_, __) => await SearchProductsAsync(true);
            _validateButton.Clicked += async (_, __) => await ValidateSelectionAsync();
            _cancelButton.Clicked += async (_, __) => await CancelAsync();
            _scanButton.Clicked += async (_, __) => await ScanProductBarcodeAsync();

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
                        EditBoxWithScanner(_filterEntry),
                        _activityIndicator,
                        _messageLabel
                    }
                }
            };

            var scroll = new ScrollView
            {
                Content = _productList
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
            RebuildProductList();
        }

        private async Task SearchProductsAsync(bool avoidDuplicate)
        {
            string keyword = (_filterEntry.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                _products.Clear();
                _selectedProduct = null;
                _lastSearchKeyword = string.Empty;
                _messageLabel.Text = "Renseignez un filtre article.";
                _messageLabel.IsVisible = true;
                RebuildProductList();
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
                var result = await EasySession.FindProductCodeListAsync(keyword, false, false) ?? Array.Empty<ReturnableArticle>();
                _products.Clear();
                foreach (var product in result)
                    _products.Add(product);

                _selectedProduct = null;
                _lastSearchKeyword = keyword;
                _messageLabel.Text = result.Length == 0 ? "Aucun article trouvé." : string.Empty;
                _messageLabel.IsVisible = result.Length == 0;
                RebuildProductList();
            }
            catch (Exception ex)
            {
                _products.Clear();
                _selectedProduct = null;
                _messageLabel.Text = ex.Message;
                _messageLabel.IsVisible = true;
                RebuildProductList();
            }
            finally
            {
                _activityIndicator.IsRunning = false;
                _activityIndicator.IsVisible = false;
                _isSearching = false;
            }
        }

        private async Task OnFilterEntryCompletedAsync()
        {
            if (_scanInputTracker.ConsumeCompletedAsScan(_filterEntry.Text))
            {
                ReturnableArticle? product = await ProductBarcodeScanService.ResolveScannedProductAsync(this, _filterEntry.Text);
                if (product == null)
                    return;

                _completion.TrySetResult(product);
                await CloseAsync();
                return;
            }

            await SearchProductsAsync(true);
        }

        private void RebuildProductList()
        {
            _productList.Children.Clear();

            foreach (var product in _products)
                _productList.Children.Add(ProductCard(product));

            _validateButton.IsVisible = _products.Count > 0 && _selectedProduct != null;
        }

        private View ProductCard(ReturnableArticle product)
        {
            bool selected = ReferenceEquals(_selectedProduct, product)
                || string.Equals(_selectedProduct?.ProductCode, product.ProductCode, StringComparison.OrdinalIgnoreCase);

            var stack = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label
                    {
                        Text = product.ProductCode ?? string.Empty,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = Color.FromArgb("#0F172A")
                    },
                    new Label
                    {
                        Text = product.ProductLabel ?? string.Empty,
                        FontSize = 14,
                        TextColor = Color.FromArgb("#334155"),
                        LineBreakMode = LineBreakMode.WordWrap,
                        IsVisible = !string.IsNullOrWhiteSpace(product.ProductLabel)
                    },
                    new Label
                    {
                        Text = product.SupplierName ?? string.Empty,
                        FontSize = 13,
                        TextColor = Color.FromArgb("#64748B"),
                        LineBreakMode = LineBreakMode.WordWrap,
                        IsVisible = !string.IsNullOrWhiteSpace(product.SupplierName)
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
                    _selectedProduct = product;
                    RebuildProductList();
                })
            });

            return card;
        }

        private async Task ScanProductBarcodeAsync()
        {
            if (_isSearching)
                return;

            try
            {
                ReturnableArticle? product = await ProductBarcodeScanService.ScanAndResolveProductAsync(this);
                if (product == null)
                    return;

                _completion.TrySetResult(product);
                await CloseAsync();
            }
            catch (Exception ex)
            {
                await ModernAlertService.ShowWarningAsync(ex.Message);
            }
        }

        private async Task ValidateSelectionAsync()
        {
            if (_selectedProduct == null)
                return;

            _completion.TrySetResult(_selectedProduct);
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

        private Border EditBoxWithScanner(Entry entry)
        {
            entry.HorizontalTextAlignment = TextAlignment.Center;
            entry.VerticalTextAlignment = TextAlignment.Center;
            entry.HorizontalOptions = LayoutOptions.Fill;
            entry.VerticalOptions = LayoutOptions.Center;
            entry.HeightRequest = 48;

            var grid = new Grid
            {
                HeightRequest = 52,
                VerticalOptions = LayoutOptions.Center,
                ColumnDefinitions = Columns("*,Auto"),
                ColumnSpacing = 8
            };
            grid.Add(entry, 0, 0);
            grid.Add(_scanButton, 1, 0);

            return new Border
            {
                Content = grid,
                Stroke = Color.FromArgb("#CBD5E1"),
                StrokeThickness = 1,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Padding = new Thickness(12, 0),
                HeightRequest = 56,
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
