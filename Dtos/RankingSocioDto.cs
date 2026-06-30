namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class RankingSocioDto
    {
        public int Posicion { get; set; }
        public int NroSocio { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoSocio { get; set; } = string.Empty;
        public int CantidadPrestamos { get; set; }
        public bool TieneMultasPendientes { get; set; }
    }
}
