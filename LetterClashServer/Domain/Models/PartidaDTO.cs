using System;
using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract]
  public class PartidaDTO {
    [DataMember]
    public int IDPartida { get; set; }

    [DataMember]
    public int IDAnfitrion { get; set; }

    [DataMember]
    public string NombreAnfitrion { get; set; }

    [DataMember]
    public int? IDAdivinador { get; set; }

    [DataMember]
    public string NombreAdivinador { get; set; }

    [DataMember]
    public int IDPalabra { get; set; }

    [DataMember]
    public string PalabraRevelada { get; set; }

    [DataMember]
    public string Estado { get; set; }

    [DataMember]
    public string Resultado { get; set; }

    [DataMember]
    public string Privacidad { get; set; }

    [DataMember]
    public int Turno { get; set; }

    [DataMember]
    public string CodigoAcceso { get; set; }

    [DataMember]
    public DateTime FechaDeJuego { get; set; }
  }
}
