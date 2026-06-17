using System.IO;

#if ANDROID
using Android.Content;
using Microsoft.Maui.ApplicationModel;
#endif

namespace AGRA_EASY_MOBILE.Services;

public static class RefusedReturnPictureService
{
    public static async Task<byte[]?> CaptureOrPickJpegBytesAsync(string preferredFileName)
    {
#if ANDROID
        var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (permission != PermissionStatus.Granted)
            permission = await Permissions.RequestAsync<Permissions.Camera>();

        if (permission != PermissionStatus.Granted)
            throw new PermissionException("L'autorisation caméra est nécessaire pour ouvrir l'interface de prise de photo.");

        var jpegPath = await RefusedReturnCameraCoordinator.OpenCameraAsync(preferredFileName);
        if (string.IsNullOrWhiteSpace(jpegPath) || !File.Exists(jpegPath))
            return null;

        return await File.ReadAllBytesAsync(jpegPath);
#elif IOS
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
        return memory.ToArray();
#else
        await Task.CompletedTask;
        return null;
#endif
    }
}

#if ANDROID
internal static class RefusedReturnCameraCoordinator
{
    internal const string ExtraPreferredFileName = "AGRA_EASY_MOBILE.PREFERRED_FILE_NAME";

    private static TaskCompletionSource<string?>? _completionSource;

    internal static Task<string?> OpenCameraAsync(string preferredFileName)
    {
        var currentActivity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("L'activité Android courante est indisponible.");

        var previousCompletion = _completionSource;
        if (previousCompletion != null && !previousCompletion.Task.IsCompleted)
            previousCompletion.TrySetResult(null);

        _completionSource = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var intent = new Intent(currentActivity, typeof(RefusedReturnCameraActivity));
        intent.PutExtra(ExtraPreferredFileName, preferredFileName);
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
