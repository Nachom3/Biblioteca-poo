using Microsoft.EntityFrameworkCore;
using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Models;
using Biblioteca_Mastrangelo_Portela.Common;

namespace Biblioteca_Mastrangelo_Portela.Services
{
    public class MultaService
    {
        private readonly BibliotecaDbContext _context;

        public MultaService(BibliotecaDbContext context)
        {
            _context = context;
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

        public decimal CalcularTotalPendiente(int nroSocio)
        {
            return _context.Multas
                .Where(m => m.Prestamo.NroSocio == nroSocio && !m.Abonada)
                .Sum(m => m.Monto);
        }

        public void RegistrarPago(int multaId)
        {
            var multa = _context.Multas.FirstOrDefault(m => m.Id == multaId);
            if (multa == null)
                throw new ReglaNegocioException("La multa no existe.");

            if (multa.Abonada)
                throw new ReglaNegocioException("La multa ya fue abonada.");

            multa.Abonada = true;
            multa.FechaPago = DateTime.Today;
            _context.SaveChanges();
        }

        public bool PuedeSolicitarPrestamos(int nroSocio)
        {
            return !_context.Multas.Any(m => m.Prestamo.NroSocio == nroSocio && !m.Abonada);
        }

        public Multa GenerarMulta(Prestamo prestamo, int diasDemora)
        {
            var multaExistente = _context.Multas.FirstOrDefault(m => m.PrestamoId == prestamo.Id);
            if (multaExistente != null)
                return multaExistente;

            var monto = diasDemora * prestamo.Socio.TipoSocio.MultaPorDia;

            var multa = new Multa
            {
                PrestamoId = prestamo.Id,
                DiasDemora = diasDemora,
                Monto = monto,
                FechaGeneracion = DateTime.Today,
                Abonada = false
            };

            _context.Multas.Add(multa);
            _context.SaveChanges();

            return multa;
        }
    }
}
