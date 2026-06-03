using System;
using System.Collections.Generic;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.Services {
  public class JugadorService : IJugadorService {
    public bool ActualizarPerfil(JugadorDTO jugadorDTO) {
      return false;
    }

    public List<PartidaDTO> ConsultarHistorial(int jugadorID) {
      return new List<PartidaDTO>();
    }

    public List<JugadorDTO> ConsultarMarcadores() {
      return new List<JugadorDTO>();
    }

    public bool CambiarIdiomaPreferido(int jugadorID, string idioma) {
      return false;
    }
  }
}
