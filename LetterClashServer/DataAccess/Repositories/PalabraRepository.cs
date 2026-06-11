using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using LetterClashServer.DataAccess.Context;

namespace LetterClashServer.DataAccess.Repositories {
  public class PalabraRepository {
    public virtual List<Palabra> ObtenerPalabrasPorIdioma(string idioma) {
      using (var context = new LetterClashDBEntities()) {
        return context.Palabras
                      .AsNoTracking()
                      .Where(p => p.Idioma == idioma)
                      .ToList();
      }
    }

    public virtual bool ExistePalabra(int palabraID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Palabras
                      .AsNoTracking()
                      .Any(p => p.IDPalabra == palabraID);
      }
    }
  }
}
