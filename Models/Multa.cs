namespace Biblioteca_Mastrangelo_Portela.Models
{
    public class Multa
    {
        public int Id { get; set; }
        public int PrestamoId { get; set; }
        public Prestamo Prestamo { get; set; } = null!;
        public int DiasDemora { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public bool Abonada { get; set; }
        public DateTime? FechaPago { get; set; }
    }
}
