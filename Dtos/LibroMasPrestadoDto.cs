namespace Biblioteca_Mastrangelo_Portela.Dtos
{
    public class LibroMasPrestadoDto
    {
        public string ISBN { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int CantidadPrestamos { get; set; }
    }
}
