using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using WpfListBoxItem = System.Windows.Controls.ListBoxItem;
using WpfColor       = System.Windows.Media.Color;
using WpfBrushes     = System.Windows.Media.Brushes;
using WpfMessageBox  = System.Windows.MessageBox;

namespace SimTools
{
    public partial class MusicPlayerWindow : Window
    {
        // ── State ─────────────────────────────────────────────────────────
        private Window?         _parent;
        private DispatcherTimer _timer      = null!;
        private bool            _appClosing = false;

        // ── Constructor ───────────────────────────────────────────────────
        public MusicPlayerWindow()
        {
            InitializeComponent();

            // Timer — fires every second to keep the time display current
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) =>
            {
                MusicPlayerService.Tick();
                RefreshTimeDisplay();
                RefreshPlayPauseButton();
            };
            _timer.Start();

            MusicPlayerService.TrackChanged += OnTrackChanged;

            // Hide instead of close while the app is running
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

        // ── Enable / disable (called from SettingsWindow) ─────────────────
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
        /// <summary>
        /// Shows the first-run download prompt. If the user accepts, a modal
        /// progress window opens and blocks the owner until the download
        /// completes or is cancelled. Returns when the window is dismissed.
        /// </summary>
        public void ShowFirstRunPrompt(string musicFolder, Window owner)
        {
            bool prompted = IniHelper.ReadBool("Music", "DownloadPromptShown", false);
            if (prompted) return;
            IniHelper.WriteBool("Music", "DownloadPromptShown", true);

            var result = WpfMessageBox.Show(
                "Would you like to download a free background music pack?\n\n" +
                "You can also drop your own songs (MP3, WAV, FLAC, M4A) into\n" +
                $"the /res/music folder at any time:\n{musicFolder}",
                "Background Music",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Modal — IntroductoryPage is blocked until download completes or cancels
            new MusicDownloadWindow(musicFolder) { Owner = owner }.ShowDialog();
        }



        // ── Drag bar ──────────────────────────────────────────────────────
        private void DragBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        // ── Button handlers ───────────────────────────────────────────────
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (MusicPlayerService.Playlist.Count == 0) return;
            MusicPlayerService.TogglePlayPause();
            RefreshPlayPauseButton();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
            => MusicPlayerService.Stop();

        private void Mute_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayerService.ToggleMute();
            MuteBtn.Content = MusicPlayerService.IsMuted ? "🔇" : "🔊";
        }

        private void Note_Click(object sender, RoutedEventArgs e)
        {
            PopulateSongList();
            SongPopup.IsOpen = !SongPopup.IsOpen;
        }

        private void SongList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SongList.SelectedItem is WpfListBoxItem { Tag: int idx })
            {
                MusicPlayerService.SelectTrack(idx);
                SongPopup.IsOpen = false;
            }
        }

        // ── Refresh helpers ───────────────────────────────────────────────
        private void OnTrackChanged()
        {
            Dispatcher.InvokeAsync(() =>
            {
                var song = MusicPlayerService.CurrentSong;
                TitleBlock.Text  = song?.Title  ?? "No track loaded";
                ArtistBlock.Text = song?.Artist ?? "";

                if (song?.AlbumArt != null)
                {
                    AlbumArtImage.Source     = song.AlbumArt;
                    AlbumArtImage.Visibility = Visibility.Visible;
                    ArtPlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    AlbumArtImage.Visibility  = Visibility.Collapsed;
                    ArtPlaceholder.Visibility = Visibility.Visible;
                }

                RefreshPlayPauseButton();
                RefreshTimeDisplay();
            });
        }

        private void RefreshPlayPauseButton()
            => PlayPauseBtn.Content = MusicPlayerService.IsPlaying ? "⏸" : "▶";

        private void RefreshTimeDisplay()
        {
            static string Fmt(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
            TimeBlock.Text = $"{Fmt(MusicPlayerService.CurrentTime)} / {Fmt(MusicPlayerService.TotalTime)}";
        }

        private void PopulateSongList()
        {
            SongList.Items.Clear();
            int i = 0;
            foreach (var path in MusicPlayerService.Playlist)
            {
                SongList.Items.Add(new WpfListBoxItem
                {
                    Content    = Path.GetFileNameWithoutExtension(path),
                    Tag        = i,
                    Background = (i == MusicPlayerService.CurrentIndex)
                        ? new SolidColorBrush(WpfColor.FromRgb(0x2A, 0x4A, 0x30))
                        : WpfBrushes.Transparent,
                    Foreground = WpfBrushes.White,
                    Padding    = new Thickness(6, 3, 6, 3)
                });
                i++;
            }

            if (MusicPlayerService.CurrentIndex >= 0 &&
                MusicPlayerService.CurrentIndex < SongList.Items.Count)
                SongList.ScrollIntoView(SongList.Items[MusicPlayerService.CurrentIndex]);
        }
    }
}
