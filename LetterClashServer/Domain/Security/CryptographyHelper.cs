using System;

namespace LetterClashServer.Domain.Security {
  public static class CryptographyHelper {
    public static string EncriptarContrasena(string contrasenaPlana) {
      if (string.IsNullOrEmpty(contrasenaPlana)) {
        throw new ArgumentException("La contraseña no puede estar vacía.");
      }
      return BCrypt.Net.BCrypt.HashPassword(contrasenaPlana);
    }

    public static bool VerificarContrasena(string contrasenaPlana, string contrasenaEncriptada) {
      if (string.IsNullOrEmpty(contrasenaPlana) || string.IsNullOrEmpty(contrasenaEncriptada)) {
        return false;
      }
      try {
        return BCrypt.Net.BCrypt.Verify(contrasenaPlana, contrasenaEncriptada);
      } catch {
        return false;
      }
    }
  }
}
