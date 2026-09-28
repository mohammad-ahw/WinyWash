using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace CleanC.UI
{
    public static class WallpaperService
    {
        private static bool? _isCachedDark = null;
        private static DateTime _lastCheck = DateTime.MinValue;

        public static async Task<bool> IsWallpaperDarkAsync()
        {
            // Cache result for 5 seconds to avoid repeated disk reads during frequent UI events
            if (_isCachedDark.HasValue && (DateTime.Now - _lastCheck).TotalSeconds < 5)
            {
                return _isCachedDark.Value;
            }

            try
            {
                string wallpaperPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Themes\TranscodedWallpaper");

                if (!File.Exists(wallpaperPath))
                {
                    using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
                    wallpaperPath = key?.GetValue("WallPaper") as string ?? "";
                }

                if (!File.Exists(wallpaperPath))
                {
                    // Check solid color background
                    using var colorKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors");
                    string? bgRgb = colorKey?.GetValue("Background") as string;
                    if (!string.IsNullOrWhiteSpace(bgRgb))
                    {
                        var parts = bgRgb.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 3 &&
                            byte.TryParse(parts[0], out byte r) &&
                            byte.TryParse(parts[1], out byte g) &&
                            byte.TryParse(parts[2], out byte b))
                        {
                            double lum = 0.299 * r + 0.587 * g + 0.114 * b;
                            _isCachedDark = lum < 130.0;
                            _lastCheck = DateTime.Now;
                            return _isCachedDark.Value;
                        }
                    }

                    _isCachedDark = false;
                    return false;
                }

                var file = await StorageFile.GetFileFromPathAsync(wallpaperPath);
                using var stream = await file.OpenAsync(FileAccessMode.Read);
                var decoder = await BitmapDecoder.CreateAsync(stream);

                // Sample down to a fast 16x16 thumbnail for instant luminance check
                var transform = new BitmapTransform
                {
                    ScaledWidth = 16,
                    ScaledHeight = 16,
                    InterpolationMode = BitmapInterpolationMode.Linear
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Rgba8,
                    BitmapAlphaMode.Premultiplied,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage);

                byte[] pixels = pixelData.DetachPixelData();
                double totalLuminance = 0;
                int pixelCount = pixels.Length / 4;

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte r = pixels[i];
                    byte g = pixels[i + 1];
                    byte b = pixels[i + 2];
                    totalLuminance += (0.299 * r + 0.587 * g + 0.114 * b);
                }

                double avgLuminance = totalLuminance / Math.Max(1, pixelCount);
                // If average luminance is below 130 out of 255, it is a dark wallpaper
                _isCachedDark = avgLuminance < 130.0;
                _lastCheck = DateTime.Now;
                return _isCachedDark.Value;
            }
            catch
            {
                // Safe fallback: default to dark so white text remains readable over dark backgrounds
                return true;
            }
        }
    }
}
