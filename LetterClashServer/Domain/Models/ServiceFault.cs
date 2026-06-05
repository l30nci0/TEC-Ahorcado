using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract]
  public class ServiceFault {
    [DataMember]
    public string Mensaje { get; set; }

    [DataMember]
    public CodigoError CodigoError { get; set; }

    [DataMember]
    public string Detalle { get; set; }
  }
}
