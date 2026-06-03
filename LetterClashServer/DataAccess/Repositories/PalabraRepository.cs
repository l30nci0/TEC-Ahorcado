using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LetterClashServer.DataAccess.Context;

namespace LetterClashServer.DataAccess.Repositories {
  public class PalabraRepository {
    public virtual List<Palabra> ObtenerPalabrasPorIdioma(string idioma) {
      using (var context = new LetterClashDBEntities()) {
        // Usamos AsNoTracking() para optimizar consultas de solo lectura
        return context.Palabras
                      .AsNoTracking()
                      .Where(p => p.Idioma == idioma)
                      .ToList();
      }
    }
  }
}
