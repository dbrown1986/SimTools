using System;
using System.Collections.Generic;
using System.Windows;

namespace SimTools;

/// <summary>
/// Interaction logic for MusicPacks.xaml
/// </summary>
using System;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

public partial class MusicPacks : Window
{
    #region Track Collections (VGM Links Repository)
    private static class TrackData
    {
        // --- THE SIMS 1 ---
        public static readonly string[] Sims1 = new string[]
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
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/evoibesa/19.%20Latin%205.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/mogphpeb/20.%20Snobs%20%28Latin%206%29.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/zttmcdih/21.%20Groovy%20Neighors%20%28Latin%207%29.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/azuccgze/22%20-%20Loading%20Loop.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/uohxqjyq/23%20-%20BbMaj.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/irnmwyex/24%20-%20EbMaj.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/bqzxqkeo/25%20-%20Fmaj.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/khffmvoh/26%20-%20Gmaj.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/wlpphqit/27%20-%20SIMnata%20%2315%20%28Cmaj%29.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/efqkeqrm/28.%20Rock%201.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/kqwmsfxv/29.%20Rock%202.mp3",
            "https://vgmtreasurechest.com/soundtracks/the-sims-original-video-game-soundtrack/ncwamihm/30.%20Rock%203.mp3",
            "",
        };
        public static readonly string[] Sims1LivinLarge = new string[] { /* URL List Here */ };
        public static readonly string[] Sims1HouseParty = new string[] { /* URL List Here */ };
        public static readonly string[] Sims1HotDate = new string[] { /* URL List Here */ };
        public static readonly string[] Sims1Vacation = new string[] { /* URL List Here */ };
        public static readonly string[] Sims1Unleashed = new string[] { /* URL List Here */ };
        public static readonly string[] Sims1Superstar = new string[] { /* URL List Here */ };
        public static readonly string[] Sims1MakinMagic = new string[] { /* URL List Here */ };

        // --- THE SIMS 2 ---
        public static readonly string[] Sims2 = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2University = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2Nightlife = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2OpenForBusiness = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2Pets = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2Seasons = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2BonVoyage = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2Freetime = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2ApartmentLife = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2FamilyFunStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2GlamourLifeStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2HappyHolidayStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2CelebrationStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2HMStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2TeenStyleStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2KitchenBathStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2IKEAStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims2MansionGardenStuff = new string[] { /* URL List Here */ };

        // --- THE SIMS 3 ---
        public static readonly string[] Sims3 = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3WorldAdventures = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3Ambitions = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3LateNight = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3Generations = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3Pets = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3Showtime = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3Supernatural = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3Seasons = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3UniversityLife = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3IslandParadise = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3IntoTheFuture = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3HighEndLoftStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3FastLaneStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3OutdoorLivingStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3TownLifeStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3MasterSuiteStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3KatyPerryStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3DieselStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims370s80s90sStuff = new string[] { /* URL List Here */ };
        public static readonly string[] Sims3MovieStuff = new string[] { /* URL List Here */ };

        // --- THE SIMS STORIES ---
        public static readonly string[] SimsStoriesLife = new string[] { /* URL List Here */ };
        public static readonly string[] SimsStoriesPet = new string[] { /* URL List Here */ };
        public static readonly string[] SimsStoriesCastaway = new string[] { /* URL List Here */ };

        // --- THE SIMS MEDIEVAL ---
        public static readonly string[] SimsMedievalBase = new string[] { /* URL List Here */ };
        public static readonly string[] SimsMedievalPiratesNobles = new string[] { /* URL List Here */ };

        // --- SIMCITY & SPINOFFS ---
        public static readonly string[] SimCityClassic = new string[] { /* URL List Here */ };
        public static readonly string[] SimCitySNES = new string[] { /* URL List Here */ };
        public static readonly string[] SimCity2000 = new string[] { /* URL List Here */ };
        public static readonly string[] SimCity3000 = new string[] { /* URL List Here */ };
        public static readonly string[] SimCity4 = new string[] { /* URL List Here */ };
        public static readonly string[] SimCity2013 = new string[] { /* URL List Here */ };
        public static readonly string[] SimCopter = new string[] { /* URL List Here */ };
        public static readonly string[] StreetsOfSimCity = new string[] { /* URL List Here */ };
    }
    #endregion

    public MusicPacks()
    {
        InitializeComponent();
        ApplyLanguage();
        ApplyHolidayThemes();
    }

    private void ApplyHolidayThemes()
    {
        // Hide all holiday elements by default
        Clover.Visibility = Visibility.Collapsed;
        Clover2.Visibility = Visibility.Collapsed;
        Clover3.Visibility = Visibility.Collapsed;
        Clover4.Visibility = Visibility.Collapsed;
        Clover5.Visibility = Visibility.Collapsed;
        Fireworks.Visibility = Visibility.Collapsed;
        SantaHat.Visibility = Visibility.Collapsed;

        DateTime today = DateTime.Today;
        int currentYear = today.Year;

        // 2. Check St. Patrick's Day (March 17)
        // Range: March 14 to March 20
        DateTime stPatricksDay = new DateTime(currentYear, 3, 17);
        if (today >= stPatricksDay.AddDays(-3) && today <= stPatricksDay.AddDays(3))
        {
            Clover.Visibility = Visibility.Visible;
            Clover2.Visibility = Visibility.Visible;
            Clover3.Visibility = Visibility.Visible;
            Clover4.Visibility = Visibility.Visible;
            Clover5.Visibility = Visibility.Visible;
        }

        // 3. Check Independence Day (July 4)
        // Range: July 1 to July 7
        DateTime independenceDay = new DateTime(currentYear, 7, 4);
        if (today >= independenceDay.AddDays(-3) && today <= independenceDay.AddDays(3))
        {
            Fireworks.Visibility = Visibility.Visible;
        }

        // 4. Check New Year's Eve (December 31 / January 1 window)
        // Range: Dec 28 to Jan 4. (Handles wrapping over the year bound perfectly)
        if (IsDateInNewYearsWindow(today))
        {
            Fireworks.Visibility = Visibility.Visible;
        }

        // 5. Check Christmas (December 25)
        // Range: December 22 to December 28
        DateTime christmas = new DateTime(currentYear, 12, 25);
        if (today >= christmas.AddDays(-3) && today <= christmas.AddDays(3))
        {
            SantaHat.Visibility = Visibility.Visible;
        }
    }

    private bool IsDateInNewYearsWindow(DateTime targetDate)
    {
        // Check if we are at the end of the current year (Dec 28 - Dec 31)
        DateTime nyeThisYear = new DateTime(targetDate.Year, 12, 31);
        if (targetDate >= nyeThisYear.AddDays(-3) && targetDate <= nyeThisYear)
        {
            return true;
        }

        // Check if we are at the very start of a new year (Jan 1 - Jan 4)
        DateTime nydThisYear = new DateTime(targetDate.Year, 1, 1);
        if (targetDate >= nydThisYear && targetDate <= nydThisYear.AddDays(3))
        {
            return true;
        }

        return false;
    }

    private void Sims1Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }
    private void Sims2Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }
    private void Sims3Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    private void SimsStoriesButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    private void SimsMedievalButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    public void ApplyLanguage()
    {
        Title = LanguageManager.Get("MusicPacks", "Window_Title", "Music Packs");
        MusicPacksInfoText.Text= LanguageManager.Get("MusicPacks", "Info_Text", "The music system in SimTools has been vastly overhauled. Rather than a single song playing in the background, the software now presents a fully fleshed-out media player.\n\nAside from this addition, SimTools actively scans its music directory on start and shuffles the playlist each time.\n\nYou may now place your own MP3, M4A, FLAC or WAV files within SimTools' music directory, and they will play on next start.\n\nIf you want to expand your library, I have provided music packs for each individual game for automatic download and play on next start.\n\nAdditional soundtracks have been provided by KHInsider / VGMTreasureChest. Check them out and support their site!");
        Sims1Button.Content = LanguageManager.Get("MusicPacks", "Sims1_Button", "The Sims 1");
        Sims2Button.Content = LanguageManager.Get("MusicPacks", "Sims2_Button", "The Sims 2");
        Sims3Button.Content = LanguageManager.Get("MusicPacks", "Sims3_Button", "The Sims 3");
        SimsStoriesButton.Content = LanguageManager.Get("MusicPacks", "SimsStories_Button", "The Sims Stories");
        SimsMedievalButton.Content = LanguageManager.Get("MusicPacks", "SimsMedieval_Button", "The Sims Medieval");
        SimCityClassicButton.Content = LanguageManager.Get("MusicPacks", "SimCity_Button", "SimCity Classic");
        SimCitySNESButton.Content = LanguageManager.Get("MusicPacks", "SimCitySNES_Button", "SimCity SNES");
        SimCity2000Button.Content = LanguageManager.Get("MusicPacks", "SimCity2000_Button", "SimCity 2000");
        SimCity3000Button.Content = LanguageManager.Get("MusicPacks", "SimCity3000_Button", "SimCity 3000");
        SimCity4Button.Content = LanguageManager.Get("MusicPacks", "SimCity4_Button", "SimCity 4");
        SimCity2013Button.Content = LanguageManager.Get("MusicPacks", "SimCity2013_Button", "SimCity 2013");
        SimCopterButton.Content = LanguageManager.Get("MusicPacks", "SimCopter_Button", "SimCopter");
        StreetsOfSimCityButton.Content = LanguageManager.Get("MusicPacks", "StreetsOfSimCity_Button", "Streets of SimCity");
        MusicFolderButton.Content = LanguageManager.Get("MusicPacks", "Open_Music_Folder_Button", "Open Music Folder");
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

    #region Execution Core Engine
    /// <summary>
    /// Processes track installations using a generic framework method to bypass redundant UI handlers.
    /// </summary>
    private void ExecuteTrackDownload(string[] targetTrackUrls)
    {
        try
        {
            string relativePath = System.IO.Path.Combine("Resources", "Music");
            string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            var downloadWindow = new MusicDownloadWindow(targetFolder, targetTrackUrls)
            {
                Owner = this
            };

            downloadWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LanguageManager.Format("MusicPacks", "Browser_Error", $"An unexpected error occurred: {ex.Message}"),
                LanguageManager.Get("MusicPacks", "Error_Title", "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    #endregion

    #region Context Menu Handlers: The Sims 1
    private void Sims1_Base_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1);
    private void Sims1_LivinLarge_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1LivinLarge);
    private void Sims1_HouseParty_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1HouseParty);
    private void Sims1_HotDate_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1HotDate);
    private void Sims1_Vacation_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1Vacation);
    private void Sims1_Unleashed_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1Unleashed);
    private void Sims1_Superstar_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1Superstar);
    private void Sims1_MakinMagic_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims1MakinMagic);
    #endregion

    #region Context Menu Handlers: The Sims 2
    private void Sims2_Base_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2);
    private void Sims2_University_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2University);
    private void Sims2_Nightlife_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2Nightlife);
    private void Sims2_OpenForBusiness_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2OpenForBusiness);
    private void Sims2_Pets_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2Pets);
    private void Sims2_Seasons_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2Seasons);
    private void Sims2_BonVoyage_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2BonVoyage);
    private void Sims2_Freetime_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2Freetime);
    private void Sims2_ApartmentLife_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2ApartmentLife);
    private void Sims2_FamilyFunStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2FamilyFunStuff);
    private void Sims2_GlamourLifeStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2GlamourLifeStuff);
    private void Sims2_HappyHolidayStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2HappyHolidayStuff);
    private void Sims2_CelebrationStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2CelebrationStuff);
    private void Sims2_HMStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2HMStuff);
    private void Sims2_TeenStyleStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2TeenStyleStuff);
    private void Sims2_KitchenBathStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2KitchenBathStuff);
    private void Sims2_IKEAStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2IKEAStuff);
    private void Sims2_MansionGardenStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims2MansionGardenStuff);
    #endregion

    #region Context Menu Handlers: The Sims 3
    private void Sims3_Base_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3);
    private void Sims3_WorldAdventures_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3WorldAdventures);
    private void Sims3_Ambitions_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3Ambitions);
    private void Sims3_LateNight_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3LateNight);
    private void Sims3_Generations_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3Generations);
    private void Sims3_Pets_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3Pets);
    private void Sims3_Showtime_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3Showtime);
    private void Sims3_Supernatural_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3Supernatural);
    private void Sims3_Seasons_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3Seasons);
    private void Sims3_UniversityLife_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3UniversityLife);
    private void Sims3_IslandParadise_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3IslandParadise);
    private void Sims3_IntoTheFuture_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3IntoTheFuture);
    private void Sims3_HighEndLoftStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3HighEndLoftStuff);
    private void Sims3_FastLaneStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3FastLaneStuff);
    private void Sims3_OutdoorLivingStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3OutdoorLivingStuff);
    private void Sims3_TownLifeStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3TownLifeStuff);
    private void Sims3_MasterSuiteStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3MasterSuiteStuff);
    private void Sims3_KatyPerryStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3KatyPerryStuff);
    private void Sims3_DieselStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3DieselStuff);
    private void Sims3_70s80s90sStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims370s80s90sStuff);
    private void Sims3_MovieStuff_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.Sims3MovieStuff);
    #endregion

    #region Context Menu Handlers: The Sims Stories
    private void SimsStories_Life_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimsStoriesLife);
    private void SimsStories_Pet_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimsStoriesPet);
    private void SimsStories_Castaway_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimsStoriesCastaway);
    #endregion

    #region Context Menu Handlers: The Sims Medieval
    private void SimsMedieval_Base_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimsMedievalBase);
    private void SimsMedieval_PiratesNobles_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimsMedievalPiratesNobles);
    #endregion

    #region Direct Button Click Handlers: SimCity & Spinoffs
    private void SimCityClassicButton_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCityClassic);
    private void SimCitySNESButton_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCitySNES);
    private void SimCity2000Button_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCity2000);
    private void SimCity3000Button_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCity3000);
    private void SimCity4Button_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCity4);
    private void SimCity2013Button_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCity2013);
    private void SimCopterButton_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.SimCopter);
    private void StreetsOfSimCityButton_Click(object sender, RoutedEventArgs e) => ExecuteTrackDownload(TrackData.StreetsOfSimCity);
    #endregion

    #region Directory Management
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
                LanguageManager.Format("MusicPacks", "FolderBrowser_Error", $"Could not open the music folder: {ex.Message}"),
                LanguageManager.Get("MusicPacks", "Error_Title", "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
#endregion

