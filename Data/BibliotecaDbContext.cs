using Microsoft.EntityFrameworkCore;
using Biblioteca_Mastrangelo_Portela.Models;

namespace Biblioteca_Mastrangelo_Portela.Data
{
    public class BibliotecaDbContext : DbContext
    {
        public DbSet<Libro> Libros { get; set; } = null!;
        public DbSet<Socio> Socios { get; set; } = null!;
        public DbSet<TipoSocio> TiposSocio { get; set; } = null!;
        public DbSet<Prestamo> Prestamos { get; set; } = null!;
        public DbSet<EstadoPrestamo> EstadosPrestamo { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;
        public DbSet<EstadoReserva> EstadosReserva { get; set; } = null!;
        public DbSet<Multa> Multas { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(@"Data Source=C:\Users\ignac\Desktop\Biblioteca-poo\biblioteca.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoSocio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaximoLibrosSimultaneos).IsRequired();
                entity.Property(e => e.DiasPrestamo).IsRequired();
                entity.Property(e => e.MultaPorDia).IsRequired().HasPrecision(18, 2);
            });

            modelBuilder.Entity<EstadoPrestamo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<EstadoReserva>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Libro>(entity =>
            {
                entity.HasKey(e => e.ISBN);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Autor).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Genero).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CantidadCopias).IsRequired();
                entity.HasIndex(e => e.Titulo);
            });

            modelBuilder.Entity<Socio>(entity =>
            {
                entity.HasKey(e => e.NroSocio);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Activo).IsRequired();
                entity.HasOne(e => e.TipoSocio)
                      .WithMany(t => t.Socios)
                      .HasForeignKey(e => e.TipoSocioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Prestamo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaPrestamo).IsRequired();
                entity.Property(e => e.FechaVencimiento).IsRequired();
                entity.Property(e => e.Renovado).IsRequired();

                entity.HasOne(e => e.Socio)
                      .WithMany(s => s.Prestamos)
                      .HasForeignKey(e => e.NroSocio)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Libro)
                      .WithMany(l => l.Prestamos)
                      .HasForeignKey(e => e.ISBN)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EstadoPrestamo)
                      .WithMany(ep => ep.Prestamos)
                      .HasForeignKey(e => e.EstadoPrestamoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaReserva).IsRequired();

                entity.HasOne(e => e.Socio)
                      .WithMany(s => s.Reservas)
                      .HasForeignKey(e => e.NroSocio)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Libro)
                      .WithMany(l => l.Reservas)
                      .HasForeignKey(e => e.ISBN)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EstadoReserva)
                      .WithMany(er => er.Reservas)
                      .HasForeignKey(e => e.EstadoReservaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Multa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DiasDemora).IsRequired();
                entity.Property(e => e.Monto).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.FechaGeneracion).IsRequired();
                entity.Property(e => e.Abonada).IsRequired();

                entity.HasOne(e => e.Prestamo)
                      .WithOne(p => p.Multa)
                      .HasForeignKey<Multa>(e => e.PrestamoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PrestamoId).IsUnique();
            });
        }
    }
}
