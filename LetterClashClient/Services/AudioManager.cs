using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace LetterClashClient.Services {
  public static class AudioManager {
    private enum TipoMusica {
      Ninguna,
      Menu,
      Juego
    }

    private static readonly object syncLock = new object();
    private static readonly List<MediaPlayer> efectosActivos = new List<MediaPlayer>();
    private static MediaPlayer reproductorMusica;
    private static string musicaActual;
    private static TipoMusica tipoMusicaActual = TipoMusica.Ninguna;
    private static double volumenMusica = 0.7;
    private static double volumenEfectos = 0.7;

    public static double VolumenMusica {
      get => volumenMusica;
      set {
        volumenMusica = LimitarVolumen(value);
        lock (syncLock) {
          if (reproductorMusica != null) {
            reproductorMusica.Volume = volumenMusica;
          }
        }
      }
    }

    public static double VolumenEfectos {
      get => volumenEfectos;
      set => volumenEfectos = LimitarVolumen(value);
    }

    public static void ReproducirMusicaMenu() {
      ReproducirMusica("MenuOst.mp3", TipoMusica.Menu);
    }

    public static void ReproducirMusicaJuego() {
      ReproducirMusica("GameOst.mp3", TipoMusica.Juego);
    }

    public static void DetenerMusica() {
      lock (syncLock) {
        if (reproductorMusica != null) {
          reproductorMusica.Stop();
          reproductorMusica.Close();
          reproductorMusica = null;
        }

        musicaActual = null;
        tipoMusicaActual = TipoMusica.Ninguna;
      }
    }

    public static void ReproducirEfecto(string nombreArchivo) {
      if (string.IsNullOrWhiteSpace(nombreArchivo)) {
        return;
      }

      try {
        string ruta = ObtenerRutaAudio(nombreArchivo);
        if (!File.Exists(ruta)) {
          return;
        }

        var reproductor = new MediaPlayer {
          Volume = volumenEfectos
        };

        reproductor.MediaEnded += (sender, args) => CerrarEfecto(reproductor);
        reproductor.MediaFailed += (sender, args) => CerrarEfecto(reproductor);

        lock (syncLock) {
          efectosActivos.Add(reproductor);
        }

        reproductor.Open(new Uri(ruta, UriKind.Absolute));
        reproductor.Play();
      } catch {
        // El audio nunca debe bloquear el flujo principal de la app.
      }
    }

    private static void ReproducirMusica(string nombreArchivo, TipoMusica tipoMusica) {
      if (string.IsNullOrWhiteSpace(nombreArchivo)) {
        return;
      }

      try {
        string ruta = ObtenerRutaAudio(nombreArchivo);
        if (!File.Exists(ruta)) {
          return;
        }

        lock (syncLock) {
          if (reproductorMusica != null && musicaActual == ruta && tipoMusicaActual == tipoMusica) {
            reproductorMusica.Volume = volumenMusica;
            return;
          }

          DetenerMusica();

          reproductorMusica = new MediaPlayer {
            Volume = volumenMusica
          };
          musicaActual = ruta;
          tipoMusicaActual = tipoMusica;

          reproductorMusica.MediaEnded += (sender, args) => {
            lock (syncLock) {
              if (reproductorMusica == null) {
                return;
              }

              reproductorMusica.Position = TimeSpan.Zero;
              reproductorMusica.Play();
            }
          };

          reproductorMusica.MediaFailed += (sender, args) => DetenerMusica();
          reproductorMusica.Open(new Uri(ruta, UriKind.Absolute));
          reproductorMusica.Play();
        }
      } catch {
        DetenerMusica();
      }
    }

    private static string ObtenerRutaAudio(string nombreArchivo) {
      return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", nombreArchivo);
    }

    private static void CerrarEfecto(MediaPlayer reproductor) {
      lock (syncLock) {
        efectosActivos.Remove(reproductor);
      }

      reproductor.Close();
    }

    private static double LimitarVolumen(double valor) {
      return Math.Max(0.0, Math.Min(1.0, valor));
    }
  }
}
