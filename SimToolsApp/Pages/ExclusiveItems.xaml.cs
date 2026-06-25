using System.Windows;

using MessageBox = System.Windows.MessageBox;

namespace SimTools
{
    public partial class ExclusiveItems : Window
    {
        public ExclusiveItems()
        {
            InitializeComponent();
            ApplyLanguage();

            ApplyHolidayThemes(); // <-- Holiday theme application
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

        // ── Translation strings ───────────────────────────────────────────
        private void ApplyLanguage()
        {
            Title = LanguageManager.Get("ExclusiveItems", "Title", "SimTools — Exclusive Items");
            ExclusiveItemsText.Text = LanguageManager.Get("ExclusiveItems", "ContentText1",
                "Exclusive donor content will appear here.");
            ExclusiveItemsText2.Text = LanguageManager.Get("ExclusiveItems", "ContentText2",
                "Users of SimTools who have donated can share these items with family and friends, but do not upload them to any modding sites.");
        }

        // ── Close button ──────────────────────────────────────────────────
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BackButton_Clic(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
