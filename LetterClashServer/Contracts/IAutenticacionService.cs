using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface IAutenticacionService {
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    JugadorDTO IniciarSesion(string correoONombreUsuario, string contrasenaPlana);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool RegistrarJugador(JugadorDTO datosJugador, string contrasenaPlana);
  }
}
