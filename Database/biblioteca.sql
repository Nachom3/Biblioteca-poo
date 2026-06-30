CREATE TABLE IF NOT EXISTS TipoSocio (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    MaximoLibrosSimultaneos INTEGER NOT NULL,
    DiasPrestamo INTEGER NOT NULL,
    MultaPorDia REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS EstadoPrestamo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS EstadoReserva (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Libro (
    ISBN TEXT PRIMARY KEY,
    Titulo TEXT NOT NULL,
    Autor TEXT NOT NULL,
    Genero TEXT NOT NULL,
    CantidadCopias INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Socio (
    NroSocio INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Apellido TEXT NOT NULL,
    Email TEXT NOT NULL,
    TipoSocioId INTEGER NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (TipoSocioId) REFERENCES TipoSocio(Id)
);

CREATE TABLE IF NOT EXISTS Prestamo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NroSocio INTEGER NOT NULL,
    ISBN TEXT NOT NULL,
    FechaPrestamo TEXT NOT NULL,
    FechaVencimiento TEXT NOT NULL,
    FechaDevolucion TEXT,
    EstadoPrestamoId INTEGER NOT NULL,
    Renovado INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (NroSocio) REFERENCES Socio(NroSocio),
    FOREIGN KEY (ISBN) REFERENCES Libro(ISBN),
    FOREIGN KEY (EstadoPrestamoId) REFERENCES EstadoPrestamo(Id)
);

CREATE TABLE IF NOT EXISTS Reserva (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NroSocio INTEGER NOT NULL,
    ISBN TEXT NOT NULL,
    FechaReserva TEXT NOT NULL,
    EstadoReservaId INTEGER NOT NULL,
    FOREIGN KEY (NroSocio) REFERENCES Socio(NroSocio),
    FOREIGN KEY (ISBN) REFERENCES Libro(ISBN),
    FOREIGN KEY (EstadoReservaId) REFERENCES EstadoReserva(Id)
);

CREATE TABLE IF NOT EXISTS Multa (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PrestamoId INTEGER NOT NULL UNIQUE,
    DiasDemora INTEGER NOT NULL,
    Monto REAL NOT NULL,
    FechaGeneracion TEXT NOT NULL,
    Abonada INTEGER NOT NULL DEFAULT 0,
    FechaPago TEXT,
    FOREIGN KEY (PrestamoId) REFERENCES Prestamo(Id)
);

INSERT INTO TipoSocio (Id, Nombre, MaximoLibrosSimultaneos, DiasPrestamo, MultaPorDia) VALUES
(1, 'Común', 3, 7, 150.00),
(2, 'Estudiante', 5, 14, 75.00),
(3, 'Docente', 8, 30, 50.00);

INSERT INTO EstadoPrestamo (Id, Nombre) VALUES
(1, 'Activo'),
(2, 'Devuelto'),
(3, 'Vencido');

INSERT INTO EstadoReserva (Id, Nombre) VALUES
(1, 'Pendiente'),
(2, 'Cumplida'),
(3, 'Cancelada');

INSERT INTO Libro (ISBN, Titulo, Autor, Genero, CantidadCopias) VALUES
('978-3-16-148410-0', 'Cien años de soledad', 'Gabriel García Márquez', 'Realismo mágico', 5),
('978-0-7432-7356-5', 'El código Da Vinci', 'Dan Brown', 'Thriller', 3),
('978-0-452-28423-4', '1984', 'George Orwell', 'Distopía', 4),
('978-0-06-112008-4', 'Matar a un ruiseñor', 'Harper Lee', 'Drama', 2),
('978-0-14-243724-7', 'El principito', 'Antoine de Saint-Exupéry', 'Infantil', 6),
('978-0-261-10236-9', 'El Señor de los Anillos', 'J.R.R. Tolkien', 'Fantasía', 3);

INSERT INTO Socio (NroSocio, Nombre, Apellido, Email, TipoSocioId, Activo) VALUES
(1, 'Profe', 'Gonzales', 'mgonzales@ips.edu.ar', 1, 1),
(2, 'Juan', 'Rabasedas', 'raba@mail.com', 2, 1),
(3, 'Carlos', 'López', 'carlos@mail.com', 3, 1),
(4, 'Ana', 'Martínez', 'ana@mail.com', 1, 0),
(5, 'Pedro', 'Rodríguez', 'pedro@mail.com', 2, 1),
(6, 'Laura', 'Fernández', 'laura@mail.com', 3, 1);

INSERT INTO Prestamo (Id, NroSocio, ISBN, FechaPrestamo, FechaVencimiento, FechaDevolucion, EstadoPrestamoId, Renovado) VALUES
(1, 1, '978-3-16-148410-0', '2026-06-01', '2026-06-08', NULL, 3, 0),
(2, 2, '978-0-7432-7356-5', '2026-05-01', '2026-05-15', NULL, 3, 0),
(3, 1, '978-0-14-243724-7', '2026-05-15', '2026-05-22', NULL, 3, 0),
(4, 3, '978-0-452-28423-4', '2026-06-10', '2026-07-10', NULL, 1, 0),
(5, 2, '978-0-06-112008-4', '2026-04-01', '2026-04-15', '2026-04-20', 2, 0),
(6, 5, '978-0-261-10236-9', '2026-06-20', '2026-07-04', NULL, 1, 0),
(7, 6, '978-3-16-148410-0', '2026-06-15', '2026-07-15', NULL, 1, 0),
(8, 2, '978-0-14-243724-7', '2026-03-01', '2026-03-15', '2026-03-25', 2, 0);

INSERT INTO Reserva (Id, NroSocio, ISBN, FechaReserva, EstadoReservaId) VALUES
(1, 5, '978-0-7432-7356-5', '2026-06-25', 1),
(2, 6, '978-0-7432-7356-5', '2026-06-28', 1),
(3, 3, '978-0-06-112008-4', '2026-06-20', 1);

INSERT INTO Multa (Id, PrestamoId, DiasDemora, Monto, FechaGeneracion, Abonada, FechaPago) VALUES
(1, 5, 5, 375.00, '2026-04-20', 1, '2026-04-21'),
(2, 8, 10, 750.00, '2026-03-25', 0, NULL);
