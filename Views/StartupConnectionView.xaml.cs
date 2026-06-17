using AGRA_EASY_MOBILE.Services;
using Microsoft.Maui.Storage;

namespace AGRA_EASY_MOBILE;

public partial class StartupConnectionView : ContentPage
{
    private bool _started;

    public StartupConnectionView()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_started)
            return;

        _started = true;
        await StartAsync();
    }

    private async Task StartAsync()
    {
        string user = Preferences.Default.Get("UserLogin", string.Empty);
        string pass = Preferences.Default.Get("UserPassword", string.Empty);

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            await EasySession.ShowLoginPageAsync();
            return;
        }

        try
        {
            StartupMessage.Text = "Connexion en cours...";
            bool connected = await EasySession.OpenConnectionAsync();

            if (connected)
            {
                await EasySession.ShowShellAfterSuccessfulConnectionAsync("//home");
                return;
            }
        }
        catch
        {
        }

        await EasySession.ShowLoginPageAsync();
    }
}
