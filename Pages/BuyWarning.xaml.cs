using System.Windows;

namespace SimTools;

public partial class BuyWarning : Window
{
    public BuyWarning()
    {
        InitializeComponent();
        ApplyLanguage();
    }

    // Called on load and by SettingsWindow after a language change
    public void ApplyLanguage()
    {
        // ── RTL / LTR layout direction ─────────────────────────────────────
        var lang = IniHelper.Read("Language", "SelectedLanguage", "en");

        // Add any future RTL languages to this set
        var rtlLanguages = new HashSet<string> { "ar" };
        FlowDirection = rtlLanguages.Contains(lang)
        ? System.Windows.FlowDirection.RightToLeft
        : System.Windows.FlowDirection.LeftToRight;

        // ── Text strings ───────────────────────────────────────────────────
        Text1.Text = LanguageManager.Get("BuyWarning", "BWText1", Text1.Text);
        Text2.Text = LanguageManager.Get("BuyWarning", "BWText2", Text2.Text);
    }
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
