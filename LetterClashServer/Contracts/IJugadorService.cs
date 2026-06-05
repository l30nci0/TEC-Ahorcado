using System.Collections.Generic;
using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface IJugadorService {
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool ActualizarPerfil(JugadorDTO jugadorDTO);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<PartidaDTO> ConsultarHistorial(int jugadorID);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<JugadorPublicoDTO> ConsultarMarcadores();

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool CambiarIdiomaPreferido(int jugadorID, string idioma);

    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    bool CambiarContrasena(int jugadorID, string contrasenaActual, string nuevaContrasena);
  }
}
