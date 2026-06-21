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
        NavigationService.Navigate(new GuiGameHubView());
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
            byte[] avatarRivalBytes = isHost ? p.AvatarAdivinador : p.AvatarAnfitrion;
            string fecha = p.FechaDeJuego.ToString("dd/MM/yyyy");
            string palabra = p.PalabraRevelada ?? "";

            string resultado = "";
            string puntuacion = "0";
            int errores = ObtenerErroresDePartida(p);
            bool cuentaComoDesconexion = false;

            if (p.Resultado == "ADIVINADA") {
              if (isHost) {
                resultado = resLoss;
                puntuacion = "0";
              } else {
                resultado = resWin;
                puntuacion = "+10";
              }
            } else if (p.Resultado == "SIN_ADIVINAR") {
              if (isHost) {
                resultado = resWin;
                puntuacion = "+5";
              } else {
                resultado = resLoss;
                puntuacion = "0";
              }
            } else if (p.Resultado == "ABANDONADA") {
              resultado = resDisconnect;
              puntuacion = EsAbandonoDelJugador(p, usuario.IDJugador) && EsAbandonoPenalizado(p) ? "-3" : "0";
              cuentaComoDesconexion = EsAbandonoDelJugador(p, usuario.IDJugador);
            } else {
              resultado = resDisconnect;
              puntuacion = "0";
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
              Errores = errores,
              NombreUsuario = usuario.NombreDeUsuario,
              RolUsuario = rol,
              RolRival = rolRival,
              AvatarUsuario = avatarUsuario,
              AvatarRival = AvatarHelper.ObtenerImagen(avatarRivalBytes),
              Idioma = idioma,
              Tipo = tipo,
              CuentaComoDesconexion = cuentaComoDesconexion
            });
          }

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

    private int ObtenerErroresDePartida(PartidaDTO partida) {
      if (partida == null) {
        return 0;
      }

      if (partida.Resultado == "ABANDONADA") {
        return 0;
      }

      if (partida.Resultado == "SIN_ADIVINAR") {
        return 6;
      }

      return LimitarErrores(partida.Turno - 1);
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

    private bool EsAbandonoPenalizado(PartidaDTO partida) {
      return partida != null && (partida.Turno == 1 || partida.Turno == 2);
    }

    private int LimitarErrores(int errores) {
      if (errores < 0) {
        return 0;
      }

      if (errores > 6) {
        return 6;
      }

      return errores;
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
      int disconnected = battles.Count(battle => battle.CuentaComoDesconexion);

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
      NavigationHelper.NavigateToMainMenu(NavigationService);
    }

    private void OnClicPerfil(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToProfile(NavigationService);
    }

    private void OnClicHistorial(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToHistory(NavigationService);
    }

    private void OnClicMarcadores(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToScoreboard(NavigationService);
    }

    private void OnClicAjustes(object sender, RoutedEventArgs e) {
      NavigationHelper.NavigateToSettings(NavigationService);
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
    public int Errores { get; set; }

    // Nuevas propiedades para la visualización estilo SF6
    public string NombreUsuario { get; set; }
    public string RolUsuario { get; set; }
    public string RolRival { get; set; }
    public ImageSource AvatarUsuario { get; set; }
    public ImageSource AvatarRival { get; set; }
    public bool CuentaComoDesconexion { get; set; }
  }
}
