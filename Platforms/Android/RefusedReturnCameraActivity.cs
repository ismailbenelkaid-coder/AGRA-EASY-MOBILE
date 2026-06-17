#if ANDROID
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AGRA_EASY_MOBILE.Services;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using HardwareCamera = Android.Hardware.Camera;
using AImageButton = Android.Widget.ImageButton;
using IOFile = System.IO.File;
using IOPath = System.IO.Path;

namespace AGRA_EASY_MOBILE;

[Activity(
    Theme = "@style/AgraCameraTheme",
    ScreenOrientation = ScreenOrientation.FullSensor,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class RefusedReturnCameraActivity : Activity, TextureView.ISurfaceTextureListener
{
    private const int PickPhotoRequestCode = 41489;

    private TextureView? _preview;
    private SurfaceTexture? _surfaceTexture;
    private HardwareCamera? _camera;
    private bool _isClosing;
    private string _preferredFileName = "retour_refuse.jpg";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _preferredFileName = Intent?.GetStringExtra(RefusedReturnCameraCoordinator.ExtraPreferredFileName) ?? "retour_refuse.jpg";

        RequestWindowFeature(WindowFeatures.NoTitle);
        Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        if (Window?.DecorView != null)
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation |
                SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.LayoutFullscreen |
                SystemUiFlags.LayoutHideNavigation |
                SystemUiFlags.LayoutStable);
        }

        SetContentView(CreateContent());
    }

    private AView CreateContent()
    {
        var root = new FrameLayout(this)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
        };
        root.SetBackgroundColor(AColor.Black);

        _preview = new TextureView(this)
        {
            LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent)
        };
        _preview.SurfaceTextureListener = this;
        root.AddView(_preview);

        var bottomBar = new LinearLayout(this)
        {
            Orientation = Orientation.Horizontal,
        };
        bottomBar.SetGravity(GravityFlags.Center);
        bottomBar.SetPadding(0, 0, 0, Dp(24));

        var bottomParams = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.WrapContent)
        {
            Gravity = GravityFlags.Bottom | GravityFlags.CenterHorizontal
        };

        var pickButton = CreateFloatingButton(Android.Resource.Drawable.IcMenuGallery, Dp(62));
        pickButton.ContentDescription = "Choisir une photo";
        pickButton.Click += (_, _) => OpenPhotoPicker();

        var captureButton = CreateFloatingButton(Android.Resource.Drawable.IcMenuCamera, Dp(78));
        captureButton.ContentDescription = "Prendre la photo";
        captureButton.Click += (_, _) => CapturePhoto();

        bottomBar.AddView(pickButton);
        var spacer = new Space(this);
        bottomBar.AddView(spacer, new LinearLayout.LayoutParams(Dp(64), 1));
        bottomBar.AddView(captureButton);

        root.AddView(bottomBar, bottomParams);
        return root;
    }

    private AImageButton CreateFloatingButton(int imageResource, int size)
    {
        var button = new AImageButton(this)
        {
            LayoutParameters = new LinearLayout.LayoutParams(size, size)
        };
        button.SetScaleType(ImageView.ScaleType.Center);

        var background = new GradientDrawable();
        background.SetShape(ShapeType.Oval);
        background.SetColor(AColor.Argb(230, 255, 255, 255));
        button.Background = background;
        button.SetImageResource(imageResource);
        button.SetColorFilter(AColor.Rgb(15, 23, 42));
        button.SetPadding(Dp(16), Dp(16), Dp(16), Dp(16));
        return button;
    }

    private void OpenPhotoPicker()
    {
        try
        {
            ReleaseCamera();

            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("image/*");
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            StartActivityForResult(Intent.CreateChooser(intent, "Choisir une photo"), PickPhotoRequestCode);
        }
        catch (Exception ex)
        {
            CompleteWithError(ex);
        }
    }

    private void CapturePhoto()
    {
        try
        {
            var camera = _camera;
            if (camera == null)
            {
                Toast.MakeText(this, "La caméra n'est pas prête.", ToastLength.Short)?.Show();
                return;
            }

            camera.TakePicture(null, null, new PictureCallback(this));
        }
        catch (Exception ex)
        {
            CompleteWithError(ex);
        }
    }

    private void StartCameraPreview()
    {
        if (_surfaceTexture == null || _camera != null)
            return;

        try
        {
            var camera = HardwareCamera.Open()
                ?? throw new InvalidOperationException("Aucune caméra Android disponible.");

            _camera = camera;
            ConfigureCamera(camera);
            camera.SetPreviewTexture(_surfaceTexture);
            camera.StartPreview();
        }
        catch (Exception ex)
        {
            ReleaseCamera();
            CompleteWithError(new InvalidOperationException("Impossible d'ouvrir la caméra Android.", ex));
        }
    }

    private static void ConfigureCamera(HardwareCamera camera)
    {
        camera.SetDisplayOrientation(90);

        var parameters = camera.GetParameters();
        var pictureSize = parameters?.SupportedPictureSizes?
            .OrderBy(size => Math.Abs((size.Width * size.Height) - (1600 * 1200)))
            .FirstOrDefault();

        if (parameters != null && pictureSize != null)
        {
            parameters.SetPictureSize(pictureSize.Width, pictureSize.Height);
            camera.SetParameters(parameters);
        }
    }

    private void SaveCapturedJpeg(byte[] data)
    {
        try
        {
            if (data.Length == 0)
                throw new InvalidOperationException("La photo capturée est vide.");

            var path = BuildCacheJpegPath();
            IOFile.WriteAllBytes(path, data);
            CompleteAndFinish(path);
        }
        catch (Exception ex)
        {
            CompleteWithError(ex);
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != PickPhotoRequestCode)
            return;

        if (resultCode != Result.Ok || data?.Data == null)
        {
            StartCameraPreview();
            return;
        }

        try
        {
            var path = ConvertSelectedImageToJpeg(data.Data);
            CompleteAndFinish(path);
        }
        catch (Exception ex)
        {
            CompleteWithError(ex);
        }
    }

    private string ConvertSelectedImageToJpeg(Android.Net.Uri uri)
    {
        using var input = ContentResolver?.OpenInputStream(uri)
            ?? throw new InvalidOperationException("La photo sélectionnée ne peut pas être ouverte.");

        using var bitmap = BitmapFactory.DecodeStream(input)
            ?? throw new InvalidOperationException("La photo sélectionnée ne peut pas être lue.");

        var path = BuildCacheJpegPath();
        using var output = IOFile.Create(path);
        if (!bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, output))
            throw new InvalidOperationException("La conversion de la photo en JPG a échoué.");

        return path;
    }

    private string BuildCacheJpegPath()
    {
        var cleanName = SanitizeFileName(_preferredFileName);
        if (!cleanName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !cleanName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            cleanName += ".jpg";

        var nameWithoutExtension = IOPath.GetFileNameWithoutExtension(cleanName);
        var fileName = $"{nameWithoutExtension}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        return IOPath.Combine(CacheDir?.AbsolutePath ?? IOPath.GetTempPath(), fileName);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = IOPath.GetInvalidFileNameChars();
        var cleaned = new string((fileName ?? string.Empty)
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray());

        return string.IsNullOrWhiteSpace(cleaned) ? "retour_refuse.jpg" : cleaned;
    }

    private void CompleteAndFinish(string? path)
    {
        if (_isClosing)
            return;

        _isClosing = true;
        ReleaseCamera();
        RefusedReturnCameraCoordinator.Complete(path);
        Finish();
    }

    private void CompleteWithError(Exception ex)
    {
        if (_isClosing)
            return;

        _isClosing = true;
        ReleaseCamera();
        RefusedReturnCameraCoordinator.CompleteWithError(ex);
        Finish();
    }

    private void ReleaseCamera()
    {
        var camera = _camera;
        if (camera == null)
            return;

        try
        {
            camera.StopPreview();
        }
        catch
        {
            // Ignorer : la caméra peut déjà être arrêtée par Android.
        }

        camera.Release();
        _camera = null;
    }

    protected override void OnPause()
    {
        ReleaseCamera();
        base.OnPause();
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (_surfaceTexture != null)
            StartCameraPreview();
    }

    protected override void OnDestroy()
    {
        ReleaseCamera();
        if (!_isClosing)
            RefusedReturnCameraCoordinator.Complete(null);

        base.OnDestroy();
    }

    public override void OnBackPressed()
    {
        CompleteAndFinish(null);
    }

    public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
    {
        _surfaceTexture = surface;
        StartCameraPreview();
    }

    public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
    {
        _surfaceTexture = null;
        ReleaseCamera();
        return true;
    }

    public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
    {
    }

    public void OnSurfaceTextureUpdated(SurfaceTexture surface)
    {
    }

    private int Dp(int value)
        => (int)(value * Resources.DisplayMetrics.Density + 0.5f);

    private sealed class PictureCallback : Java.Lang.Object, HardwareCamera.IPictureCallback
    {
        private readonly RefusedReturnCameraActivity _activity;

        public PictureCallback(RefusedReturnCameraActivity activity)
        {
            _activity = activity;
        }

        public void OnPictureTaken(byte[]? data, HardwareCamera? camera)
        {
            if (data == null)
            {
                _activity.CompleteWithError(new InvalidOperationException("La caméra n'a retourné aucune photo."));
                return;
            }

            _activity.SaveCapturedJpeg(data);
        }
    }
}
#endif
