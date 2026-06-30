namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class DisponibilidadLibroDto
    {
        public string ISBN { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public int CantidadTotalCopias { get; set; }
        public int CantidadPrestada { get; set; }
        public int CantidadDisponible { get; set; }
        public int CantidadReservasPendientes { get; set; }
    }
}
