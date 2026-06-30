using Biblioteca_Mastrangelo_Portela.Models;

namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class ResultadoDevolucionDto
    {
        public Prestamo Prestamo { get; set; } = null!;
        public Multa? MultaGenerada { get; set; }
        public Reserva? ReservaCumplida { get; set; }
    }
}
