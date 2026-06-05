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

    public bool ActualizarPerfil(JugadorDTO jugadorDTO) {
      if (jugadorDTO == null) {
        var fault = new ServiceFault {
          Mensaje = "Los datos del perfil no pueden ser nulos.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "jugadorDTO es null"
        };
        throw new FaultException<ServiceFault>(fault, "Datos de perfil nulos.");
      }

      if (jugadorDTO.IDJugador <= 0) {
        var fault = new ServiceFault {
          Mensaje = "El ID del jugador debe ser un entero positivo.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"IDJugador = {jugadorDTO.IDJugador}"
        };
        throw new FaultException<ServiceFault>(fault, "ID de jugador inválido.");
      }

      if (string.IsNullOrWhiteSpace(jugadorDTO.Nombre)) {
        var fault = new ServiceFault {
          Mensaje = "El nombre del jugador es requerido y no puede estar vacío.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "Nombre es nulo o vacío"
        };
        throw new FaultException<ServiceFault>(fault, "Nombre inválido.");
      }

      if (!jugadorRepository.ExisteJugador(jugadorDTO.IDJugador)) {
        var fault = new ServiceFault {
          Mensaje = "El jugador especificado no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Jugador con ID {jugadorDTO.IDJugador} no encontrado."
        };
        throw new FaultException<ServiceFault>(fault, "Jugador no encontrado.");
      }

      try {
        return jugadorRepository.ActualizarPerfil(jugadorDTO);
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible actualizar su perfil debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno del servidor.");
      }
    }

    public List<PartidaDTO> ConsultarHistorial(int jugadorID) {
      if (jugadorID <= 0) {
        var fault = new ServiceFault {
          Mensaje = "El ID del jugador debe ser un entero positivo.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "jugadorID <= 0"
        };
        throw new FaultException<ServiceFault>(fault, "ID de jugador inválido.");
      }

      if (!jugadorRepository.ExisteJugador(jugadorID)) {
        var fault = new ServiceFault {
          Mensaje = "El jugador especificado no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Jugador con ID {jugadorID} no encontrado."
        };
        throw new FaultException<ServiceFault>(fault, "Jugador no encontrado.");
      } 

      var partidas = jugadorRepository.ObtenerHistorialPartidas(jugadorID);

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
    }

    public List<JugadorPublicoDTO> ConsultarMarcadores() {
      var jugadores = jugadorRepository.ObtenerMarcadores();

      return jugadores.Select(j => new JugadorPublicoDTO {
        IDJugador = j.IDJugador,
        NombreDeUsuario = j.NombreDeUsuario,
        Puntuacion = j.Puntuacion,
        Avatar = j.Avatar
      }).ToList();
    }

    public bool CambiarIdiomaPreferido(int jugadorID, string idioma) {
      if (jugadorID <= 0) {
        var fault = new ServiceFault {
          Mensaje = "El ID del jugador debe ser un entero positivo.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"jugadorID = {jugadorID}"
        };
        throw new FaultException<ServiceFault>(fault, "ID de jugador inválido.");
      }

      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        var fault = new ServiceFault {
          Mensaje = "El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"idioma = '{idioma}'"
        };
        throw new FaultException<ServiceFault>(fault, "Idioma inválido.");
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
        return jugadorRepository.ActualizarIdioma(jugadorID, idioma);
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible cambiar el idioma debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno del servidor.");
      }
    }

    public bool CambiarContrasena(int jugadorID, string contrasenaActual, string nuevaContrasena) {
      if (jugadorID <= 0) {
        var fault = new ServiceFault {
          Mensaje = "El ID del jugador debe ser un entero positivo.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = $"jugadorID = {jugadorID}"
        };
        throw new FaultException<ServiceFault>(fault, "ID de jugador inválido.");
      }

      if (string.IsNullOrEmpty(contrasenaActual) || string.IsNullOrEmpty(nuevaContrasena)) {
        var fault = new ServiceFault {
          Mensaje = "Las contraseñas no pueden estar vacías.",
          CodigoError = "PARAMETRO_INVALIDO",
          Detalle = "contrasenaActual o nuevaContrasena vacías"
        };
        throw new FaultException<ServiceFault>(fault, "Contraseña inválida.");
      }

      var jugador = jugadorRepository.ObtenerJugadorPorID(jugadorID);
      if (jugador == null) {
        var fault = new ServiceFault {
          Mensaje = "El jugador especificado no existe en el sistema.",
          CodigoError = "RECURSO_NO_ENCONTRADO",
          Detalle = $"Jugador con ID {jugadorID} no encontrado."
        };
        throw new FaultException<ServiceFault>(fault, "Jugador no encontrado.");
      }

      if (!CryptographyHelper.VerificarContrasena(contrasenaActual, jugador.Contrasena)) {
        var fault = new ServiceFault {
          Mensaje = "La contraseña actual introducida no es correcta.",
          CodigoError = "CREDENCIAIS_INVALIDAS",
          Detalle = "Contraseña actual no coincide con la almacenada"
        };
        throw new FaultException<ServiceFault>(fault, "Contraseña incorrecta.");
      }

      try {
        string nuevaContrasenaEncriptada = CryptographyHelper.EncriptarContrasena(nuevaContrasena);
        return jugadorRepository.ActualizarContrasena(jugadorID, nuevaContrasenaEncriptada);
      } catch (Exception ex) {
        var fault = new ServiceFault {
          Mensaje = "No ha sido posible cambiar la contraseña debido a un error en el sistema. Intente de nuevo más tarde.",
          CodigoError = "ERROR_INTERNO",
          Detalle = ex.Message
        };
        throw new FaultException<ServiceFault>(fault, "Error interno del servidor.");
      }
    }
  }
}
