using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfBrush = System.Windows.Media.SolidColorBrush;
using WpfColor = System.Windows.Media.Color;
using WpfFont = System.Windows.Media.FontFamily;
using WpfHAlign = System.Windows.HorizontalAlignment;
using FolderDlg = System.Windows.Forms.FolderBrowserDialog;

namespace SimTools_v4
{
    public class SettingsWindow : Window
    {
        private static readonly (string Key, string Name)[] Games =
        [
            ("Sims1",           "The Sims"),
            ("Sims2",           "The Sims 2"),
            ("SimsLifeStories",     "The Sims Life Stories"),
            ("SimsPetStories",     "The Sims Pet Stories"),
            ("SimsCastawayStories",     "The Sims Castaway Stories"),
            ("Sims3",           "The Sims 3"),
            ("Sims4",           "The Sims 4"),
            ("SimsMedieval",    "The Sims Medieval"),
            ("SimCopter",       "SimCopter"),
            ("StreetsOfSimCity","Streets of SimCity"),
            ("SimCity2000",     "SimCity 2000"),
            ("SimCity3000",     "SimCity 3000 Unlimited"),
            ("SimCity4",        "SimCity 4 Deluxe"),
            ("SimCity2013",     "SimCity (2013)"),
        ];

        private static readonly HashSet<string> HasMods =
        [
            "Sims2", "SimsLifeStories", "SimsPetStories", "SimsCastawayStories", "Sims3", "Sims4", "SimsMedieval"
        ];

        private static readonly (string Code, string Name)[] Languages =
        [
            ("ar", "عربي"),   ("zh", "中国人"),  ("de", "Deutsch"),
            ("en", "English"),   ("es", "Español"),  ("fr", "Français"),
            ("ja", "日本語"),    ("pt", "Português"), ("ru", "Русский"),
        ];

        // ── Fields ─────────────────────────────────────────────────────────────
        private WpfComboBox _langCombo = null!;
        private readonly Dictionary<string, (WpfTextBox GameDir, WpfTextBox? ModDir)> _dirs = new();

        // ── Constructor ────────────────────────────────────────────────────────
        public SettingsWindow()
        {
            Title = LanguageManager.Get("Settings", "Window_Title", "SimTools - Settings");
            Width = 680;
            Height = 720;
            WindowStyle = WindowStyle.ToolWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new WpfBrush(WpfColor.FromRgb(26, 26, 26));
            ResizeMode = ResizeMode.NoResize;

            Content = BuildLayout();
            LoadFromIni();
        }

        // ── Layout ─────────────────────────────────────────────────────────────
        private UIElement BuildLayout()
        {
            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(16)
            };

            var root = new StackPanel();
            scroll.Content = root;

            // Language section
            root.Children.Add(SectionHeader(LanguageManager.Get("Settings", "Section_Language", "Language")));

            var langRow = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var langLabel = RowLabel(LanguageManager.Get("Settings", "Label_Language", "Language:"));
            DockPanel.SetDock(langLabel, Dock.Left);
            langRow.Children.Add(langLabel);

            _langCombo = new WpfComboBox
            {
                Width = 160,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            foreach (var (code, display) in Languages)
                _langCombo.Items.Add(new ComboBoxItem { Content = display, Tag = code });
            langRow.Children.Add(_langCombo);
            root.Children.Add(langRow);

            var resetBtn = MakeButton(LanguageManager.Get("Settings", "Btn_Reset", "Reset - Show Language Selection on Next Launch"),
            WpfColor.FromRgb(80, 35, 35),
                width: double.NaN);
            resetBtn.HorizontalAlignment = WpfHAlign.Left;
            resetBtn.Margin = new Thickness(0, 6, 0, 20);
            resetBtn.Click += ResetLanguage_Click;
            root.Children.Add(resetBtn);

            // Directory section
            root.Children.Add(SectionHeader(LanguageManager.Get("Settings", "Section_Directories", "Game & Mod Directories")));

            foreach (var (key, name) in Games)
            {
                root.Children.Add(GameHeader(name));

                var (gameBox, gameBrowse) = MakeDirRow();
                root.Children.Add(DirRow(LanguageManager.Get("Settings", "Label_GameDir", "Game Directory:"), gameBox, gameBrowse));

                WpfTextBox? modBox = null;
                if (HasMods.Contains(key))
                {
                    var (mb, mbb) = MakeDirRow();
                    modBox = mb;
                    root.Children.Add(DirRow(LanguageManager.Get("Settings", "Label_ModsDir", "Mods Directory:"), mb, mbb));
                }

                _dirs[key] = (gameBox, modBox);

                root.Children.Add(new Separator
                {
                    Margin = new Thickness(0, 8, 0, 8),
                    Background = new WpfBrush(WpfColor.FromRgb(55, 55, 55))
                });
            }

            // Save / Cancel
            var btnRow = new StackPanel
            {
                Orientation = WpfOrientation.Horizontal,
                HorizontalAlignment = WpfHAlign.Right,
                Margin = new Thickness(0, 12, 0, 4)
            };

            var saveBtn = MakeButton(LanguageManager.Get("Settings", "Btn_Save", "Save"), WpfColor.FromRgb(255, 210, 0), 90);
            saveBtn.Foreground = WpfBrushes.Black;
            saveBtn.Margin = new Thickness(0, 0, 8, 0);
            saveBtn.Click += Save_Click;

            var cancelBtn = MakeButton(LanguageManager.Get("Settings", "Btn_Cancel", "Cancel"), WpfColor.FromRgb(55, 55, 55), 90);
            cancelBtn.Click += (_, _) => Close();

            btnRow.Children.Add(saveBtn);
            btnRow.Children.Add(cancelBtn);
            root.Children.Add(btnRow);

            return scroll;
        }

        // ── UI helpers ─────────────────────────────────────────────────────────
        private static TextBlock SectionHeader(string text) => new()
        {
            Text = text,
            FontFamily = new WpfFont("Tahoma"),
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            Foreground = new WpfBrush(WpfColor.FromRgb(255, 210, 0)),
            Margin = new Thickness(0, 12, 0, 8)
        };

        private static TextBlock GameHeader(string text) => new()
        {
            Text = text,
            FontFamily = new WpfFont("Tahoma"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = WpfBrushes.White,
            Margin = new Thickness(0, 6, 0, 3)
        };

        private static TextBlock RowLabel(string text) => new()
        {
            Text = text,
            FontFamily = new WpfFont("Tahoma"),
            FontSize = 11,
            Foreground = WpfBrushes.Gray,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 110
        };

        private static (WpfTextBox box, WpfButton browse) MakeDirRow()
        {
            var box = new WpfTextBox
            {
                Height = 26,
                Background = new WpfBrush(WpfColor.FromRgb(42, 42, 42)),
                Foreground = WpfBrushes.White,
                BorderBrush = new WpfBrush(WpfColor.FromRgb(75, 75, 75)),
                FontFamily = new WpfFont("Tahoma"),
                FontSize = 11,
                Padding = new Thickness(4, 0, 4, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            var btn = new WpfButton
            {
                Content = LanguageManager.Get("Settings", "Btn_Browse", "Browse..."),
                Width = 70,
                Height = 26,
                Margin = new Thickness(4, 0, 0, 0),
                FontFamily = new WpfFont("Tahoma"),
                FontSize = 11,
                Foreground = WpfBrushes.White,
                Background = new WpfBrush(WpfColor.FromRgb(55, 55, 55))
            };

            btn.Click += (_, _) =>
            {
                using var dlg = new FolderDlg { ShowNewFolderButton = true };
                if (!string.IsNullOrWhiteSpace(box.Text) && Directory.Exists(box.Text))
                    dlg.SelectedPath = box.Text;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    box.Text = dlg.SelectedPath;
            };

            return (box, btn);
        }

        private static Grid DirRow(string label, WpfTextBox box, WpfButton browse)
        {
            var grid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lbl = new TextBlock
            {
                Text = label,
                FontFamily = new WpfFont("Tahoma"),
                FontSize = 10,
                Foreground = WpfBrushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            };

            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(box, 1);
            Grid.SetColumn(browse, 2);
            grid.Children.Add(lbl);
            grid.Children.Add(box);
            grid.Children.Add(browse);
            return grid;
        }

        private static WpfButton MakeButton(string content, WpfColor bg, double width = 120) => new()
        {
            Content = content,
            Width = width,
            Height = 30,
            Padding = new Thickness(10, 0, 10, 0),
            FontFamily = new WpfFont("Tahoma"),
            FontWeight = FontWeights.Bold,
            Foreground = WpfBrushes.White,
            Background = new WpfBrush(bg)
        };

        // ── INI load / save ────────────────────────────────────────────────────
        private void LoadFromIni()
        {
            var saved = IniHelper.Read("Language", "SelectedLanguage", "en");
            foreach (ComboBoxItem item in _langCombo.Items)
            {
                if (item.Tag?.ToString() == saved)
                {
                    _langCombo.SelectedItem = item;
                    break;
                }
            }
            if (_langCombo.SelectedItem is null && _langCombo.Items.Count > 0)
                _langCombo.SelectedIndex = 0;

            foreach (var (key, _) in Games)
            {
                if (!_dirs.TryGetValue(key, out var pair)) continue;
                pair.GameDir.Text = IniHelper.Read("Directories", $"{key}_Game", "");
                if (pair.ModDir is not null)
                    pair.ModDir.Text = IniHelper.Read("Directories", $"{key}_Mods", "");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_langCombo.SelectedItem is ComboBoxItem item)
            {
                var code = item.Tag?.ToString() ?? "en";
                IniHelper.Write("Language", "SelectedLanguage", code);
                LanguageManager.LoadCode(code);              // apply immediately
            }

            foreach (var (key, _) in Games)
            {
                if (!_dirs.TryGetValue(key, out var pair)) continue;
                IniHelper.Write("Directories", $"{key}_Game", pair.GameDir.Text.Trim());
                if (pair.ModDir is not null)
                    IniHelper.Write("Directories", $"{key}_Mods", pair.ModDir.Text.Trim());
            }

            // Refresh MainWindow text in the current language
            if (Owner is MainWindow main)
                main.ApplyLanguage();

            System.Windows.MessageBox.Show(
                LanguageManager.Get("Settings", "Msg_Saved", "Settings saved successfully."),
                "SimTools", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void ResetLanguage_Click(object sender, RoutedEventArgs e)
        {
            IniHelper.WriteBool("Language", "DoNotAskAgain", false);
            System.Windows.MessageBox.Show(
                LanguageManager.Get("Settings", "Msg_Reset", "Language selection will appear on the next launch."),
                "SimTools", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}