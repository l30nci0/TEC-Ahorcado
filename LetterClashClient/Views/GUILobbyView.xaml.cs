using System;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUILobbyView : Page {
    private bool openPrivate;

    public GUILobbyView() : this(false) { }

    public GUILobbyView(bool openPrivate) {
      InitializeComponent();
      this.openPrivate = openPrivate;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Lobby_WindowTitle") ?? "Lobby de Partidas";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsername.Text = $"\"{usuario.NombreDeUsuario}\"";
        string yearsText = (string) Application.Current.FindResource("MainMenu_Years") ?? "Aรฑos";
        TextBlockAge.Text = $"\"{CalculateAge(usuario.FechaDeNacimiento)} {yearsText}\"";

        AvatarHelper.AsignarAImageControl(ImageUserAvatar, usuario.Avatar);
      }

      SwitchTab(openPrivate);
    }

    private void SwitchTab(bool isPrivate) {
      if (isPrivate) {
        BorderPublicLobby.Visibility = Visibility.Collapsed;
        BorderPrivateLobby.Visibility = Visibility.Visible;
        ButtonTabPublic.FontWeight = FontWeights.Normal;
        ButtonTabPrivate.FontWeight = FontWeights.Bold;
      } else {
        BorderPublicLobby.Visibility = Visibility.Visible;
        BorderPrivateLobby.Visibility = Visibility.Collapsed;
        ButtonTabPublic.FontWeight = FontWeights.Bold;
        ButtonTabPrivate.FontWeight = FontWeights.Normal;
        CargarPartidasLobby();
      }
    }

    private void CargarPartidasLobby() {
      try {
        var lobbyService = ServiceProxyManager.GetLobbyService();
        var result = lobbyService.ObtenerPartidasLobby();

        if (result != null && result.IsSuccess) {
          DataGridPublicMatches.ItemsSource = result.Value;
        } else {
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string errMatches = (string) Application.Current.FindResource("Lobby_ErrorGetMatches") ?? "No se pudieron obtener las partidas.";
          MessageBox.Show(result?.Error?.Mensaje ?? errMatches, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexiรณn";
        string connMsg = (string) Application.Current.FindResource("Lobby_ErrorGetMatchesConn") ?? "No se pudo establecer conexiรณn con el servidor para obtener los lobbies.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurriรณ un error inesperado:";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
    }

    private void ButtonTabPublic_Click(object sender, RoutedEventArgs e) {
      SwitchTab(false);
    }

    private void ButtonTabPrivate_Click(object sender, RoutedEventArgs e) {
      SwitchTab(true);
    }

    private void ButtonRefreshLobby_Click(object sender, RoutedEventArgs e) {
      CargarPartidasLobby();
    }

    private void ButtonJoinMatch_Click(object sender, RoutedEventArgs e) {
      Button button = sender as Button;

      if (button != null) {
        PartidaDTO selectedMatch = button.DataContext as PartidaDTO;

        if (selectedMatch != null) {
          var usuario = SessionContext.UsuarioLogueado;
          if (usuario == null) {
            return;
          }

          if (selectedMatch.IDAnfitrion == usuario.IDJugador) {
            string selfMsg = (string) Application.Current.FindResource("Lobby_ErrorSelfJoin") ?? "No puedes unirte a una partida creada por ti mismo.";
            string selfTitle = (string) Application.Current.FindResource("Lobby_ErrorSelfJoinTitle") ?? "Operaciรณn Invรกlida";
            MessageBox.Show(selfMsg, selfTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
          }

          try {
            var lobbyService = ServiceProxyManager.GetLobbyService();
            var result = lobbyService.UnirseAPartidaDeLobby(usuario.IDJugador, selectedMatch.IDPartida);

          if (result != null && result.IsSuccess) {
            NavigationService.Navigate(new GUIGameView(selectedMatch.NombreAnfitrion, selectedMatch.Idioma, selectedMatch.CodigoAcceso, selectedMatch.IDPalabra, true));
            } else {
              string joinTitle = (string) Application.Current.FindResource("Lobby_ErrorJoinTitle") ?? "Error al unirse";
              string joinMsg = (string) Application.Current.FindResource("Lobby_ErrorJoin") ?? "No se pudo unir a la partida.";
              MessageBox.Show(result?.Error?.Mensaje ?? joinMsg, joinTitle, MessageBoxButton.OK, MessageBoxImage.Error);
              CargarPartidasLobby();
            }
          } catch (CommunicationException) {
            string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexiรณn";
            string connMsg = (string) Application.Current.FindResource("Lobby_ErrorJoinConn") ?? "No se pudo conectar con el servidor para unirse a la partida.";
            MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          } catch (Exception ex) {
            string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
            string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurriรณ un error inesperado:";
            MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      }
    }

    private void ButtonSearchLobby_Click(object sender, RoutedEventArgs e) {
      TextBlockAccessCodeError.Visibility = Visibility.Hidden;

      string accessCode = TextBoxAccessCode.Text.Trim().ToUpper().Replace("#", "");

      if (string.IsNullOrWhiteSpace(accessCode)) {
        TextBlockAccessCodeError.Text = (string) Application.Current.FindResource("Lobby_AccessCodeErrorEmpty") ?? "Ingrese un cรณdigo de acceso";
        TextBlockAccessCodeError.Visibility = Visibility.Visible;
        return;
      }

      if (accessCode.Length != 6) {
        TextBlockAccessCodeError.Text = (string) Application.Current.FindResource("Lobby_AccessCodeErrorLength") ?? "El cรณdigo debe tener 6 caracteres";
        TextBlockAccessCodeError.Visibility = Visibility.Visible;
        return;
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        return;
      }

      try {
        var lobbyService = ServiceProxyManager.GetLobbyService();
        var result = lobbyService.UnirseAPartidaPrivada(usuario.IDJugador, accessCode);

        if (result != null && result.IsSuccess) {
          var partida = result.Value;
          NavigationService.Navigate(new GUIGameView(partida.NombreAnfitrion, partida.Idioma, accessCode, partida.IDPalabra, false));
        } else {
          string defaultErr = (string) Application.Current.FindResource("Lobby_ErrorPrivateJoin") ?? "No se pudo unir a la sala.";
          TextBlockAccessCodeError.Text = result?.Error?.Mensaje ?? defaultErr;
          TextBlockAccessCodeError.Visibility = Visibility.Visible;
        }
      } catch (CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexiรณn";
        string connMsg = (string) Application.Current.FindResource("Msg_ConnectionError") ?? "No se pudo establecer conexiรณn con el servidor.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurriรณ un error inesperado:";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void TextBoxAccessCode_TextChanged(object sender, TextChangedEventArgs e) {
      if (TextBlockAccessCodePlaceholder != null) {
        TextBlockAccessCodePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxAccessCode.Text) ? Visibility.Visible : Visibility.Hidden;
      }
      if (TextBlockAccessCodeError != null) {
        TextBlockAccessCodeError.Visibility = Visibility.Hidden;
      }
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
