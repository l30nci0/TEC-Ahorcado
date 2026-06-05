using System.Collections.Generic;
using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface ILobbyService {
    [OperationContract]
    ServiceResult<List<PartidaDTO>> ObtenerPartidasLobby();

    [OperationContract]
    ServiceResult<string> CrearPartida(int anfitrionID, int palabraID, string privacidad, string idioma);

    [OperationContract]
    ServiceResult<bool> UnirseAPartidaDeLobby(int jugadorID, int partidaID);

    [OperationContract]
    ServiceResult<PartidaDTO> UnirseAPartidaPrivada(int jugadorID, string codigoAcceso);

    [OperationContract]
    ServiceResult<bool> PublicarPartida(string codigoAcceso, int anfitrionID);
  }
}
