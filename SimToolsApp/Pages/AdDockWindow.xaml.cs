// SimTools
// Main Application
// Advertisement Dock Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.    

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using WpfCursors = System.Windows.Input.Cursors;

namespace SimTools
{
    public partial class AdDockWindow : Window
    {
        // ── State ─────────────────────────────────────────────────────────
        private Window? _parent;
        private bool    _appClosing = false;

        // ── Ad rotation ───────────────────────────────────────────────────
        private List<(BitmapImage Image, string? Url)> _ads = new();
        private int              _adIndex        = 0;
        private DispatcherTimer? _rotationTimer;
        private const double     RotationSeconds = 15.0;
        private const double     FadeMs          = 350.0;

        // ── Current click URL ─────────────────────────────────────────────
        private string? _adUrl;

        // ── Drag / click detection ────────────────────────────────────────
        //
        //  DragMove() is synchronous — it blocks until the mouse button is
        //  released.  We snapshot Left/Top before calling it and compare
        //  afterwards.  If the window didn't move (or moved less than the
        //  threshold), the user clicked rather than dragged.
        //
        private const double DragThreshold = 4.0;

        // ── P/Invoke — bring to front without stealing focus ──────────────
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_TOP    = IntPtr.Zero;
        private const uint SWP_NOMOVE     = 0x0002;
        private const uint SWP_NOSIZE     = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;

        // ── Constructor ───────────────────────────────────────────────────
        public AdDockWindow()
        {
            InitializeComponent();

            Closing += (_, e) =>
            {
                if (!_appClosing)
                    e.Cancel = true;
            };

            _ = LoadAdsAsync();
        }

        // ── App shutdown ──────────────────────────────────────────────────
        public void AppClose()
        {
            _rotationTimer?.Stop();
            _appClosing = true;
            Close();
        }

        // ── Window attachment / positioning ───────────────────────────────
        public void AttachTo(Window parent)
        {
            if (_parent != null)
            {
                _parent.LocationChanged -= OnParentGeometryChanged;
                _parent.SizeChanged     -= OnParentGeometryChanged;
                _parent.Activated       -= OnParentActivated;
            }

            _parent = parent;
            parent.LocationChanged += OnParentGeometryChanged;
            parent.SizeChanged     += OnParentGeometryChanged;
            parent.Activated       += OnParentActivated;
            UpdatePosition();
        }

        private void OnParentGeometryChanged(object? s, EventArgs e) => UpdatePosition();

        private void OnParentActivated(object? s, EventArgs e)
        {
            if (!IsVisible) return;
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowPos(hwnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void UpdatePosition()
        {
            if (_parent == null) return;
            Left = _parent.Left + (_parent.ActualWidth - Width) / 2.0;
            Top  = _parent.Top  + _parent.ActualHeight;
        }

        // ── Click / drag handler ──────────────────────────────────────────
        private void Dock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Since DragMove() is removed, any left-click is guaranteed to be a real click
            if (_adUrl != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(_adUrl) { UseShellExecute = true });
                }
                catch { /* Silently ignore if the URL can't be opened */ }
            }
        }

        // ── Ad loading ────────────────────────────────────────────────────
        //
        //  Reads  %baseurl%/ads/manifest.txt  which lists one ad per line:
        //
        //      # Lines starting with # are comments and are ignored
        //      banner1.png|https://www.patreon.com/simtools
        //      banner2.png|https://ko-fi.com/simtools
        //      banner3.png|https://store.simtools-app.com
        //
        //  The image filename and click URL are separated by a pipe (|).
        //  The URL is optional — omit the pipe and the ad will show but
        //  will not be clickable.
        //
        //  All images listed in the manifest are downloaded in parallel.
        //  Any entry whose image fails to download is silently skipped.
        //  If only one ad loads, no rotation timer is started.
        //
        private async Task LoadAdsAsync()
        {
            try
            {
                // ── 1. Fetch manifest ──────────────────────────────────────
                string manifestUrl = AppSettings.ResolveUrl("%baseurl%/ads/manifest.txt");

                // Bypasses Schannel entirely using our custom BouncyCastle engine!
                string manifest = await SecureWebClient.GetStringAsync(manifestUrl);

                // ── 2. Parse entries ───────────────────────────────────────
                var entries = manifest
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0 && !l.StartsWith('#'))
                    .Select(l =>
                    {
                        int pipe = l.IndexOf('|');
                        if (pipe < 0) return (File: l, Url: (string?)null);
                        return (File: l[..pipe].Trim(), Url: (string?)l[(pipe + 1)..].Trim());
                    })
                    .Where(e => !string.IsNullOrWhiteSpace(e.File))
                    .ToList();

                if (entries.Count == 0) return;

                // ── 3. Download all images in parallel ─────────────────────
                var downloadTasks = entries.Select(async entry =>
                {
                    // Generate a safe unique temp path for this image
                    string tempImagePath = Path.Combine(Path.GetTempPath(), $"simtools_ad_{Guid.NewGuid():N}.png");
                    try
                    {
                        string imageUrl = AppSettings.ResolveUrl($"%baseurl%/ads/{entry.File}");

                        // Download directly to temp location bypassing Schannel
                        await SecureWebClient.DownloadFileAsync(imageUrl, tempImagePath);

                        if (!File.Exists(tempImagePath))
                            return (Image: (BitmapImage?)null, entry.Url);

                        // Load the image from disk into memory
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = new MemoryStream(await File.ReadAllBytesAsync(tempImagePath));
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        return (Image: (BitmapImage?)bmp, entry.Url);
                    }
                    catch
                    {
                        return (Image: (BitmapImage?)null, entry.Url);
                    }
                    finally
                    {
                        // Clean up the temporary file
                        try
                        {
                            if (File.Exists(tempImagePath))
                                File.Delete(tempImagePath);
                        }
                        catch { /* Ignore cleanup failures */ }
                    }
                });

                var results = await Task.WhenAll(downloadTasks);

                var loaded = results
                    .Where(r => r.Image != null)
                    .Select(r => (r.Image!, r.Url))
                    .ToList();

                if (loaded.Count == 0) return;

                // ── 4. Display first ad and start rotation timer ───────────
                Dispatcher.Invoke(() =>
                {
                    _ads = loaded;
                    _adIndex = 0;
                    ShowAd(_adIndex, animate: false);

                    if (_ads.Count > 1)
                    {
                        _rotationTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(RotationSeconds)
                        };
                        _rotationTimer.Tick += RotationTimer_Tick;
                        _rotationTimer.Start();
                    }
                });
            }
            catch
            {
                // Manifest unavailable — placeholder stays visible
            }
        }

        // ── Show a specific ad slot ───────────────────────────────────────
        private void ShowAd(int index, bool animate = true)
        {
            var (image, url) = _ads[index];

            if (animate)
            {
                // Fade out → swap → fade in
                var fadeOut = new DoubleAnimation(1.0, 0.0,
                    TimeSpan.FromMilliseconds(FadeMs))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                fadeOut.Completed += (_, _) =>
                {
                    AdImage.Source = image;
                    _adUrl         = string.IsNullOrWhiteSpace(url) ? null : url;
                    Cursor         = _adUrl != null ? WpfCursors.Hand : WpfCursors.Arrow;

                    var fadeIn = new DoubleAnimation(0.0, 1.0,
                        TimeSpan.FromMilliseconds(FadeMs))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };
                    AdImage.BeginAnimation(OpacityProperty, fadeIn);
                };

                AdImage.BeginAnimation(OpacityProperty, fadeOut);
            }
            else
            {
                AdImage.Source           = image;
                AdImage.Opacity          = 1.0;
                AdImage.Visibility       = Visibility.Visible;
                AdPlaceholder.Visibility = Visibility.Collapsed;
                _adUrl  = string.IsNullOrWhiteSpace(url) ? null : url;
                Cursor  = _adUrl != null ? WpfCursors.Hand : WpfCursors.Arrow;
            }
        }

        // ── Rotation tick ─────────────────────────────────────────────────
        private void RotationTimer_Tick(object? sender, EventArgs e)
        {
            _adIndex = (_adIndex + 1) % _ads.Count;
            ShowAd(_adIndex, animate: true);
        }
    }
}
