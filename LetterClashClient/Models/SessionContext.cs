using LetterClashServer.Domain.Models;

namespace LetterClashClient.Models {
  public static class SessionContext {
    public static JugadorDTO UsuarioLogueado { get; set; }

    public static bool IsLoggedIn => UsuarioLogueado != null;

    public static void LimpiarSesion() {
      UsuarioLogueado = null;
    }
  }
}
