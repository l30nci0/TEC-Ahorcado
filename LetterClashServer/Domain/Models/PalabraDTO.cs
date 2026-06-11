using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract]
  public class PalabraDTO {
    [DataMember]
    public int IDPalabra { get; set; }

    [DataMember]
    public string PalabraTexto { get; set; }

    [DataMember]
    public string Descripcion { get; set; }

    [DataMember]
    public string Idioma { get; set; }
  }
}
