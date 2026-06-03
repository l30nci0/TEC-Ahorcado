using System.ServiceModel;
using LetterClashServer.Contracts;

namespace LetterClashServer.Services {
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class GameService : IGameService {
    public void ConectarJuego(int jugadorID, string codigoAcceso) {
      var callback = OperationContext.Current.GetCallbackChannel<IGameServiceCallback>();
    }

    public void EscribirLetra(string codigoAcceso, int jugadorID, char letra) {
    }

    public void AbandonarPartida(string codigoAcceso, int jugadorID) {
    }

    public void VerPista(string codigoAcceso, int jugadorID) {
    }

    public void EnviarMensaje(string codigoAcceso, int jugadorID, string mensaje) {
    }
  }
}
