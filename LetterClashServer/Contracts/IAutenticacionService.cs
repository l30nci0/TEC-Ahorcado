using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface IAutenticacionService {
    [OperationContract]
    ServiceResult<JugadorDTO> IniciarSesion(string correoONombreUsuario, string contrasenaPlana);

    [OperationContract]
    ServiceResult<bool> RegistrarJugador(JugadorDTO datosJugador, string contrasenaPlana);
  }
}
