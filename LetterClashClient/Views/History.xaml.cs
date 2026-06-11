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
  public partial class History : Page {
    public History() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Historial";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        MessageBox.Show("Sesión de usuario inválida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new MainMenu());
        return;
      }

      TextBlockUsername.Text = usuario.NombreDeUsuario;
      if (usuario.FechaDeNacimiento != null) {
        TextBlockAge.Text = CalculateAge(usuario.FechaDeNacimiento).ToString();
      } else {
        TextBlockAge.Text = "N/D";
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
          List<BattleHistoryItem> battles = new List<BattleHistoryItem>();
          foreach (var p in result.Value) {
            bool isHost = p.IDAnfitrion == usuario.IDJugador;
            string rol = isHost ? "Verdugo" : "Adivino";
            string rival = isHost ? (p.NombreAdivinador ?? "Sin adivinador") : p.NombreAnfitrion;
            string fecha = p.FechaDeJuego.ToString("dd/MM/yyyy");
            string palabra = p.PalabraRevelada ?? "";

            string resultado = "";
            string puntuacion = "0";
            int progreso = 0;

            if (p.Resultado == "ADIVINADA") {
              if (isHost) {
                resultado = "Derrota";
                puntuacion = "0";
                progreso = 5;
              } else {
                resultado = "Victoria";
                puntuacion = "+50";
                progreso = 5;
              }
            } else if (p.Resultado == "SIN_ADIVINAR") {
              if (isHost) {
                resultado = "Victoria";
                puntuacion = "+50";
                progreso = 5;
              } else {
                resultado = "Derrota";
                puntuacion = "0";
                progreso = 0;
              }
            } else if (p.Resultado == "ABANDONADA") {
              resultado = "Desconectada";
              puntuacion = "0";
              progreso = 0;
            } else {
              resultado = "Desconectada";
              puntuacion = "0";
              progreso = 0;
            }

            string tipo = (p.Privacidad == "PÚBLICA") ? "Publico" : "Privado";
            string idioma = "Español";
            if (p.Idioma != null && p.Idioma.ToUpper() == "INGLÉS") {
              idioma = "Ingles";
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
          MessageBox.Show(result?.Error?.Mensaje ?? "No se pudo obtener el historial de partidas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (System.ServiceModel.CommunicationException) {
        MessageBox.Show("No se pudo conectar con el servidor para obtener el historial.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado al cargar el historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
      int wins = battles.Count(battle => battle.Resultado == "Victoria");
      int losses = battles.Count(battle => battle.Resultado == "Derrota");
      int disconnected = battles.Count(battle => battle.Resultado == "Desconectada");

      TextBlockTotalWins.Text = $"Total Ganadas = {wins}";
      TextBlockPercentWins.Text = $"%{GetPercentage(wins, total)}";
      TextBlockTotalLosses.Text = $"Total Perdidas = {losses}";
      TextBlockPercentLosses.Text = $"%{GetPercentage(losses, total)}";
      TextBlockTotalDisconnected.Text = $"Total Desconectadas = {disconnected}";
      TextBlockPercentDisconnected.Text = $"%{GetPercentage(disconnected, total)}";
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
        NavigationService.Navigate(new BattleHistoryDetail(selectedBattle));
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
