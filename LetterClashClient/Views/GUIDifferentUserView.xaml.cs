using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIDifferentUserView : Page {
    private string username;

    public GUIDifferentUserView() {
      InitializeComponent();
      username = "Usuario 1";
    }

    public GUIDifferentUserView(string username) {
      InitializeComponent();
      this.username = string.IsNullOrWhiteSpace(username) ? "Usuario 1" : username;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Diferente Usuario";
      }

      LoadUserData();
    }

    private void LoadUserData() {
      try {
        var service = ServiceProxyManager.GetJugadorService();
        var result = service.ObtenerPerfilPorNombre(username);
        if (result != null && result.IsSuccess && result.Value != null) {
          var dto = result.Value;

          TextBlockUsernameHeader.Text = $"\"{dto.NombreDeUsuario}\"";
          TextBlockFullName.Text = dto.Nombre;
          TextBlockBirthDate.Text = dto.FechaDeNacimiento.ToString("dd/MM/yyyy");
          TextBlockAge.Text = CalculateAge(dto.FechaDeNacimiento).ToString();
          TextBlockEmail.Text = dto.Correo;
          TextBlockPhone.Text = dto.Telefono;

          // Cargar avatar de rival
          if (dto.Avatar != null && dto.Avatar.Length > 0) {
            try {
              var image = new BitmapImage();
              using (var mem = new System.IO.MemoryStream(dto.Avatar)) {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = mem;
                image.EndInit();
              }
              ImageUserAvatar.Source = image;
            } catch { }
          }

          // Cargar historial de partidas del rival para calcular estadísticas
          var historyResult = service.ConsultarHistorial(dto.IDJugador);
          if (historyResult != null && historyResult.IsSuccess && historyResult.Value != null) {
            List<BattleHistoryItem> battles = new List<BattleHistoryItem>();
            foreach (var p in historyResult.Value) {
              bool isHost = p.IDAnfitrion == dto.IDJugador;
              string resultado = "";

              if (p.Resultado == "ADIVINADA") {
                resultado = isHost ? "Derrota" : "Victoria";
              } else if (p.Resultado == "SIN_ADIVINAR") {
                resultado = isHost ? "Victoria" : "Derrota";
              } else {
                resultado = "Desconectada";
              }

              battles.Add(new BattleHistoryItem { Resultado = resultado });
            }

            int total = battles.Count;
            int wins = battles.Count(battle => battle.Resultado == "Victoria");
            int losses = battles.Count(battle => battle.Resultado == "Derrota");
            int disconnected = battles.Count(battle => battle.Resultado == "Desconectada");

            TextBlockTotalWins.Text = $"Total Ganadas = {wins}";
            TextBlockPercentWins.Text = $"%{GetPercentage(wins, total)}";
            TextBlockTotalLosses.Text = $"Total Perdidas = {losses}";
            TextBlockPercentLosses.Text = $"%{GetPercentage(losses, total)}";
            TextBlockTotalDisconnected.Text = $"Total Desconectadas = {disconnected}";
            TextBlockPercentDisconnected.Text = $"%{GetPercentage(disconnected, total)}";
          } else {
            SetEmptyStatistics();
          }
        } else {
          MessageBox.Show(result?.Error?.Mensaje ?? "No se pudo obtener la información del usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          NavigationService.GoBack();
        }
      } catch (System.ServiceModel.CommunicationException) {
        MessageBox.Show("No se pudo establecer conexión con el servidor.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.GoBack();
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado al cargar el perfil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.GoBack();
      }
    }

    private void SetEmptyStatistics() {
      TextBlockTotalWins.Text = "Total Ganadas = 0";
      TextBlockPercentWins.Text = "%0";
      TextBlockTotalLosses.Text = "Total Perdidas = 0";
      TextBlockPercentLosses.Text = "%0";
      TextBlockTotalDisconnected.Text = "Total Desconectadas = 0";
      TextBlockPercentDisconnected.Text = "%0";
    }

    private int GetPercentage(int amount, int total) {
      if (total == 0) {
        return 0;
      }
      return amount * 100 / total;
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      if (NavigationService != null && NavigationService.CanGoBack) {
        NavigationService.GoBack();
      } else {
        NavigationService.Navigate(new GUILeaderboardView());
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
