using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Services;

namespace LetterClashClient.Views {
  public partial class GUIAudioSettingsView : Page {
    public GUIAudioSettingsView() {
      InitializeComponent();
    }

    private void OnPaginaCargada(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("AudioSettings_WindowTitle") ?? "Configuracion de Audio";
      }

      if (SliderMusicVolume != null) {
        SliderMusicVolume.Value = AudioManager.VolumenMusica * 100;
      }

      if (SliderEffectsVolume != null) {
        SliderEffectsVolume.Value = AudioManager.VolumenEfectos * 100;
      }
    }

    private void SliderMusicVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      AudioManager.VolumenMusica = e.NewValue / 100.0;
    }

    private void SliderEffectsVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      AudioManager.VolumenEfectos = e.NewValue / 100.0;
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
