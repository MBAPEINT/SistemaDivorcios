using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class AsistenciaAudienciaConfiguracion
        : IEntityTypeConfiguration<AsistenciaAudiencia>
    {
        public void Configure(
            EntityTypeBuilder<AsistenciaAudiencia> builder)
        {
            builder.ToTable(
                "asistencia_audiencia",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_asistencia_modalidad",
                        """
                        modalidad_codigo IN (
                            'DIRECTA',
                            'APODERADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_asistencia_representacion",
                        """
                        (
                            modalidad_codigo = 'DIRECTA'
                            AND representacion_id IS NULL
                        )
                        OR
                        (
                            modalidad_codigo = 'APODERADO'
                            AND representacion_id IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_asistencia_ratificacion",
                        """
                        ratifico_voluntad = FALSE
                        OR (
                            asistio = TRUE
                            AND identidad_verificada_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_asistencia_verificacion",
                        """
                        identidad_verificada_en IS NULL
                        OR asistio = TRUE
                        """);

                    tabla.HasCheckConstraint(
                        "ck_asistencia_inasistencia",
                        """
                        asistio = TRUE
                        OR (
                            ratifico_voluntad = FALSE
                            AND identidad_verificada_en IS NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_asistencia_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.AsistenciaAudienciaId);

            builder.Property(x => x.AsistenciaAudienciaId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.ModalidadCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("DIRECTA")
                .IsRequired();

            builder.Property(x => x.Asistio)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.RatificoVoluntad)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.IdentidadVerificadaEn);

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.AudienciaRatificacionId,
                x.CasoConyugeId
            })
                .IsUnique();

            builder.HasIndex(x => x.CasoConyugeId);

            builder.HasIndex(x => x.RepresentacionId);

            builder.HasOne(x => x.AudienciaRatificacion)
                .WithMany(x => x.Asistencias)
                .HasForeignKey(x => x.AudienciaRatificacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CasoConyuge)
                .WithMany(x => x.AsistenciasAudiencia)
                .HasForeignKey(x => x.CasoConyugeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Representacion)
                .WithMany(x => x.AsistenciasAudiencia)
                .HasForeignKey(x => x.RepresentacionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}