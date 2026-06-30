using System.Text;
using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Services;
using Biblioteca_Mastrangelo_Portela.ConsoleUI;

namespace Biblioteca_Mastrangelo_Portela
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            using var context = new BibliotecaDbContext();

            var libroService = new LibroService(context);
            var socioService = new SocioService(context);
            var multaService = new MultaService(context);
            var reservaService = new ReservaService(context);
            var prestamoService = new PrestamoService(context, libroService, multaService, reservaService);
            var reporteService = new ReporteService(context, libroService, socioService);

            prestamoService.ActualizarPrestamosVencidos();

            var menu = new MenuPrincipal(libroService, socioService, prestamoService, reservaService, multaService, reporteService);

            menu.MostrarLibrosDisponiblesAlInicio();
            menu.Ejecutar();
        }
    }
}
