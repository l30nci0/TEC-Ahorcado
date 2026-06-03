using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LetterClashServer.DataAccess.Context;

namespace LetterClashServer.DataAccess.Repositories {
  public class JugadorRepository {
    public List<Partida> ObtenerHistorialPartidas(int jugadorID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Partidas
                      .AsNoTracking()
                      .Include(p => p.Jugador)
                      .Include(p => p.Jugador1)
                      .Include(p => p.Palabra)
                      .Where(p => p.IDAnfitrion == jugadorID || p.IDAdivinador == jugadorID)
                      .OrderByDescending(p => p.FechaDeJuego)
                      .ToList();
      }
    }

    public bool ExisteJugador(int jugadorID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .Any(j => j.IDJugador == jugadorID);
      }
    } 

    public List<Jugador> ObtenerMarcadores() {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .OrderByDescending(j => j.Puntuacion)
                      .ToList();
      }
    }
  }
}
