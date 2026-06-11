using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class PrivateLobby : Page {
    private const string VALID_ACCESS_CODE = "4233232";

    public PrivateLobby() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Menu Principal (Entrar en Lobby Privado)";
      }

      TextBlockUsername.Text = "\"jugador1\"";
      TextBlockAge.Text = "\"20\"";
    }

    private void ButtonSearchLobby_Click(object sender, RoutedEventArgs e) {
      TextBlockAccessCodeError.Visibility = Visibility.Hidden;

      string accessCode = TextBoxAccessCode.Text.Trim().Replace("#", "");

      if (string.IsNullOrWhiteSpace(accessCode)) {
        TextBlockAccessCodeError.Text = "Ingrese un código de acceso";
        TextBlockAccessCodeError.Visibility = Visibility.Visible;
        return;
      }

      if (accessCode != VALID_ACCESS_CODE) {
        TextBlockAccessCodeError.Text = "No se encontró una partida con ese código";
        TextBlockAccessCodeError.Visibility = Visibility.Visible;
        return;
      }

      NavigationService.Navigate(new GameGuesser("Usuario 1", "Español"));
    }

    private void TextBoxAccessCode_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockAccessCodePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxAccessCode.Text) ? Visibility.Visible : Visibility.Hidden;
      TextBlockAccessCodeError.Visibility = Visibility.Hidden;
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new MainMenu());
    }

    private void ButtonProfile_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Profile());
    }

    private void ButtonHistory_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new History());
    }

    private void ButtonScoreboard_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Scoreboard());
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Settings());
    }
  }
}
