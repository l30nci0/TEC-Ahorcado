using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;

using LetterClashServer.Contracts;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Services {
  public class JugadorSesion {
    public int JugadorID { get; set; }
    public string NombreUsuario { get; set; }
    public IGameServiceCallback Callback { get; set; }
  }

  public class PartidaEnCurso {
    public string CodigoAcceso { get; set; }
    public int IDPartida { get; set; }
    public string PalabraObjetivo { get; set; }
    public string PalabraRevelada { get; set; }
    public int VidasRestantes { get; set; } = GameService.VidasIniciales;
    public HashSet<char> LetrasPropuestas { get; set; } = new HashSet<char>();
    public int HostID { get; set; }
    public int GuesserID { get; set; }
    public DateTime UltimaActividadUtc { get; set; } = DateTime.UtcNow;
  }

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class GameService : IGameService {
    public const int VidasIniciales = 6;
    private const int PuntosVictoriaAdivinador = 10;
    private const int PuntosVictoriaAnfitrion = 5;
    private const int PenalizacionAbandono = -3;
    private static readonly TimeSpan TiempoMaximoInactividad = TimeSpan.FromMinutes(5);
    private static readonly Timer inactivityTimer = new Timer(VerificarInactividad, null, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));
    private readonly PartidaRepository partidaRepository;
    private readonly JugadorRepository jugadorRepository;

    // Diccionario de sesiones en memoria: CodigoAcceso -> Lista de jugadores en la sala
    private static readonly ConcurrentDictionary<string, List<JugadorSesion>> salasDeJuego =
        new ConcurrentDictionary<string, List<JugadorSesion>>();

    // Diccionario de partidas activas en memoria
    private static readonly ConcurrentDictionary<string, PartidaEnCurso> partidasActivas =
        new ConcurrentDictionary<string, PartidaEnCurso>();

    public GameService() : this(new PartidaRepository(), new JugadorRepository()) { }

    public GameService(PartidaRepository repository, JugadorRepository jugadorRepo) {
      this.partidaRepository = repository;
      this.jugadorRepository = jugadorRepo;
    }

    public void ConectarJuego(int jugadorID, string codigoAcceso) {
      var callback = OperationContext.Current.GetCallbackChannel<IGameServiceCallback>();
      if (callback == null) return;

      if (jugadorID <= 0 || string.IsNullOrEmpty(codigoAcceso) || codigoAcceso.Length != 6) {
        var fault = new ServiceFault {
          Mensaje = "Los parámetros de conexión son inválidos.",
          CodigoError = CodigoError.PARAMETRO_INVALIDO,
          Detalle = $"jugadorID = {jugadorID}, codigoAcceso = '{codigoAcceso}'"
        };
        try { callback.OnErrorOcurrido(fault); } catch { }
        return;
      }

      try {
        var jugador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
        if (jugador == null) {
          var fault = new ServiceFault {
            Mensaje = "El jugador especificado no existe en el sistema.",
            CodigoError = CodigoError.RECURSO_NO_ENCONTRADO,
            Detalle = $"Jugador con ID {jugadorID} no encontrado."
          };
          callback.OnErrorOcurrido(fault);
          return;
        }

        var partida = partidaRepository.ObtenerPartidaPorCodigo(codigoAcceso);
        if (partida == null) {
          var fault = new ServiceFault {
            Mensaje = "La partida especificada no existe.",
            CodigoError = CodigoError.RECURSO_NO_ENCONTRADO,
            Detalle = $"Partida con código {codigoAcceso} no encontrada."
          };
          callback.OnErrorOcurrido(fault);
          return;
        }

        if (partida.IDAnfitrion != jugadorID && partida.IDAdivinador != jugadorID) {
          var fault = new ServiceFault {
            Mensaje = "No tienes permiso para ingresar a esta partida.",
            CodigoError = CodigoError.ACCESO_DENEGADO,
            Detalle = $"Jugador ID {jugadorID} no es participante de la partida ID {partida.IDPartida}."
          };
          callback.OnErrorOcurrido(fault);
          return;
        }

        var nuevaSesion = new JugadorSesion {
          JugadorID = jugadorID,
          NombreUsuario = jugador.NombreDeUsuario,
          Callback = callback
        };

        // Suscribirse a eventos de desconexión del canal de comunicación
        var clientChannel = (ICommunicationObject) callback;
        clientChannel.Closed += (sender, args) => LimpiarSesionJugador(codigoAcceso, jugadorID);
        clientChannel.Faulted += (sender, args) => LimpiarSesionJugador(codigoAcceso, jugadorID);

        var jugadoresEnSala = salasDeJuego.GetOrAdd(codigoAcceso, _ => new List<JugadorSesion>());

        lock (jugadoresEnSala) {
          // Remover sesión previa del mismo jugador si existe (ej. por reconexión)
          jugadoresEnSala.RemoveAll(j => j.JugadorID == jugadorID);
          jugadoresEnSala.Add(nuevaSesion);

          // Buscar si el oponente ya está conectado
          var oponente = jugadoresEnSala.FirstOrDefault(j => j.JugadorID != jugadorID);
          if (oponente != null) {
            // 1. Notificar al oponente que este jugador se unió
            var datosUnido = new JugadorPublicoDTO {
              IDJugador = jugador.IDJugador,
              NombreDeUsuario = jugador.NombreDeUsuario,
              Puntuacion = jugador.Puntuacion,
              Avatar = jugador.Avatar
            };
            try {
              oponente.Callback.OnJugadorSeUnio(datosUnido);
            } catch (CommunicationException) {
              // El oponente se desconectó silenciosamente
            }

            // 2. Notificar al jugador que se está uniendo sobre el perfil del oponente existente
            var oponenteJugador = jugadorRepository.ObtenerJugadorPorID(oponente.JugadorID);
            if (oponenteJugador != null) {
              var datosOponente = new JugadorPublicoDTO {
                IDJugador = oponenteJugador.IDJugador,
                NombreDeUsuario = oponenteJugador.NombreDeUsuario,
                Puntuacion = oponenteJugador.Puntuacion,
                Avatar = oponenteJugador.Avatar
              };
              try {
                callback.OnJugadorSeUnio(datosOponente);
              } catch (CommunicationException) {
                // Falla en el callback de red propio
              }
            }

            // 3. Inicializar partida activa en memoria si no existe
            var partidaActiva = partidasActivas.GetOrAdd(codigoAcceso, _ => {
              string palabraOriginal = partida.Palabra?.Palabra1 ?? "SOFTWARE";
              palabraOriginal = palabraOriginal.ToUpper();
              int guesserId = (partida.IDAnfitrion == jugadorID) ? oponente.JugadorID : jugadorID;
              return new PartidaEnCurso {
                CodigoAcceso = codigoAcceso,
                IDPartida = partida.IDPartida,
                PalabraObjetivo = palabraOriginal,
                PalabraRevelada = new string('_', palabraOriginal.Length),
                VidasRestantes = VidasIniciales,
                HostID = partida.IDAnfitrion,
                GuesserID = guesserId,
                UltimaActividadUtc = DateTime.UtcNow
              };
            });

            // 4. Enviar el estado de letras inicial a ambos
            try {
              oponente.Callback.OnLetraPropuesta('\0', true, partidaActiva.PalabraRevelada, partidaActiva.VidasRestantes);
            } catch (CommunicationException) { }
            try {
              callback.OnLetraPropuesta('\0', true, partidaActiva.PalabraRevelada, partidaActiva.VidasRestantes);
            } catch (CommunicationException) { }
          }
        }
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "Ocurrió un error al intentar conectarse a la partida.",
          CodigoError = CodigoError.ERROR_INTERNO,
          Detalle = ex.Message
        };
        try { callback.OnErrorOcurrido(fault); } catch { }
      }
    }

    private void LimpiarSesionJugador(string codigoAcceso, int jugadorID) {
      if (salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        lock (jugadoresEnSala) {
          jugadoresEnSala.RemoveAll(j => j.JugadorID == jugadorID);
          if (jugadoresEnSala.Count == 0) {
            salasDeJuego.TryRemove(codigoAcceso, out _);
          }
        }
      }

      if (partidasActivas.TryRemove(codigoAcceso, out var partida)) {
        if (jugadorID == partida.GuesserID && DateTime.UtcNow - partida.UltimaActividadUtc >= TiempoMaximoInactividad) {
          PenalizarAdivinadorPorInactividad(codigoAcceso, partida);
        } else {
          RegistrarDesconexionInesperada(codigoAcceso, jugadorID, partida);
        }
      } else {
        partidaRepository.EliminarPartidaPendienteSinAdivinador(codigoAcceso, jugadorID);
      }
    }

    private void RegistrarDesconexionInesperada(string codigoAcceso, int jugadorID, PartidaEnCurso partida) {
      try {
        partidaRepository.ConcluirPartidaPorAbandono(partida.IDPartida, jugadorID, false);
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine($"Error al cerrar partida por desconexion: {ex.Message}");
      }

      if (!salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        return;
      }

      List<JugadorSesion> copiaJugadores;
      lock (jugadoresEnSala) {
        copiaJugadores = jugadoresEnSala.ToList();
      }

      string nombreDesconectado = ObtenerNombreJugadorDesconectado(jugadorID);

      foreach (var jugador in copiaJugadores) {
        if (jugador.JugadorID == jugadorID) {
          continue;
        }

        try {
          jugador.Callback.OnOponenteDesconectado(nombreDesconectado);
        } catch (CommunicationException) { }
      }
    }

    private string ObtenerNombreJugadorDesconectado(int jugadorID) {
      var jugador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
      return jugador?.NombreDeUsuario ?? "El oponente";
    }

    private static void VerificarInactividad(object state) {
      DateTime ahora = DateTime.UtcNow;

      foreach (var entrada in partidasActivas.ToArray()) {
        var partida = entrada.Value;
        if (ahora - partida.UltimaActividadUtc < TiempoMaximoInactividad) {
          continue;
        }

        if (!partidasActivas.TryRemove(entrada.Key, out var partidaInactiva)) {
          continue;
        }

        PenalizarAdivinadorPorInactividad(entrada.Key, partidaInactiva);
      }
    }

    private static void PenalizarAdivinadorPorInactividad(string codigoAcceso, PartidaEnCurso partida) {
      var partidaRepositoryLocal = new PartidaRepository();
      var jugadorRepositoryLocal = new JugadorRepository();

      try {
        partidaRepositoryLocal.ConcluirPartidaPorAbandono(partida.IDPartida, partida.GuesserID, true);
        jugadorRepositoryLocal.IncrementarPuntuacion(partida.GuesserID, PenalizacionAbandono);
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine($"Error al cerrar partida por inactividad: {ex.Message}");
      }

      if (!salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        return;
      }

      List<JugadorSesion> copiaJugadores;
      lock (jugadoresEnSala) {
        copiaJugadores = jugadoresEnSala.ToList();
        salasDeJuego.TryRemove(codigoAcceso, out _);
      }

      var adivinador = copiaJugadores.FirstOrDefault(j => j.JugadorID == partida.GuesserID);
      string nombreAdivinador = adivinador?.NombreUsuario ?? "El adivinador";

      foreach (var jugador in copiaJugadores) {
        try {
          if (jugador.JugadorID == partida.GuesserID) {
            jugador.Callback.OnErrorOcurrido(new ServiceFault {
              Mensaje = "La partida se cerro por inactividad. Se aplico una penalizacion de -3 puntos.",
              CodigoError = CodigoError.OPERACION_INVALIDA,
              Detalle = "Timeout de inactividad del adivinador."
            });
          } else {
            jugador.Callback.OnOponenteAbandono(nombreAdivinador);
          }
        } catch (CommunicationException) { }
      }
    }

    public void EscribirLetra(string codigoAcceso, int jugadorID, char letra) {
      if (string.IsNullOrEmpty(codigoAcceso) || jugadorID <= 0) {
        return;
      }

      if (!partidasActivas.TryGetValue(codigoAcceso, out var partida)) {
        return;
      }

      // Solo el adivinador puede proponer letras
      if (jugadorID != partida.GuesserID) {
        return;
      }

      char letraUpper = char.ToUpper(letra);
      if (partida.LetrasPropuestas.Contains(letraUpper) || partida.VidasRestantes <= 0) {
        return;
      }

      partida.UltimaActividadUtc = DateTime.UtcNow;
      partida.LetrasPropuestas.Add(letraUpper);

      bool esCorrecta = partida.PalabraObjetivo.Contains(letraUpper);
      if (esCorrecta) {
        char[] arrayRevelado = partida.PalabraRevelada.ToCharArray();
        for (int i = 0; i < partida.PalabraObjetivo.Length; i++) {
          if (partida.PalabraObjetivo[i] == letraUpper) {
            arrayRevelado[i] = letraUpper;
          }
        }
        partida.PalabraRevelada = new string(arrayRevelado);
      } else {
        partida.VidasRestantes--;
      }

      if (salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        List<JugadorSesion> copiaJugadores;
        lock (jugadoresEnSala) {
          copiaJugadores = jugadoresEnSala.ToList();
        }

        foreach (var jugador in copiaJugadores) {
          try {
            jugador.Callback.OnLetraPropuesta(letraUpper, esCorrecta, partida.PalabraRevelada, partida.VidasRestantes);
          } catch (CommunicationException) { }
        }

        bool juegoTerminado = false;
        string resultado = "";
        int ganadorID = 0;

        if (partida.PalabraRevelada == partida.PalabraObjetivo) {
          juegoTerminado = true;
          resultado = "ADIVINADA";
          ganadorID = partida.GuesserID;
        } else if (partida.VidasRestantes <= 0) {
          juegoTerminado = true;
          resultado = "SIN_ADIVINAR";
          ganadorID = partida.HostID;
        }

        if (juegoTerminado) {
          TerminarPartida(codigoAcceso, partida, resultado, ganadorID, copiaJugadores);
        }
      }
    }

    private void TerminarPartida(string codigoAcceso, PartidaEnCurso partida, string resultado, int ganadorID, List<JugadorSesion> jugadoresEnSala) {
      int puntosGanador = ObtenerPuntosPorResultado(resultado);
      int erroresCometidos = VidasIniciales - partida.VidasRestantes;

      try {
        partidaRepository.ConcluirPartida(partida.IDPartida, resultado, erroresCometidos);
        if (ganadorID > 0 && puntosGanador != 0) {
          jugadorRepository.IncrementarPuntuacion(ganadorID, puntosGanador);
        }
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine($"Error al guardar partida/puntuacion en DB: {ex.Message}");
      }

      partidasActivas.TryRemove(codigoAcceso, out _);

      string nombreGanador = "";
      if (ganadorID > 0) {
        var ganador = jugadorRepository.ObtenerJugadorPorID(ganadorID);
        if (ganador != null) {
          nombreGanador = ganador.NombreDeUsuario;
        }
      }

      foreach (var jugador in jugadoresEnSala) {
        try {
          jugador.Callback.OnPartidaFinalizada(nombreGanador, puntosGanador);
        } catch (CommunicationException) { }
      }
    }

    private int ObtenerPuntosPorResultado(string resultado) {
      if (resultado == "ADIVINADA") {
        return PuntosVictoriaAdivinador;
      }

      if (resultado == "SIN_ADIVINAR") {
        return PuntosVictoriaAnfitrion;
      }

      return 0;
    }

    public void AbandonarPartida(string codigoAcceso, int jugadorID) {
      if (string.IsNullOrEmpty(codigoAcceso) || jugadorID <= 0) {
        return;
      }

      if (!partidasActivas.TryRemove(codigoAcceso, out var partida)) {
        partidaRepository.EliminarPartidaPendienteSinAdivinador(codigoAcceso, jugadorID);
        return;
      }

      try {
        partidaRepository.ConcluirPartidaPorAbandono(partida.IDPartida, jugadorID, true);
        jugadorRepository.IncrementarPuntuacion(jugadorID, PenalizacionAbandono);
      } catch (Exception ex) {
        System.Diagnostics.Debug.WriteLine($"Error al registrar abandono en DB: {ex.Message}");
      }

      if (salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        List<JugadorSesion> copiaJugadores;
        lock (jugadoresEnSala) {
          copiaJugadores = jugadoresEnSala.ToList();
        }

        var abandonador = copiaJugadores.FirstOrDefault(j => j.JugadorID == jugadorID);
        string nombreAbandonador = abandonador?.NombreUsuario ?? "El oponente";

        foreach (var jugador in copiaJugadores) {
          if (jugador.JugadorID != jugadorID) {
            try {
              jugador.Callback.OnOponenteAbandono(nombreAbandonador);
            } catch (CommunicationException) { }
          }
        }
      }
    }

    private void NotificarErrorJugador(string codigoAcceso, int jugadorID, string mensaje, CodigoError codigoError) {
      if (!salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        return;
      }

      JugadorSesion jugador;
      lock (jugadoresEnSala) {
        jugador = jugadoresEnSala.FirstOrDefault(j => j.JugadorID == jugadorID);
      }

      if (jugador == null) {
        return;
      }

      try {
        jugador.Callback.OnErrorOcurrido(new ServiceFault {
          Mensaje = mensaje,
          CodigoError = codigoError,
          Detalle = mensaje
        });
      } catch (CommunicationException) { }
    }

    public void EnviarMensaje(string codigoAcceso, int jugadorID, string mensaje) {
      if (string.IsNullOrEmpty(codigoAcceso) || jugadorID <= 0 || string.IsNullOrWhiteSpace(mensaje)) {
        return;
      }

      if (!salasDeJuego.TryGetValue(codigoAcceso, out var jugadoresEnSala)) {
        return;
      }

      string nombreEmisor = "";
      List<JugadorSesion> destinatarios = new List<JugadorSesion>();

      lock (jugadoresEnSala) {
        var emisor = jugadoresEnSala.FirstOrDefault(j => j.JugadorID == jugadorID);
        if (emisor == null) return;

        nombreEmisor = emisor.NombreUsuario;
        destinatarios = jugadoresEnSala.Where(j => j.JugadorID != jugadorID).ToList();
      }

      foreach (var destinatario in destinatarios) {
        try {
          destinatario.Callback.OnMensajeRecibido(nombreEmisor, mensaje);
        } catch (CommunicationException) {
          lock (jugadoresEnSala) {
            jugadoresEnSala.Remove(destinatario);
          }
        }
      }
    }
  }
}
