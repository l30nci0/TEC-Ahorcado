using System.Windows.Navigation;

using LetterClashClient.Views;

namespace LetterClashClient.Services {
  internal static class NavigationHelper {
    public static void NavigateToMainMenu(NavigationService navigationService) {
      navigationService?.Navigate(new GuiGameHubView());
    }

    public static void NavigateToProfile(NavigationService navigationService) {
      navigationService?.Navigate(new GUIProfileView());
    }

    public static void NavigateToHistory(NavigationService navigationService) {
      navigationService?.Navigate(new GUIHistoryView());
    }

    public static void NavigateToScoreboard(NavigationService navigationService) {
      navigationService?.Navigate(new GUILeaderboardView());
    }

    public static void NavigateToSettings(NavigationService navigationService) {
      navigationService?.Navigate(new GUISettingsView());
    }
  }
}
