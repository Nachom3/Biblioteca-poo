# Biblioteca_Mastrangelo_Portela

Sistema de Gestión de Biblioteca para una biblioteca municipal.

## Integrantes

- Ignacio Mastrangelo
- Renzo Portela


## GUIA INSTALACION EN 3 SIMPLISIMOS PASOS!!!

### 1. Cambiar la ruta de la base de datos

En `Data/BibliotecaDbContext.cs`, línea 19, modificar el `Data Source` por la ruta absoluta a `biblioteca.db` en tu máquina:

```csharp
options.UseSqlite(@"Data Source=/ruta/absoluta/a/Biblioteca-poo/biblioteca.db");
```

### 2. Crear la base de datos desde el script SQL

Ejecutar desde la raíz del proyecto (requiere `sqlite3` instalado):

```bash
sqlite3 biblioteca.db < Database/biblioteca.sql
```

Esto crea el archivo `biblioteca.db` con las tablas y datos de prueba.

### 3. Ejecutar

```bash
dotnet run
```

