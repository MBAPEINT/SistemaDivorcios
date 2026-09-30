using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class AudienciaRatificacionConfiguracion
        : IEntityTypeConfiguration<AudienciaRatificacion>
    {
        public void Configure(
            EntityTypeBuilder<AudienciaRatificacion> builder)
        {
            builder.ToTable(
                "audiencia_ratificacion",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_audiencia_numero_programacion",
                        "numero_programacion > 0");

                    tabla.HasCheckConstraint(
                        "ck_audiencia_estado",
                        """
                        estado_codigo IN (
                            'PROGRAMADA',
                            'REALIZADA',
                            'NO_REALIZADA',
                            'REPROGRAMADA',
                            'CANCELADA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_audiencia_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_audiencia_resultado_observacion",
                        """
                        estado_codigo IN (
                            'PROGRAMADA',
                            'REALIZADA'
                        )
                        OR (
                            observacion IS NOT NULL
                            AND btrim(observacion) <> ''
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_audiencia_fechas_estado",
                        """
                        (
                            estado_codigo = 'PROGRAMADA'
                            AND fecha_hora_realizacion IS NULL
                            AND cerrado_en IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'REALIZADA'
                            AND fecha_hora_realizacion IS NOT NULL
                            AND cerrado_en IS NOT NULL
                            AND cerrado_en >= fecha_hora_realizacion
                        )
                        OR
                        (
                            estado_codigo IN (
                                'NO_REALIZADA',
                                'REPROGRAMADA',
                                'CANCELADA'
                            )
                            AND fecha_hora_realizacion IS NULL
                            AND cerrado_en IS NOT NULL
                        )
                        """);
                });

            builder.HasKey(x => x.AudienciaRatificacionId);

            builder.Property(x => x.AudienciaRatificacionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroProgramacion)
                .IsRequired();

            builder.Property(x => x.FechaHoraProgramada)
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("PROGRAMADA")
                .IsRequired();

            builder.Property(x => x.FechaHoraRealizacion);

            builder.Property(x => x.CerradoEn);

            builder.Property(x => x.CreadaEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.NumeroProgramacion
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.FechaHoraProgramada
            });

            builder.HasIndex(x => x.CreadaPorUsuarioId);

            builder.HasIndex(x => x.ExpedienteId)
                .IsUnique()
                .HasFilter("estado_codigo = 'PROGRAMADA'");

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.AudienciasRatificacion)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadaPorUsuario)
                .WithMany(x => x.AudienciasCreadas)
                .HasForeignKey(x => x.CreadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}