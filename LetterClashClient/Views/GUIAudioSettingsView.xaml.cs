using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Services;

namespace LetterClashClient.Views {
  public partial class GUIAudioSettingsView : Page {
    private bool cargandoControles = true;

    public GUIAudioSettingsView() {
      InitializeComponent();
    }

    private void OnPaginaCargada(object sender, RoutedEventArgs e) {
      cargandoControles = true;
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

      cargandoControles = false;
    }

    private void SliderMusicVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (cargandoControles) {
        return;
      }

      AudioManager.VolumenMusica = e.NewValue / 100.0;
    }

    private void SliderEffectsVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (cargandoControles) {
        return;
      }

      AudioManager.VolumenEfectos = e.NewValue / 100.0;
    }

    private void ButtonTestAudio_Click(object sender, RoutedEventArgs e) {
      AudioManager.ReproducirEfecto("ConfirmSelection.mp3");
    }

    private void ButtonSaveAudio_Click(object sender, RoutedEventArgs e) {
      AudioManager.GuardarConfiguracionAudio();
      AudioManager.ReproducirEfecto("ConfirmSelection.mp3");
    }

    private void ButtonBackAudio_Click(object sender, RoutedEventArgs e) {
      if (NavigationService != null && NavigationService.CanGoBack) {
        NavigationService.GoBack();
        return;
      }

      NavigationService?.Navigate(new GUISettingsView());
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToMainMenu(NavigationService);
    }

    private void ButtonProfile_Click(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToProfile(NavigationService);
    }

    private void ButtonHistory_Click(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToHistory(NavigationService);
    }

    private void ButtonScoreboard_Click(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToScoreboard(NavigationService);
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToSettings(NavigationService);
    }
  }
}
