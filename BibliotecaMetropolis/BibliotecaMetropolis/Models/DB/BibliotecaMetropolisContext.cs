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

    public virtual DbSet<Editorial> Editorials { get; set; }

    public virtual DbSet<Institucion> Institucions { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<PalabraClave> PalabraClaves { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=ZHEIKPC\\SQLEXPRESS; Database=BibliotecaMetropolis; TrustServerCertificate=True; Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autor>(entity =>
        {
            entity.HasKey(e => e.IdAutor).HasName("PK__Autor__DD33B0315A6E1918");

            entity.ToTable("Autor");

            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Nacionalidad).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Editorial>(entity =>
        {
            entity.HasKey(e => e.IdEditorial).HasName("PK__Editoria__EF838671D9C47AE8");

            entity.ToTable("Editorial");

            entity.Property(e => e.Ciudad).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Pais).HasMaxLength(50);
        });

        modelBuilder.Entity<Institucion>(entity =>
        {
            entity.HasKey(e => e.IdInstitucion).HasName("PK__Instituc__4231815ABBF78729");

            entity.ToTable("Institucion");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.IdMaterial).HasName("PK__Material__94356E58340566C9");

            entity.ToTable("Material");

            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Tipo).HasMaxLength(50);
            entity.Property(e => e.Titulo).HasMaxLength(150);

            entity.HasOne(d => d.IdAutorNavigation).WithMany(p => p.Materials)
                .HasForeignKey(d => d.IdAutor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Material__IdAuto__4316F928");

            entity.HasOne(d => d.IdEditorialNavigation).WithMany(p => p.Materials)
                .HasForeignKey(d => d.IdEditorial)
                .HasConstraintName("FK__Material__IdEdit__440B1D61");

            entity.HasOne(d => d.IdInstitucionNavigation).WithMany(p => p.Materials)
                .HasForeignKey(d => d.IdInstitucion)
                .HasConstraintName("FK__Material__IdInst__44FF419A");

            entity.HasMany(d => d.IdPalabraClaves).WithMany(p => p.IdMaterials)
                .UsingEntity<Dictionary<string, object>>(
                    "MaterialPalabraClave",
                    r => r.HasOne<PalabraClave>().WithMany()
                        .HasForeignKey("IdPalabraClave")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__MaterialP__IdPal__4AB81AF0"),
                    l => l.HasOne<Material>().WithMany()
                        .HasForeignKey("IdMaterial")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__MaterialP__IdMat__49C3F6B7"),
                    j =>
                    {
                        j.HasKey("IdMaterial", "IdPalabraClave").HasName("PK__Material__D566D38B6A7ADB14");
                        j.ToTable("MaterialPalabraClave");
                    });
        });

        modelBuilder.Entity<PalabraClave>(entity =>
        {
            entity.HasKey(e => e.IdPalabraClave).HasName("PK__PalabraC__153BDD356DE1A3C0");

            entity.ToTable("PalabraClave");

            entity.Property(e => e.Palabra).HasMaxLength(50);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__2A49584CD781713D");

            entity.ToTable("Rol");

            entity.Property(e => e.NombreRol).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF97504AA7F1");

            entity.ToTable("Usuario");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Contrasena).HasMaxLength(100);
            entity.Property(e => e.NombreCompleto).HasMaxLength(100);
            entity.Property(e => e.NombreUsuario).HasMaxLength(50);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__IdRol__3A81B327");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
