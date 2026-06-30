using Microsoft.EntityFrameworkCore;
using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Dtos;
using Biblioteca_Mastrangelo_Portela.Models;
using Biblioteca_Mastrangelo_Portela.Common;

namespace Biblioteca_Mastrangelo_Portela.Services
{
    public class PrestamoService
    {
        private readonly BibliotecaDbContext _context;
        private readonly LibroService _libroService;
        private readonly MultaService _multaService;
        private readonly ReservaService _reservaService;

        public PrestamoService(BibliotecaDbContext context, LibroService libroService, MultaService multaService, ReservaService reservaService)
        {
            _context = context;
            _libroService = libroService;
            _multaService = multaService;
            _reservaService = reservaService;
        }

        public Prestamo RegistrarPrestamo(int nroSocio, string isbn)
        {
            var socio = _context.Socios
                .Include(s => s.TipoSocio)
                .FirstOrDefault(s => s.NroSocio == nroSocio);

            if (socio == null)
                throw new ReglaNegocioException("El socio no existe.");

            if (!socio.Activo)
                throw new ReglaNegocioException("El socio está inactivo y no puede retirar libros.");

            if (_multaService.PuedeSolicitarPrestamos(nroSocio) == false)
                throw new ReglaNegocioException("El socio tiene multas pendientes y no puede retirar libros.");

            var libro = _context.Libros.FirstOrDefault(l => l.ISBN == isbn);
            if (libro == null)
                throw new ReglaNegocioException("El libro no existe.");

            int prestamosActivos = _context.Prestamos
                .Count(p => p.NroSocio == nroSocio && p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoActivo);

            if (prestamosActivos >= socio.TipoSocio.MaximoLibrosSimultaneos)
                throw new ReglaNegocioException($"El socio ha alcanzado el límite de {socio.TipoSocio.MaximoLibrosSimultaneos} préstamos simultáneos.");

            int disponibles = _libroService.CalcularCopiasDisponibles(isbn);
            if (disponibles <= 0)
                throw new ReglaNegocioException("No hay copias disponibles.");

            var estadoActivo = _context.EstadosPrestamo.First(ep => ep.Nombre == CodigosDominio.PrestamoActivo);

            var hoy = DateTime.Today;
            var prestamo = new Prestamo
            {
                NroSocio = nroSocio,
                ISBN = isbn,
                FechaPrestamo = hoy,
                FechaVencimiento = hoy.AddDays(socio.TipoSocio.DiasPrestamo),
                EstadoPrestamoId = estadoActivo.Id,
                Renovado = false
            };

            _context.Prestamos.Add(prestamo);
            _context.SaveChanges();

            return prestamo;
        }

        public ResultadoDevolucionDto RegistrarDevolucion(int prestamoId)
        {
            var prestamo = _context.Prestamos
                .Include(p => p.Socio)
                .ThenInclude(s => s!.TipoSocio)
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.Multa)
                .FirstOrDefault(p => p.Id == prestamoId);

            if (prestamo == null)
                throw new ReglaNegocioException("El préstamo no existe.");

            if (prestamo.EstadoPrestamo.Nombre == CodigosDominio.PrestamoDevuelto)
                throw new ReglaNegocioException("El préstamo ya fue devuelto.");

            var hoy = DateTime.Today;
            prestamo.FechaDevolucion = hoy;

            var estadoDevuelto = _context.EstadosPrestamo.First(ep => ep.Nombre == CodigosDominio.PrestamoDevuelto);
            prestamo.EstadoPrestamoId = estadoDevuelto.Id;

            Multa? multaGenerada = null;
            if (hoy > prestamo.FechaVencimiento)
            {
                int diasDemora = (hoy - prestamo.FechaVencimiento).Days;
                multaGenerada = _multaService.GenerarMulta(prestamo, diasDemora);
            }

            _context.SaveChanges();

            Reserva? reservaCumplida = null;
            if (_reservaService.TieneReservasPendientes(prestamo.ISBN))
            {
                var reserva = _reservaService.BuscarReservaPendienteMasAntigua(prestamo.ISBN);
                if (reserva != null)
                {
                    reservaCumplida = _reservaService.MarcarReservaCumplida(reserva.Id);
                }
            }

            return new ResultadoDevolucionDto
            {
                Prestamo = prestamo,
                MultaGenerada = multaGenerada,
                ReservaCumplida = reservaCumplida
            };
        }

        public void ActualizarPrestamosVencidos()
        {
            var hoy = DateTime.Today;
            var estadoActivo = _context.EstadosPrestamo.First(ep => ep.Nombre == CodigosDominio.PrestamoActivo);
            var estadoVencido = _context.EstadosPrestamo.First(ep => ep.Nombre == CodigosDominio.PrestamoVencido);

            var vencidos = _context.Prestamos
                .Where(p => p.EstadoPrestamoId == estadoActivo.Id && p.FechaVencimiento < hoy)
                .ToList();

            foreach (var prestamo in vencidos)
            {
                prestamo.EstadoPrestamoId = estadoVencido.Id;
            }

            _context.SaveChanges();
        }

        public Prestamo RenovarPrestamo(int prestamoId)
        {
            var prestamo = _context.Prestamos
                .Include(p => p.Socio)
                .ThenInclude(s => s!.TipoSocio)
                .Include(p => p.EstadoPrestamo)
                .FirstOrDefault(p => p.Id == prestamoId);

            if (prestamo == null)
                throw new ReglaNegocioException("El préstamo no existe.");

            if (prestamo.EstadoPrestamo.Nombre != CodigosDominio.PrestamoActivo)
                throw new ReglaNegocioException("Solo se pueden renovar préstamos activos.");

            if (prestamo.Renovado)
                throw new ReglaNegocioException("El préstamo ya fue renovado anteriormente.");

            if (prestamo.FechaVencimiento < DateTime.Today)
                throw new ReglaNegocioException("No se puede renovar un préstamo vencido.");

            bool tieneReservasPendientesDeOtros = _context.Reservas.Any(r =>
                r.ISBN == prestamo.ISBN &&
                r.EstadoReserva.Nombre == CodigosDominio.ReservaPendiente &&
                r.NroSocio != prestamo.NroSocio);

            if (tieneReservasPendientesDeOtros)
                throw new ReglaNegocioException("No se puede renovar el préstamo porque el libro tiene reservas pendientes de otros socios.");

            prestamo.FechaVencimiento = prestamo.FechaVencimiento.AddDays(prestamo.Socio.TipoSocio.DiasPrestamo);
            prestamo.Renovado = true;
            _context.SaveChanges();

            return prestamo;
        }

        public Prestamo? BuscarPrestamoActivoPorId(int prestamoId)
        {
            return _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .FirstOrDefault(p => p.Id == prestamoId && p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoActivo);
        }

        public List<Prestamo> ObtenerPrestamosActivos()
        {
            return _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .Where(p => p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoActivo)
                .ToList();
        }

        public List<Prestamo> ObtenerPrestamosVencidos()
        {
            return _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .Where(p => p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoVencido)
                .ToList();
        }
    }
}
