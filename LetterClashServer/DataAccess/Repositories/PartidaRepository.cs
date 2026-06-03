using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LetterClashServer.DataAccess.Context;

namespace LetterClashServer.DataAccess.Repositories {
  public class PartidaRepository {
    public virtual List<Partida> ObtenerPartidasDisponibles() {
      using (var context = new LetterClashDBEntities()) {
        return context.Partidas
                      .AsNoTracking()
                      .Include(p => p.Jugador)
                      .Include(p => p.Palabra)
                      .Where(p => p.Estado == "PENDIENTE" && p.Privacidad == "PÚBLICA")
                      .ToList();
      }
    }
  }
}