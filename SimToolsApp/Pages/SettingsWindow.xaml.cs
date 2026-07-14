// SimTools
// Main Application
// Settings Menu Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimTools;

// ══════════════════════════════════════════════════════════════════════════════
//  ViewModel — one instance per game / expansion entry in the directories list
// ══════════════════════════════════════════════════════════════════════════════
public sealed class GameSettingViewModel : INotifyPropertyChanged
{
    public string Key { get; }
    public string Name { get; }
    public bool HasMods { get; }
    public bool HasUserData { get; }

    private string _gameDir = string.Empty;
    public string GameDir
    {
        get => _gameDir;
        set { if (_gameDir == value) return; _gameDir = value; OnPropertyChanged(); }
    }

    private string _modDir = string.Empty;
    public string ModDir
    {
        get => _modDir;
        set { if (_modDir == value) return; _modDir = value; OnPropertyChanged(); }
    }

    private string _UserDataDir = string.Empty;
    public string UserDataDir
    {
        get => _UserDataDir;
        set { if (_UserDataDir == value) return; _UserDataDir = value; OnPropertyChanged(); }
    }

    public ICommand BrowseGameCommand { get; }
    public ICommand BrowseModCommand { get; }
    public ICommand BrowseUserDataCommand { get; }

    [SupportedOSPlatform("windows")]
    public GameSettingViewModel(string key, string name, bool hasMods, bool hasUserData)
    {
        Key = key;
        Name = name;
        HasMods = hasMods;
        HasUserData = hasUserData;

        BrowseGameCommand = new RelayCommand(_ =>
        {
            if (Key == "Sims3")
                System.Windows.MessageBox.Show(
                    LanguageManager.Get("Settings", "Sims3DirWarning", "Please set your Sims 3 game directory to the root directory."),
                    LanguageManager.Get("Settings", "Sims3DirWarning_Title", "SimTools — Sims 3 Directory"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            if (!string.IsNullOrWhiteSpace(GameDir) &&
                System.IO.Directory.Exists(GameDir))
                dlg.SelectedPath = GameDir;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                GameDir = dlg.SelectedPath;
        });

        BrowseModCommand = new RelayCommand(_ =>
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            if (!string.IsNullOrWhiteSpace(ModDir) &&
                System.IO.Directory.Exists(ModDir))
                dlg.SelectedPath = ModDir;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ModDir = dlg.SelectedPath;
        }, _ => HasMods);

        BrowseUserDataCommand = new RelayCommand(_ =>
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            if (!string.IsNullOrWhiteSpace(UserDataDir) &&
                System.IO.Directory.Exists(UserDataDir))
                dlg.SelectedPath = UserDataDir;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                UserDataDir = dlg.SelectedPath;
        }, _ => HasUserData);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ══════════════════════════════════════════════════════════════════════════════
//  Window code-behind
// ══════════════════════════════════════════════════════════════════════════════
public partial class SettingsWindow : Window
{
    // ── Static data (identical to original) ──────────────────────────────────
    private static readonly (string Key, string Name)[] Games = new (string Key, string Name)[]
{
    ("Sims1",                LanguageManager.Get("BuyTS3", "Sims1_Disc", "The Sims")),
    ("Sims2",                LanguageManager.Get("BuyTS3", "Sims2_Disc", "The Sims 2")),
    ("SimsLifeStories",      LanguageManager.Get("BuyTS3", "TSLife", "The Sims Life Stories")),
    ("SimsPetStories",       LanguageManager.Get("BuyTS3", "TSPets", "The Sims Pet Stories")),
    ("SimsCastawayStories",  LanguageManager.Get("BuyTS3", "TSCast", "The Sims Castaway Stories")),
    ("Sims3",                LanguageManager.Get("BuyTS3", "Sims3", "The Sims 3")),
    ("Sims4",                LanguageManager.Get("BuyTS3", "TS4", "The Sims 4")),
    ("SimsMedieval",         LanguageManager.Get("BuyTS3", "TSM1", "The Sims Medieval")),
    ("SimTower",             LanguageManager.Get("BuyTS3", "SimTower", "SimTower")),
    ("SimCopter",            LanguageManager.Get("BuyTS3", "Copter", "SimCopter")),
    ("StreetsOfSimCity",     LanguageManager.Get("BuyTS3", "Streets", "Streets of SimCity")),
    ("SimCity2000",          LanguageManager.Get("BuyTS3", "SC2K", "SimCity 2000")),
    ("SimCity3000",          LanguageManager.Get("BuyTS3", "SC3K", "SimCity 3000 Unlimited")),
    ("SimCity4",             LanguageManager.Get("BuyTS3", "SC4DE", "SimCity 4 Deluxe")),
    ("SimCity2013",          LanguageManager.Get("BuyTS3", "SC2013", "SimCity (2013)")),
};

    private static readonly System.Collections.Generic.HashSet<string> HasMods =
    new System.Collections.Generic.HashSet<string>
    {
        "Sims2", "SimsLifeStories", "SimsPetStories",
        "SimsCastawayStories", "Sims3", "Sims4", "SimsMedieval"
    };

    private static readonly System.Collections.Generic.HashSet<string> HasUserData =
    new System.Collections.Generic.HashSet<string>
    {
        "Sims2", "Sims3", "Sims4", "SimsLifeStories", "SimsPetStories", "SimsCastawayStories", "SimsMedieval", "SimCity4"
    };

    private static readonly (string Code, string Name)[] Languages = new (string Code, string Name)[]
    {
    ("ar", "عربي"),    ("zh", "中国人"),  ("de", "Deutsch"),
    ("en", "English"), ("es", "Español"), ("fr", "Français"),
    ("ja", "日本語"),   ("pt", "Português"), ("ru", "Русский"),
    };

    // ── Runtime state ────────────────────────────────────────────────────────
    private readonly List<GameSettingViewModel> _gameViewModels;

    // ── Constructor ──────────────────────────────────────────────────────────
    [SupportedOSPlatform("windows")]
    public SettingsWindow()
    {
        InitializeComponent();

        // Apply localised text to named elements
        Title = LanguageManager.Get("Settings", "Window_Title", "SimTools - Settings");
        SectionLangHeader.Text = LanguageManager.Get("Settings", "Section_Language", "Language");
        ChangeLang.Text = LanguageManager.Get("Settings", "Change_Language", "Change Language");
        SectionDirsHeader.Text = LanguageManager.Get("Settings", "Section_Directories", "Game & Mod Directories");
        ResetLangBtn.Content = LanguageManager.Get("Settings", "ResetLangBtn", "Reset - Show Language Selection on Next Launch");
        ResetUrlBtn.Content = LanguageManager.Get("Settings", "ResetRepoURL", $"Reset to Default  ({AppSettings.DefaultBaseUrl})");
        MusicPlayerText.Text = LanguageManager.Get("Settings", "MusicPlayerText", "Music Player");
        MusicEnabledCheck.Content = LanguageManager.Get("Settings", "MusicEnabledCheck", "Enable Music Player");
        BYOM.Text = LanguageManager.Get("Settings", "BYOM_Text", "Place your own songs (MP3 / WAV / FLAC / M4A) in the /Resources/Music folder.");
        UpdateText.Text = LanguageManager.Get("Settings", "UpdateText", "Updates");
        UpdatesSupressed.Text = LanguageManager.Get("Settings", "UpdatesSupressed", "If you previously suppressed automatic update notifications (either for 30 days or indefinitely), use the button below to re-enable them.");
        ResetAutoUpdateBtn.Content = LanguageManager.Get("Settings", "ResetAutoUpdateBtn", "Reset Auto-Update Check");
        SaveBtn.Content = LanguageManager.Get("Settings", "Btn_Save", "Save");
        CancelBtn.Content = LanguageManager.Get("Settings", "Btn_Cancel", "Cancel");
        RepoLocationURL.Text = LanguageManager.Get("Settings", "RepoLocationURL", "SimTools Repo Location");

        // Populate language combo
        foreach (var (code, display) in Languages)
            LangCombo.Items.Add(new ComboBoxItem { Content = display, Tag = code });

        // Build one ViewModel per game entry
        _gameViewModels = new List<GameSettingViewModel>(Games.Length);
        foreach (var (key, name) in Games)
            _gameViewModels.Add(new GameSettingViewModel(key, name, HasMods.Contains(key), HasUserData.Contains(key)));

        GameDirsControl.ItemsSource = _gameViewModels;

        LoadFromIni();
    }

    // ── Load ─────────────────────────────────────────────────────────────────
    private void LoadFromIni()
    {
        // Language
        var saved = IniHelper.Read("Language", "SelectedLanguage", "en");
        foreach (ComboBoxItem item in LangCombo.Items)
        {
            if (item.Tag?.ToString() == saved)
            {
                LangCombo.SelectedItem = item;
                break;
            }
        }
        if (LangCombo.SelectedItem is null && LangCombo.Items.Count > 0)
            LangCombo.SelectedIndex = 0;

        // Network
        BaseUrlBox.Text = IniHelper.Read("Settings", "BaseUrl", AppSettings.DefaultBaseUrl);

        // Music
        MusicEnabledCheck.IsChecked = IniHelper.ReadBool("Music", "Enabled", true);

        // Game, Mod, & User Data directories
        foreach (var vm in _gameViewModels)
        {
            vm.GameDir = IniHelper.Read("Directories", $"{vm.Key}_Game", string.Empty);
            if (vm.HasMods)
                vm.ModDir = IniHelper.Read("Directories", $"{vm.Key}_Mods", string.Empty);

            // FIX: Ensure previously saved User Data directory values are loaded into the UI on startup
            if (vm.HasUserData)
                vm.UserDataDir = IniHelper.Read("Directories", $"{vm.Key}_UserData", string.Empty);
        }
    }

    // ── Save ─────────────────────────────────────────────────────────────────
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Language
        if (LangCombo.SelectedItem is ComboBoxItem selected)
        {
            var code = selected.Tag?.ToString() ?? "en";
            IniHelper.Write("Language", "SelectedLanguage", code);
            LanguageManager.LoadCode(code);
        }

        // Network
        IniHelper.Write("Network", "BaseUrl", BaseUrlBox.Text.Trim());

        // Music
        bool wasMusicEnabled = IniHelper.ReadBool("Music", "Enabled", true);
        bool musicEnabled = MusicEnabledCheck.IsChecked == true;

        IniHelper.WriteBool("Music", "Enabled", musicEnabled);

        // Only command the music player to change states if the setting was actually toggled.
        // This leaves the player in its current paused/stopped state if the user didn't disable it.
        if (musicEnabled != wasMusicEnabled)
        {
            App.MusicPlayer?.SetEnabled(musicEnabled);
        }

        // Game & Mod directories
        foreach (var vm in _gameViewModels)
        {
            IniHelper.Write("Directories", $"{vm.Key}_Game", vm.GameDir.Trim());
            if (vm.HasMods)
                IniHelper.Write("Directories", $"{vm.Key}_Mods", vm.ModDir.Trim());
            if (vm.HasUserData)
                IniHelper.Write("Directories", $"{vm.Key}_UserData", vm.UserDataDir.Trim());
        }

        // Refresh MainWindow text in the new language
        if (Owner is MainWindow main)
            main.ApplyLanguage();

        System.Windows.MessageBox.Show(
            LanguageManager.Get("Settings", "Msg_Saved", "Settings saved successfully."),
            "SimTools", MessageBoxButton.OK, MessageBoxImage.Information);

        Close();
    }

    // ── Cancel ───────────────────────────────────────────────────────────────
    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    // ── Reset Language ────────────────────────────────────────────────────────
    private void ResetLanguage_Click(object sender, RoutedEventArgs e)
    {
        IniHelper.WriteBool("Language", "DoNotAskAgain", false);
        System.Windows.MessageBox.Show(
            LanguageManager.Get("Settings", "Msg_Reset",
                "Language selection will appear on the next launch."),
            "SimTools", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Auto-Detect Directories ───────────────────────────────────────────────
    [SupportedOSPlatform("windows")]
    private void AutoDetect_Click(object sender, RoutedEventArgs e)
    {
        var results = GamePathDetector.DetectAll();

        int filled = 0;
        foreach (var vm in _gameViewModels)
        {
            if (!results.TryGetValue(vm.Key, out var r)) continue;

            // Game directory — only fill if currently empty
            if (r.GamePath is not null && string.IsNullOrWhiteSpace(vm.GameDir))
            {
                vm.GameDir = r.GamePath;
                filled++;
            }

            // Mod directory — only fill if currently empty
            if (vm.HasMods && r.ModPath is not null && string.IsNullOrWhiteSpace(vm.ModDir))
            {
                vm.ModDir = r.ModPath;
                filled++;
            }
        }

        if (filled == 0)
        {
            System.Windows.MessageBox.Show(
                LanguageManager.Get("Settings", "AutoDetect_Nothing", "No game or mod directories could be detected automatically."),
                LanguageManager.Get("Settings", "AutoDetect_Nothing_Title", "Auto-Detect — Nothing Found"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            string dirWord = filled == 1 ? "y was" : "ies were";
            System.Windows.MessageBox.Show(
                LanguageManager.Format("Settings", "AutoDetect_Found", filled, dirWord),
                LanguageManager.Get("Settings", "AutoDetect_Found_Title", "Auto-Detect Complete"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // ── Reset Base URL ────────────────────────────────────────────────────────
    private void ResetUrl_Click(object sender, RoutedEventArgs e)
        => BaseUrlBox.Text = AppSettings.DefaultBaseUrl;

    // ── Reset Auto-Update Check ───────────────────────────────────────────────
    private void ResetAutoUpdateCheck_Click(object sender, RoutedEventArgs e)
    {
        IniHelper.Write("Updates", "SuppressAutoCheckUntil", "");
        System.Windows.MessageBox.Show(
            LanguageManager.Get("Settings", "ReenabledUpdates", "Automatic update notifications have been re-enabled."),
            LanguageManager.Get("Settings", "ReenabledUpdates_Title", "Update Check Reset"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
