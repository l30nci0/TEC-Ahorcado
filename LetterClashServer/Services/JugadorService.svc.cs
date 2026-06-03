using System;
using System.Collections.Generic;
using System.Linq;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;
using LetterClashServer.DataAccess.Repositories;

namespace LetterClashServer.Services {
  public class JugadorService : IJugadorService {
    private readonly JugadorRepository jugadorRepository = new JugadorRepository();

    public bool ActualizarPerfil(JugadorDTO jugadorDTO) {
      return false;
    }

    public List<PartidaDTO> ConsultarHistorial(int jugadorID) {
      if (jugadorID <= 0) {
        throw new ArgumentException("El ID del jugador debe ser mayor a cero.");
      }

      var partidas = jugadorRepository.ObtenerHistorialPartidas(jugadorID);

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

    public List<JugadorPublicoDTO> ConsultarMarcadores() {
      var jugadores = jugadorRepository.ObtenerMarcadores();

      return jugadores.Select(j => new JugadorPublicoDTO {
        IDJugador = j.IDJugador,
        NombreDeUsuario = j.NombreDeUsuario,
        Puntuacion = j.Puntuacion,
        Avatar = j.Avatar
      }).ToList();
    }

    public bool CambiarIdiomaPreferido(int jugadorID, string idioma) {
      return false;
    }
  }
}
