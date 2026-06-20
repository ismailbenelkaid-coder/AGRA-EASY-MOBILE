using System.Xml.Linq;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using AGRA_EASY_MOBILE.Services;
using AGRA_EASY_MOBILE.Models;

namespace AGRA_EASY_MOBILE;

public partial class LoginView : ContentPage
{
    public LoginView()
    {
        InitializeComponent();
        LoadWarehouseList();
    }

    private void LoadWarehouseList()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("platforms.xml").Result;
            var doc = XDocument.Load(stream);
            var names = doc.Descendants("Warehouse")
                           .Select(x => x.Attribute("Name").Value)
                           .ToList();
            PickerWarehouse.ItemsSource = names;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur XML : {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EntryLogin.Text = Preferences.Default.Get("UserLogin", string.Empty);
        EntryPassword.Text = Preferences.Default.Get("UserPassword", string.Empty);
        PickerWarehouse.SelectedItem = Preferences.Default.Get("SelectedWarehouseName", "Meyzieu");
        SwitchLinkType.IsToggled = Preferences.Default.Get("UseSecondaryLink", false);
    }

    private async void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        SetConnectionLoading(true);

        var newLogin = EntryLogin.Text ?? string.Empty;

        Preferences.Default.Set("UserLogin", newLogin);
        Preferences.Default.Set("UserPassword", EntryPassword.Text ?? string.Empty);
        Preferences.Default.Set("SelectedWarehouseName", PickerWarehouse.SelectedItem?.ToString() ?? "Meyzieu");
        Preferences.Default.Set("UseSecondaryLink", SwitchLinkType.IsToggled);
        Preferences.Default.Remove("AutoReconnect");
        Preferences.Default.Remove("MaxAttempts");

        string alertMessage = string.Empty;
        Color alertColor = Colors.Transparent;

        try
        {
            bool success = await EasySession.ValidateAndNavigateAsync();

            if (success)
            {
                alertMessage = "Connexion reussie.";
                alertColor = Color.FromArgb("#00C569");
            }
            else
            {
                alertMessage = "Connexion impossible.";
                alertColor = Colors.Red;
            }
        }
        catch (Exception ex)
        {
            alertMessage = ex.Message;
            alertColor = Colors.Red;
        }
        finally
        {
            SetConnectionLoading(false);
        }

        if (!string.IsNullOrWhiteSpace(alertMessage))
            await ShowModernAlert(alertMessage, alertColor);
    }

    private void SetConnectionLoading(bool isLoading)
    {
        SaveSettingsButton.IsEnabled = !isLoading;
        ConnectionLoadingOverlay.IsVisible = isLoading;
        ConnectionLoadingIndicator.IsRunning = isLoading;
    }

    private async Task ShowModernAlert(string message, Color backgroundColor)
    {
        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = backgroundColor,
            TextColor = Colors.White,
            CornerRadius = new CornerRadius(10)
        };
        var snackbar = Snackbar.Make(message, duration: TimeSpan.FromSeconds(5), visualOptions: snackbarOptions);
        await snackbar.Show();
    }
}
