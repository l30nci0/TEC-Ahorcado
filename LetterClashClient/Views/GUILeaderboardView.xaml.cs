using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUILeaderboardView : Page {
    public GUILeaderboardView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Leaderboard_WindowTitle") ?? "Marcadores";
      }

      try {
        var service = ServiceProxyManager.GetJugadorService();
        var result = service.ConsultarMarcadores();
        if (result != null && result.IsSuccess && result.Value != null) {
          var jugadores = result.Value;

          // 1. Cargar el Top 3
          // Primer lugar (índice 0)
          if (jugadores.Count > 0) {
            var top1 = jugadores[0];
            TextBlockTopName1.Text = top1.NombreDeUsuario;
            ImageTopAvatar1.Source = AvatarHelper.ObtenerImagen(top1.Avatar);
          } else {
            TextBlockTopName1.Text = "-";
          }

          // Segundo lugar (índice 1)
          if (jugadores.Count > 1) {
            var top2 = jugadores[1];
            TextBlockTopName2.Text = top2.NombreDeUsuario;
            ImageTopAvatar2.Source = AvatarHelper.ObtenerImagen(top2.Avatar);
          } else {
            TextBlockTopName2.Text = "-";
          }

          // Tercer lugar (índice 2)
          if (jugadores.Count > 2) {
            var top3 = jugadores[2];
            TextBlockTopName3.Text = top3.NombreDeUsuario;
            ImageTopAvatar3.Source = AvatarHelper.ObtenerImagen(top3.Avatar);
          } else {
            TextBlockTopName3.Text = "-";
          }

          // 2. Cargar todos los mejores 50 en la lista del DataGrid
          List<ScoreboardItem> listado = new List<ScoreboardItem>();
          int limit = Math.Min(jugadores.Count, 50);
          for (int i = 0; i < limit; i++) {
            var j = jugadores[i];

            listado.Add(new ScoreboardItem {
              Nombre = j.NombreDeUsuario,
              PartidasGanadas = j.PartidasGanadas,
              Porcentaje = j.PartidasConcluidas > 0 ? $"%{j.PartidasGanadas * 100 / j.PartidasConcluidas}" : "%0",
              Puntuacion = j.Puntuacion,
              Puesto = $"{i + 1}*"
            });
          }

          DataGridScoreboard.ItemsSource = listado;
        } else {
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string errLoad = (string) Application.Current.FindResource("Leaderboard_ErrorLoad") ?? "No se pudieron obtener los marcadores.";
          MessageBox.Show(result?.Error?.Mensaje ?? errLoad, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (System.ServiceModel.CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        string connMsg = (string) Application.Current.FindResource("Leaderboard_ErrorLoadConn") ?? "No se pudo establecer conexión con el servidor para cargar los marcadores.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private BitmapImage CrearBitmapDeBytes(byte[] bytes) {
      try {
        var image = new BitmapImage();
        using (var mem = new System.IO.MemoryStream(bytes)) {
          image.BeginInit();
          image.CacheOption = BitmapCacheOption.OnLoad;
          image.StreamSource = mem;
          image.EndInit();
        }
        return image;
      } catch {
        return null;
      }
    }

    private void DataGridScoreboard_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      ScoreboardItem selectedUser = DataGridScoreboard.SelectedItem as ScoreboardItem;

      if (selectedUser != null) {
        NavigationService.Navigate(new GUIDifferentUserView(selectedUser.Nombre));
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

  public class ScoreboardItem {
    public string Nombre { get; set; }
    public int PartidasGanadas { get; set; }
    public string Porcentaje { get; set; }
    public int Puntuacion { get; set; }
    public string Puesto { get; set; }
  }
}
