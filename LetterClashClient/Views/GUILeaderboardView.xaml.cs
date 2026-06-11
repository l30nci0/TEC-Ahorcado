using System;
using System.Collections.Generic;
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
        window.Title = "Marcadores";
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
            if (top1.Avatar != null && top1.Avatar.Length > 0) {
              var img = CrearBitmapDeBytes(top1.Avatar);
              if (img != null) ImageTopAvatar1.Source = img;
            }
          } else {
            TextBlockTopName1.Text = "-";
          }

          // Segundo lugar (índice 1)
          if (jugadores.Count > 1) {
            var top2 = jugadores[1];
            TextBlockTopName2.Text = top2.NombreDeUsuario;
            if (top2.Avatar != null && top2.Avatar.Length > 0) {
              var img = CrearBitmapDeBytes(top2.Avatar);
              if (img != null) ImageTopAvatar2.Source = img;
            }
          } else {
            TextBlockTopName2.Text = "-";
          }

          // Tercer lugar (índice 2)
          if (jugadores.Count > 2) {
            var top3 = jugadores[2];
            TextBlockTopName3.Text = top3.NombreDeUsuario;
            if (top3.Avatar != null && top3.Avatar.Length > 0) {
              var img = CrearBitmapDeBytes(top3.Avatar);
              if (img != null) ImageTopAvatar3.Source = img;
            }
          } else {
            TextBlockTopName3.Text = "-";
          }

          // 2. Cargar todos los mejores 50 en la lista del DataGrid
          List<ScoreboardItem> listado = new List<ScoreboardItem>();
          int limit = Math.Min(jugadores.Count, 50);
          for (int i = 0; i < limit; i++) {
            var j = jugadores[i];
            int victorias = j.Puntuacion / 50;
            string winRate = j.Puntuacion > 0 ? "%100" : "%0";

            listado.Add(new ScoreboardItem {
              Nombre = j.NombreDeUsuario,
              PartidasGanadas = victorias,
              Porcentaje = winRate,
              Puntuacion = j.Puntuacion,
              Puesto = $"{i + 1}*"
            });
          }

          DataGridScoreboard.ItemsSource = listado;
        } else {
          MessageBox.Show(result?.Error?.Mensaje ?? "No se pudieron obtener los marcadores.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (System.ServiceModel.CommunicationException) {
        MessageBox.Show("No se pudo establecer conexión con el servidor para cargar los marcadores.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

  public class ScoreboardItem {
    public string Nombre { get; set; }
    public int PartidasGanadas { get; set; }
    public string Porcentaje { get; set; }
    public int Puntuacion { get; set; }
    public string Puesto { get; set; }
  }
}
