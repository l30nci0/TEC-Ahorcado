using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class GUISettingsView : Page {
    public GUISettingsView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Settings_WindowTitle") ?? "Ajustes";
      }

      UpdateLanguageButtons();
    }

    private void UpdateLanguageButtons() {
      if (Services.LanguageManager.CurrentLanguage == "EN") {
        ComboBoxPreferredLanguage.SelectedIndex = 1;
        ButtonLangEN.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangES.Style = (Style) FindResource("ModernSecondaryButton");
      } else {
        ComboBoxPreferredLanguage.SelectedIndex = 0;
        ButtonLangES.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangEN.Style = (Style) FindResource("ModernSecondaryButton");
      }
    }

    private void ButtonLangES_Click(object sender, RoutedEventArgs e) {
      Services.LanguageManager.SetLanguage("ES");
      UpdateLanguageButtons();

      // Update window title dynamically on language change
      Window window = Window.GetWindow(this);
      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Settings_WindowTitle") ?? "Ajustes";
      }
    }

    private void ButtonLangEN_Click(object sender, RoutedEventArgs e) {
      Services.LanguageManager.SetLanguage("EN");
      UpdateLanguageButtons();

      // Update window title dynamically on language change
      Window window = Window.GetWindow(this);
      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Settings_WindowTitle") ?? "Ajustes";
      }
    }

    private void ButtonLogout_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILoginView());
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIMainMenuView());
    }

    private void ButtonProfile_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIProfileView());
    }

    private void ButtonHistory_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIHistoryView());
    }

    private void ButtonScoreboard_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILeaderboardView());
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUISettingsView());
    }
  }
}
