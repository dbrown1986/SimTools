using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

using WpfButton      = System.Windows.Controls.Button;
using WpfImage       = System.Windows.Controls.Image;
using WpfListBox     = System.Windows.Controls.ListBox;
using WpfListBoxItem = System.Windows.Controls.ListBoxItem;
using WpfSeparator   = System.Windows.Controls.Separator;
using WpfColor       = System.Windows.Media.Color;
using WpfColors      = System.Windows.Media.Colors;
using WpfBrushes     = System.Windows.Media.Brushes;
using WpfCursors     = System.Windows.Input.Cursors;
using WpfMessageBox  = System.Windows.MessageBox;
using WpfHAlign      = System.Windows.HorizontalAlignment;
using WpfVAlign      = System.Windows.VerticalAlignment;

namespace SimTools
{
    public class MusicPlayerWindow : Window
    {
        // ── Brushes (match app dark theme) ────────────────────────────────
        private static readonly SolidColorBrush BgBrush     = new(WpfColor.FromRgb(0x1E, 0x1E, 0x1E));
        private static readonly SolidColorBrush AccentBrush = new(WpfColor.FromRgb(0x4A, 0x9E, 0x5C));
        private static readonly SolidColorBrush TextBrush   = new(WpfColors.White);
        private static readonly SolidColorBrush DimBrush    = new(WpfColor.FromRgb(0xAA, 0xAA, 0xAA));
        private static readonly SolidColorBrush BtnBrush    = new(WpfColor.FromRgb(0x2D, 0x2D, 0x2D));
        private static readonly SolidColorBrush BtnHover    = new(WpfColor.FromRgb(0x3A, 0x3A, 0x3A));
        private static readonly SolidColorBrush BorderBrush2 = new(WpfColor.FromRgb(0x3A, 0x3A, 0x3A));

        // ── UI controls ───────────────────────────────────────────────────
        private WpfImage    _artImage     = null!;
        private TextBlock _titleBlock   = null!;
        private TextBlock _artistBlock  = null!;
        private TextBlock _timeBlock    = null!;
        private WpfButton   _playPauseBtn = null!;
        private WpfButton   _muteBtn      = null!;
        private Popup     _songPopup    = null!;
        private WpfListBox  _songList     = null!;

        // ── State ─────────────────────────────────────────────────────────
        private Window?         _parent;
        private DispatcherTimer _timer = null!;
        private bool            _appClosing = false;

        // ── Constructor ───────────────────────────────────────────────────
        public MusicPlayerWindow()
        {
            WindowStyle        = WindowStyle.None;
            AllowsTransparency = true;
            Background         = WpfBrushes.Transparent;
            Width              = 256;
            SizeToContent      = SizeToContent.Height;
            ShowInTaskbar      = false;
            ResizeMode         = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.Manual;

            Content = BuildUI();

            // Timer — fires every second to update the time display
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) =>
            {
                MusicPlayerService.Tick();
                RefreshTimeDisplay();
                RefreshPlayPauseButton();
            };
            _timer.Start();

            MusicPlayerService.TrackChanged += OnTrackChanged;

            // Prevent close — hide instead (unless app is shutting down)
            Closing += (_, e) =>
            {
                if (!_appClosing)
                    e.Cancel = true;
            };
        }

        // ── App shutdown ──────────────────────────────────────────────────
        public void AppClose()
        {
            _appClosing = true;
            _timer.Stop();
            MusicPlayerService.TrackChanged -= OnTrackChanged;
            MusicPlayerService.Shutdown();
        }

        // ── Window attachment / positioning ───────────────────────────────
        public void AttachTo(Window parent)
        {
            if (_parent != null)
            {
                _parent.LocationChanged -= OnParentGeometryChanged;
                _parent.SizeChanged     -= OnParentGeometryChanged;
            }

            _parent = parent;
            parent.LocationChanged += OnParentGeometryChanged;
            parent.SizeChanged     += OnParentGeometryChanged;
            UpdatePosition();
        }

        private void OnParentGeometryChanged(object? s, EventArgs e) => UpdatePosition();

        private void UpdatePosition()
        {
            if (_parent == null) return;
            Left = _parent.Left + _parent.ActualWidth + 10;
            Top  = _parent.Top;
        }

        // ── Enable / disable (called from Settings) ───────────────────────
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                Show();
                if (!MusicPlayerService.IsPlaying && MusicPlayerService.Playlist.Count > 0)
                    MusicPlayerService.Play();
            }
            else
            {
                MusicPlayerService.Stop();
                Hide();
            }
        }

        // ── First-run music prompt (called from IntroductoryPage) ─────────
        public async Task ShowFirstRunPromptAsync(string musicFolder)
        {
            bool prompted = IniHelper.ReadBool("Music", "DownloadPromptShown", false);
            if (prompted) return;
            IniHelper.WriteBool("Music", "DownloadPromptShown", true);

            var result = WpfMessageBox.Show(
                "Would you like to download a free background music pack?\n\n" +
                "Tip: You can also drop your own songs (MP3, WAV, FLAC, M4A)\n" +
                $"into the /res/music folder at any time:\n{musicFolder}",
                "Background Music",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                await DownloadMusicPackAsync(musicFolder);
        }

        private static async Task DownloadMusicPackAsync(string musicFolder)
        {
            try
            {
                Directory.CreateDirectory(musicFolder);

                string manifestUrl = AppSettings.ResolveUrl("%baseurl%/res/music/manifest.txt");
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                string manifest = await http.GetStringAsync(manifestUrl);

                var tracks = manifest.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var track in tracks)
                {
                    string name = track.Trim();
                    if (string.IsNullOrEmpty(name)) continue;

                    string dest = Path.Combine(musicFolder, name);
                    if (File.Exists(dest)) continue;

                    try
                    {
                        string fileUrl = AppSettings.ResolveUrl($"%baseurl%/res/music/{name}");
                        using var resp = await http.GetAsync(fileUrl);
                        if (!resp.IsSuccessStatusCode) continue;

                        await using var fs = new FileStream(dest, FileMode.Create, FileAccess.Write);
                        await resp.Content.CopyToAsync(fs);
                    }
                    catch { /* skip individual failed track */ }
                }
            }
            catch
            {
                WpfMessageBox.Show(
                    "Could not reach the music server. You can manually place songs in the /res/music folder.",
                    "Music Download",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // ── UI builder ────────────────────────────────────────────────────
        private UIElement BuildUI()
        {
            var outer = new Border
            {
                Background   = BgBrush,
                CornerRadius = new CornerRadius(8),
                Padding      = new Thickness(10),
                Effect       = new DropShadowEffect
                {
                    BlurRadius  = 14,
                    ShadowDepth = 3,
                    Color       = WpfColors.Black,
                    Opacity     = 0.65
                }
            };

            var root = new StackPanel();

            // ── Drag bar ──────────────────────────────────────────────────
            var dragBar = new DockPanel { Margin = new Thickness(0, 0, 0, 6), LastChildFill = true };
            var dragTitle = new TextBlock
            {
                Text              = "♪  SimTools Music",
                Foreground        = AccentBrush,
                FontSize          = 11,
                FontWeight        = FontWeights.SemiBold,
                VerticalAlignment = WpfVAlign.Center
            };
            dragBar.Children.Add(dragTitle);
            dragBar.MouseLeftButtonDown += (_, _) => DragMove();
            root.Children.Add(dragBar);

            // ── Thin separator ────────────────────────────────────────────
            root.Children.Add(new WpfSeparator
            {
                Background = BorderBrush2,
                Margin     = new Thickness(0, 0, 0, 8)
            });

            // ── Art + Info row ────────────────────────────────────────────
            var infoRow = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            infoRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            infoRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Album art
            var artBorder = new Border
            {
                Width        = 76,
                Height       = 76,
                CornerRadius = new CornerRadius(4),
                Background   = BtnBrush,
                Margin       = new Thickness(0, 0, 10, 0),
                ClipToBounds = true
            };
            var artGrid = new Grid();
            var artPlaceholder = new TextBlock
            {
                Text                = "♪",
                FontSize            = 28,
                Foreground          = DimBrush,
                HorizontalAlignment = WpfHAlign.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };
            _artImage = new WpfImage
            {
                Stretch    = Stretch.UniformToFill,
                Visibility = Visibility.Collapsed
            };
            artGrid.Children.Add(artPlaceholder);
            artGrid.Children.Add(_artImage);
            artBorder.Child = artGrid;
            Grid.SetColumn(artBorder, 0);

            // Song info
            var infoStack = new StackPanel { VerticalAlignment = WpfVAlign.Center };
            _titleBlock = new TextBlock
            {
                Text         = "No track loaded",
                Foreground   = TextBrush,
                FontSize     = 12,
                FontWeight   = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap
            };
            _artistBlock = new TextBlock
            {
                Text         = "",
                Foreground   = DimBrush,
                FontSize     = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap,
                Margin       = new Thickness(0, 3, 0, 0)
            };
            _timeBlock = new TextBlock
            {
                Text       = "0:00 / 0:00",
                Foreground = DimBrush,
                FontSize   = 10,
                Margin     = new Thickness(0, 8, 0, 0)
            };
            infoStack.Children.Add(_titleBlock);
            infoStack.Children.Add(_artistBlock);
            infoStack.Children.Add(_timeBlock);
            Grid.SetColumn(infoStack, 1);

            infoRow.Children.Add(artBorder);
            infoRow.Children.Add(infoStack);
            root.Children.Add(infoRow);

            // ── Control buttons ───────────────────────────────────────────
            // Layout: [▶/⏸]  [■]  [🔊]  [♪]
            var controls = new UniformGrid { Rows = 1, Margin = new Thickness(0, 2, 0, 0) };

            _playPauseBtn = MakeBtn("▶", PlayPause_Click);
            var stopBtn   = MakeBtn("■", Stop_Click);
            _muteBtn      = MakeBtn("🔊", Mute_Click);
            var noteBtn   = MakeBtn("♪", Note_Click);

            controls.Children.Add(_playPauseBtn);
            controls.Children.Add(stopBtn);
            controls.Children.Add(_muteBtn);
            controls.Children.Add(noteBtn);
            root.Children.Add(controls);

            // ── Song-selection popup ───────────────────────────────────────
            _songList = new WpfListBox
            {
                Background  = BgBrush,
                Foreground  = TextBrush,
                BorderBrush = BorderBrush2,
                MaxHeight   = 280,
                MinWidth    = 234,
                FontSize    = 11
            };
            _songList.MouseDoubleClick += SongList_DoubleClick;

            _songPopup = new Popup
            {
                Child = new Border
                {
                    Child           = _songList,
                    Background      = BgBrush,
                    BorderBrush     = AccentBrush,
                    BorderThickness = new Thickness(1),
                    CornerRadius    = new CornerRadius(4),
                    Padding         = new Thickness(2)
                },
                PlacementTarget    = noteBtn,
                Placement          = PlacementMode.Top,
                StaysOpen          = false,
                AllowsTransparency = true
            };

            outer.Child = root;
            return outer;
        }

        private WpfButton MakeBtn(string content, RoutedEventHandler handler)
        {
            var btn = new WpfButton
            {
                Content         = content,
                FontSize        = 16,
                Height          = 36,
                Background      = BtnBrush,
                Foreground      = TextBrush,
                BorderBrush     = BorderBrush2,
                BorderThickness = new Thickness(1),
                Cursor          = WpfCursors.Hand,
                Margin          = new Thickness(2)
            };
            btn.Click += handler;
            return btn;
        }

        // ── Button handlers ───────────────────────────────────────────────
        private void PlayPause_Click(object s, RoutedEventArgs e)
        {
            if (MusicPlayerService.Playlist.Count == 0) return;
            MusicPlayerService.TogglePlayPause();
            RefreshPlayPauseButton();
        }

        private void Stop_Click(object s, RoutedEventArgs e)
        {
            MusicPlayerService.Stop();
        }

        private void Mute_Click(object s, RoutedEventArgs e)
        {
            MusicPlayerService.ToggleMute();
            _muteBtn.Content = MusicPlayerService.IsMuted ? "🔇" : "🔊";
        }

        private void Note_Click(object s, RoutedEventArgs e)
        {
            PopulateSongList();
            _songPopup.IsOpen = !_songPopup.IsOpen;
        }

        private void SongList_DoubleClick(object s, MouseButtonEventArgs e)
        {
            if (_songList.SelectedItem is WpfListBoxItem { Tag: int idx })
            {
                MusicPlayerService.SelectTrack(idx);
                _songPopup.IsOpen = false;
            }
        }

        // ── Refresh helpers ───────────────────────────────────────────────
        private void OnTrackChanged()
        {
            Dispatcher.InvokeAsync(() =>
            {
                var song = MusicPlayerService.CurrentSong;
                _titleBlock.Text  = song?.Title  ?? "No track loaded";
                _artistBlock.Text = song?.Artist ?? "";

                if (song?.AlbumArt != null)
                {
                    _artImage.Source     = song.AlbumArt;
                    _artImage.Visibility = Visibility.Visible;
                }
                else
                {
                    _artImage.Visibility = Visibility.Collapsed;
                }

                RefreshPlayPauseButton();
                RefreshTimeDisplay();
            });
        }

        private void RefreshPlayPauseButton()
            => _playPauseBtn.Content = MusicPlayerService.IsPlaying ? "⏸" : "▶";

        private void RefreshTimeDisplay()
        {
            static string Fmt(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
            _timeBlock.Text = $"{Fmt(MusicPlayerService.CurrentTime)} / {Fmt(MusicPlayerService.TotalTime)}";
        }

        private void PopulateSongList()
        {
            _songList.Items.Clear();
            int i = 0;
            foreach (var path in MusicPlayerService.Playlist)
            {
                var item = new WpfListBoxItem
                {
                    Content    = Path.GetFileNameWithoutExtension(path),
                    Tag        = i,
                    Background = (i == MusicPlayerService.CurrentIndex)
                        ? new SolidColorBrush(WpfColor.FromRgb(0x2A, 0x4A, 0x30))
                        : WpfBrushes.Transparent,
                    Foreground = TextBrush,
                    Padding    = new Thickness(6, 3, 6, 3)
                };
                _songList.Items.Add(item);
                i++;
            }

            if (MusicPlayerService.CurrentIndex >= 0 &&
                MusicPlayerService.CurrentIndex < _songList.Items.Count)
                _songList.ScrollIntoView(_songList.Items[MusicPlayerService.CurrentIndex]);
        }
    }
}
