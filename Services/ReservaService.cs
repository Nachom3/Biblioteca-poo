using Microsoft.EntityFrameworkCore;
using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Models;
using Biblioteca_Mastrangelo_Portela.Common;

namespace Biblioteca_Mastrangelo_Portela.Services
{
    public class ReservaService
    {
        private readonly BibliotecaDbContext _context;

        public ReservaService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public Reserva RegistrarReserva(int nroSocio, string isbn)
        {
            var socio = _context.Socios
                .Include(s => s.TipoSocio)
                .FirstOrDefault(s => s.NroSocio == nroSocio);

            if (socio == null)
                throw new ReglaNegocioException("El socio no existe.");

            if (!socio.Activo)
                throw new ReglaNegocioException("El socio está inactivo y no puede realizar reservas.");

            var libro = _context.Libros.FirstOrDefault(l => l.ISBN == isbn);
            if (libro == null)
                throw new ReglaNegocioException("El libro no existe.");

            bool tieneReservaActiva = _context.Reservas.Any(r => r.NroSocio == nroSocio && r.ISBN == isbn && r.EstadoReserva.Nombre == CodigosDominio.ReservaPendiente);
            if (tieneReservaActiva)
                throw new ReglaNegocioException("El socio ya tiene una reserva activa para este libro.");

            var estadoPendiente = _context.EstadosReserva.First(er => er.Nombre == CodigosDominio.ReservaPendiente);

            var reserva = new Reserva
            {
                NroSocio = nroSocio,
                ISBN = isbn,
                FechaReserva = DateTime.Today,
                EstadoReservaId = estadoPendiente.Id
            };

            _context.Reservas.Add(reserva);
            _context.SaveChanges();

            return reserva;
        }

        public Reserva? BuscarReservaPendienteMasAntigua(string isbn)
        {
            return _context.Reservas
                .Include(r => r.Socio)
                .Where(r => r.ISBN == isbn && r.EstadoReserva.Nombre == CodigosDominio.ReservaPendiente)
                .OrderBy(r => r.FechaReserva)
                .FirstOrDefault();
        }

        public Reserva MarcarReservaCumplida(int reservaId)
        {
            var reserva = _context.Reservas
                .Include(r => r.EstadoReserva)
                .FirstOrDefault(r => r.Id == reservaId);

            if (reserva == null)
                throw new ReglaNegocioException("La reserva no existe.");

            if (reserva.EstadoReserva.Nombre != CodigosDominio.ReservaPendiente)
                throw new ReglaNegocioException("Solo se pueden cumplir reservas pendientes.");

            var estadoCumplida = _context.EstadosReserva.First(er => er.Nombre == CodigosDominio.ReservaCumplida);
            reserva.EstadoReservaId = estadoCumplida.Id;
            _context.SaveChanges();

            return reserva;
        }

        public Reserva CancelarReserva(int reservaId)
        {
            var reserva = _context.Reservas
                .Include(r => r.EstadoReserva)
                .FirstOrDefault(r => r.Id == reservaId);

            if (reserva == null)
                throw new ReglaNegocioException("La reserva no existe.");

            if (reserva.EstadoReserva.Nombre != CodigosDominio.ReservaPendiente)
                throw new ReglaNegocioException("Solo se pueden cancelar reservas pendientes.");

            var estadoCancelada = _context.EstadosReserva.First(er => er.Nombre == CodigosDominio.ReservaCancelada);
            reserva.EstadoReservaId = estadoCancelada.Id;
            _context.SaveChanges();

            return reserva;
        }

        public bool TieneReservasPendientes(string isbn)
        {
            return _context.Reservas.Any(r => r.ISBN == isbn && r.EstadoReserva.Nombre == CodigosDominio.ReservaPendiente);
        }

        public List<Reserva> ConsultarReservasPendientes(string isbn)
        {
            return _context.Reservas
                .Include(r => r.Socio)
                .Where(r => r.ISBN == isbn && r.EstadoReserva.Nombre == CodigosDominio.ReservaPendiente)
                .OrderBy(r => r.FechaReserva)
                .ToList();
        }
    }
}
