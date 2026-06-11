using System.Collections.Generic;
using System.ServiceModel;

using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface IJugadorService {
    [OperationContract]
    ServiceResult<bool> ActualizarPerfil(JugadorDTO jugadorDTO);

    [OperationContract]
    ServiceResult<List<PartidaDTO>> ConsultarHistorial(int jugadorID);

    [OperationContract]
    ServiceResult<List<JugadorPublicoDTO>> ConsultarMarcadores();

    [OperationContract]
    ServiceResult<bool> CambiarIdiomaPreferido(int jugadorID, string idioma);

    [OperationContract]
    ServiceResult<bool> CambiarContrasena(int jugadorID, string contrasenaActual, string nuevaContrasena);

    [OperationContract]
    ServiceResult<JugadorDTO> ObtenerPerfilPorNombre(string nombreUsuario);
  }
}
