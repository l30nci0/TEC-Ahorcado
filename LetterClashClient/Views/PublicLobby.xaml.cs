using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LetterClashClient.Views {
  public partial class PublicLobby : Page {
    public PublicLobby() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Menu Principal (Entrar a Lobby Publico)";
      }

      TextBlockUsername.Text = "\"jugador1\"";
      TextBlockAge.Text = "\"20\"";

      DataGridPublicMatches.ItemsSource = new List<PublicMatchItem>
{
        new PublicMatchItem { Host = "Usuario 1", Language = "Español" },
        new PublicMatchItem { Host = "Usuario 5", Language = "Ingles" },
        new PublicMatchItem { Host = "Usuario 3", Language = "Ingles" },
        new PublicMatchItem { Host = "Usuario 4", Language = "Español" },
        new PublicMatchItem { Host = "Usuario 8", Language = "Español" },
        new PublicMatchItem { Host = "Usuario 9", Language = "Ingles" }
      };
    }

    private void ButtonJoinMatch_Click(object sender, RoutedEventArgs e) {
      Button button = sender as Button;

      if (button != null) {
        PublicMatchItem selectedMatch = button.DataContext as PublicMatchItem;

        if (selectedMatch != null) {
          NavigationService.Navigate(new GameGuesser(selectedMatch.Host, selectedMatch.Language));
        }
      }
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

  public class PublicMatchItem {
    public string Host { get; set; }
    public string Language { get; set; }
  }
}
