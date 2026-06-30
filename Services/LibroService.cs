using Microsoft.EntityFrameworkCore;
using Biblioteca_Mastrangelo_Portela.Data;
using Biblioteca_Mastrangelo_Portela.Models;

namespace Biblioteca_Mastrangelo_Portela.Services
{
    public class LibroService
    {
        private readonly BibliotecaDbContext _context;

        public LibroService(BibliotecaDbContext context)
        {
            _context = context;
        }

        public List<Libro> ObtenerLibrosDisponibles()
        {
            return _context.Libros
                .AsEnumerable()
                .Where(l => CalcularCopiasDisponibles(l.ISBN) > 0)
                .OrderBy(l => l.Titulo)
                .ToList();
        }

        public List<Libro> BuscarPorTitulo(string titulo)
        {
            return _context.Libros
                .Where(l => l.Titulo.Contains(titulo))
                .OrderBy(l => l.Titulo)
                .ToList();
        }

        public List<Libro> BuscarPorAutor(string autor)
        {
            return _context.Libros
                .Where(l => l.Autor.Contains(autor))
                .OrderBy(l => l.Titulo)
                .ToList();
        }

        public List<Libro> BuscarPorTituloOAutor(string criterio)
        {
            return _context.Libros
                .Where(l => l.Titulo.Contains(criterio) || l.Autor.Contains(criterio))
                .OrderBy(l => l.Titulo)
                .ToList();
        }

        public Libro? BuscarPorISBN(string isbn)
        {
            return _context.Libros.FirstOrDefault(l => l.ISBN == isbn);
        }

        public Libro? BuscarPorISBNoTitulo(string criterio)
        {
            return _context.Libros
                .FirstOrDefault(l => l.ISBN == criterio || l.Titulo == criterio);
        }

        public int CalcularCopiasDisponibles(string isbn)
        {
            var libro = _context.Libros.FirstOrDefault(l => l.ISBN == isbn);
            if (libro == null)
                return 0;

            int prestados = _context.Prestamos
                .Count(p => p.ISBN == isbn && (p.EstadoPrestamo.Nombre == Common.CodigosDominio.PrestamoActivo || p.EstadoPrestamo.Nombre == Common.CodigosDominio.PrestamoVencido));

            return libro.CantidadCopias - prestados;
        }

        public int CalcularCopiasPrestadas(string isbn)
        {
            return _context.Prestamos
                .Count(p => p.ISBN == isbn && (p.EstadoPrestamo.Nombre == Common.CodigosDominio.PrestamoActivo || p.EstadoPrestamo.Nombre == Common.CodigosDominio.PrestamoVencido));
        }

        public List<Reserva> ConsultarReservasPendientes(string isbn)
        {
            return _context.Reservas
                .Include(r => r.Socio)
                .Where(r => r.ISBN == isbn && r.EstadoReserva.Nombre == Common.CodigosDominio.ReservaPendiente)
                .OrderBy(r => r.FechaReserva)
                .ToList();
        }

        public bool ExisteLibro(string isbn)
        {
            return _context.Libros.Any(l => l.ISBN == isbn);
        }
    }
}
