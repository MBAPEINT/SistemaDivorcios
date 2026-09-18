using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class SolicitudDisolucionConfiguracion
        : IEntityTypeConfiguration<SolicitudDisolucion>
    {
        public void Configure(
            EntityTypeBuilder<SolicitudDisolucion> builder)
        {
            builder.ToTable(
                "solicitud_disolucion",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_solicitud_disolucion_estado",
                        """
                        estado_codigo IN (
                            'PRESENTADA',
                            'ADMITIDA',
                            'OBSERVADA',
                            'RECHAZADA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_solicitud_disolucion_validacion",
                        """
                        (
                            estado_codigo = 'PRESENTADA'
                            AND validada_por_usuario_id IS NULL
                            AND validada_en IS NULL
                        )
                        OR
                        (
                            estado_codigo IN (
                                'ADMITIDA',
                                'OBSERVADA',
                                'RECHAZADA'
                            )
                            AND validada_por_usuario_id IS NOT NULL
                            AND validada_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_solicitud_disolucion_fechas",
                        """
                        validada_en IS NULL
                        OR validada_en >= registrada_en
                        """);

                    tabla.HasCheckConstraint(
                        "ck_solicitud_disolucion_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_solicitud_disolucion_resultado_observacion",
                        """
                        estado_codigo NOT IN (
                            'OBSERVADA',
                            'RECHAZADA'
                        )
                        OR (
                            observacion IS NOT NULL
                            AND btrim(observacion) <> ''
                        )
                        """);
                });

            builder.HasKey(x => x.SolicitudDisolucionId);

            builder.Property(x => x.SolicitudDisolucionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.FechaPresentacionMesaPartes)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("PRESENTADA")
                .IsRequired();

            builder.Property(x => x.RegistradaEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.ValidadaEn);

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.CasoId)
                .IsUnique();

            builder.HasIndex(x => x.DocumentoSolicitudId)
                .IsUnique();

            builder.HasIndex(x => x.FechaPresentacionMesaPartes);

            builder.HasIndex(x => x.RegistradaPorUsuarioId);

            builder.HasIndex(x => x.ValidadaPorUsuarioId);

            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.Caso)
                .WithOne(x => x.SolicitudDisolucion)
                .HasForeignKey<SolicitudDisolucion>(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoSolicitud)
                .WithOne(x => x.SolicitudDisolucion)
                .HasForeignKey<SolicitudDisolucion>(
                    x => x.DocumentoSolicitudId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradaPorUsuario)
                .WithMany(x => x.SolicitudesDisolucionRegistradas)
                .HasForeignKey(x => x.RegistradaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ValidadaPorUsuario)
                .WithMany(x => x.SolicitudesDisolucionValidadas)
                .HasForeignKey(x => x.ValidadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}