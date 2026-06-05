using System.Runtime.Serialization;

namespace LetterClashServer.Domain.Models {
  [DataContract(Name = "ServiceResultOf{0}")]
  public class ServiceResult<T> {
    [DataMember]
    public bool IsSuccess { get; set; }

    [DataMember]
    public T Value { get; set; }

    [DataMember]
    public ServiceFault Error { get; set; }

    public static ServiceResult<T> Success(T value) {
      return new ServiceResult<T> {
        IsSuccess = true,
        Value = value
      };
    }

    public static ServiceResult<T> Failure(CodigoError codigoError, string mensaje, string detalle = null) {
      return new ServiceResult<T> {
        IsSuccess = false,
        Error = new ServiceFault {
          CodigoError = codigoError,
          Mensaje = mensaje,
          Detalle = detalle
        }
      };
    }
  }
}
