using System.Collections.Generic;
using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface ILobbyService {
    [OperationContract]
    List<PartidaDTO> ObtenerPartidasLobby();

    [OperationContract]
    string CrearPartida(int anfitrionID, int palabraID, string privacidad, string idioma);

    [OperationContract]
    bool UnirseAPartidaDeLobby(int jugadorID, int partidaID);

    [OperationContract]
    PartidaDTO UnirseAPartidaPrivada(int jugadorID, string codigoAcceso);
  }
}
