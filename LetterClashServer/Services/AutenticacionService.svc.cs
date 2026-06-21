using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Text.RegularExpressions;

using LetterClashServer.Contracts;
using LetterClashServer.DataAccess.Context;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.Domain.Models;
using LetterClashServer.Domain.Security;

namespace LetterClashServer.Services {
  public class AutenticacionService : IAutenticacionService {
    private static readonly ConcurrentDictionary<int, DateTime> sesionesActivas =
        new ConcurrentDictionary<int, DateTime>();
    private static readonly TimeSpan TiempoSesionActiva = TimeSpan.FromSeconds(75);
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private readonly JugadorRepository jugadorRepository;

    public AutenticacionService() : this(new JugadorRepository()) {
      // Constructor requerido por WCF; delega la inicializacion al constructor inyectable.
    }

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

      if (!EsContrasenaValida(contrasenaPlana)) {
        return ServiceResult<JugadorDTO>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "La contraseña debe tener de 6 a 15 caracteres e incluir mayúscula, minúscula, número y carácter especial.",
          "Formato de contraseña inválido"
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

        if (TieneSesionActiva(jugador.IDJugador)) {
          return ServiceResult<JugadorDTO>.Failure(
            CodigoError.OPERACION_INVALIDA,
            "Este usuario ya tiene una sesiÃ³n activa en otra instancia.",
            $"Jugador ID {jugador.IDJugador} ya tiene una sesiÃ³n activa."
          );
        }

        sesionesActivas[jugador.IDJugador] = DateTime.UtcNow;

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

    private bool EsNombreUsuarioValido(string nombreUsuario) {
      return !string.IsNullOrWhiteSpace(nombreUsuario) &&
             Regex.IsMatch(nombreUsuario.Trim(), @"^[A-Za-z0-9_-]{3,12}$", RegexOptions.None, RegexTimeout);
    }

    private bool EsContrasenaValida(string contrasena) {
      return !string.IsNullOrEmpty(contrasena) &&
             Regex.IsMatch(contrasena, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,15}$", RegexOptions.None, RegexTimeout);
    }

    private static bool TieneSesionActiva(int jugadorID) {
      if (!sesionesActivas.TryGetValue(jugadorID, out DateTime ultimaActividadUtc)) {
        return false;
      }

      if (DateTime.UtcNow - ultimaActividadUtc <= TiempoSesionActiva) {
        return true;
      }

      sesionesActivas.TryRemove(jugadorID, out _);
      return false;
    }

    public ServiceResult<bool> RenovarSesion(int jugadorID) {
      if (jugadorID <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador debe ser un entero positivo.",
          $"jugadorID = {jugadorID}"
        );
      }

      sesionesActivas[jugadorID] = DateTime.UtcNow;
      return ServiceResult<bool>.Success(true);
    }

    public ServiceResult<bool> CerrarSesion(int jugadorID) {
      if (jugadorID <= 0) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El ID del jugador debe ser un entero positivo.",
          $"jugadorID = {jugadorID}"
        );
      }

      sesionesActivas.TryRemove(jugadorID, out _);
      return ServiceResult<bool>.Success(true);
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

      if (!EsNombreCompletoValido(datosJugador.Nombre)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El nombre completo debe incluir nombre y apellido, sin números ni signos especiales.",
          $"Nombre: {datosJugador.Nombre}"
        );
      }

      if (!EsNombreUsuarioValido(datosJugador.NombreDeUsuario)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El nombre de usuario debe tener entre 3 y 12 caracteres, usando letras, números, guion o guion bajo.",
          $"Nombre de usuario: {datosJugador.NombreDeUsuario}"
        );
      }

      if (!EsContrasenaValida(contrasenaPlana)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "La contraseña debe tener de 6 a 15 caracteres e incluir mayúscula, minúscula, número y carácter especial.",
          "Formato de contraseña inválido"
        );
      }

      if (!EsCorreoValido(datosJugador.Correo)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El correo electrónico proporcionado no tiene un formato válido.",
          $"Correo: {datosJugador.Correo}"
        );
      }

      if (datosJugador.FechaDeNacimiento == default(DateTime) || !EsFechaNacimientoValida(datosJugador.FechaDeNacimiento)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "La edad del jugador debe estar entre 3 y 100 años.",
          $"Fecha de nacimiento: {datosJugador.FechaDeNacimiento}"
        );
      }

      if (!EsTelefonoValido(datosJugador.Telefono)) {
        return ServiceResult<bool>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El teléfono debe contener exactamente 10 dígitos numéricos.",
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
      var regex = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,24}$", RegexOptions.IgnoreCase, RegexTimeout);
      return regex.IsMatch(correo);
    }
  }
}
