using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PlazoExpedienteConfiguracion
        : IEntityTypeConfiguration<PlazoExpediente>
    {
        public void Configure(EntityTypeBuilder<PlazoExpediente> builder)
        {
            builder.ToTable(
                "plazo_expediente",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_plazo_expediente_estado_aplicacion",
                        "numero_aplicacion > 0");

                    tabla.HasCheckConstraint(
                        "ck_plazo_expediente_fechas",
                        "fecha_vencimiento >= fecha_inicio");

                    tabla.HasCheckConstraint(
                        "ck_plazo_expediente_estado",
                        """
                        estado_codigo IN (
                            'PENDIENTE',
                            'CUMPLIDO',
                            'CANCELADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_plazo_expediente_cierre",
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
                        "ck_plazo_expediente_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.PlazoExpedienteId);

            builder.Property(x => x.PlazoExpedienteId)
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
                x.ExpedienteId,
                x.ReglaPlazoId,
                x.NumeroAplicacion
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.EstadoCodigo,
                x.FechaVencimiento
            });

            builder.HasIndex(x => x.ReglaPlazoId);

            builder.HasIndex(x => x.HistorialEstadoExpedienteOrigenId);

            builder.HasIndex(x => x.CreadoPorUsuarioId);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.Plazos)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReglaPlazo)
                .WithMany(x => x.Aplicaciones)
                .HasForeignKey(x => x.ReglaPlazoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HistorialEstadoExpedienteOrigen)
                .WithMany(x => x.PlazosOriginados)
                .HasForeignKey(x => x.HistorialEstadoExpedienteOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadoPorUsuario)
                .WithMany(x => x.PlazosCreados)
                .HasForeignKey(x => x.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}