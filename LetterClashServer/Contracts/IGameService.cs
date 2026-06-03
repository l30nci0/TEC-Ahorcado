using System.ServiceModel;

namespace LetterClashServer.Contracts {
  [ServiceContract(CallbackContract = typeof(IGameServiceCallback))]
  public interface IGameService {
    [OperationContract]
    void ConectarJuego(int jugadorID, string codigoAcceso);

    [OperationContract]
    void EscribirLetra(string codigoAcceso, int jugadorID, char letra);

    [OperationContract]
    void AbandonarPartida(string codigoAcceso, int jugadorID);

    [OperationContract]
    void VerPista(string codigoAcceso, int jugadorID);

    [OperationContract]
    void EnviarMensaje(string codigoAcceso, int jugadorID, string mensaje);
  }
}
