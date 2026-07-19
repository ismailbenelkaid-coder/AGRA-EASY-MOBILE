using System.IO;

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
#endif

namespace AGRA_EASY_MOBILE.Services;

public sealed class SelectedUploadFile
{
    public SelectedUploadFile(byte[] bytes, string extension)
    {
        Bytes = bytes;
        Extension = extension;
    }

    public byte[] Bytes { get; }
    public string Extension { get; }
}

public static class RefusedReturnPictureService
{
    public static async Task<byte[]?> CaptureOrPickJpegBytesAsync(string preferredFileName)
    {
        var file = await CaptureOrPickFileAsync(preferredFileName, false);
        return file?.Bytes;
    }

    public static async Task<SelectedUploadFile?> CaptureOrPickDocumentBytesAsync(string preferredFileName)
        => await CaptureOrPickFileAsync(preferredFileName, true);

    private static async Task<SelectedUploadFile?> CaptureOrPickFileAsync(string preferredFileName, bool allowAnyFileSelection)
    {
#if ANDROID
        var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (permission != PermissionStatus.Granted)
            permission = await Permissions.RequestAsync<Permissions.Camera>();

        if (permission != PermissionStatus.Granted)
            throw new PermissionException("L'autorisation caméra est nécessaire pour ouvrir l'interface de prise de photo.");

        var filePath = await RefusedReturnCameraCoordinator.OpenCameraAsync(preferredFileName, allowAnyFileSelection);
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        return new SelectedUploadFile(await File.ReadAllBytesAsync(filePath), GetExtension(filePath));
#elif IOS
        if (allowAnyFileSelection)
        {
            var page = Application.Current?.MainPage;
            var action = page == null
                ? "Prendre une photo"
                : await page.DisplayActionSheet("Justificatif fournisseur", "Annuler", null, "Prendre une photo", "Sélectionner un fichier");

            if (action == "Annuler")
                return null;

            if (action == "Sélectionner un fichier")
            {
                var pickedFile = await FilePicker.Default.PickAsync();
                if (pickedFile == null)
                    return null;

                await using var pickedStream = await pickedFile.OpenReadAsync();
                using var pickedMemory = new MemoryStream();
                await pickedStream.CopyToAsync(pickedMemory);
                return new SelectedUploadFile(pickedMemory.ToArray(), GetExtension(pickedFile.FileName));
            }
        }
        else
        {
            var page = Application.Current?.MainPage;
            var action = page == null
                ? "Prendre une photo"
                : await page.DisplayActionSheet("Justificatif retour refusé", "Annuler", null, "Prendre une photo", "Sélectionner une photo");

            if (action == "Annuler")
                return null;

            if (action == "Sélectionner une photo")
            {
                var pickedFile = await FilePicker.Default.PickAsync(PickOptions.Images);
                if (pickedFile == null)
                    return null;

                await using var pickedStream = await pickedFile.OpenReadAsync();
                using var pickedMemory = new MemoryStream();
                await pickedStream.CopyToAsync(pickedMemory);
                return new SelectedUploadFile(pickedMemory.ToArray(), GetExtension(pickedFile.FileName));
            }
        }

        var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (permission != PermissionStatus.Granted)
            permission = await Permissions.RequestAsync<Permissions.Camera>();

        if (permission != PermissionStatus.Granted)
            throw new PermissionException("L'autorisation camera est necessaire pour ouvrir l'interface de prise de photo.");

        var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
        {
            Title = preferredFileName
        });

        if (photo == null)
            return null;

        await using var stream = await photo.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return new SelectedUploadFile(memory.ToArray(), GetExtension(photo.FileName));
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    private static string GetExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).Trim().TrimStart('.').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(extension) ? "dat" : extension;
    }
}

#if ANDROID
internal static class RefusedReturnCameraCoordinator
{
    internal const string ExtraPreferredFileName = "AGRA_EASY_MOBILE.PREFERRED_FILE_NAME";
    internal const string ExtraAllowAnyFileSelection = "AGRA_EASY_MOBILE.ALLOW_ANY_FILE_SELECTION";

    private static TaskCompletionSource<string?>? _completionSource;

    internal static Task<string?> OpenCameraAsync(string preferredFileName, bool allowAnyFileSelection = false)
    {
        var currentActivity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("L'activité Android courante est indisponible.");

        var previousCompletion = _completionSource;
        if (previousCompletion != null && !previousCompletion.Task.IsCompleted)
            previousCompletion.TrySetResult(null);

        _completionSource = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var intent = new Intent(currentActivity, typeof(RefusedReturnCameraActivity));
        intent.PutExtra(ExtraPreferredFileName, preferredFileName);
        intent.PutExtra(ExtraAllowAnyFileSelection, allowAnyFileSelection);
        currentActivity.StartActivity(intent);

        return _completionSource.Task;
    }

    internal static void Complete(string? jpegPath)
    {
        _completionSource?.TrySetResult(jpegPath);
        _completionSource = null;
    }

    internal static void CompleteWithError(Exception exception)
    {
        _completionSource?.TrySetException(exception);
        _completionSource = null;
    }
}
#endif
