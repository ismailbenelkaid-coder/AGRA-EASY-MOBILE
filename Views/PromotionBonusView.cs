using Microsoft.Maui.Controls.Shapes;
using Services;

namespace AGRA_EASY_MOBILE
{
    public class PromotionBonusView : ContentPage
    {
        private readonly TaskCompletionSource<string?> _completion = new();
        private readonly PromotionBonusLine[] _bonusLines;

        private PromotionBonusView(PromotionBonusLine[] bonusLines)
        {
            _bonusLines = bonusLines ?? Array.Empty<PromotionBonusLine>();
            Title = "Bonus promotion";
            BackgroundColor = Color.FromArgb("#F6F8FB");
            BuildContent();
        }

        public static async Task<string?> SelectBonusAsync(INavigation navigation, PromotionBonusLine[] bonusLines)
        {
            var page = new PromotionBonusView(bonusLines);
            await navigation.PushAsync(page);
            return await page._completion.Task;
        }

        private void BuildContent()
        {
            var content = new VerticalStackLayout { Padding = new Thickness(14, 12, 14, 24), Spacing = 14 };
            content.Children.Add(Header());

            var list = new VerticalStackLayout { Spacing = 8 };
            if (_bonusLines.Length == 0)
            {
                list.Children.Add(new Label { Text = "Aucun bonus à sélectionner.", TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center });
            }
            else
            {
                foreach (var bonus in _bonusLines)
                    list.Children.Add(BonusCard(bonus));
            }
            content.Children.Add(Card(list));

            var cancel = SecondaryButton("Annuler");
            cancel.Clicked += async (_, __) => await CloseAsync(null);
            content.Children.Add(cancel);

            Content = new ScrollView { Content = content };
        }

        private View BonusCard(PromotionBonusLine bonus)
        {
            var grid = new Grid { ColumnDefinitions = Columns("*,Auto"), ColumnSpacing = 8 };
            grid.Add(new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    new Label { Text = bonus.ProductCode, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") },
                    new Label { Text = $"Quantité : {bonus.Quantity:0.##}", FontSize = 13, TextColor = Color.FromArgb("#64748B") }
                }
            }, 0, 0);
            var choose = PrimaryButton("Choisir");
            choose.Clicked += async (_, __) => await CloseAsync(bonus.ProductCode);
            grid.Add(choose, 1, 0);
            return InnerCard(grid);
        }

        private async Task CloseAsync(string? productCode)
        {
            _completion.TrySetResult(productCode);
            await Navigation.PopAsync();
        }

        private static Border Header()
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
                        new Label { Text = "Bonus promotion", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") },
                        new Label { Text = "Choisissez le produit bonus à appliquer au panier.", FontSize = 13, TextColor = Color.FromArgb("#64748B") }
                    }
                }
            };
        }

        private static Border Card(View content)
        {
            return new Border { Content = content, BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2E8F0"), StrokeThickness = 1, Padding = 12, StrokeShape = new RoundRectangle { CornerRadius = 18 } };
        }

        private static Border InnerCard(View content)
        {
            return new Border { Content = content, BackgroundColor = Color.FromArgb("#F8FAFC"), Stroke = Color.FromArgb("#E2E8F0"), StrokeThickness = 1, Padding = 10, StrokeShape = new RoundRectangle { CornerRadius = 14 } };
        }

        private static Button PrimaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#16A34A"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44, Padding = new Thickness(12, 0) };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button { Text = text, BackgroundColor = Color.FromArgb("#E2E8F0"), TextColor = Color.FromArgb("#0F172A"), FontAttributes = FontAttributes.Bold, CornerRadius = 14, HeightRequest = 44, Padding = new Thickness(12, 0) };
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
