namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class PrestamoVencidoDto
    {
        public int PrestamoId { get; set; }
        public int NroSocio { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string TituloLibro { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public int DiasDemora { get; set; }
    }
}
