using LetterClashServer.Domain.Models;
using LetterClashClient.Services;

using System;
using System.Windows.Threading;

namespace LetterClashClient.Models {
  public static class SessionContext {
    private static readonly DispatcherTimer heartbeatTimer = new DispatcherTimer {
      Interval = TimeSpan.FromSeconds(30)
    };

    static SessionContext() {
      heartbeatTimer.Tick += (sender, args) => RenovarSesion();
    }

    public static JugadorDTO UsuarioLogueado { get; set; }

    public static bool IsLoggedIn => UsuarioLogueado != null;

    public static void IniciarSesion(JugadorDTO usuario) {
      UsuarioLogueado = usuario;
      if (usuario != null) {
        heartbeatTimer.Start();
      }
    }

    private static void RenovarSesion() {
      if (UsuarioLogueado == null) {
        heartbeatTimer.Stop();
        return;
      }

      try {
        var authService = ServiceProxyManager.GetAutenticacionService();
        authService.RenovarSesion(UsuarioLogueado.IDJugador);
      } catch { }
    }

    public static void LimpiarSesion() {
      int? jugadorID = UsuarioLogueado?.IDJugador;
      heartbeatTimer.Stop();
      UsuarioLogueado = null;

      if (jugadorID == null) {
        return;
      }

      try {
        var authService = ServiceProxyManager.GetAutenticacionService();
        authService.CerrarSesion(jugadorID.Value);
      } catch { }
    }
  }
}
