using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PlazoCasoConfiguracion
        : IEntityTypeConfiguration<PlazoCaso>
    {
        public void Configure(EntityTypeBuilder<PlazoCaso> builder)
        {
            builder.ToTable(
                "plazo_caso",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_plazo_caso_aplicacion",
                        "numero_aplicacion > 0");

                    tabla.HasCheckConstraint(
                        "ck_plazo_caso_fechas",
                        "fecha_vencimiento >= fecha_inicio");

                    tabla.HasCheckConstraint(
                        "ck_plazo_caso_estado",
                        """
                        estado_codigo IN (
                            'PENDIENTE',
                            'CUMPLIDO',
                            'CANCELADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_plazo_caso_cierre",
                        """
                        (
                            estado_codigo = 'PENDIENTE'
                            AND cerrado_en IS NULL
                        )
                        OR
                        (
                            estado_codigo IN (
                                'CUMPLIDO',
                                'CANCELADO'
                            )
                            AND cerrado_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_plazo_caso_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.PlazoCasoId);

            builder.Property(x => x.PlazoCasoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroAplicacion)
                .IsRequired();

            builder.Property(x => x.FechaInicio)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.FechaVencimiento)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("PENDIENTE")
                .IsRequired();

            builder.Property(x => x.CerradoEn);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.ReglaPlazoId,
                x.NumeroAplicacion
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.EstadoCodigo,
                x.FechaVencimiento
            });

            builder.HasIndex(x => x.ReglaPlazoId);

            builder.HasIndex(x => x.HistorialEstadoCasoOrigenId);

            builder.HasIndex(x => x.CreadoPorUsuarioId);

            builder.HasOne(x => x.Caso)
                .WithMany(x => x.Plazos)
                .HasForeignKey(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReglaPlazo)
                .WithMany(x => x.Aplicaciones)
                .HasForeignKey(x => x.ReglaPlazoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HistorialEstadoCasoOrigen)
                .WithMany(x => x.PlazosOriginados)
                .HasForeignKey(x => x.HistorialEstadoCasoOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadoPorUsuario)
                .WithMany(x => x.PlazosCreados)
                .HasForeignKey(x => x.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}