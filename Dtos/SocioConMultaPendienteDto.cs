namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class SocioConMultaPendienteDto
    {
        public int NroSocio { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoSocio { get; set; } = string.Empty;
        public int CantidadMultasPendientes { get; set; }
        public decimal MontoTotalPendiente { get; set; }
    }
}
