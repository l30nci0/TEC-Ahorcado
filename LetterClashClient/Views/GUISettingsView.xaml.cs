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
        window.Title = "Ajustes";
      }

      UpdateLanguageButtons();
    }

    private void UpdateLanguageButtons() {
      if (ComboBoxPreferredLanguage.SelectedIndex == 1) { // Ingles
        ButtonLangEN.Style = (Style)FindResource("ModernPrimaryButton");
        ButtonLangES.Style = (Style)FindResource("ModernSecondaryButton");
      } else { // Español
        ButtonLangES.Style = (Style)FindResource("ModernPrimaryButton");
        ButtonLangEN.Style = (Style)FindResource("ModernSecondaryButton");
      }
    }

    private void ButtonLangES_Click(object sender, RoutedEventArgs e) {
      ComboBoxPreferredLanguage.SelectedIndex = 0;
      UpdateLanguageButtons();
    }

    private void ButtonLangEN_Click(object sender, RoutedEventArgs e) {
      ComboBoxPreferredLanguage.SelectedIndex = 1;
      UpdateLanguageButtons();
    }

    private void ButtonPrivacy_Click(object sender, RoutedEventArgs e) {
      MessageBox.Show("La privacidad del perfil se modificará aquí.",
                      "TecnoHorcado",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
    }

    private void ButtonSound_Click(object sender, RoutedEventArgs e) {
      MessageBox.Show("La configuración de sonido se modificará aquí.",
                      "TecnoHorcado",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
    }

    private void ButtonHowToPlay_Click(object sender, RoutedEventArgs e) {
      MessageBox.Show("Aquí se mostrarán las instrucciones del juego.",
                      "TecnoHorcado",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
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
