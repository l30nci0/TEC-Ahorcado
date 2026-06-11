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
  public partial class PublicLobby : Page {
    public PublicLobby() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Menu Principal (Entrar a Lobby Publico)";
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

      CargarPartidasLobby();
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
              NavigationService.Navigate(new GameGuesser(selectedMatch.NombreAnfitrion, selectedMatch.Idioma));
            } else {
              MessageBox.Show(result?.Error?.Mensaje ?? "No se pudo unir a la partida.", "Error al unirse", MessageBoxButton.OK, MessageBoxImage.Error);
              CargarPartidasLobby(); // Recargar por si el lobby ya no existe o cambió de estado
            }
          } catch (CommunicationException) {
            MessageBox.Show("No se pudo conectar con el servidor para unirse a la partida.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
          } catch (Exception ex) {
            MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          }
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
}
