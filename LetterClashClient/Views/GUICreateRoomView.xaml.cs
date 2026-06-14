using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUICreateRoomView : Page {
    private PalabraDTO selectedWord;
    private string selectedLanguage;
    private int privacyIndex;

    public GUICreateRoomView() {
      InitializeComponent();
      selectedWord = null;
    }

    public GUICreateRoomView(PalabraDTO word, string language, int privacyIndex) {
      InitializeComponent();
      this.selectedWord = word;
      this.selectedLanguage = language;
      this.privacyIndex = privacyIndex;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Menu Principal (Crear Sala)";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsername.Text = $"\"{usuario.NombreDeUsuario}\"";
        TextBlockAge.Text = $"\"{CalculateAge(usuario.FechaDeNacimiento)} Años\"";

        if (usuario.Avatar != null && usuario.Avatar.Length > 0) {
          try {
            using (var stream = new System.IO.MemoryStream(usuario.Avatar)) {
              var bitmap = new BitmapImage();
              bitmap.BeginInit();
              bitmap.StreamSource = stream;
              bitmap.CacheOption = BitmapCacheOption.OnLoad;
              bitmap.EndInit();
              ImageUserAvatar.Source = bitmap;
            }
          } catch {
            // Mantiene el default en caso de error
          }
        }
      }

      if (selectedWord != null) {
        TextBlockSelectedWord.Text = selectedWord.PalabraTexto;
      } else {
        TextBlockSelectedWord.Text = "";
      }

      if (!string.IsNullOrEmpty(selectedLanguage)) {
        ComboBoxLanguage.SelectedIndex = selectedLanguage == Idiomas.INGLES ? 1 : 2;
        TextBlockSelectedLanguageLabel.Text = selectedLanguage;
      } else {
        ComboBoxLanguage.SelectedIndex = 0;
        TextBlockSelectedLanguageLabel.Text = "NINGUNO SELECCIONADO";
      }

      if (privacyIndex > 0) {
        ComboBoxGameType.SelectedIndex = privacyIndex;
      } else {
        ComboBoxGameType.SelectedIndex = 0;
      }

      UpdateGameTypeButtons();
      TextBlockAccessKey.Text = "------";
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
    }

    private void UpdateGameTypeButtons() {
      if (ComboBoxGameType.SelectedIndex == 1) { // Publica
        ButtonTypePublic.Style = (Style)FindResource("ModernPrimaryButton");
        ButtonTypePrivate.Style = (Style)FindResource("ModernSecondaryButton");
      } else if (ComboBoxGameType.SelectedIndex == 2) { // Privada
        ButtonTypePrivate.Style = (Style)FindResource("ModernPrimaryButton");
        ButtonTypePublic.Style = (Style)FindResource("ModernSecondaryButton");
      } else { // Ninguno
        ButtonTypePublic.Style = (Style)FindResource("ModernSecondaryButton");
        ButtonTypePrivate.Style = (Style)FindResource("ModernSecondaryButton");
      }
    }

    private void ButtonTypePublic_Click(object sender, RoutedEventArgs e) {
      ComboBoxGameType.SelectedIndex = 1;
      UpdateGameTypeButtons();
    }

    private void ButtonTypePrivate_Click(object sender, RoutedEventArgs e) {
      ComboBoxGameType.SelectedIndex = 2;
      UpdateGameTypeButtons();
    }

    private void ButtonSelectWord_Click(object sender, RoutedEventArgs e) {
      string lang = null;
      if (ComboBoxLanguage.SelectedIndex == 1) {
        lang = Idiomas.INGLES;
      } else if (ComboBoxLanguage.SelectedIndex == 2) {
        lang = Idiomas.ESPANOL;
      } else if (!string.IsNullOrEmpty(selectedLanguage)) {
        lang = selectedLanguage;
      } else {
        lang = Idiomas.ESPANOL; // default to ESPANOL
      }
      
      int privacy = ComboBoxGameType.SelectedIndex;
      NavigationService.Navigate(new GUISelectWordView(lang, privacy));
    }

    private void ButtonCreateRoom_Click(object sender, RoutedEventArgs e) {
      bool hasLanguage = ComboBoxLanguage.SelectedIndex > 0;
      bool hasGameType = ComboBoxGameType.SelectedIndex > 0;
      bool hasWord = selectedWord != null;

      if (!hasLanguage || !hasGameType || !hasWord) {
        MessageBox.Show("Seleccione idioma, palabra y tipo de partida.",
                        "TecnoHorcado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
        return;
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        return;
      }

      string lang = ComboBoxLanguage.SelectedIndex == 1 ? Idiomas.INGLES : Idiomas.ESPANOL;
      string privacy = ComboBoxGameType.SelectedIndex == 1 ? "PÚBLICA" : "PRIVADA";

      try {
        var lobbyService = ServiceProxyManager.GetLobbyService();
        var result = lobbyService.CrearPartida(usuario.IDJugador, selectedWord.IDPalabra, privacy, lang);

        if (result != null && result.IsSuccess) {
          string accessCode = result.Value;
          MessageBox.Show("Partida creada con éxito.", "Sala Creada", MessageBoxButton.OK, MessageBoxImage.Information);
          NavigationService.Navigate(new GUIGameView(selectedWord.PalabraTexto, accessCode));
        } else {
          MessageBox.Show(result?.Error?.Mensaje ?? "No se pudo crear la partida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (CommunicationException) {
        MessageBox.Show("No se pudo conectar con el servidor para registrar la partida.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIMainMenuView());
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
