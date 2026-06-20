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
        window.Title = (string) Application.Current.FindResource("DiffUser_WindowTitle") ?? "Diferente Usuario";
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
          AvatarHelper.AsignarAImageControl(ImageDifferentUserAvatar, dto.Avatar);



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
              } else if (p.Resultado == "ABANDONADA") {
                if (!EsAbandonoDelJugador(p, dto.IDJugador)) {
                  continue;
                }

                resultado = "Desconectada";
              } else {
                continue;
              }

              battles.Add(new BattleHistoryItem { Resultado = resultado });
            }

            int total = battles.Count;
            int wins = battles.Count(battle => battle.Resultado == "Victoria");
            int losses = battles.Count(battle => battle.Resultado == "Derrota");
            int disconnected = battles.Count(battle => battle.Resultado == "Desconectada");

            string percentFormat = (string) Application.Current.FindResource("History_StatPercentFormat") ?? "{0}%";

            TextBlockTotalWins.Text = string.Format((string) Application.Current.FindResource("History_StatTotalWins") ?? "Total Ganadas = {0}", wins);
            TextBlockPercentWins.Text = string.Format(percentFormat, GetPercentage(wins, total));
            TextBlockTotalLosses.Text = string.Format((string) Application.Current.FindResource("History_StatTotalLosses") ?? "Total Perdidas = {0}", losses);
            TextBlockPercentLosses.Text = string.Format(percentFormat, GetPercentage(losses, total));
            TextBlockTotalDisconnected.Text = string.Format((string) Application.Current.FindResource("History_StatTotalDisconnected") ?? "Total Desconectadas = {0}", disconnected);
            TextBlockPercentDisconnected.Text = string.Format(percentFormat, GetPercentage(disconnected, total));
          } else {
            SetEmptyStatistics();
          }
        } else {
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string errRetrieve = (string) Application.Current.FindResource("DiffUser_ErrorRetrieve") ?? "No se pudo obtener la información del usuario.";
          MessageBox.Show(result?.Error?.Mensaje ?? errRetrieve, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          NavigationService.GoBack();
        }
      } catch (System.ServiceModel.CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        string connMsg = (string) Application.Current.FindResource("Msg_ConnectionError") ?? "No se pudo establecer conexión con el servidor.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.GoBack();
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("DiffUser_ErrorUnexpected") ?? "Ocurrió un error inesperado al cargar el perfil:";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.GoBack();
      }
    }

    private void SetEmptyStatistics() {
      string statWins = (string) Application.Current.FindResource("History_StatTotalWins") ?? "Total Ganadas = {0}";
      string statLosses = (string) Application.Current.FindResource("History_StatTotalLosses") ?? "Total Perdidas = {0}";
      string statDisconnected = (string) Application.Current.FindResource("History_StatTotalDisconnected") ?? "Total Desconectadas = {0}";
      string percentFormat = (string) Application.Current.FindResource("History_StatPercentFormat") ?? "{0}%";

      TextBlockTotalWins.Text = string.Format(statWins, 0);
      TextBlockPercentWins.Text = string.Format(percentFormat, 0);
      TextBlockTotalLosses.Text = string.Format(statLosses, 0);
      TextBlockPercentLosses.Text = string.Format(percentFormat, 0);
      TextBlockTotalDisconnected.Text = string.Format(statDisconnected, 0);
      TextBlockPercentDisconnected.Text = string.Format(percentFormat, 0);
    }

    private bool EsAbandonoDelJugador(PartidaDTO partida, int jugadorID) {
      if (partida == null || partida.Resultado != "ABANDONADA") {
        return false;
      }

      bool abandonoAnfitrion = partida.Turno == 1 || partida.Turno == 3;
      bool abandonoAdivinador = partida.Turno == 2 || partida.Turno == 4;

      return (abandonoAnfitrion && partida.IDAnfitrion == jugadorID) ||
             (abandonoAdivinador && partida.IDAdivinador == jugadorID);
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
