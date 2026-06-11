using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

using LetterClashServer.Contracts;
using LetterClashServer.DataAccess.Repositories;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Services {
  public class PalabraService : IPalabraService {
    private readonly PalabraRepository palabraRepository;

    public PalabraService() : this(new PalabraRepository()) { }

    public PalabraService(PalabraRepository repository) {
      this.palabraRepository = repository;
    }

    public ServiceResult<List<PalabraDTO>> ObtenerPalabrasPorIdioma(string idioma) {
      // Validamos que el idioma sea uno de los definidos en el sistema
      if (string.IsNullOrEmpty(idioma) || !Idiomas.EsValido(idioma)) {
        return ServiceResult<List<PalabraDTO>>.Failure(
          CodigoError.PARAMETRO_INVALIDO,
          "El idioma proporcionado no es válido. Debe ser 'ESPAÑOL' o 'INGLÉS'.",
          $"idioma = '{idioma}'"
        );
      }

      try {
        var palabras = palabraRepository.ObtenerPalabrasPorIdioma(idioma);

        // Nota: En C# la propiedad de la palabra se llama Palabra1 debido a la desambiguación de EF
        var dtos = palabras.Select(p => new PalabraDTO {
          IDPalabra = p.IDPalabra,
          PalabraTexto = p.Palabra1,
          Descripcion = p.Descripcion,
          Idioma = p.Idioma
        }).ToList();

        return ServiceResult<List<PalabraDTO>>.Success(dtos);
      } catch (System.Exception ex) {
        return ServiceResult<List<PalabraDTO>>.Failure(
          CodigoError.ERROR_INTERNO,
          "No ha sido posible obtener las palabras debido a un error en el servidor. Intente de nuevo más tarde.",
          ex.Message
        );
      }
    }
  }
}
