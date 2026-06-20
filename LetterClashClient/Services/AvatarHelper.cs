using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LetterClashClient.Services {
  /// <summary>
  /// Clase utilitaria para la carga de avatares con soporte de imagen por defecto.
  /// Centraliza la lógica de conversión de bytes a ImageSource y el fallback a UserAvatar.png.
  /// </summary>
  public static class AvatarHelper {
    private static ImageSource _avatarDefault;
    private static readonly object _lockDefault = new object();

    /// <summary>
    /// Obtiene la imagen por defecto (UserAvatar.png) en caso de que un jugador no tenga avatar.
    /// Se carga una sola vez y se cachea.
    /// </summary>
    public static ImageSource AvatarDefault {
      get {
        if (_avatarDefault == null) {
          lock (_lockDefault) {
            if (_avatarDefault == null) {
              try {
                _avatarDefault = new BitmapImage(
                  new Uri("pack://application:,,,/Assets/Images/UserAvatar.png", UriKind.Absolute)
                );
              } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[AvatarHelper] Error al cargar avatar por defecto: {ex.Message}");
              }
            }
          }
        }
        return _avatarDefault;
      }
    }

    /// <summary>
    /// Convierte un arreglo de bytes a un ImageSource (BitmapImage).
    /// Si los bytes son nulos o vacíos, retorna la imagen por defecto.
    /// </summary>
    /// <param name="bytesAvatar">Arreglo de bytes del avatar.</param>
    /// <returns>ImageSource con el avatar o la imagen por defecto.</returns>
    public static ImageSource ObtenerImagen(byte[] bytesAvatar) {
      if (bytesAvatar == null || bytesAvatar.Length == 0) {
        return AvatarDefault;
      }

      try {
        var image = new BitmapImage();
        using (var mem = new MemoryStream(bytesAvatar)) {
          image.BeginInit();
          image.CacheOption = BitmapCacheOption.OnLoad;
          image.StreamSource = mem;
          image.EndInit();
        }
        image.Freeze(); // Permite usar la imagen desde hilos secundarios
        return image;
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine($"[AvatarHelper] Error al convertir bytes a imagen: {ex.Message}");
        return AvatarDefault;
      }
    }

    /// <summary>
    /// Asigna un avatar a un control Image. Si los bytes son nulos/vacíos, asigna la imagen por defecto.
    /// </summary>
    /// <param name="controlImagen">El control Image al que se le asignará la fuente.</param>
    /// <param name="bytesAvatar">Arreglo de bytes del avatar.</param>
    public static void AsignarAImageControl(System.Windows.Controls.Image controlImagen, byte[] bytesAvatar) {
      if (controlImagen == null) return;
      controlImagen.Source = ObtenerImagen(bytesAvatar);
    }
  }
}