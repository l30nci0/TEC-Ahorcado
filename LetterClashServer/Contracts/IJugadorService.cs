using System.Collections.Generic;
using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface IJugadorService {
    [OperationContract]
    bool ActualizarPerfil(JugadorDTO jugadorDTO);

    [OperationContract]
    List<PartidaDTO> ConsultarHistorial(int jugadorID);

    [OperationContract]
    List<JugadorDTO> ConsultarMarcadores();

    [OperationContract]
    bool CambiarIdiomaPreferido(int jugadorID, string idioma);
  }
}
