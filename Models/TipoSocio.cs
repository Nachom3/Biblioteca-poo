namespace Biblioteca_Mastrangelo_Portela.Models
{
    public class TipoSocio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int MaximoLibrosSimultaneos { get; set; }
        public int DiasPrestamo { get; set; }
        public decimal MultaPorDia { get; set; }

        public ICollection<Socio> Socios { get; set; } = new List<Socio>();
    }
}
