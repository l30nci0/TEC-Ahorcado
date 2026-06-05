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

    public ServiceResult<List<PartidaDTO>> ObtenerPartidasLobby() {
      try {
        var partidas = partidaRepository.ObtenerPartidasDisponibles();

        var dtos = partidas.Select(p => new PartidaDTO {
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

        return ServiceResult<List<PartidaDTO>>.Success(dtos);
      } catch (Exception ex) {
        return ServiceResult<List<PartidaDTO>>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible recuperar la información del lobby debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<string> CrearPartida(int anfitrionID, int palabraID, string privacidad, string idioma) {
      if (anfitrionID <= 0 || palabraID <= 0) {
        return ServiceResult<string>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del anfitrión y de la palabra deben ser enteros positivos.",
          $"anfitrionID = {anfitrionID}, palabraID = {palabraID}"
        );
      }

      if (string.IsNullOrEmpty(privacidad) || (privacidad != "PRIVADA" && privacidad != "PÚBLICA")) {
        return ServiceResult<string>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "La privacidad debe ser 'PRIVADA' o 'PÚBLICA'.",
          $"privacidad = '{privacidad}'"
        );
      }

      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        return ServiceResult<string>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.",
          $"idioma = '{idioma}'"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(anfitrionID)) {
          return ServiceResult<string>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El jugador anfitrión especificado no existe en el sistema.",
            $"Jugador con ID {anfitrionID} no encontrado."
          );
        }

        if (!palabraRepository.ExistePalabra(palabraID)) {
          return ServiceResult<string>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "La palabra especificada no existe en el catálogo.",
            $"Palabra con ID {palabraID} no encontrada."
          );
        }

        string codigo = partidaRepository.CrearPartida(anfitrionID, palabraID, privacidad);
        return ServiceResult<string>.Success(codigo);
      } catch (Exception ex) {
        return ServiceResult<string>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible registrar la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<bool> UnirseAPartidaDeLobby(int jugadorID, int partidaID) {
      if (jugadorID <= 0 || partidaID <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador y de la partida deben ser enteros positivos.",
          $"jugadorID = {jugadorID}, partidaID = {partidaID}"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorID)) {
          return ServiceResult<bool>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        if (!partidaRepository.ExistePartida(partidaID)) {
          return ServiceResult<bool>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "La partida especificada no existe en el sistema.",
            $"Partida con ID {partidaID} no encontrada."
          );
        }

        bool exito = partidaRepository.UnirseAPartidaDeLobby(jugadorID, partidaID);
        if (!exito) {
          return ServiceResult<bool>.Failure(
            CodigoError.OPERACION_INVALIDA,
            "No ha sido posible unirse a la partida. Asegúrese de que no sea el anfitrión, que la partida esté PENDIENTE y no tenga un adivinador asignado.",
            $"No se pudo unir Jugador ID {jugadorID} a Partida ID {partidaID}"
          );
        }
        return ServiceResult<bool>.Success(true);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible comenzar la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<PartidaDTO> UnirseAPartidaPrivada(int jugadorID, string codigoAcceso) {
      if (jugadorID <= 0 || string.IsNullOrEmpty(codigoAcceso) || codigoAcceso.Length != 6) {
        return ServiceResult<PartidaDTO>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "Parámetros de unión inválidos. El ID del jugador debe ser positivo y el código de acceso debe tener 6 caracteres.",
          $"jugadorID = {jugadorID}, codigoAcceso = '{codigoAcceso}'"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorID)) {
          return ServiceResult<PartidaDTO>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        var partida = partidaRepository.ObtenerPartidaPorCodigo(codigoAcceso);
        if (partida == null) {
          return ServiceResult<PartidaDTO>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "Partida no encontrada.",
            $"No existe una partida con el código de acceso '{codigoAcceso}'."
          );
        }

        if (partida.IDAnfitrion == jugadorID) {
          return ServiceResult<PartidaDTO>.Failure(
            CodigoError.OPERACION_INVALIDA,
            "No puedes unirte a una partida creada por ti mismo.",
            $"Jugador ID {jugadorID} intentó unirse a su propia partida ID {partida.IDPartida}."
          );
        }

        if (partida.Estado != "PENDIENTE" || partida.IDAdivinador != null) {
          return ServiceResult<PartidaDTO>.Failure(
            CodigoError.OPERACION_INVALIDA,
            "La partida ya no está disponible para unirse (ya ha comenzado o concluido).",
            $"Partida ID {partida.IDPartida} tiene Estado = '{partida.Estado}' e IDAdivinador = '{partida.IDAdivinador}'."
          );
        }

        bool exito = partidaRepository.UnirseAPartidaDeLobby(jugadorID, partida.IDPartida);
        if (!exito) {
          return ServiceResult<PartidaDTO>.Failure(
            CodigoError.ERROR_CONCURRENTE,
            "No ha sido posible unirse a la partida debido a un conflicto de estado.",
            "Fallo en la actualización de la base de datos."
          );
        }

        var jugadorAdivinador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
        var dto = new PartidaDTO {
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

        return ServiceResult<PartidaDTO>.Success(dto);
      } catch (Exception ex) {
        return ServiceResult<PartidaDTO>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible comenzar la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<bool> PublicarPartida(string codigoAcceso, int anfitrionID) {
      if (string.IsNullOrEmpty(codigoAcceso) || codigoAcceso.Length != 6 || anfitrionID <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "Parámetros inválidos. El código de acceso debe tener 6 caracteres y el ID del anfitrión debe ser un entero positivo.",
          $"codigoAcceso = '{codigoAcceso}', anfitrionID = {anfitrionID}"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(anfitrionID)) {
          return ServiceResult<bool>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El anfitrión especificado no existe en el sistema.",
            $"Jugador con ID {anfitrionID} no encontrado."
          );
        }

        var partida = partidaRepository.ObtenerPartidaPorCodigo(codigoAcceso);
        if (partida == null) {
          return ServiceResult<bool>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "Partida no encontrada.",
            $"No existe una partida con el código de acceso '{codigoAcceso}'."
          );
        }

        if (partida.IDAnfitrion != anfitrionID) {
          return ServiceResult<bool>.Failure(
            CodigoError.OPERACION_INVALIDA,
            "No tienes permisos para modificar esta partida.",
            $"Jugador ID {anfitrionID} intentó publicar una partida cuyo anfitrión es ID {partida.IDAnfitrion}."
          );
        }

        if (partida.Privacidad == "PÚBLICA") {
          return ServiceResult<bool>.Success(true);
        }

        if (partida.Estado != "PENDIENTE") {
          return ServiceResult<bool>.Failure(
            CodigoError.OPERACION_INVALIDA,
            "La partida ya no puede hacerse pública porque ya ha comenzado o concluido.",
            $"Partida ID {partida.IDPartida} tiene Estado = '{partida.Estado}'."
          );
        }

        bool exito = partidaRepository.PublicarPartida(codigoAcceso, anfitrionID);
        if (!exito) {
          return ServiceResult<bool>.Failure(
            CodigoError.ERROR_INTERNO,
            "No ha sido posible publicar la partida debido a un error del servidor.",
            "Fallo al guardar los cambios en la base de datos."
          );
        }

        return ServiceResult<bool>.Success(true);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible cambiar la privacidad de la partida debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }
  }
}
