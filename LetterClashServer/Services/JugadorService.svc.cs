using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;

using LetterClashServer.Contracts;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.Domain.Models;
using LetterClashServer.Domain.Security;

namespace LetterClashServer.Services {
  public class JugadorService : IJugadorService {
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private readonly JugadorRepository jugadorRepository;

    public JugadorService() : this(new JugadorRepository()) { }

    public JugadorService(JugadorRepository repository) {
      this.jugadorRepository = repository;
    }

    private bool EsTelefonoValido(string telefono) {
      return !string.IsNullOrWhiteSpace(telefono) &&
             Regex.IsMatch(telefono.Trim(), @"^[0-9]{10}$", RegexOptions.None, RegexTimeout);
    }

    private bool EsFechaNacimientoValida(DateTime fechaDeNacimiento) {
      DateTime today = DateTime.Today;
      return fechaDeNacimiento.Date <= today.AddYears(-3) &&
             fechaDeNacimiento.Date >= today.AddYears(-100) &&
             fechaDeNacimiento.Date <= today;
    }

    private bool EsNombreCompletoValido(string nombre) {
      return !string.IsNullOrWhiteSpace(nombre) &&
             Regex.IsMatch(nombre.Trim(), @"^(?=.{8,}$)\p{L}{3,}(?:\s+\p{L}+)*\s+\p{L}{4,}$", RegexOptions.None, RegexTimeout);
    }

    private bool EsContrasenaValida(string contrasena) {
      return !string.IsNullOrEmpty(contrasena) &&
             Regex.IsMatch(contrasena, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,15}$", RegexOptions.None, RegexTimeout);
    }

    public ServiceResult<bool> ActualizarPerfil(JugadorDTO jugadorDTO) {
      if (jugadorDTO == null) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "Los datos del perfil no pueden ser nulos.",
          "jugadorDTO es null"
        );
      }

      if (jugadorDTO.IDJugador <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador debe ser un entero positivo.",
          $"IDJugador = {jugadorDTO.IDJugador}"
        );
      }

      if (!EsNombreCompletoValido(jugadorDTO.Nombre)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El nombre completo debe incluir nombre y apellido, sin números ni signos especiales.",
          $"Nombre: {jugadorDTO.Nombre}"
        );
      }

      if (!EsTelefonoValido(jugadorDTO.Telefono)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El teléfono debe contener exactamente 10 dígitos numéricos.",
          $"Teléfono: {jugadorDTO.Telefono}"
        );
      }

      if (jugadorDTO.FechaDeNacimiento == default(DateTime) || !EsFechaNacimientoValida(jugadorDTO.FechaDeNacimiento)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "La edad del jugador debe estar entre 3 y 100 años.",
          $"Fecha de nacimiento: {jugadorDTO.FechaDeNacimiento}"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorDTO.IDJugador)) {
          return ServiceResult<bool>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorDTO.IDJugador} no encontrado."
          );
        }

        bool exito = jugadorRepository.ActualizarPerfil(jugadorDTO);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible actualizar su perfil debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<List<PartidaDTO>> ConsultarHistorial(int jugadorID) {
      if (jugadorID <= 0) {
        return ServiceResult<List<PartidaDTO>>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador debe ser un entero positivo.",
          "jugadorID <= 0"
        );
      }

      try {
        if (!jugadorRepository.ExisteJugador(jugadorID)) {
          return ServiceResult<List<PartidaDTO>>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        var partidas = jugadorRepository.ObtenerHistorialPartidas(jugadorID);

        var dtos = partidas.Select(p => new PartidaDTO {
          IDPartida = p.IDPartida,
          IDAnfitrion = p.IDAnfitrion,
          NombreAnfitrion = p.Jugador1.NombreDeUsuario,
          AvatarAnfitrion = p.Jugador1.Avatar,
          IDAdivinador = p.IDAdivinador,
          NombreAdivinador = p.Jugador != null ? p.Jugador.NombreDeUsuario : null,
          AvatarAdivinador = p.Jugador != null ? p.Jugador.Avatar : null,
          IDPalabra = p.IDPalabra,
          PalabraRevelada = p.Palabra != null ? p.Palabra.Palabra1 : null,
          Estado = p.Estado,
          Resultado = p.Resultado,
          Privacidad = p.Privacidad,
          Turno = p.Turno,
          CodigoAcceso = p.CodigoAcceso,
          FechaDeJuego = p.FechaDeJuego,
          Idioma = p.Palabra != null ? p.Palabra.Idioma : null
        }).ToList();

        return ServiceResult<List<PartidaDTO>>.Success(dtos);
      } catch (Exception ex) {
        return ServiceResult<List<PartidaDTO>>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible obtener el historial de partidas debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<List<JugadorPublicoDTO>> ConsultarMarcadores() {
      try {
        var jugadores = jugadorRepository.ObtenerMarcadores();
        var jugadorIDs = jugadores.Select(j => j.IDJugador).ToList();
        var victoriasPorJugador = jugadorRepository.ObtenerVictoriasPorJugadores(jugadorIDs);
        var partidasPorJugador = jugadorRepository.ObtenerPartidasConcluidasPorJugadores(jugadorIDs);

        var dtos = jugadores.Select(j => new JugadorPublicoDTO {
          IDJugador = j.IDJugador,
          NombreDeUsuario = j.NombreDeUsuario,
          Puntuacion = j.Puntuacion,
          PartidasGanadas = victoriasPorJugador.ContainsKey(j.IDJugador) ? victoriasPorJugador[j.IDJugador] : 0,
          PartidasConcluidas = partidasPorJugador.ContainsKey(j.IDJugador) ? partidasPorJugador[j.IDJugador] : 0,
          Avatar = j.Avatar
        }).ToList();

        return ServiceResult<List<JugadorPublicoDTO>>.Success(dtos);
      } catch (Exception ex) {
        return ServiceResult<List<JugadorPublicoDTO>>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible obtener los marcadores debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<bool> CambiarIdiomaPreferido(int jugadorID, string idioma) {
      if (jugadorID <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador debe ser un entero positivo.",
          $"jugadorID = {jugadorID}"
        );
      }

      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.",
          $"idioma = '{idioma}'"
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

        bool exito = jugadorRepository.ActualizarIdioma(jugadorID, idioma);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible cambiar el idioma debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<bool> CambiarContrasena(int jugadorID, string contrasenaActual, string nuevaContrasena) {
      if (jugadorID <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador debe ser un entero positivo.",
          $"jugadorID = {jugadorID}"
        );
      }

      if (string.IsNullOrEmpty(contrasenaActual) || string.IsNullOrEmpty(nuevaContrasena)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "Las contraseñas no pueden estar vacías.",
          "contrasenaActual o nuevaContrasena vacías"
        );
      }

      if (!EsContrasenaValida(nuevaContrasena)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "La contraseña debe tener de 6 a 15 caracteres e incluir mayúscula, minúscula, número y carácter especial.",
          "Formato de contraseña inválido"
        );
      }

      try {
        var jugador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
        if (jugador == null) {
          return ServiceResult<bool>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El jugador especificado no existe en el sistema.",
            $"Jugador con ID {jugadorID} no encontrado."
          );
        }

        if (!CryptographyHelper.VerificarContrasena(contrasenaActual, jugador.Contrasena)) {
          return ServiceResult<bool>.Failure(
            CodigoError.CREDENCIALES_INVALIDAS,
            "La contraseña actual introducida no es correcta.",
            "Contraseña actual no coincide con la almacenada"
          );
        }

        string nuevaContrasenaEncriptada = CryptographyHelper.EncriptarContrasena(nuevaContrasena);
        bool exito = jugadorRepository.ActualizarContrasena(jugadorID, nuevaContrasenaEncriptada);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible cambiar la contraseña debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    public ServiceResult<JugadorDTO> ObtenerPerfilPorNombre(string nombreUsuario) {
      if (string.IsNullOrWhiteSpace(nombreUsuario)) {
        return ServiceResult<JugadorDTO>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El nombre de usuario no puede estar vacío.",
          "nombreUsuario es nulo o vacío"
        );
      }

      try {
        var jugador = jugadorRepository.ObtenerJugadorPorCredenciales(nombreUsuario);
        if (jugador == null) {
          return ServiceResult<JugadorDTO>.Failure(
            CodigoError.RECURSO_NO_ENCONTRADO,
            "El usuario especificado no existe.",
            $"Usuario '{nombreUsuario}' no encontrado."
          );
        }

        var dto = new JugadorDTO {
          IDJugador = jugador.IDJugador,
          Nombre = jugador.Nombre,
          NombreDeUsuario = jugador.NombreDeUsuario,
          Correo = jugador.Correo,
          Telefono = jugador.Telefono,
          Puntuacion = jugador.Puntuacion,
          Avatar = jugador.Avatar,
          IdiomaPreferido = jugador.IdiomaPreferido,
          FechaDeNacimiento = jugador.FechaDeNacimiento
        };

        return ServiceResult<JugadorDTO>.Success(dto);
      } catch (Exception ex) {
        return ServiceResult<JugadorDTO>.Failure(
          CodigoError.ERROR_INTERNO,
          "Error al obtener el perfil del usuario.",
          ex.Message
        );
      }
    }
  }
}
