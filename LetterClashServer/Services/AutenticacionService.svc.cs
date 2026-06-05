using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;
using LetterClashServer.Domain.Security;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.DataAccess.Context;

namespace LetterClashServer.Services {
  public class AutenticacionService : IAutenticacionService {
    private readonly JugadorRepository jugadorRepository;

    public AutenticacionService() : this(new JugadorRepository()) {}

    public AutenticacionService(JugadorRepository repository) {
      this.jugadorRepository = repository;
    }

    public JugadorDTO IniciarSesion(string correoONombreUsuario, string contrasenaPlana) {
      if (string.IsNullOrWhiteSpace(correoONombreUsuario) || string.IsNullOrEmpty(contrasenaPlana)) {
        var fault = new ServiceFault {
          Mensaje = "El correo/usuario y la contraseña son obligatorios.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "correoONombreUsuario o contrasenaPlana vacías"
        };
        throw new FaultException<ServiceFault>(fault, "Parámetros inválidos.");
      }

      try {
        var jugador = jugadorRepository.ObtenerJugadorPorCredenciales(correoONombreUsuario);
        if (jugador == null) {
          var fault = new ServiceFault {
            Mensaje = "Las credenciales de inicio de sesión no son correctas.",
            CodigoError = "CREDENCIALES_INVALIDAS",
            Detalle = "Usuario no encontrado"
          };
          throw new FaultException<ServiceFault>(fault, "Credenciales incorrectas.");
        }

        if (!CryptographyHelper.VerificarContrasena(contrasenaPlana, jugador.Contrasena)) {
          var fault = new ServiceFault {
            Mensaje = "Las credenciales de inicio de sesión no son correctas.",
            CodigoError = "CREDENCIALES_INVALIDAS",
            Detalle = "Contraseña no coincide"
          };
          throw new FaultException<ServiceFault>(fault, "Credenciales incorrectas.");
        }

        return new JugadorDTO {
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
      } catch (FaultException<ServiceFault>) {
        throw;
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible iniciar sesión debido a un error en el servidor. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno.");
      }
    }

    public bool RegistrarJugador(JugadorDTO datosJugador, string contrasenaPlana) {
      if (datosJugador == null || string.IsNullOrEmpty(contrasenaPlana)) {
        var fault = new ServiceFault {
          Mensaje = "Los datos de registro del jugador no pueden ser nulos o vacíos.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "datosJugador o contrasenaPlana nulos"
        };
        throw new FaultException<ServiceFault>(fault, "Datos de registro nulos.");
      }

      if (string.IsNullOrWhiteSpace(datosJugador.Nombre) ||
          string.IsNullOrWhiteSpace(datosJugador.NombreDeUsuario) ||
          string.IsNullOrWhiteSpace(datosJugador.Correo)) {
        var fault = new ServiceFault {
          Mensaje = "El nombre, el nombre de usuario y el correo electrónico son campos obligatorios.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "Campos obligatorios vacíos"
        };
        throw new FaultException<ServiceFault>(fault, "Datos obligatorios faltantes.");
      }

      if (datosJugador.NombreDeUsuario.Length < 3 || datosJugador.NombreDeUsuario.Length > 16) {
        var fault = new ServiceFault {
          Mensaje = "El nombre de usuario debe tener entre 3 y 16 caracteres.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"Longitud de usuario: {datosJugador.NombreDeUsuario.Length}"
        };
        throw new FaultException<ServiceFault>(fault, "Nombre de usuario inválido.");
      }

      if (!EsCorreoValido(datosJugador.Correo)) {
        var fault = new ServiceFault {
          Mensaje = "El correo electrónico proporcionado no tiene un formato válido.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"Correo: {datosJugador.Correo}"
        };
        throw new FaultException<ServiceFault>(fault, "Correo electrónico inválido.");
      }

      if (datosJugador.FechaDeNacimiento == default(DateTime) || datosJugador.FechaDeNacimiento > DateTime.Today) {
        var fault = new ServiceFault {
          Mensaje = "La fecha de nacimiento no es válida.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"Fecha de nacimiento: {datosJugador.FechaDeNacimiento}"
        };
        throw new FaultException<ServiceFault>(fault, "Fecha de nacimiento inválida.");
      }

      // Validar si el usuario o correo ya existen
      if (jugadorRepository.ExisteNombreDeUsuario(datosJugador.NombreDeUsuario)) {
        var fault = new ServiceFault {
          Mensaje = "El nombre de usuario ya se encuentra registrado en el sistema.",
          CodigoError = "RECURSO_DUPLICADO",
          Detalle = $"Nombre de usuario ya en uso: '{datosJugador.NombreDeUsuario}'"
        };
        throw new FaultException<ServiceFault>(fault, "Usuario duplicado.");
      }

      if (jugadorRepository.ExisteCorreo(datosJugador.Correo)) {
        var fault = new ServiceFault {
          Mensaje = "El correo electrónico ya se encuentra registrado en el sistema.",
          CodigoError = "RECURSO_DUPLICADO",
          Detalle = $"Correo ya en uso: '{datosJugador.Correo}'"
        };
        throw new FaultException<ServiceFault>(fault, "Correo duplicado.");
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

        return jugadorRepository.RegistrarJugador(nuevoJugador);
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible registrar su cuenta debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno.");
      }
    }

    private bool EsCorreoValido(string correo) {
      if (string.IsNullOrWhiteSpace(correo)) return false;
      var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
      return regex.IsMatch(correo);
    }
  }
}
