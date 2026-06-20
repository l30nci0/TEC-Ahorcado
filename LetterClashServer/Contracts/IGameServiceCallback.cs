using System.ServiceModel;

using LetterClashServer.Domain.Models;

namespace LetterClashServer.Contracts {
  public interface IGameServiceCallback {
    [OperationContract(IsOneWay = true)]
    void OnJugadorSeUnio(JugadorPublicoDTO jugadorDTO);

    [OperationContract(IsOneWay = true)]
    void OnLetraPropuesta(char letra, bool esCorrecta, string palabraRevelada, int vidaRestante);

    [OperationContract(IsOneWay = true)]
    void OnPartidaFinalizada(string ganador, int puntuacionObtenida);

    [OperationContract(IsOneWay = true)]
    void OnMensajeRecibido(string emisor, string mensaje);

    [OperationContract(IsOneWay = true)]
    void OnOponenteAbandono(string oponenteNombre);

    [OperationContract(IsOneWay = true)]
    void OnOponenteDesconectado(string oponenteNombre);

    [OperationContract(IsOneWay = true)]
    void OnErrorOcurrido(ServiceFault fault);
  }
}
