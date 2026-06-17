using AGRA_EASY_MOBILE.Services;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls.Shapes;
using System.Globalization;
using System.IO.Compression;

namespace AGRA_EASY_MOBILE;

public class ZipViewerPage : ContentPage
{
    private readonly string _zipPath;
    private readonly string _fileName;
    private readonly VerticalStackLayout _entryList = new() { Spacing = 8 };

    public ZipViewerPage(string zipPath, string fileName)
    {
        _zipPath = zipPath;
        _fileName = fileName;
        Title = "Archive ZIP";
        BackgroundColor = Color.FromArgb("#F6F8FB");
        BuildContent();
    }

    private void BuildContent()
    {
        var root = new Grid { RowDefinitions = Rows("Auto,*") };

        var header = new Grid
        {
            ColumnDefinitions = Columns("*,Auto,Auto,Auto"),
            ColumnSpacing = 10,
            Padding = new Thickness(14, 14, 14, 10),
            BackgroundColor = Colors.White
        };
        header.Add(new Label
        {
            Text = _fileName,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0F172A"),
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        }, 0, 0);

        var saveButton = RoundButton("↓", "Enregistrer", "#2563EB");
        saveButton.Clicked += OnSaveClicked;
        var shareButton = RoundButton("↗", "Partager", "#7C3AED");
        shareButton.Clicked += OnShareClicked;
        var closeButton = RoundButton("✕", "Fermer", "#DC2626");
        closeButton.Clicked += OnCloseClicked;
        header.Add(saveButton, 1, 0);
        header.Add(shareButton, 2, 0);
        header.Add(closeButton, 3, 0);
        root.Add(header, 0, 0);

        var content = new VerticalStackLayout { Padding = new Thickness(14), Spacing = 12 };
        content.Children.Add(Card(new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label { Text = "Contenu de l'archive", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") },
                new Label { Text = "Cette vue permet de visualiser la liste des fichiers de l'archive ZIP, puis de l'enregistrer ou de la partager.", FontSize = 12, TextColor = Color.FromArgb("#64748B"), LineBreakMode = LineBreakMode.WordWrap }
            }
        }));
        content.Children.Add(_entryList);
        root.Add(new ScrollView { Content = content }, 0, 1);

        Content = root;
        LoadZipEntries();
    }

    private void LoadZipEntries()
    {
        _entryList.Children.Clear();
        if (!File.Exists(_zipPath))
        {
            _entryList.Children.Add(EmptyLabel("Le fichier ZIP est introuvable sur l'appareil."));
            return;
        }

        try
        {
            using var archive = ZipFile.OpenRead(_zipPath);
            if (archive.Entries.Count == 0)
            {
                _entryList.Children.Add(EmptyLabel("Archive ZIP vide."));
                return;
            }

            foreach (var entry in archive.Entries.OrderBy(e => e.FullName))
                _entryList.Children.Add(EntryCard(entry));
        }
        catch (Exception ex)
        {
            _entryList.Children.Add(EmptyLabel($"Impossible de lire l'archive ZIP : {ex.Message}"));
        }
    }

    private static View EntryCard(ZipArchiveEntry entry)
    {
        bool folder = entry.FullName.EndsWith("/", StringComparison.Ordinal);
        var grid = new Grid { ColumnDefinitions = Columns("Auto,*"), ColumnSpacing = 10 };
        grid.Add(new Label
        {
            Text = folder ? "📁" : "📄",
            FontSize = 22,
            VerticalTextAlignment = TextAlignment.Center
        }, 0, 0);

        var stack = new VerticalStackLayout { Spacing = 3 };
        stack.Children.Add(new Label
        {
            Text = entry.FullName,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0F172A"),
            LineBreakMode = LineBreakMode.WordWrap
        });
        if (!folder)
        {
            stack.Children.Add(new Label
            {
                Text = $"Taille : {FormatSize(entry.Length)}",
                FontSize = 11,
                TextColor = Color.FromArgb("#64748B")
            });
        }
        grid.Add(stack, 1, 0);
        return InnerCard(grid);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            await using var stream = File.OpenRead(_zipPath);
            var result = await FileSaver.Default.SaveAsync(_fileName, stream, CancellationToken.None);
            if (result.IsSuccessful)
                await ModernAlertService.ShowSuccessAsync("Le ZIP a été enregistré.");
            else
                await ModernAlertService.ShowErrorAsync(result.Exception?.Message ?? "L'enregistrement a échoué.");
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Partager l'archive ZIP",
                File = new ShareFile(_zipPath)
            });
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
        => await Navigation.PopModalAsync();

    private static Button RoundButton(string text, string automationId, string background)
    {
        return new Button
        {
            Text = text,
            AutomationId = automationId,
            BackgroundColor = Color.FromArgb(background),
            TextColor = Colors.White,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 22,
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = 0
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

    private static Label EmptyLabel(string text)
    {
        return new Label { Text = text, FontSize = 13, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.WordWrap };
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} o";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024m:0.##} Ko";
        return $"{bytes / 1024m / 1024m:0.##} Mo";
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
