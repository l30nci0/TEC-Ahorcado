using System.Collections.Generic;
using System.ServiceModel;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  [ServiceContract]
  public interface IPalabraService {
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    List<PalabraDTO> ObtenerPalabrasPorIdioma(string idioma);
  }
}
