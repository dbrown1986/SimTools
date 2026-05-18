using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using NAudio.Wave;

namespace SimTools
{
    // ── Metadata for the currently playing track ───────────────────────────
    public sealed class SongInfo
    {
        public string       FilePath { get; init; } = "";
        public string       Title    { get; init; } = "Unknown Title";
        public string       Artist   { get; init; } = "Unknown Artist";
        public BitmapImage? AlbumArt { get; init; }
    }

    // ── Static playback service ────────────────────────────────────────────
    public static class MusicPlayerService
    {
        // ── Engine ────────────────────────────────────────────────────────
        private static IWavePlayer?    _output;
        private static AudioFileReader? _reader;

        // ── Playlist ──────────────────────────────────────────────────────
        private static List<string> _playlist = new();
        private static int          _index    = -1;

        // ── Volume / mute ─────────────────────────────────────────────────
        private static float _savedVolume = 0.8f;
        private static bool  _muted       = false;
        private static bool  _stopping    = false;

        // ── Events ────────────────────────────────────────────────────────
        /// <summary>Fired on the UI thread whenever the current track changes.</summary>
        public static event Action?                     TrackChanged;
        /// <summary>Fired every second with (currentTime, totalTime).</summary>
        public static event Action<TimeSpan, TimeSpan>? PositionChanged;

        // ── State ─────────────────────────────────────────────────────────
        public static SongInfo? CurrentSong  { get; private set; }
        public static bool      IsPlaying    => _output?.PlaybackState == PlaybackState.Playing;
        public static bool      IsPaused     => _output?.PlaybackState == PlaybackState.Paused;
        public static bool      IsMuted      => _muted;
        public static IReadOnlyList<string> Playlist => _playlist.AsReadOnly();
        public static int       CurrentIndex => _index;
        public static TimeSpan  CurrentTime  => _reader?.CurrentTime ?? TimeSpan.Zero;
        public static TimeSpan  TotalTime    => _reader?.TotalTime   ?? TimeSpan.Zero;

        // ── Playlist ──────────────────────────────────────────────────────
        /// <summary>Scans <paramref name="folder"/> for supported audio files and shuffles them.</summary>
        public static void LoadPlaylist(string folder)
        {
            _playlist.Clear();
            _index = -1;

            if (!Directory.Exists(folder)) return;

            var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".mp3", ".wav", ".flac", ".m4a" };

            var files = Directory.GetFiles(folder)
                .Where(f => exts.Contains(Path.GetExtension(f)))
                .ToList();

            var rng = new Random();
            _playlist = files.OrderBy(_ => rng.Next()).ToList();
        }

        // ── Controls ──────────────────────────────────────────────────────
        public static void Play()
        {
            if (_output?.PlaybackState == PlaybackState.Paused)
            {
                _stopping = false;
                _output.Play();
                return;
            }
            PlayNext();
        }

        public static void Pause()
        {
            if (_output?.PlaybackState == PlaybackState.Playing)
                _output.Pause();
        }

        public static void TogglePlayPause()
        {
            if (IsPlaying) Pause();
            else           Play();
        }

        public static void Stop()
        {
            _stopping = true;
            DisposeEngine();
            CurrentSong = null;
            TrackChanged?.Invoke();
        }

        public static void PlayNext()
        {
            if (_playlist.Count == 0) return;
            _index = (_index + 1) % _playlist.Count;
            OpenAndPlay(_playlist[_index]);
        }

        public static void SelectTrack(int index)
        {
            if (index < 0 || index >= _playlist.Count) return;
            _index = index;
            OpenAndPlay(_playlist[_index]);
        }

        public static void ToggleMute()
        {
            _muted = !_muted;
            if (_reader != null)
                _reader.Volume = _muted ? 0f : _savedVolume;
        }

        /// <summary>Call from a DispatcherTimer tick to fire PositionChanged.</summary>
        public static void Tick()
        {
            if (_reader != null)
                PositionChanged?.Invoke(CurrentTime, TotalTime);
        }

        // ── Internals ─────────────────────────────────────────────────────
        private static void OpenAndPlay(string path)
        {
            DisposeEngine();
            _stopping = false;

            try
            {
                _reader        = new AudioFileReader(path);
                _reader.Volume = _muted ? 0f : _savedVolume;

                _output = new WaveOutEvent();
                _output.Init(_reader);
                _output.PlaybackStopped += OnPlaybackStopped;
                _output.Play();

                CurrentSong = ReadTags(path);
                TrackChanged?.Invoke();
            }
            catch
            {
                DisposeEngine();
                // Skip unreadable / unsupported file and move on
                if (_playlist.Count > 1) PlayNext();
            }
        }

        private static void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (_stopping || e.Exception != null) return;

            // Auto-advance when the track ends naturally (within 2 s of total time)
            bool endedNaturally = _reader != null
                && _reader.TotalTime > TimeSpan.Zero
                && (_reader.TotalTime - _reader.CurrentTime).TotalSeconds < 2.0;

            if (endedNaturally)
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(PlayNext);
        }

        private static SongInfo ReadTags(string path)
        {
            string       title  = Path.GetFileNameWithoutExtension(path);
            string       artist = "Unknown Artist";
            BitmapImage? art    = null;

            try
            {
                using var file = TagLib.File.Create(path);

                if (!string.IsNullOrWhiteSpace(file.Tag.Title))
                    title = file.Tag.Title;

                if (file.Tag.Performers?.Length > 0)
                    artist = string.Join(", ", file.Tag.Performers);

                var pic = file.Tag.Pictures?.FirstOrDefault();
                if (pic?.Data?.Data?.Length > 0)
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = new System.IO.MemoryStream(pic.Data.Data);
                    bmp.CacheOption  = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    art = bmp;
                }
            }
            catch { /* tag read failure — use filename fallbacks */ }

            return new SongInfo { FilePath = path, Title = title, Artist = artist, AlbumArt = art };
        }

        private static void DisposeEngine()
        {
            _output?.Stop();
            _output?.Dispose();
            _output = null;
            _reader?.Dispose();
            _reader = null;
        }

        public static void Shutdown()
        {
            _stopping = true;
            DisposeEngine();
        }
    }
}
