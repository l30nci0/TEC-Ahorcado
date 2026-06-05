using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;
using LetterClashServer.Domain.Security;
using LetterClashServer.DataAccess.Repositories;

namespace LetterClashServer.Services {
  public class JugadorService : IJugadorService {
    private readonly JugadorRepository jugadorRepository;

    public JugadorService() : this(new JugadorRepository()) {}

    public JugadorService(JugadorRepository repository) {
      this.jugadorRepository = repository;
    }

    public ServiceResult<bool> ActualizarPerfil(JugadorDTO jugadorDTO) {
      if (jugadorDTO == null) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "Los datos del perfil no pueden ser nulos.",
          "jugadorDTO es null"
        );
      }

      if (jugadorDTO.IDJugador <= 0) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "El ID del jugador debe ser un entero positivo.",
          $"IDJugador = {jugadorDTO.IDJugador}"
        );
      }

      if (string.IsNullOrWhiteSpace(jugadorDTO.Nombre)) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "El nombre del jugador es requerido y no puede estar vacío.",
          "Nombre es nulo o vacío"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorDTO.IDJugador)) {
          return ServiceResult<bool>.Failure(
            "RECURSO_NO_ENCONTRADO",
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorDTO.IDJugador} no encontrado."
          );
        }

        bool exito = jugadorRepository.ActualizarPerfil(jugadorDTO);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          "ERROR_INTERNO",
          "No ha sido posible actualizar su perfil debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<List<PartidaDTO>> ConsultarHistorial(int jugadorID) {
      if (jugadorID <= 0) {
        return ServiceResult<List<PartidaDTO>>.Failure(
          "PARAMETRO_INVALIDO",
          "El ID del jugador debe ser un entero positivo.",
          "jugadorID <= 0"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorID)) {
          return ServiceResult<List<PartidaDTO>>.Failure(
            "RECURSO_NO_ENCONTRADO",
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        var partidas = jugadorRepository.ObtenerHistorialPartidas(jugadorID);

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
          "ERROR_INTERNO",
          "No ha sido posible obtener el historial de partidas debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<List<JugadorPublicoDTO>> ConsultarMarcadores() {
      try {
        var jugadores = jugadorRepository.ObtenerMarcadores();

        var dtos = jugadores.Select(j => new JugadorPublicoDTO {
          IDJugador = j.IDJugador,
          NombreDeUsuario = j.NombreDeUsuario,
          Puntuacion = j.Puntuacion,
          Avatar = j.Avatar
        }).ToList();

        return ServiceResult<List<JugadorPublicoDTO>>.Success(dtos);
      } catch (Exception ex) {
        return ServiceResult<List<JugadorPublicoDTO>>.Failure(
          "ERROR_INTERNO",
          "No ha sido posible obtener los marcadores debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<bool> CambiarIdiomaPreferido(int jugadorID, string idioma) {
      if (jugadorID <= 0) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "El ID del jugador debe ser un entero positivo.",
          $"jugadorID = {jugadorID}"
        );
      }

      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.",
          $"idioma = '{idioma}'"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorID)) {
          return ServiceResult<bool>.Failure(
            "RECURSO_NO_ENCONTRADO",
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        bool exito = jugadorRepository.ActualizarIdioma(jugadorID, idioma);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          "ERROR_INTERNO",
          "No ha sido posible cambiar el idioma debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<bool> CambiarContrasena(int jugadorID, string contrasenaActual, string nuevaContrasena) {
      if (jugadorID <= 0) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "El ID del jugador debe ser un entero positivo.",
          $"jugadorID = {jugadorID}"
        );
      }

      if (string.IsNullOrEmpty(contrasenaActual) || string.IsNullOrEmpty(nuevaContrasena)) {
        return ServiceResult<bool>.Failure(
          "PARAMETRO_INVALIDO",
          "Las contraseñas no pueden estar vacías.",
          "contrasenaActual o nuevaContrasena vacías"
        );
      }

      try {
        var jugador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
        if (jugador == null) {
          return ServiceResult<bool>.Failure(
            "RECURSO_NO_ENCONTRADO",
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        if (!CryptographyHelper.VerificarContrasena(contrasenaActual, jugador.Contrasena)) {
          return ServiceResult<bool>.Failure(
            "CREDENCIALES_INVALIDAS",
            "La contraseña actual introducida no es correcta.",
            "Contraseña actual no coincide con la almacenada"
          );
        }

        string nuevaContrasenaEncriptada = CryptographyHelper.EncriptarContrasena(nuevaContrasena);
        bool exito = jugadorRepository.ActualizarContrasena(jugadorID, nuevaContrasenaEncriptada);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          "ERROR_INTERNO",
          "No ha sido posible cambiar la contraseña debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }
  }
}
