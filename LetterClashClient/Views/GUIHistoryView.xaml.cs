using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;

using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIHistoryView : Page {
    public GUIHistoryView() {
      InitializeComponent();
    }

    private void OnPaginaCargada(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("History_WindowTitle") ?? "Historial";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string invalidSession = (string) Application.Current.FindResource("Msg_InvalidUserSession") ?? "Sesión de usuario inválida.";
        MessageBox.Show(invalidSession, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIGameHubView());
        return;
      }

      TextBlockUsername.Text = usuario.NombreDeUsuario;

      // Cargar avatar local (o default si no tiene)
      ImageSource avatarUsuario = AvatarHelper.ObtenerImagen(usuario.Avatar);
      ImageUserAvatar.Source = avatarUsuario;

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
            string nombreRival = isHost ? (p.NombreAdivinador ?? noChallenger) : p.NombreAnfitrion;
            string fecha = p.FechaDeJuego.ToString("dd/MM/yyyy");
            string palabra = p.PalabraRevelada ?? "";

            string resultado = "";
            string puntuacion = "0";
            int progreso = 0;

            if (p.Resultado == "ADIVINADA") {
              if (isHost) {
                resultado = resLoss;
                puntuacion = "0";
                progreso = 6;
              } else {
                resultado = resWin;
                puntuacion = "+10";
                progreso = 6;
              }
            } else if (p.Resultado == "SIN_ADIVINAR") {
              if (isHost) {
                resultado = resWin;
                puntuacion = "+5";
                progreso = 6;
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

            string rolRival = isHost ? rolChallenger : rolHost;
            string tipo = (p.Privacidad == "PÚBLICA" || p.Privacidad == "PUBLICA") ? typePublic : typePrivate;
            string idioma = langES;
            if (p.Idioma != null && (p.Idioma.ToUpper() == "INGLÉS" || p.Idioma.ToUpper() == "INGLES")) {
              idioma = langEN;
            }

            battles.Add(new BattleHistoryItem {
              Rival = nombreRival,
              Fecha = fecha,
              Palabra = palabra,
              Resultado = resultado,
              Puntuacion = puntuacion,
              Rol = rol,
              Progreso = progreso,
              NombreUsuario = usuario.NombreDeUsuario,
              RolUsuario = rol,
              RolRival = rolRival,
              AvatarUsuario = avatarUsuario,
              Idioma = idioma,
              Tipo = tipo
            });
          }

          // Cargar avatares de los rivales
          CargarAvataresRivales(battles, service);

          ItemsControlHistory.ItemsSource = battles;
          CargarEstadisticas(battles);
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

    private void CargarAvataresRivales(List<BattleHistoryItem> battles, IJugadorService service) {
      var nombresVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var cacheAvatares = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

      foreach (var battle in battles) {
        string nombreRival = battle.Rival;
        if (string.IsNullOrEmpty(nombreRival) || nombresVistos.Contains(nombreRival)) continue;
        nombresVistos.Add(nombreRival);

        try {
          var profileResult = service.ObtenerPerfilPorNombre(nombreRival);
          if (profileResult != null && profileResult.IsSuccess && profileResult.Value != null) {
            cacheAvatares[nombreRival] = AvatarHelper.ObtenerImagen(profileResult.Value.Avatar);
          }
        } catch (Exception ex) {
          System.Diagnostics.Debug.WriteLine($"[GUIHistoryView] Error al cargar avatar del rival '{nombreRival}': {ex.Message}");
        }
      }

      // Asignar avatares a las batallas (usar default si no se encontró)
      foreach (var battle in battles) {
        if (cacheAvatares.TryGetValue(battle.Rival, out var avatar)) {
          battle.AvatarRival = avatar;
        } else {
          battle.AvatarRival = AvatarHelper.AvatarDefault;
        }
      }
    }

    private int CalcularEdad(DateTime fechaNacimiento) {
      DateTime today = DateTime.Today;
      int age = today.Year - fechaNacimiento.Year;
      if (fechaNacimiento.Date > today.AddYears(-age)) {
        age--;
      }
      return age;
    }

    private void CargarEstadisticas(List<BattleHistoryItem> battles) {
      int total = battles.Count;
      string resWin = (string) Application.Current.FindResource("History_ResultWin") ?? "Victoria";
      string resLoss = (string) Application.Current.FindResource("History_ResultLoss") ?? "Derrota";
      string resDisconnect = (string) Application.Current.FindResource("History_ResultDisconnect") ?? "Desconectada";

      int wins = battles.Count(battle => battle.Resultado == resWin);
      int losses = battles.Count(battle => battle.Resultado == resLoss);
      int disconnected = battles.Count(battle => battle.Resultado == resDisconnect);

      string percentFormat = (string) Application.Current.FindResource("History_StatPercentFormat") ?? "{0}%";

      TextBlockTotalWins.Text = string.Format((string) Application.Current.FindResource("History_StatTotalWins") ?? "Total Ganadas = {0}", wins);
      TextBlockPercentWins.Text = string.Format(percentFormat, ObtenerPorcentaje(wins, total));
      TextBlockTotalLosses.Text = string.Format((string) Application.Current.FindResource("History_StatTotalLosses") ?? "Total Perdidas = {0}", losses);
      TextBlockPercentLosses.Text = string.Format(percentFormat, ObtenerPorcentaje(losses, total));
      TextBlockTotalDisconnected.Text = string.Format((string) Application.Current.FindResource("History_StatTotalDisconnected") ?? "Total Desconectadas = {0}", disconnected);
      TextBlockPercentDisconnected.Text = string.Format(percentFormat, ObtenerPorcentaje(disconnected, total));
    }

    private int ObtenerPorcentaje(int cantidad, int total) {
      if (total == 0) {
        return 0;
      }

      return cantidad * 100 / total;
    }

    private void OnClicMenuPrincipal(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIGameHubView());
    }

    private void OnClicPerfil(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIProfileView());
    }

    private void OnClicHistorial(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIHistoryView());
    }

    private void OnClicMarcadores(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILeaderboardView());
    }

    private void OnClicAjustes(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUISettingsView());
    }

    private void OnClicItemHistorial(object sender, MouseButtonEventArgs e) {
      if (sender is Border border && border.DataContext is BattleHistoryItem selectedBattle) {
        NavigationService.Navigate(new GUIBattleHistoryDetailView(selectedBattle));
      }
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

    // Nuevas propiedades para la visualización estilo SF6
    public string NombreUsuario { get; set; }
    public string RolUsuario { get; set; }
    public string RolRival { get; set; }
    public ImageSource AvatarUsuario { get; set; }
    public ImageSource AvatarRival { get; set; }
  }
}
