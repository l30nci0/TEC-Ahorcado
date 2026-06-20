using System;
using System.ServiceModel;
using System.Text.RegularExpressions;

using LetterClashServer.Contracts;
using LetterClashServer.DataAccess.Context;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.Domain.Models;
using LetterClashServer.Domain.Security;

namespace LetterClashServer.Services {
  public class AutenticacionService : IAutenticacionService {
    private readonly JugadorRepository jugadorRepository;

    public AutenticacionService() : this(new JugadorRepository()) { }

    public AutenticacionService(JugadorRepository repository) {
      this.jugadorRepository = repository;
    }

    public ServiceResult<JugadorDTO> IniciarSesion(string correoONombreUsuario, string contrasenaPlana) {
      if (string.IsNullOrWhiteSpace(correoONombreUsuario) || string.IsNullOrEmpty(contrasenaPlana)) {
        return ServiceResult<JugadorDTO>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El correo/usuario y la contraseña son obligatorios.",
          "correoONombreUsuario o contrasenaPlana vacías"
        );
      }

      try {
        var jugador = jugadorRepository.ObtenerJugadorPorCredenciales(correoONombreUsuario);
        if (jugador == null) {
          return ServiceResult<JugadorDTO>.Failure(
            CodigoError.CREDENCIALES_INVALIDAS,
            "Las credenciales de inicio de sesión no son correctas.",
            "Usuario no encontrado"
          );
        }

        if (!CryptographyHelper.VerificarContrasena(contrasenaPlana, jugador.Contrasena)) {
          return ServiceResult<JugadorDTO>.Failure(
            CodigoError.CREDENCIALES_INVALIDAS,
            "Las credenciales de inicio de sesión no son correctas.",
            "Contraseña no coincide"
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
          "No ha sido posible iniciar sesión debido a un error en el servidor. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    private bool EsTelefonoValido(string telefono) {
      return !string.IsNullOrWhiteSpace(telefono) && Regex.IsMatch(telefono.Trim(), @"^[0-9]{10,}$");
    }

    private bool TieneEdadMinima(DateTime fechaDeNacimiento) {
      return fechaDeNacimiento.Date <= DateTime.Today.AddYears(-3);
    }

    public ServiceResult<bool> RegistrarJugador(JugadorDTO datosJugador, string contrasenaPlana) {
      if (datosJugador == null || string.IsNullOrEmpty(contrasenaPlana)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "Los datos de registro del jugador no pueden ser nulos o vacíos.",
          "datosJugador o contrasenaPlana nulos"
        );
      }

      if (string.IsNullOrWhiteSpace(datosJugador.Nombre) ||
          string.IsNullOrWhiteSpace(datosJugador.NombreDeUsuario) ||
          string.IsNullOrWhiteSpace(datosJugador.Correo)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El nombre, el nombre de usuario y el correo electrónico son campos obligatorios.",
          "Campos obligatorios vacíos"
        );
      }

      if (datosJugador.NombreDeUsuario.Length < 3 || datosJugador.NombreDeUsuario.Length > 16) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El nombre de usuario debe tener entre 3 y 16 caracteres.",
          $"Longitud de usuario: {datosJugador.NombreDeUsuario.Length}"
        );
      }

      if (!EsCorreoValido(datosJugador.Correo)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El correo electrónico proporcionado no tiene un formato válido.",
          $"Correo: {datosJugador.Correo}"
        );
      }

      if (datosJugador.FechaDeNacimiento == default(DateTime) || !TieneEdadMinima(datosJugador.FechaDeNacimiento)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El jugador debe tener al menos 3 años.",
          $"Fecha de nacimiento: {datosJugador.FechaDeNacimiento}"
        );
      }

      if (!EsTelefonoValido(datosJugador.Telefono)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El teléfono debe contener mínimo 10 dígitos numéricos.",
          $"Teléfono: {datosJugador.Telefono}"
        );
      }

      // Validar si el usuario o correo ya existen
      if (jugadorRepository.ExisteNombreDeUsuario(datosJugador.NombreDeUsuario)) {
        return ServiceResult<bool>.Failure(
          CodigoError.RECURSO_DUPLICADO,
          "El nombre de usuario ya se encuentra registrado en el sistema.",
          $"Nombre de usuario ya en uso: '{datosJugador.NombreDeUsuario}'"
        );
      }

      if (jugadorRepository.ExisteCorreo(datosJugador.Correo)) {
        return ServiceResult<bool>.Failure(
          CodigoError.RECURSO_DUPLICADO,
          "El correo electrónico ya se encuentra registrado en el sistema.",
          $"Correo ya en uso: '{datosJugador.Correo}'"
        );
      }

      try {
        var nuevoJugador = new Jugador {
          Nombre = datosJugador.Nombre,
          NombreDeUsuario = datosJugador.NombreDeUsuario,
          Correo = datosJugador.Correo,
          Telefono = datosJugador.Telefono,
          Contrasena = CryptographyHelper.EncriptarContrasena(contrasenaPlana),
          Puntuacion = 0,
          Avatar = datosJugador.Avatar,
          IdiomaPreferido = string.IsNullOrEmpty(datosJugador.IdiomaPreferido) ? "ESPAÑOL" : datosJugador.IdiomaPreferido,
          FechaDeNacimiento = datosJugador.FechaDeNacimiento
        };

        bool exito = jugadorRepository.RegistrarJugador(nuevoJugador);
        return ServiceResult<bool>.Success(exito);
      } catch (Exception ex) {
        return ServiceResult<bool>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible registrar su cuenta debido a un error en el sistema. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }

    private bool EsCorreoValido(string correo) {
      if (string.IsNullOrWhiteSpace(correo)) return false;
      var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
      return regex.IsMatch(correo);
    }
  }
}
