using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract(Name = "CodigoError")]
  public enum CodigoError {
    [EnumMember]
    PARAMETRO_INVALIDO,

    [EnumMember]
    RECURSO_NO_ENCONTRADO,

    [EnumMember]
    RECURSO_DUPLICADO,

    [EnumMember]
    CREDENCIALES_INVALIDAS,

    [EnumMember]
    OPERACION_INVALIDA,

    [EnumMember]
    ERROR_CONCURRENTE,

    [EnumMember]
    ERROR_INTERNO,

    [EnumMember]
    ACCESO_DENEGADO
  }
}
