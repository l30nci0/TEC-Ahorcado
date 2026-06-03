using System;
using System.Collections.Generic;
using System.Linq;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;
using LetterClashServer.DataAccess.Repositories;

namespace LetterClashServer.Services {
  public class LobbyService : ILobbyService {
    private readonly PartidaRepository partidaRepository;

    public LobbyService() : this(new PartidaRepository()) {}

    public LobbyService(PartidaRepository repository) {
      this.partidaRepository = repository;
    }

    public List<PartidaDTO> ObtenerPartidasLobby() {
      var partidas = partidaRepository.ObtenerPartidasDisponibles();

      return partidas.Select(p => new PartidaDTO {
        IDPartida = p.IDPartida,
        IDAnfitrion = p.IDAnfitrion,
        NombreAnfitrion = p.Jugador.NombreDeUsuario,
        IDAdivinador = p.IDAdivinador,
        NombreAdivinador = p.Jugador1 != null ? p.Jugador1.NombreDeUsuario : null,
        IDPalabra = p.IDPalabra,
        PalabraRevelada = p.Palabra != null ? p.Palabra.Palabra1 : null,
        Estado = p.Estado,
        Resultado = p.Resultado,
        Privacidad = p.Privacidad,
        Turno = p.Turno,
        CodigoAcceso = p.CodigoAcceso,
        FechaDeJuego = p.FechaDeJuego
      }).ToList();
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
