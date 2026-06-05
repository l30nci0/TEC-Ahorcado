using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;
using LetterClashServer.DataAccess.Repositories;

namespace LetterClashServer.Services {
  public class LobbyService : ILobbyService {
    private readonly PartidaRepository partidaRepository;
    private readonly JugadorRepository jugadorRepository;
    private readonly PalabraRepository palabraRepository;

    public LobbyService() : this(new PartidaRepository(), new JugadorRepository(), new PalabraRepository()) {}

    public LobbyService(PartidaRepository repository) : this(repository, new JugadorRepository(), new PalabraRepository()) {}

    public LobbyService(PartidaRepository repository, JugadorRepository jugadorRepo, PalabraRepository palabraRepo) {
      this.partidaRepository = repository;
      this.jugadorRepository = jugadorRepo;
      this.palabraRepository = palabraRepo;
    }

    public List<PartidaDTO> ObtenerPartidasLobby() {
      try {
        var partidas = partidaRepository.ObtenerPartidasDisponibles();

        return partidas.Select(p => new PartidaDTO {
          IDPartida = p.IDPartida,
          IDAnfitrion = p.IDAnfitrion,
          NombreAnfitrion = p.Jugador.NombreDeUsuario,
          IDAdivinador = p.IDAdivinador,
          NombreAdivinador = p.Jugador1 != null ? p.Jugador1.NombreDeUsuario : null,
          IDPalabra = p.IDPalabra,
          PalabraRevelada = p.Palabra != null ? p.Palabra.Palabra1 : null,
          Estado = p.Estado,
          Resultado = p.Resultado,
          Privacidad = p.Privacidad,
          Turno = p.Turno,
          CodigoAcceso = p.CodigoAcceso,
          FechaDeJuego = p.FechaDeJuego
        }).ToList();
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible recuperar la información del lobby debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno.");
      }
    }

    public string CrearPartida(int anfitrionID, int palabraID, string privacidad, string idioma) {
      if (anfitrionID <= 0 || palabraID <= 0) {
        var fault = new ServiceFault {
          Mensaje = "El ID del anfitrión y de la palabra deben ser enteros positivos.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"anfitrionID = {anfitrionID}, palabraID = {palabraID}"
        };
        throw new FaultException<ServiceFault>(fault, "Parámetros inválidos.");
      }

      if (string.IsNullOrEmpty(privacidad) || (privacidad != "PRIVADA" && privacidad != "PÚBLICA")) {
        var fault = new ServiceFault {
          Mensaje = "La privacidad debe ser 'PRIVADA' o 'PÚBLICA'.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"privacidad = '{privacidad}'"
        };
        throw new FaultException<ServiceFault>(fault, "Privacidad inválida.");
      }

      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        var fault = new ServiceFault {
          Mensaje = "El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"idioma = '{idioma}'"
        };
        throw new FaultException<ServiceFault>(fault, "Idioma inválido.");
      }

      if (!jugadorRepository.ExisteJugador(anfitrionID)) {
        var fault = new ServiceFault {
          Mensaje = "El jugador anfitrión especificado no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Jugador con ID {anfitrionID} no encontrado."
        };
        throw new FaultException<ServiceFault>(fault, "Anfitrión no encontrado.");
      }

      if (!palabraRepository.ExistePalabra(palabraID)) {
        var fault = new ServiceFault {
          Mensaje = "La palabra especificada no existe en el catálogo.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Palabra con ID {palabraID} no encontrada."
        };
        throw new FaultException<ServiceFault>(fault, "Palabra no encontrada.");
      }

      try {
        return partidaRepository.CrearPartida(anfitrionID, palabraID, privacidad);
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible registrar la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno del servidor.");
      }
    }

    public bool UnirseAPartidaDeLobby(int jugadorID, int partidaID) {
      if (jugadorID <= 0 || partidaID <= 0) {
        var fault = new ServiceFault {
          Mensaje = "El ID del jugador y de la partida deben ser enteros positivos.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"jugadorID = {jugadorID}, partidaID = {partidaID}"
        };
        throw new FaultException<ServiceFault>(fault, "Parámetros inválidos.");
      }

      if (!jugadorRepository.ExisteJugador(jugadorID)) {
        var fault = new ServiceFault {
          Mensaje = "El jugador especificado no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Jugador con ID {jugadorID} no encontrado."
        };
        throw new FaultException<ServiceFault>(fault, "Jugador no encontrado.");
      }

      if (!partidaRepository.ExistePartida(partidaID)) {
        var fault = new ServiceFault {
          Mensaje = "La partida especificada no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Partida con ID {partidaID} no encontrada."
        };
        throw new FaultException<ServiceFault>(fault, "Partida no encontrada.");
      }

      try {
        bool exito = partidaRepository.UnirseAPartidaDeLobby(jugadorID, partidaID);
        if (!exito) {
          var fault = new ServiceFault {
            Mensaje = "No ha sido posible unirse a la partida. Asegúrese de que no sea el anfitrión, que la partida esté PENDIENTE y no tenga un adivinador asignado.",
            CodigoError = "OPERACION_INVALIDA",
            Detalle = $"No se pudo unir Jugador ID {jugadorID} a Partida ID {partidaID}"
          };
          throw new FaultException<ServiceFault>(fault, "Unión fallida.");
        }
        return true;
      } catch (FaultException<ServiceFault>) {
        throw;
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible comenzar la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno del servidor.");
      }
    }

    public PartidaDTO UnirseAPartidaPrivada(int jugadorID, string codigoAcceso) {
      if (jugadorID <= 0 || string.IsNullOrEmpty(codigoAcceso) || codigoAcceso.Length != 6) {
        var fault = new ServiceFault {
          Mensaje = "Parámetros de unión inválidos. El ID del jugador debe ser positivo y el código de acceso debe tener 6 caracteres.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"jugadorID = {jugadorID}, codigoAcceso = '{codigoAcceso}'"
        };
        throw new FaultException<ServiceFault>(fault, "Parámetros inválidos.");
      }

      if (!jugadorRepository.ExisteJugador(jugadorID)) {
        var fault = new ServiceFault {
          Mensaje = "El jugador especificado no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Jugador con ID {jugadorID} no encontrado."
        };
        throw new FaultException<ServiceFault>(fault, "Jugador no encontrado.");
      }

      try {
        var partida = partidaRepository.ObtenerPartidaPorCodigo(codigoAcceso);
        if (partida == null) {
          var fault = new ServiceFault {
            Mensaje = "Partida no encontrada.",
            CodigoError = "RECURSO_NO_ENCONTRADO",
            Detalle = $"No existe una partida con el código de acceso '{codigoAcceso}'."
          };
          throw new FaultException<ServiceFault>(fault, "Partida no encontrada.");
        }

        if (partida.IDAnfitrion == jugadorID) {
          var fault = new ServiceFault {
            Mensaje = "No puedes unirte a una partida creada por ti mismo.",
            CodigoError = "OPERACION_INVALIDA",
            Detalle = $"Jugador ID {jugadorID} intentó unirse a su propia partida ID {partida.IDPartida}."
          };
          throw new FaultException<ServiceFault>(fault, "Operación inválida.");
        }

        if (partida.Estado != "PENDIENTE" || partida.IDAdivinador != null) {
          var fault = new ServiceFault {
            Mensaje = "La partida ya no está disponible para unirse (ya ha comenzado o concluido).",
            CodigoError = "OPERACION_INVALIDA",
            Detalle = $"Partida ID {partida.IDPartida} tiene Estado = '{partida.Estado}' e IDAdivinador = '{partida.IDAdivinador}'."
          };
          throw new FaultException<ServiceFault>(fault, "Partida no disponible.");
        }

        bool exito = partidaRepository.UnirseAPartidaDeLobby(jugadorID, partida.IDPartida);
        if (!exito) {
          var fault = new ServiceFault {
            Mensaje = "No ha sido posible unirse a la partida debido a un conflicto de estado.",
            CodigoError = "ERROR_CONCURRENTE",
            Detalle = "Fallo en la actualización de la base de datos."
          };
          throw new FaultException<ServiceFault>(fault, "Conflicto.");
        }

        var jugadorAdivinador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
        return new PartidaDTO {
          IDPartida = partida.IDPartida,
          IDAnfitrion = partida.IDAnfitrion,
          NombreAnfitrion = partida.Jugador.NombreDeUsuario,
          IDAdivinador = jugadorID,
          NombreAdivinador = jugadorAdivinador?.NombreDeUsuario,
          IDPalabra = partida.IDPalabra,
          PalabraRevelada = partida.Palabra != null ? partida.Palabra.Palabra1 : null,
          Estado = "EN_JUEGO",
          Resultado = partida.Resultado,
          Privacidad = partida.Privacidad,
          Turno = partida.Turno,
          CodigoAcceso = partida.CodigoAcceso,
          FechaDeJuego = partida.FechaDeJuego
        };
      } catch (FaultException<ServiceFault>) {
        throw;
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible comenzar la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno.");
      }
    }
  }
}
