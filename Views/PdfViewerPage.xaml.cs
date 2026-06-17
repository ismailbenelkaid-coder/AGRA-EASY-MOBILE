using AGRA_EASY_MOBILE.Services;
using CommunityToolkit.Maui.Storage;

#if ANDROID || IOS
using MauiNativePdfView;
using MauiNativePdfView.Abstractions;
#endif

namespace AGRA_EASY_MOBILE;

public partial class PdfViewerPage : ContentPage
{
    private readonly string _pdfPath;
    private readonly string _fileName;
    private bool _viewerInitialized;

#if ANDROID || IOS
    private PdfView? _pdfView;
#endif

    public PdfViewerPage(string pdfPath, string fileName)
    {
        InitializeComponent();
        _pdfPath = pdfPath;
        _fileName = fileName;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewerInitialized)
            return;

        _viewerInitialized = true;
        InitializeViewer();
    }

    private void InitializeViewer()
    {
        if (!File.Exists(_pdfPath))
        {
            ShowFallback("Le fichier PDF est introuvable sur l'appareil.");
            return;
        }

#if ANDROID || IOS
        try
        {
            _pdfView = new PdfView
            {
                EnableZoom = true,
                EnableSwipe = true,
                EnableLinkNavigation = true,
                EnableAnnotationRendering = true,
                EnableAntialiasing = true,
                UseBestQuality = false,
                MinZoom = 1.0f,
                MaxZoom = 5.0f,
                PageSpacing = 0,
                BackgroundColor = Colors.White,
                Source = PdfSource.FromFile(_pdfPath)
            };

            ViewerHost.Children.Clear();
            ViewerHost.Children.Add(_pdfView);
            FallbackLabel.IsVisible = false;
        }
        catch (Exception ex)
        {
            ShowFallback($"Impossible d'ouvrir le PDF dans le visualiseur natif : {ex.Message}");
        }
#else
        ShowFallback("Le visualiseur PDF natif de cette version est prévu pour Android et iOS. Utilisez Partager ou Enregistrer sur cette plateforme.");
#endif
    }

    private void ShowFallback(string message)
    {
        ViewerHost.Children.Clear();
        FallbackLabel.Text = message;
        FallbackLabel.IsVisible = true;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            await using var stream = File.OpenRead(_pdfPath);
            var result = await FileSaver.Default.SaveAsync(_fileName, stream, CancellationToken.None);

            if (result.IsSuccessful)
            {
                await ModernAlertService.ShowSuccessAsync("Le PDF a été enregistré.");
            }
            else
            {
                await ModernAlertService.ShowErrorAsync(result.Exception?.Message ?? "L'enregistrement a échoué.");
            }
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Partager le bon de livraison",
                File = new ShareFile(_pdfPath)
            });
        }
        catch (Exception ex)
        {
            await ModernAlertService.ShowErrorAsync(ex.Message);
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
        => await Navigation.PopModalAsync();
}
