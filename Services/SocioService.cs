using Microsoft.EntityFrameworkCore;
using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Dtos;
using Biblioteca_Mastrangelo_Portela.Models;
using Biblioteca_Mastrangelo_Portela.Common;

namespace Biblioteca_Mastrangelo_Portela.Services
{
    public class SocioService
    {
        private readonly BibliotecaDbContext _context;

        public SocioService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public Socio? BuscarPorNumero(int nroSocio)
        {
            return _context.Socios
                .Include(s => s.TipoSocio)
                .FirstOrDefault(s => s.NroSocio == nroSocio);
        }

        public bool EstaActivo(int nroSocio)
        {
            var socio = BuscarPorNumero(nroSocio);
            return socio != null && socio.Activo;
        }

        public DetalleSocioDto ObtenerDetalle(int nroSocio)
        {
            var socio = BuscarPorNumero(nroSocio);
            if (socio == null)
                throw new ReglaNegocioException("El socio no existe.");

            var multasPendientes = ConsultarMultasPendientes(nroSocio);

            return new DetalleSocioDto
            {
                Socio = socio,
                TipoSocio = socio.TipoSocio.Nombre,
                PrestamosActivos = ObtenerPrestamosActivos(nroSocio),
                HistorialDevoluciones = ObtenerHistorialDevoluciones(nroSocio),
                Reservas = ObtenerReservas(nroSocio),
                MultasPendientes = multasPendientes,
                MontoTotalAdeudado = multasPendientes.Sum(m => m.Monto)
            };
        }

        public List<Prestamo> ObtenerPrestamosActivos(int nroSocio)
        {
            return _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .Where(p => p.NroSocio == nroSocio && p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoActivo)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToList();
        }

        public List<Prestamo> ObtenerHistorialDevoluciones(int nroSocio)
        {
            return _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.EstadoPrestamo)
                .Where(p => p.NroSocio == nroSocio && p.EstadoPrestamo.Nombre == CodigosDominio.PrestamoDevuelto)
                .OrderByDescending(p => p.FechaDevolucion)
                .ToList();
        }

        public List<Reserva> ObtenerReservas(int nroSocio)
        {
            return _context.Reservas
                .Include(r => r.Libro)
                .Include(r => r.EstadoReserva)
                .Where(r => r.NroSocio == nroSocio)
                .OrderByDescending(r => r.FechaReserva)
                .ToList();
        }

        public List<Multa> ConsultarMultasPendientes(int nroSocio)
        {
            return _context.Multas
                .Include(m => m.Prestamo)
                .ThenInclude(p => p!.Libro)
                .Where(m => m.Prestamo.NroSocio == nroSocio && !m.Abonada)
                .OrderByDescending(m => m.FechaGeneracion)
                .ToList();
        }

        public bool TieneMultasPendientes(int nroSocio)
        {
            return _context.Multas.Any(m => m.Prestamo.NroSocio == nroSocio && !m.Abonada);
        }
    }
}
