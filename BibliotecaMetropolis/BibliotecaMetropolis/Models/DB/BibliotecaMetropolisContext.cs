using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMetropolis.Models.DB;

public partial class BibliotecaMetropolisContext : DbContext
{
    public BibliotecaMetropolisContext()
    {
    }

    public BibliotecaMetropolisContext(DbContextOptions<BibliotecaMetropolisContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Autor> Autors { get; set; }

    public virtual DbSet<AutoresRecurso> AutoresRecursos { get; set; }

    public virtual DbSet<Editorial> Editorials { get; set; }

    public virtual DbSet<Pais> Pais { get; set; }

    public virtual DbSet<PalabraClave> PalabraClaves { get; set; }

    public virtual DbSet<Recurso> Recursos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<TipoRecurso> TipoRecursos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Usar la cadena de conexión desde la configuración
            optionsBuilder.UseSqlServer("Name=BibliotecaMetropolisConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autor>(entity =>
        {
            entity.HasKey(e => e.IdAutor).HasName("PK__Autor__DD33B0319C47E021");

            entity.ToTable("Autor");

            entity.Property(e => e.Apellidos).HasMaxLength(150);
            entity.Property(e => e.Nombres).HasMaxLength(150);
        });

        modelBuilder.Entity<AutoresRecurso>(entity =>
        {
            entity.HasKey(e => new { e.IdRec, e.IdAutor }).HasName("PK__AutoresR__27997717988F4205");

            entity.HasOne(d => d.IdAutorNavigation).WithMany(p => p.AutoresRecursos)
                .HasForeignKey(d => d.IdAutor)
                .HasConstraintName("FK_AR_Autor");

            entity.HasOne(d => d.IdRecNavigation).WithMany(p => p.AutoresRecursos)
                .HasForeignKey(d => d.IdRec)
                .HasConstraintName("FK_AR_Recurso");
        });

        modelBuilder.Entity<Editorial>(entity =>
        {
            entity.HasKey(e => e.IdEdit).HasName("PK__Editoria__0B864DEA8D023207");

            entity.ToTable("Editorial");

            entity.Property(e => e.Descripcion).HasMaxLength(400);
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });

        modelBuilder.Entity<Pais>(entity =>
        {
            entity.HasKey(e => e.IdPais).HasName("PK__Pais__FC850A7BC84A6725");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<PalabraClave>(entity =>
        {
            entity.HasKey(e => e.IdPalabraClave).HasName("PK__PalabraC__153BDD35B626D9F5");

            entity.ToTable("PalabraClave");

            entity.HasIndex(e => e.Palabra, "IX_PalabraClave_Palabra");

            entity.Property(e => e.Palabra).HasMaxLength(100);
        });

        modelBuilder.Entity<Recurso>(entity =>
        {
            entity.HasKey(e => e.IdRec).HasName("PK__Recurso__2A4A4C14471EBBB1");

            entity.ToTable("Recurso");

            entity.HasIndex(e => e.Titulo, "IX_Recurso_Titulo");

            entity.Property(e => e.Edicion).HasMaxLength(50);
            entity.Property(e => e.PalabrasBusqueda).HasMaxLength(500);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Titulo).HasMaxLength(250);

            entity.HasOne(d => d.IdEditNavigation).WithMany(p => p.Recursos)
                .HasForeignKey(d => d.IdEdit)
                .HasConstraintName("FK_Recurso_Editorial");

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.Recursos)
                .HasForeignKey(d => d.IdPais)
                .HasConstraintName("FK_Recurso_Pais");

            entity.HasOne(d => d.IdTipoRNavigation).WithMany(p => p.Recursos)
                .HasForeignKey(d => d.IdTipoR)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recurso_TipoRecurso");

            entity.HasMany(d => d.IdPalabraClaves).WithMany(p => p.IdRecs)
                .UsingEntity<Dictionary<string, object>>(
                    "RecursoPalabraClave",
                    r => r.HasOne<PalabraClave>().WithMany()
                        .HasForeignKey("IdPalabraClave")
                        .HasConstraintName("FK_RPK_Palabra"),
                    l => l.HasOne<Recurso>().WithMany()
                        .HasForeignKey("IdRec")
                        .HasConstraintName("FK_RPK_Recurso"),
                    j =>
                    {
                        j.HasKey("IdRec", "IdPalabraClave").HasName("PK__RecursoP__6B19F1C7DEF0265A");
                        j.ToTable("RecursoPalabraClave");
                    });
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__2A49584C65370112");

            entity.ToTable("Rol");

            entity.Property(e => e.NombreRol).HasMaxLength(100);
        });

        modelBuilder.Entity<TipoRecurso>(entity =>
        {
            entity.HasKey(e => e.IdTipoR).HasName("PK__TipoRecu__5E1AF69EAA5A96DF");

            entity.ToTable("TipoRecurso");

            entity.Property(e => e.Descripcion).HasMaxLength(400);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF971513668F");

            entity.ToTable("Usuario");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Contrasena).HasMaxLength(200);
            entity.Property(e => e.NombreCompleto).HasMaxLength(200);
            entity.Property(e => e.NombreUsuario).HasMaxLength(100);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
