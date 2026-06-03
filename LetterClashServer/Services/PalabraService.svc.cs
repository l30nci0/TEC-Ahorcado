using System.Collections.Generic;
using System.Linq;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;
using LetterClashServer.DataAccess.Repositories;

namespace LetterClashServer.Services {
  public class PalabraService : IPalabraService {
    private readonly PalabraRepository palabraRepository = new PalabraRepository();

    public List<PalabraDTO> ObtenerPalabrasPorIdioma(string idioma) {
      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        throw new System.ArgumentException("El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.");
      }

      var palabras = palabraRepository.ObtenerPalabrasPorIdioma(idioma);
       
      // Nota: En C# la propiedad de la palabra se llama Palabra1 debido a la desambiguación de EF
      return palabras.Select(p => new PalabraDTO {
        IDPalabra = p.IDPalabra,
        PalabraTexto = p.Palabra1,
        Descripcion = p.Descripcion,
        Idioma = p.Idioma
      }).ToList();
    }
  }
}
