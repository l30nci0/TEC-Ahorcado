using System;
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
                      .Include(p => p.Jugador1)
                      .Include(p => p.Palabra)
                      .Where(p => p.Estado == "PENDIENTE" && p.Privacidad == "PÚBLICA")
                      .ToList();
      }
    }

    public virtual bool EliminarPartidaPendienteSinAdivinador(string codigoAcceso, int anfitrionID) {
      using (var context = new LetterClashDBEntities()) {
        var partida = context.Partidas.SingleOrDefault(p => p.CodigoAcceso == codigoAcceso);
        if (partida == null ||
            partida.IDAnfitrion != anfitrionID ||
            partida.Estado != "PENDIENTE" ||
            partida.IDAdivinador != null) {
          return false;
        }

        context.Partidas.Remove(partida);
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool ExistePartida(int partidaID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Partidas
                      .AsNoTracking()
                      .Any(p => p.IDPartida == partidaID);
      }
    }

    public virtual string CrearPartida(int anfitrionID, int palabraID, string privacidad) {
      using (var context = new LetterClashDBEntities()) {
        string codigo = GenerarCodigoAccesoUnico(context);
        var partida = new Partida {
          IDAnfitrion = anfitrionID,
          IDPalabra = palabraID,
          Privacidad = privacidad,
          Estado = "PENDIENTE",
          Resultado = "SIN_ADIVINAR",
          Turno = 1,
          CodigoAcceso = codigo,
          FechaDeJuego = DateTime.Now
        };
        context.Partidas.Add(partida);
        context.SaveChanges();
        return codigo;
      }
    }

    public virtual bool UnirseAPartidaDeLobby(int jugadorID, int partidaID) {
      using (var context = new LetterClashDBEntities()) {
        var partida = context.Partidas.SingleOrDefault(p => p.IDPartida == partidaID);
        if (partida == null || partida.Estado != "PENDIENTE" || partida.IDAdivinador != null) {
          return false;
        }

        if (partida.IDAnfitrion == jugadorID) {
          return false;
        }

        partida.IDAdivinador = jugadorID;
        partida.Estado = "EN_JUEGO";
        context.SaveChanges();
        return true;
      }
    }

    public virtual Partida ObtenerPartidaPorCodigo(string codigoAcceso) {
      using (var context = new LetterClashDBEntities()) {
        return context.Partidas
                      .AsNoTracking()
                      .Include(p => p.Jugador1)
                      .Include(p => p.Palabra)
                      .SingleOrDefault(p => p.CodigoAcceso == codigoAcceso);
      }
    }

    public virtual bool PublicarPartida(string codigoAcceso, int anfitrionID) {
      using (var context = new LetterClashDBEntities()) {
        var partida = context.Partidas.SingleOrDefault(p => p.CodigoAcceso == codigoAcceso);
        if (partida == null || partida.IDAnfitrion != anfitrionID || partida.Privacidad != "PRIVADA" || partida.Estado != "PENDIENTE") {
          return false;
        }

        partida.Privacidad = "PÚBLICA";
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool ConcluirPartida(int partidaID, string resultado) {
      return ConcluirPartida(partidaID, resultado, 0);
    }

    public virtual bool ConcluirPartida(int partidaID, string resultado, int erroresCometidos) {
      using (var context = new LetterClashDBEntities()) {
        var partida = context.Partidas.SingleOrDefault(p => p.IDPartida == partidaID);
        if (partida == null) {
          return false;
        }

        partida.Estado = "CONCLUIDA";
        partida.Resultado = resultado;
        partida.Turno = ConvertirErroresATurno(resultado, erroresCometidos);
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool ConcluirPartidaPorAbandono(int partidaID, int jugadorID, bool penalizado) {
      using (var context = new LetterClashDBEntities()) {
        var partida = context.Partidas.SingleOrDefault(p => p.IDPartida == partidaID);
        if (partida == null) {
          return false;
        }

        partida.Estado = "CONCLUIDA";
        partida.Resultado = "ABANDONADA";
        partida.Turno = ObtenerCodigoAbandono(partida, jugadorID, penalizado);
        context.SaveChanges();
        return true;
      }
    }

    private int ConvertirErroresATurno(string resultado, int erroresCometidos) {
      if (resultado == "SIN_ADIVINAR") {
        return 6;
      }

      if (erroresCometidos < 0) {
        return 1;
      }

      if (erroresCometidos > 5) {
        return 6;
      }

      return erroresCometidos + 1;
    }

    private int ObtenerCodigoAbandono(Partida partida, int jugadorID, bool penalizado) {
      bool abandonoAnfitrion = partida.IDAnfitrion == jugadorID;

      if (penalizado) {
        return abandonoAnfitrion ? 1 : 2;
      }

      return abandonoAnfitrion ? 3 : 4;
    }

    private string GenerarCodigoAccesoUnico(LetterClashDBEntities context) {
      var random = new Random();
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      string codigo;
      do {
        codigo = new string(Enumerable.Repeat(chars, 6)
                                      .Select(s => s[random.Next(s.Length)]).ToArray());
      } while (context.Partidas.Any(p => p.CodigoAcceso == codigo));
      return codigo;
    }
  }
}
