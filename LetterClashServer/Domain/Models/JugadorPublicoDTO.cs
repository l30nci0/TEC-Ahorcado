using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract]
  public class JugadorPublicoDTO {
    [DataMember]
    public int IDJugador { get; set; }

    [DataMember]
    public string NombreDeUsuario { get; set; }

    [DataMember]
    public int Puntuacion { get; set; }

    [DataMember]
    public byte[] Avatar { get; set; }
  }
}
