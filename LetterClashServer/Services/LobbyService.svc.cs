using System;
using System.Collections.Generic;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Services {
  public class LobbyService : ILobbyService {
    public List<PartidaDTO> ObtenerPartidasLobby() {
      return new List<PartidaDTO>();
    }

    public string CrearPartida(int anfitrionID, int palabraID, string privacidad, string idioma) {
      return string.Empty;
    }

    public bool UnirseAPartidaDeLobby(int jugadorID, int partidaID) {
      return false;
    }

    public PartidaDTO UnirseAPartidaPrivada(int jugadorID, string codigoAcceso) {
      return null;
    }
  }
}
