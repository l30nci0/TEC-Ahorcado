using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

using LetterClashServer.Contracts;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Services {
  public class JugadorSesion {
    public int JugadorID { get; set; }
    public string NombreUsuario { get; set; }
    public IGameServiceCallback Callback { get; set; }
  }

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class GameService : IGameService {
    private readonly PartidaRepository partidaRepository;
    private readonly JugadorRepository jugadorRepository;

    // Diccionario de sesiones en memoria: CodigoAcceso -> Lista de jugadores en la sala
    private static readonly ConcurrentDictionary<string, List<JugadorSesion>> salasDeJuego =
        new ConcurrentDictionary<string, List<JugadorSesion>>();

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
    }

    public void EscribirLetra(string codigoAcceso, int jugadorID, char letra) {
    }

    public void AbandonarPartida(string codigoAcceso, int jugadorID) {
    }

    public void VerPista(string codigoAcceso, int jugadorID) {
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
