using Microsoft.Maui.Controls.Shapes;

namespace AGRA_EASY_MOBILE;

public class ConnectionDiagnosticPage : ContentPage
{
    private readonly string _diagnosticText;
    private readonly Label _copyStatusLabel;

    public ConnectionDiagnosticPage(string diagnosticText)
    {
        _diagnosticText = string.IsNullOrWhiteSpace(diagnosticText)
            ? "Aucun detail de diagnostic disponible."
            : diagnosticText;

        Title = "Diagnostic connexion";
        BackgroundColor = Color.FromArgb("#F6F8FB");

        var titleLabel = new Label
        {
            Text = "Diagnostic connexion SOAP",
            FontAttributes = FontAttributes.Bold,
            FontSize = 20,
            TextColor = Color.FromArgb("#0F172A")
        };

        var subtitleLabel = new Label
        {
            Text = "Copiez ce texte pour analyse. Cette fenetre reste ouverte jusqu'a validation.",
            FontSize = 13,
            TextColor = Color.FromArgb("#64748B")
        };

        var diagnosticEditor = new Editor
        {
            Text = _diagnosticText,
            IsReadOnly = true,
            AutoSize = EditorAutoSizeOption.Disabled,
            FontFamily = "monospace",
            FontSize = 12,
            TextColor = Color.FromArgb("#111827"),
            BackgroundColor = Colors.White
        };

        _copyStatusLabel = new Label
        {
            Text = string.Empty,
            FontSize = 12,
            TextColor = Color.FromArgb("#16A34A"),
            HorizontalOptions = LayoutOptions.Center
        };

        var copyButton = new Button
        {
            Text = "COPIER",
            BackgroundColor = Color.FromArgb("#2563EB"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 48
        };
        copyButton.Clicked += OnCopyClicked;

        var closeButton = new Button
        {
            Text = "VALIDER",
            BackgroundColor = Color.FromArgb("#16A34A"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 14,
            HeightRequest = 48
        };
        closeButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

        var headerLayout = new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                titleLabel,
                subtitleLabel
            }
        };

        var diagnosticBorder = new Border
        {
            Margin = new Thickness(0, 14, 0, 14),
            Stroke = Color.FromArgb("#CBD5E1"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Content = diagnosticEditor
        };

        var buttonGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10,
            Children =
            {
                copyButton,
                closeButton
            }
        };
        Grid.SetColumn(closeButton, 1);

        var footerLayout = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                _copyStatusLabel,
                buttonGrid
            }
        };

        var rootGrid = new Grid
        {
            Padding = new Thickness(18, 18, 18, 14),
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children =
            {
                headerLayout,
                diagnosticBorder,
                footerLayout
            }
        };

        Grid.SetRow(diagnosticBorder, 1);
        Grid.SetRow(footerLayout, 2);
        Content = rootGrid;
    }

    public static async Task ShowAsync(Page owner, string diagnosticText)
    {
        var currentPage = Application.Current?.Windows.FirstOrDefault()?.Page
            ?? Application.Current?.MainPage
            ?? owner;

        await currentPage.Navigation.PushModalAsync(new ConnectionDiagnosticPage(diagnosticText));
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(_diagnosticText);
        _copyStatusLabel.Text = "Diagnostic copie dans le presse-papiers.";
    }
}
