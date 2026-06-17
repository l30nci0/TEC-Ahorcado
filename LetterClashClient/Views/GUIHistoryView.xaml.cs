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
  public partial class GUIHistoryView : Page {
    public GUIHistoryView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("History_WindowTitle") ?? "Historial";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string invalidSession = (string) Application.Current.FindResource("Msg_InvalidUserSession") ?? "Sesión de usuario inválida.";
        MessageBox.Show(invalidSession, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIMainMenuView());
        return;
      }

      TextBlockUsername.Text = usuario.NombreDeUsuario;
      if (usuario.FechaDeNacimiento != null) {
        TextBlockAge.Text = CalculateAge(usuario.FechaDeNacimiento).ToString();
      } else {
        TextBlockAge.Text = (string) Application.Current.FindResource("History_NotAvailable") ?? "N/D";
      }

      // Cargar avatar local
      if (usuario.Avatar != null && usuario.Avatar.Length > 0) {
        try {
          var image = new BitmapImage();
          using (var mem = new System.IO.MemoryStream(usuario.Avatar)) {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = mem;
            image.EndInit();
          }
          ImageUserAvatar.Source = image;
        } catch { }
      }

      // Cargar historial de partidas
      try {
        var service = ServiceProxyManager.GetJugadorService();
        var result = service.ConsultarHistorial(usuario.IDJugador);
        if (result != null && result.IsSuccess && result.Value != null) {
          string rolHost = (string) Application.Current.FindResource("History_RoleHost") ?? "Verdugo";
          string rolChallenger = (string) Application.Current.FindResource("History_RoleChallenger") ?? "Adivino";
          string noChallenger = (string) Application.Current.FindResource("History_NoChallenger") ?? "Sin adivinador";
          string resWin = (string) Application.Current.FindResource("History_ResultWin") ?? "Victoria";
          string resLoss = (string) Application.Current.FindResource("History_ResultLoss") ?? "Derrota";
          string resDisconnect = (string) Application.Current.FindResource("History_ResultDisconnect") ?? "Desconectada";
          string typePublic = (string) Application.Current.FindResource("History_TypePublic") ?? "Público";
          string typePrivate = (string) Application.Current.FindResource("History_TypePrivate") ?? "Privado";
          string langES = (string) Application.Current.FindResource("History_LangES") ?? "Español";
          string langEN = (string) Application.Current.FindResource("History_LangEN") ?? "Inglés";

          List<BattleHistoryItem> battles = new List<BattleHistoryItem>();
          foreach (var p in result.Value) {
            bool isHost = p.IDAnfitrion == usuario.IDJugador;
            string rol = isHost ? rolHost : rolChallenger;
            string rival = isHost ? (p.NombreAdivinador ?? noChallenger) : p.NombreAnfitrion;
            string fecha = p.FechaDeJuego.ToString("dd/MM/yyyy");
            string palabra = p.PalabraRevelada ?? "";

            string resultado = "";
            string puntuacion = "0";
            int progreso = 0;

            if (p.Resultado == "ADIVINADA") {
              if (isHost) {
                resultado = resLoss;
                puntuacion = "0";
                progreso = 5;
              } else {
                resultado = resWin;
                puntuacion = "+50";
                progreso = 5;
              }
            } else if (p.Resultado == "SIN_ADIVINAR") {
              if (isHost) {
                resultado = resWin;
                puntuacion = "+50";
                progreso = 5;
              } else {
                resultado = resLoss;
                puntuacion = "0";
                progreso = 0;
              }
            } else if (p.Resultado == "ABANDONADA") {
              resultado = resDisconnect;
              puntuacion = "0";
              progreso = 0;
            } else {
              resultado = resDisconnect;
              puntuacion = "0";
              progreso = 0;
            }

            string tipo = (p.Privacidad == "PÚBLICA") ? typePublic : typePrivate;
            string idioma = langES;
            if (p.Idioma != null && p.Idioma.ToUpper() == "INGLÉS") {
              idioma = langEN;
            }

            battles.Add(new BattleHistoryItem {
              Rival = rival,
              Fecha = fecha,
              Palabra = palabra,
              Resultado = resultado,
              Puntuacion = puntuacion,
              Rol = rol,
              Idioma = idioma,
              Tipo = tipo,
              Progreso = progreso
            });
          }

          DataGridHistory.ItemsSource = battles;
          LoadStatistics(battles);
        } else {
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string errRetrieve = (string) Application.Current.FindResource("History_ErrorRetrieve") ?? "No se pudo obtener el historial de partidas.";
          MessageBox.Show(result?.Error?.Mensaje ?? errRetrieve, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (System.ServiceModel.CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        string connMsg = (string) Application.Current.FindResource("History_ErrorConnection") ?? "No se pudo conectar con el servidor para obtener el historial.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
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

    private void LoadStatistics(List<BattleHistoryItem> battles) {
      int total = battles.Count;
      string resWin = (string) Application.Current.FindResource("History_ResultWin") ?? "Victoria";
      string resLoss = (string) Application.Current.FindResource("History_ResultLoss") ?? "Derrota";
      string resDisconnect = (string) Application.Current.FindResource("History_ResultDisconnect") ?? "Desconectada";

      int wins = battles.Count(battle => battle.Resultado == resWin);
      int losses = battles.Count(battle => battle.Resultado == resLoss);
      int disconnected = battles.Count(battle => battle.Resultado == resDisconnect);

      string percentFormat = (string) Application.Current.FindResource("History_StatPercentFormat") ?? "{0}%";

      TextBlockTotalWins.Text = string.Format((string) Application.Current.FindResource("History_StatTotalWins") ?? "Total Ganadas = {0}", wins);
      TextBlockPercentWins.Text = string.Format(percentFormat, GetPercentage(wins, total));
      TextBlockTotalLosses.Text = string.Format((string) Application.Current.FindResource("History_StatTotalLosses") ?? "Total Perdidas = {0}", losses);
      TextBlockPercentLosses.Text = string.Format(percentFormat, GetPercentage(losses, total));
      TextBlockTotalDisconnected.Text = string.Format((string) Application.Current.FindResource("History_StatTotalDisconnected") ?? "Total Desconectadas = {0}", disconnected);
      TextBlockPercentDisconnected.Text = string.Format(percentFormat, GetPercentage(disconnected, total));
    }

    private int GetPercentage(int amount, int total) {
      if (total == 0) {
        return 0;
      }

      return amount * 100 / total;
    }

    private void DataGridHistory_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      BattleHistoryItem selectedBattle = DataGridHistory.SelectedItem as BattleHistoryItem;

      if (selectedBattle != null) {
        NavigationService.Navigate(new GUIBattleHistoryDetailView(selectedBattle));
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

  public class BattleHistoryItem {
    public string Rival { get; set; }
    public string Fecha { get; set; }
    public string Palabra { get; set; }
    public string Resultado { get; set; }
    public string Puntuacion { get; set; }
    public string Rol { get; set; }
    public string Idioma { get; set; }
    public string Tipo { get; set; }
    public int Progreso { get; set; }
  }
}
