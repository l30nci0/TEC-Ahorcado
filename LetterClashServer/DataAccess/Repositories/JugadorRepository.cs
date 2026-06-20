using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using LetterClashServer.DataAccess.Context;
using LetterClashServer.Domain.Models;

namespace LetterClashServer.DataAccess.Repositories {
  public class JugadorRepository {
    public virtual List<Partida> ObtenerHistorialPartidas(int jugadorID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Partidas
                      .AsNoTracking()
                      .Include(p => p.Jugador)
                      .Include(p => p.Jugador1)
                      .Include(p => p.Palabra)
                      .Where(p => p.Estado == "CONCLUIDA" &&
                                  p.IDAdivinador != null &&
                                  (p.IDAnfitrion == jugadorID || p.IDAdivinador == jugadorID))
                      .OrderByDescending(p => p.FechaDeJuego)
                      .ToList();
      }
    }

    public virtual bool ExisteJugador(int jugadorID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .Any(j => j.IDJugador == jugadorID);
      }
    }

    public virtual List<Jugador> ObtenerMarcadores() {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .OrderByDescending(j => j.Puntuacion)
                      .ToList();
      }
    }

    public virtual Jugador ObtenerJugadorPorID(int jugadorID) {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .SingleOrDefault(j => j.IDJugador == jugadorID);
      }
    }

    public virtual bool ActualizarPerfil(JugadorDTO jugadorDTO) {
      using (var context = new LetterClashDBEntities()) {
        var jugador = context.Jugadores.SingleOrDefault(j => j.IDJugador == jugadorDTO.IDJugador);
        if (jugador == null) {
          return false;
        }
        jugador.Nombre = jugadorDTO.Nombre;
        jugador.Telefono = jugadorDTO.Telefono;
        jugador.Avatar = jugadorDTO.Avatar;
        jugador.FechaDeNacimiento = jugadorDTO.FechaDeNacimiento;
        jugador.IdiomaPreferido = jugadorDTO.IdiomaPreferido;
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool ActualizarIdioma(int jugadorID, string idioma) {
      using (var context = new LetterClashDBEntities()) {
        var jugador = context.Jugadores.SingleOrDefault(j => j.IDJugador == jugadorID);
        if (jugador == null) {
          return false;
        }
        jugador.IdiomaPreferido = idioma;
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool ActualizarContrasena(int jugadorID, string contrasenaEncriptada) {
      using (var context = new LetterClashDBEntities()) {
        var jugador = context.Jugadores.SingleOrDefault(j => j.IDJugador == jugadorID);
        if (jugador == null) {
          return false;
        }
        jugador.Contrasena = contrasenaEncriptada;
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool ExisteCorreo(string correo) {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .Any(j => j.Correo.ToLower() == correo.ToLower());
      }
    }

    public virtual bool ExisteNombreDeUsuario(string nombreUsuario) {
      using (var context = new LetterClashDBEntities()) {
        return context.Jugadores
                      .AsNoTracking()
                      .Any(j => j.NombreDeUsuario.ToLower() == nombreUsuario.ToLower());
      }
    }

    public virtual Jugador ObtenerJugadorPorCredenciales(string correoONombreUsuario) {
      using (var context = new LetterClashDBEntities()) {
        string credencialLower = correoONombreUsuario.ToLower();
        return context.Jugadores
                      .AsNoTracking()
                      .SingleOrDefault(j => j.Correo.ToLower() == credencialLower || j.NombreDeUsuario.ToLower() == credencialLower);
      }
    }

    public virtual bool IncrementarPuntuacion(int jugadorID, int puntos) {
      using (var context = new LetterClashDBEntities()) {
        var jugador = context.Jugadores.SingleOrDefault(j => j.IDJugador == jugadorID);
        if (jugador == null) {
          return false;
        }

        jugador.Puntuacion = jugador.Puntuacion + puntos;
        context.SaveChanges();
        return true;
      }
    }

    public virtual bool RegistrarJugador(Jugador jugador) {
      using (var context = new LetterClashDBEntities()) {
        context.Jugadores.Add(jugador);
        context.SaveChanges();
        return true;
      }
    }
  }
}
