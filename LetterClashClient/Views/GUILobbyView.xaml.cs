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
        window.Title = "Lobby de Partidas";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsername.Text = $"\"{usuario.NombreDeUsuario}\"";
        TextBlockAge.Text = $"\"{CalculateAge(usuario.FechaDeNacimiento)} Años\"";

        if (usuario.Avatar != null && usuario.Avatar.Length > 0) {
          try {
            using (var stream = new MemoryStream(usuario.Avatar)) {
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
          MessageBox.Show(result?.Error?.Mensaje ?? "No se pudieron obtener las partidas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (CommunicationException) {
        MessageBox.Show("No se pudo establecer conexión con el servidor para obtener los lobbies.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("No puedes unirte a una partida creada por ti mismo.", "Operación Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
          }

          try {
            var lobbyService = ServiceProxyManager.GetLobbyService();
            var result = lobbyService.UnirseAPartidaDeLobby(usuario.IDJugador, selectedMatch.IDPartida);

            if (result != null && result.IsSuccess) {
              NavigationService.Navigate(new GUIGameView(selectedMatch.NombreAnfitrion, selectedMatch.Idioma, selectedMatch.CodigoAcceso));
            } else {
              MessageBox.Show(result?.Error?.Mensaje ?? "No se pudo unir a la partida.", "Error al unirse", MessageBoxButton.OK, MessageBoxImage.Error);
              CargarPartidasLobby();
            }
          } catch (CommunicationException) {
            MessageBox.Show("No se pudo conectar con el servidor para unirse a la partida.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
          } catch (Exception ex) {
            MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      }
    }

    private void ButtonSearchLobby_Click(object sender, RoutedEventArgs e) {
      TextBlockAccessCodeError.Visibility = Visibility.Hidden;

      string accessCode = TextBoxAccessCode.Text.Trim().ToUpper().Replace("#", "");

      if (string.IsNullOrWhiteSpace(accessCode)) {
        TextBlockAccessCodeError.Text = "Ingrese un código de acceso";
        TextBlockAccessCodeError.Visibility = Visibility.Visible;
        return;
      }

      if (accessCode.Length != 6) {
        TextBlockAccessCodeError.Text = "El código debe tener 6 caracteres";
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
          NavigationService.Navigate(new GUIGameView(partida.NombreAnfitrion, partida.Idioma, accessCode));
        } else {
          TextBlockAccessCodeError.Text = result?.Error?.Mensaje ?? "No se pudo unir a la sala.";
          TextBlockAccessCodeError.Visibility = Visibility.Visible;
        }
      } catch (CommunicationException) {
        MessageBox.Show("No se pudo establecer conexión con el servidor.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
