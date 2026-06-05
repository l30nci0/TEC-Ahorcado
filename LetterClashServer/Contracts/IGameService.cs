using System.ServiceModel;

namespace LetterClashServer.Contracts {
  [ServiceContract(CallbackContract = typeof(IGameServiceCallback))]
  public interface IGameService {
    [OperationContract(IsOneWay = true)]
    void ConectarJuego(int jugadorID, string codigoAcceso);

    [OperationContract(IsOneWay = true)]
    void EscribirLetra(string codigoAcceso, int jugadorID, char letra);

    [OperationContract(IsOneWay = true)]
    void AbandonarPartida(string codigoAcceso, int jugadorID);

    [OperationContract(IsOneWay = true)]
    void VerPista(string codigoAcceso, int jugadorID);

    [OperationContract(IsOneWay = true)]
    void EnviarMensaje(string codigoAcceso, int jugadorID, string mensaje);
  }
}
