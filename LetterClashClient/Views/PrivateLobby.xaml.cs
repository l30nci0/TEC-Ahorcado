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
  public partial class PrivateLobby : Page {
    public PrivateLobby() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Menu Principal (Entrar en Lobby Privado)";
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
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
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
          NavigationService.Navigate(new GameGuesser(partida.NombreAnfitrion, partida.Idioma));
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
