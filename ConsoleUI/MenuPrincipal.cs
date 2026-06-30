using Biblioteca_Mastrangelo_Portela.Common;
using Biblioteca_Mastrangelo_Portela.Dtos;
using Biblioteca_Mastrangelo_Portela.Models;
using Biblioteca_Mastrangelo_Portela.Services;

namespace Biblioteca_Mastrangelo_Portela.ConsoleUI
{
    public class MenuPrincipal
    {
        private readonly LibroService _libroService;
        private readonly SocioService _socioService;
        private readonly PrestamoService _prestamoService;
        private readonly ReservaService _reservaService;
        private readonly MultaService _multaService;
        private readonly ReporteService _reporteService;

        public MenuPrincipal(LibroService libroService, SocioService socioService, PrestamoService prestamoService,
            ReservaService reservaService, MultaService multaService, ReporteService reporteService)
        {
            _libroService = libroService;
            _socioService = socioService;
            _prestamoService = prestamoService;
            _reservaService = reservaService;
            _multaService = multaService;
            _reporteService = reporteService;
        }

        public void Ejecutar()
        {
            bool salir = false;
            while (!salir)
            {
                EntradaConsola.LimpiarPantalla();
                Console.WriteLine("=== SISTEMA DE GESTIÓN DE BIBLIOTECA ===");
                Console.WriteLine();
                Console.WriteLine("1. Listar libros disponibles");
                Console.WriteLine("2. Registrar préstamo");
                Console.WriteLine("3. Registrar devolución");
                Console.WriteLine("4. Registrar reserva");
                Console.WriteLine("5. Ver detalle de un socio");
                Console.WriteLine("6. Libros más prestados");
                Console.WriteLine("7. Socios con multas pendientes");
                Console.WriteLine("8. Préstamos vencidos");
                Console.WriteLine("9. Consultar disponibilidad");
                Console.WriteLine("10. Historial de un socio");
                Console.WriteLine("11. Renovar préstamo");
                Console.WriteLine("12. Ranking de socios");
                Console.WriteLine("0. Salir");
                Console.WriteLine();

                int opcion = EntradaConsola.LeerEntero("Seleccione una opción: ");

                try
                {
                    switch (opcion)
                    {
                        case 1: ListarLibrosDisponibles(); break;
                        case 2: RegistrarPrestamo(); break;
                        case 3: RegistrarDevolucion(); break;
                        case 4: RegistrarReserva(); break;
                        case 5: VerDetalleSocio(); break;
                        case 6: LibrosMasPrestados(); break;
                        case 7: SociosConMultasPendientes(); break;
                        case 8: PrestamosVencidos(); break;
                        case 9: ConsultarDisponibilidad(); break;
                        case 10: HistorialSocio(); break;
                        case 11: RenovarPrestamo(); break;
                        case 12: RankingSocios(); break;
                        case 0: salir = true; break;
                        default: Console.WriteLine("Opción inválida."); EntradaConsola.Pausar(); break;
                    }
                }
                catch (ReglaNegocioException ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"[ATENCIÓN] {ex.Message}");
                    EntradaConsola.Pausar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    EntradaConsola.Pausar();
                }
            }
        }

        public void MostrarLibrosDisponiblesAlInicio()
        {
            Console.WriteLine("=== LIBROS DISPONIBLES ===");
            Console.WriteLine();
            var libros = _libroService.ObtenerLibrosDisponibles();
            if (!libros.Any())
            {
                Console.WriteLine("No hay libros disponibles en este momento.");
            }
            else
            {
                foreach (var libro in libros)
                {
                    int disponibles = _libroService.CalcularCopiasDisponibles(libro.ISBN);
                    Console.WriteLine($"ISBN: {libro.ISBN}");
                    Console.WriteLine($"Título: {libro.Titulo}");
                    Console.WriteLine($"Autor: {libro.Autor}");
                    Console.WriteLine($"Género: {libro.Genero}");
                    Console.WriteLine($"Copias disponibles: {disponibles} de {libro.CantidadCopias}");
                    Console.WriteLine("----------------------------------------");
                }
            }
            Console.WriteLine();
        }

        public void ListarLibrosDisponibles()
        {
            MostrarLibrosDisponiblesAlInicio();
            EntradaConsola.Pausar();
        }

        public void RegistrarPrestamo()
        {
            Console.WriteLine("=== REGISTRAR PRÉSTAMO ===");
            int nroSocio = EntradaConsola.LeerEntero("Número de socio: ");
            string criterio = EntradaConsola.LeerTextoObligatorio("Título o autor del libro: ");

            var resultados = _libroService.BuscarPorTituloOAutor(criterio);
            if (!resultados.Any())
            {
                Console.WriteLine();
                Console.WriteLine("No se encontraron libros con ese criterio.");
                EntradaConsola.Pausar();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Libros encontrados:");
            foreach (var libro in resultados)
            {
                int disponibles = _libroService.CalcularCopiasDisponibles(libro.ISBN);
                Console.WriteLine($"ISBN: {libro.ISBN} | {libro.Titulo} - {libro.Autor} | Disponibles: {disponibles} de {libro.CantidadCopias}");
            }

            string isbn = EntradaConsola.LeerTextoObligatorio("ISBN del libro a prestar: ");
            if (!resultados.Any(l => l.ISBN == isbn))
            {
                Console.WriteLine();
                Console.WriteLine("El ISBN ingresado no pertenece a los resultados de la búsqueda.");
                EntradaConsola.Pausar();
                return;
            }

            try
            {
                var prestamo = _prestamoService.RegistrarPrestamo(nroSocio, isbn);
                Console.WriteLine();
                Console.WriteLine("Préstamo registrado correctamente.");
                Console.WriteLine($"ID: {prestamo.Id}");
                Console.WriteLine($"Fecha de vencimiento: {prestamo.FechaVencimiento:dd/MM/yyyy}");
            }
            catch (ReglaNegocioException ex) when (ex.Message.Contains("No hay copias disponibles"))
            {
                Console.WriteLine();
                Console.WriteLine($"[ATENCIÓN] {ex.Message}");
                if (EntradaConsola.LeerSiNo("¿Desea realizar una reserva?"))
                {
                    var reserva = _reservaService.RegistrarReserva(nroSocio, isbn);
                    Console.WriteLine();
                    Console.WriteLine($"Reserva #{reserva.Id} registrada correctamente.");
                    Console.WriteLine($"Fecha: {reserva.FechaReserva:dd/MM/yyyy}");
                }
            }

            EntradaConsola.Pausar();
        }

        public void RegistrarDevolucion()
        {
            Console.WriteLine("=== REGISTRAR DEVOLUCIÓN ===");
            int nroSocio = EntradaConsola.LeerEntero("Número de socio: ");

            var prestamosActivos = _socioService.ObtenerPrestamosActivos(nroSocio);
            if (!prestamosActivos.Any())
            {
                Console.WriteLine();
                Console.WriteLine("El socio no tiene préstamos activos.");
                EntradaConsola.Pausar();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Préstamos activos del socio:");
            foreach (var p in prestamosActivos)
            {
                Console.WriteLine($"ID {p.Id}: {p.Libro.Titulo} (vence {p.FechaVencimiento:dd/MM/yyyy})");
            }

            int prestamoId = EntradaConsola.LeerEntero("ID del préstamo a devolver: ");
            if (!prestamosActivos.Any(p => p.Id == prestamoId))
            {
                Console.WriteLine();
                Console.WriteLine("El ID ingresado no corresponde a un préstamo activo del socio.");
                EntradaConsola.Pausar();
                return;
            }

            var resultado = _prestamoService.RegistrarDevolucion(prestamoId);
            Console.WriteLine();
            Console.WriteLine("Devolución registrada correctamente.");

            if (resultado.MultaGenerada != null)
            {
                Console.WriteLine($"Se generó una multa de ${resultado.MultaGenerada.Monto:F2} por {resultado.MultaGenerada.DiasDemora} día(s) de demora.");
            }

            if (resultado.ReservaCumplida != null)
            {
                Console.WriteLine($"Se cumplió la reserva #{resultado.ReservaCumplida.Id} del socio {resultado.ReservaCumplida.NroSocio}.");
            }

            EntradaConsola.Pausar();
        }

        public void RegistrarReserva()
        {
            Console.WriteLine("=== REGISTRAR RESERVA ===");
            int nroSocio = EntradaConsola.LeerEntero("Número de socio: ");
            string isbn = EntradaConsola.LeerTextoObligatorio("ISBN del libro: ");

            var reserva = _reservaService.RegistrarReserva(nroSocio, isbn);
            Console.WriteLine();
            Console.WriteLine($"Reserva #{reserva.Id} registrada correctamente.");
            Console.WriteLine($"Fecha: {reserva.FechaReserva:dd/MM/yyyy}");
            EntradaConsola.Pausar();
        }

        public void VerDetalleSocio()
        {
            Console.WriteLine("=== DETALLE DE SOCIO ===");
            int nroSocio = EntradaConsola.LeerEntero("Número de socio: ");

            var detalle = _socioService.ObtenerDetalle(nroSocio);
            Console.WriteLine();
            Console.WriteLine($"Número: {detalle.Socio.NroSocio}");
            Console.WriteLine($"Nombre: {detalle.Socio.Nombre} {detalle.Socio.Apellido}");
            Console.WriteLine($"Email: {detalle.Socio.Email}");
            Console.WriteLine($"Tipo: {detalle.TipoSocio}");
            Console.WriteLine($"Activo: {(detalle.Socio.Activo ? "Sí" : "No")}");
            Console.WriteLine();

            Console.WriteLine($"Préstamos activos ({detalle.PrestamosActivos.Count}):");
            foreach (var p in detalle.PrestamosActivos)
            {
                Console.WriteLine($"  - ID {p.Id}: {p.Libro.Titulo} (vence {p.FechaVencimiento:dd/MM/yyyy})");
            }

            Console.WriteLine();
            Console.WriteLine($"Historial de devoluciones ({detalle.HistorialDevoluciones.Count}):");
            foreach (var p in detalle.HistorialDevoluciones)
            {
                Console.WriteLine($"  - ID {p.Id}: {p.Libro.Titulo} (devuelto {p.FechaDevolucion:dd/MM/yyyy})");
            }

            Console.WriteLine();
            Console.WriteLine($"Reservas ({detalle.Reservas.Count}):");
            foreach (var r in detalle.Reservas)
            {
                Console.WriteLine($"  - ID {r.Id}: {r.Libro.Titulo} - {r.EstadoReserva.Nombre}");
            }

            Console.WriteLine();
            Console.WriteLine($"Multas pendientes ({detalle.MultasPendientes.Count}):");
            foreach (var m in detalle.MultasPendientes)
            {
                Console.WriteLine($"  - ID {m.Id}: ${m.Monto:F2} ({m.DiasDemora} días de demora)");
            }
            Console.WriteLine($"Total adeudado: ${detalle.MontoTotalAdeudado:F2}");

            EntradaConsola.Pausar();
        }

        public void LibrosMasPrestados()
        {
            Console.WriteLine("=== LIBROS MÁS PRESTADOS ===");
            var libros = _reporteService.LibrosMasPrestados();
            if (!libros.Any())
            {
                Console.WriteLine("No hay préstamos registrados.");
            }
            else
            {
                int posicion = 1;
                foreach (var libro in libros)
                {
                    Console.WriteLine($"{posicion}. {libro.Titulo} - {libro.Autor}");
                    Console.WriteLine($"   ISBN: {libro.ISBN} | Préstamos: {libro.CantidadPrestamos}");
                    posicion++;
                }
            }
            EntradaConsola.Pausar();
        }

        public void SociosConMultasPendientes()
        {
            Console.WriteLine("=== SOCIOS CON MULTAS PENDIENTES ===");
            var socios = _reporteService.SociosConMultasPendientes();
            if (!socios.Any())
            {
                Console.WriteLine("No hay socios con multas pendientes.");
            }
            else
            {
                foreach (var s in socios)
                {
                    Console.WriteLine($"Socio #{s.NroSocio}: {s.NombreCompleto}");
                    Console.WriteLine($"  Tipo: {s.TipoSocio}");
                    Console.WriteLine($"  Multas pendientes: {s.CantidadMultasPendientes}");
                    Console.WriteLine($"  Total pendiente: ${s.MontoTotalPendiente:F2}");
                    Console.WriteLine();
                }
            }
            EntradaConsola.Pausar();
        }

        public void PrestamosVencidos()
        {
            Console.WriteLine("=== PRÉSTAMOS VENCIDOS ===");
            var vencidos = _reporteService.PrestamosVencidos();
            if (!vencidos.Any())
            {
                Console.WriteLine("No hay préstamos vencidos.");
            }
            else
            {
                foreach (var p in vencidos)
                {
                    Console.WriteLine($"Préstamo #{p.PrestamoId}");
                    Console.WriteLine($"  Socio: {p.NombreSocio} (#{p.NroSocio})");
                    Console.WriteLine($"  Libro: {p.TituloLibro} ({p.ISBN})");
                    Console.WriteLine($"  Venció: {p.FechaVencimiento:dd/MM/yyyy}");
                    Console.WriteLine($"  Días de demora: {p.DiasDemora}");
                    Console.WriteLine();
                }
            }
            EntradaConsola.Pausar();
        }

        public void ConsultarDisponibilidad()
        {
            Console.WriteLine("=== CONSULTAR DISPONIBILIDAD ===");
            string criterio = EntradaConsola.LeerTextoObligatorio("ISBN o título del libro: ");

            var disponibilidad = _reporteService.DisponibilidadLibro(criterio);
            Console.WriteLine();
            Console.WriteLine($"ISBN: {disponibilidad.ISBN}");
            Console.WriteLine($"Título: {disponibilidad.Titulo}");
            Console.WriteLine($"Total de copias: {disponibilidad.CantidadTotalCopias}");
            Console.WriteLine($"Prestadas: {disponibilidad.CantidadPrestada}");
            Console.WriteLine($"Disponibles: {disponibilidad.CantidadDisponible}");
            Console.WriteLine($"Reservas pendientes: {disponibilidad.CantidadReservasPendientes}");

            EntradaConsola.Pausar();
        }

        public void HistorialSocio()
        {
            VerDetalleSocio();
        }

        public void RenovarPrestamo()
        {
            Console.WriteLine("=== RENOVAR PRÉSTAMO ===");
            int prestamoId = EntradaConsola.LeerEntero("ID del préstamo: ");

            var prestamo = _prestamoService.RenovarPrestamo(prestamoId);
            Console.WriteLine();
            Console.WriteLine("Préstamo renovado correctamente.");
            Console.WriteLine($"Nueva fecha de vencimiento: {prestamo.FechaVencimiento:dd/MM/yyyy}");
            EntradaConsola.Pausar();
        }

        public void RankingSocios()
        {
            Console.WriteLine("=== RANKING DE SOCIOS ===");
            var ranking = _reporteService.RankingSocios();
            if (!ranking.Any())
            {
                Console.WriteLine("No hay socios con préstamos registrados.");
            }
            else
            {
                foreach (var r in ranking)
                {
                    string estadoMulta = r.TieneMultasPendientes ? " [Multas pendientes]" : "";
                    Console.WriteLine($"{r.Posicion}. {r.NombreCompleto} (#{r.NroSocio}) - {r.TipoSocio} - {r.CantidadPrestamos} préstamo(s){estadoMulta}");
                }
            }
            EntradaConsola.Pausar();
        }
    }
}
