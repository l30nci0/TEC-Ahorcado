using System;

using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Services {
  public class GameCallbackHandler : IGameServiceCallback {
    public event Action<JugadorPublicoDTO> JugadorSeUnio;
    public event Action<char, bool, string, int> LetraPropuesta;
    public event Action<string, int> PartidaFinalizada;
    public event Action<string, string> MensajeRecibido;
    public event Action<string> OponenteAbandono;
    public event Action<ServiceFault> ErrorOcurrido;

    public void OnJugadorSeUnio(JugadorPublicoDTO jugadorDTO) {
      JugadorSeUnio?.Invoke(jugadorDTO);
    }

    public void OnLetraPropuesta(char letra, bool esCorrecta, string palabraRevelada, int vidaRestante) {
      LetraPropuesta?.Invoke(letra, esCorrecta, palabraRevelada, vidaRestante);
    }

    public void OnPartidaFinalizada(string ganador, int puntuacionObtenida) {
      PartidaFinalizada?.Invoke(ganador, puntuacionObtenida);
    }

    public void OnMensajeRecibido(string emisor, string mensaje) {
      OnMensajeRecibido(emisor, mensaje, false); // Standard handler invocation
    }

    private void OnMensajeRecibido(string emisor, string mensaje, bool isSystem) {
      MensajeRecibido?.Invoke(emisor, mensaje);
    }

    public void OnOponenteAbandono(string oponenteNombre) {
      OponenteAbandono?.Invoke(oponenteNombre);
    }

    public void OnErrorOcurrido(ServiceFault fault) {
      ErrorOcurrido?.Invoke(fault);
    }
  }
}
