using Biblioteca_Mastrangelo_Portela.Models;

namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class DetalleSocioDto
    {
        public Socio Socio { get; set; } = null!;
        public string TipoSocio { get; set; } = string.Empty;
        public List<Prestamo> PrestamosActivos { get; set; } = new List<Prestamo>();
        public List<Prestamo> HistorialDevoluciones { get; set; } = new List<Prestamo>();
        public List<Reserva> Reservas { get; set; } = new List<Reserva>();
        public List<Multa> MultasPendientes { get; set; } = new List<Multa>();
        public decimal MontoTotalAdeudado { get; set; }
    }
}
