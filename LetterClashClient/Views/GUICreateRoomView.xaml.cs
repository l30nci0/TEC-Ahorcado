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
        window.Title = (string) Application.Current.FindResource("CreateRoom_WindowTitle") ?? "Menu Principal (Crear Sala)";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsername.Text = $"\"{usuario.NombreDeUsuario}\"";

        int age = CalculateAge(usuario.FechaDeNacimiento);
        string yearsSuffix = (string) Application.Current.FindResource("MainMenu_Years") ?? "Años";
        TextBlockAge.Text = $"\"{age} {yearsSuffix}\"";

        AvatarHelper.AsignarAImageControl(ImageUserAvatar, usuario.Avatar);
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
        TextBlockSelectedLanguageLabel.Text = (string) Application.Current.FindResource("CreateRoom_LangNone") ?? "NINGUNO SELECCIONADO";
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
        ButtonTypePublic.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonTypePrivate.Style = (Style) FindResource("ModernSecondaryButton");
      } else if (ComboBoxGameType.SelectedIndex == 2) { // Privada
        ButtonTypePrivate.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonTypePublic.Style = (Style) FindResource("ModernSecondaryButton");
      } else { // Ninguno
        ButtonTypePublic.Style = (Style) FindResource("ModernSecondaryButton");
        ButtonTypePrivate.Style = (Style) FindResource("ModernSecondaryButton");
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
        string warningMsg = (string) Application.Current.FindResource("CreateRoom_WarningFields") ?? "Seleccione idioma, palabra y tipo de partida.";
        MessageBox.Show(warningMsg,
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
          string successMsg = (string) Application.Current.FindResource("CreateRoom_SuccessCreate") ?? "Partida creada con éxito.";
          string successTitle = (string) Application.Current.FindResource("CreateRoom_SuccessTitle") ?? "Sala Creada";
          MessageBox.Show(successMsg, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
          NavigationService.Navigate(new GUIGameView(selectedWord, accessCode));
        } else {
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string errCreate = (string) Application.Current.FindResource("CreateRoom_ErrorCreate") ?? "No se pudo crear la partida.";
          MessageBox.Show(result?.Error?.Mensaje ?? errCreate, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        string connMsg = (string) Application.Current.FindResource("CreateRoom_ErrorConnection") ?? "No se pudo conectar con el servidor para registrar la partida.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIGameHubView());
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIGameHubView());
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
