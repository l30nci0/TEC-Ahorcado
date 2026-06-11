using System;
using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract]
  public class JugadorDTO {
    [DataMember]
    public int IDJugador { get; set; }

    [DataMember]
    public string Nombre { get; set; }

    [DataMember]
    public string NombreDeUsuario { get; set; }

    [DataMember]
    public string Correo { get; set; }

    [DataMember]
    public string Telefono { get; set; }

    [DataMember]
    public int Puntuacion { get; set; }

    [DataMember]
    public byte[] Avatar { get; set; }

    [DataMember]
    public string IdiomaPreferido { get; set; }

    [DataMember]
    public DateTime FechaDeNacimiento { get; set; }
  }
}
