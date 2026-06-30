using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Dtos;
using Biblioteca_Mastrangelo_Portela.Models;
using Biblioteca_Mastrangelo_Portela.Common;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_Mastrangelo_Portela.Services
{
    public class ReporteService
    {
        private readonly BibliotecaDbContext _context;
        private readonly LibroService _libroService;
        private readonly SocioService _socioService;

        public ReporteService(BibliotecaDbContext context, LibroService libroService, SocioService socioService)
        {
            _context = context;
            _libroService = libroService;
            _socioService = socioService;
        }

        public List<LibroMasPrestadoDto> LibrosMasPrestados(int cantidad = 5)
        {
            return _context.Prestamos
                .GroupBy(p => new { p.ISBN, p.Libro.Titulo, p.Libro.Autor })
                .Select(g => new LibroMasPrestadoDto
                {
                    ISBN = g.Key.ISBN,
                    Titulo = g.Key.Titulo,
                    Autor = g.Key.Autor,
                    CantidadPrestamos = g.Count()
                })
                .OrderByDescending(l => l.CantidadPrestamos)
                .ThenBy(l => l.Titulo)
                .Take(cantidad)
                .ToList();
        }

        public List<SocioConMultaPendienteDto> SociosConMultasPendientes()
        {
            return _context.Multas
                .Where(m => !m.Abonada)
                .Include(m => m.Prestamo)
                .ThenInclude(p => p!.Socio)
                .ThenInclude(s => s!.TipoSocio)
                .AsEnumerable()
                .GroupBy(m => m.Prestamo.Socio)
                .Select(g => new SocioConMultaPendienteDto
                {
                    NroSocio = g.Key.NroSocio,
                    NombreCompleto = $"{g.Key.Nombre} {g.Key.Apellido}",
                    TipoSocio = g.Key.TipoSocio.Nombre,
                    CantidadMultasPendientes = g.Count(),
                    MontoTotalPendiente = g.Sum(m => m.Monto)
                })
                .OrderByDescending(s => s.MontoTotalPendiente)
                .ToList();
        }

        public List<PrestamoVencidoDto> PrestamosVencidos()
        {
            var hoy = DateTime.Today;
            return _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .Where(p => p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoVencido && p.FechaDevolucion == null)
                .AsEnumerable()
                .Select(p => new PrestamoVencidoDto
                {
                    PrestamoId = p.Id,
                    NroSocio = p.NroSocio,
                    NombreSocio = $"{p.Socio.Nombre} {p.Socio.Apellido}",
                    ISBN = p.ISBN,
                    TituloLibro = p.Libro.Titulo,
                    FechaVencimiento = p.FechaVencimiento,
                    DiasDemora = Math.Max(0, (hoy - p.FechaVencimiento).Days)
                })
                .OrderByDescending(p => p.DiasDemora)
                .ToList();
        }

        public DisponibilidadLibroDto DisponibilidadLibro(string criterio)
        {
            var libro = _libroService.BuscarPorISBNoTitulo(criterio);
            if (libro == null)
                throw new ReglaNegocioException("El libro no existe.");

            int prestadas = _libroService.CalcularCopiasPrestadas(libro.ISBN);
            int disponibles = _libroService.CalcularCopiasDisponibles(libro.ISBN);
            int reservasPendientes = _context.Reservas
                .Count(r => r.ISBN == libro.ISBN && r.EstadoReserva.Nombre == CodigosDominio.ReservaPendiente);

            return new DisponibilidadLibroDto
            {
                ISBN = libro.ISBN,
                Titulo = libro.Titulo,
                CantidadTotalCopias = libro.CantidadCopias,
                CantidadPrestada = prestadas,
                CantidadDisponible = disponibles,
                CantidadReservasPendientes = reservasPendientes
            };
        }

        public DetalleSocioDto HistorialSocio(int nroSocio)
        {
            return _socioService.ObtenerDetalle(nroSocio);
        }

        public List<RankingSocioDto> RankingSocios(int cantidad = 10)
        {
            return _context.Prestamos
                .Include(p => p.Socio)
                .ThenInclude(s => s!.TipoSocio)
                .AsEnumerable()
                .GroupBy(p => p.Socio)
                .Select(g => new RankingSocioDto
                {
                    NroSocio = g.Key.NroSocio,
                    NombreCompleto = $"{g.Key.Nombre} {g.Key.Apellido}",
                    TipoSocio = g.Key.TipoSocio.Nombre,
                    CantidadPrestamos = g.Count(),
                    TieneMultasPendientes = _context.Multas.Any(m => m.Prestamo.NroSocio == g.Key.NroSocio && !m.Abonada)
                })
                .OrderByDescending(r => r.CantidadPrestamos)
                .ThenBy(r => r.NombreCompleto)
                .Take(cantidad)
                .ToList()
                .Select((r, index) => new RankingSocioDto
                {
                    Posicion = index + 1,
                    NroSocio = r.NroSocio,
                    NombreCompleto = r.NombreCompleto,
                    TipoSocio = r.TipoSocio,
                    CantidadPrestamos = r.CantidadPrestamos,
                    TieneMultasPendientes = r.TieneMultasPendientes
                })
                .ToList();
        }
    }
}
