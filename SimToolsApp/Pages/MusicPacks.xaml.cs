using System;
using System.Collections.Generic;
using System.Windows;

namespace SimTools;

/// <summary>
/// Interaction logic for MusicPacks.xaml
/// </summary>
public partial class MusicPacks : Window
{
    // Hardcoded direct URLs embedded into the code layout
    private readonly string[] _sims1Tracks = new string[]
    {
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/bwbqcnic/01%20-%20Now%20Entering%20%28Neighborhood%201%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/lauurzwj/02%20-%20Neighborhood%20%28Neighborhood%202%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/eulurytw/03%20-%20Create%20A%20Family%20%28Neighborhood%203%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/eeknkmca/04%20-%20Under%20Construction%20%28Build%201%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/vczpdwvp/05%20-%20Buying%20Lumber%20%28Build%202%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/ilgwpkeq/06.%20Planting%20%28Build%203%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/rlgwpjab/07.%20Dream%20House%20%28Build%204%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/kgbldhdt/08.%20Blueprint%20%28Build%205%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/llvvrubx/09.%20Builder%27s%20Motto%20%28Build%206%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/fvpmqroq/10%20-%20Mall%20Rat%20%28Buy%201%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/cdihkfwp/11.%20Groceries%20%28Buy%202%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/qggnycpu/12.%20Decorator%27s%20Touch%20%28Buy%203%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/ljlgxufs/13%20-%20The%20Market%20%28Buy%204%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/btnknhza/14.%20Latin%201.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/vyhozggn/15.%20Neighborhood%204%20%28Latin%202%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/ntftdltl/16.%20Samba%20SIM%20%28Latin%203%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/wvmznncz/17%20-%20Neighborhood%205%20%28Latin%204%29.mp3",
        "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/zqfcxhpe/18.%20BoSIM%20Nova%20%28Latin%204%29.mp3",
        "https://downloads.khinsider.com/game-soundtracks/album/the-sims-original-video-game-soundtrack/19.%2520Latin%25205.mp3"
        // Add further URL string mappings here separated by commas
    };

    public MusicPacks()
    {
        InitializeComponent();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    private void MusicSims1Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Resolve target directory route path
            string relativePath = System.IO.Path.Combine("Resources", "Music");
            string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            // Open the MusicDownloadWindow as a Modal dialog with our hardcoded track collection
            var downloadWindow = new MusicDownloadWindow(targetFolder, _sims1Tracks)
            {
                Owner = this
            };

            // ShowDialog blocks input interaction to the parent window while progress completes cleanly
            downloadWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LanguageManager.Format("Main", "Browser_Error", $"An unexpected error occurred: {ex.Message}"),
                LanguageManager.Get("Main", "Error_Title", "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MusicFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string relativePath = System.IO.Path.Combine("Resources", "Music");
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LanguageManager.Format("Main", "Browser_Error", $"Could not open the music folder: {ex.Message}"),
                LanguageManager.Get("Main", "Error_Title", "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}