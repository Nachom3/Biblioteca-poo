namespace Biblioteca_Mastrangelo_Portela.ConsoleUI
{
    public static class EntradaConsola
    {
        public static int LeerEntero(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string? entrada = Console.ReadLine();
                if (int.TryParse(entrada, out int resultado))
                    return resultado;

                Console.WriteLine("Entrada inválida. Debe ingresar un número entero.");
            }
        }

        public static string LeerTextoObligatorio(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string? entrada = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(entrada))
                    return entrada.Trim();

                Console.WriteLine("El campo es obligatorio.");
            }
        }

        public static bool LeerSiNo(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje + " (s/n): ");
                string? entrada = Console.ReadLine()?.Trim().ToLower();
                if (entrada == "s" || entrada == "si" || entrada == "sí")
                    return true;
                if (entrada == "n" || entrada == "no")
                    return false;

                Console.WriteLine("Respuesta inválida. Ingrese 's' o 'n'.");
            }
        }

        public static void Pausar()
        {
            Console.WriteLine();
            if (Console.IsInputRedirected)
                return;

            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey(true);
        }

        public static void LimpiarPantalla()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
            }
        }
    }
}
