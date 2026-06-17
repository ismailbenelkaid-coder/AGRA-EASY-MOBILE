namespace AGRA_EASY_MOBILE;

public partial class RefusedReturnImageView : ContentPage
{
    public RefusedReturnImageView(string picturePath)
    {
        InitializeComponent();
        FullImage.Source = CreateImageSource(picturePath);
    }

    private static ImageSource CreateImageSource(string picturePath)
    {
        var path = picturePath?.Trim() ?? string.Empty;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri)
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return ImageSource.FromUri(uri);
        }

        return ImageSource.FromFile(path);
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
