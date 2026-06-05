using System.Collections.Generic;
using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface ILobbyService {
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<PartidaDTO> ObtenerPartidasLobby();

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    string CrearPartida(int anfitrionID, int palabraID, string privacidad, string idioma);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool UnirseAPartidaDeLobby(int jugadorID, int partidaID);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    PartidaDTO UnirseAPartidaPrivada(int jugadorID, string codigoAcceso);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool PublicarPartida(string codigoAcceso, int anfitrionID);
  }
}
