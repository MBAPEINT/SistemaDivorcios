using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PreregistroVersionConfiguracion
        : IEntityTypeConfiguration<PreregistroVersion>
    {
        public void Configure(
            EntityTypeBuilder<PreregistroVersion> builder)
        {
            builder.ToTable(
                "preregistro_version",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_numero",
                        "numero_version > 0");

                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_hijos_menores",
                        "cantidad_hijos_menores >= 0");

                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_hijos_mayores",
                        "cantidad_hijos_mayores >= 0");

                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_hijos_mayores_situacion_especial",
                        """
                        tiene_hijos_mayores_situacion_especial IS NOT TRUE
                        OR (tiene_hijos AND cantidad_hijos_mayores > 0)
                        """);

                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_sin_hijos",
                        """
                        tiene_hijos
                        OR (
                            cantidad_hijos_menores = 0
                            AND cantidad_hijos_mayores = 0
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_acuerdo_bienes",
                        """
                        tiene_bienes
                        OR tiene_acuerdo_bienes = FALSE
                        """);

                    tabla.HasCheckConstraint(
                        "ck_preregistro_version_motivo",
                        """
                        numero_version = 1
                        OR (
                            motivo_cambio IS NOT NULL
                            AND btrim(motivo_cambio) <> ''
                        )
                        """);
                });

            builder.HasKey(x => x.PreregistroVersionId);

            builder.Property(x => x.PreregistroVersionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroVersion)
                .IsRequired();

            builder.Property(x => x.FechaMatrimonio)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.MatrimonioEnPorvenir)
                .IsRequired();

            builder.Property(x => x.UltimoDomicilioConyugalPorvenir)
                .IsRequired();

            builder.Property(x => x.DomicilioConyugal)
                .HasMaxLength(250);

            builder.Property(x => x.TieneHijos)
                .IsRequired();

            builder.Property(x => x.CantidadHijosMenores)
                .HasDefaultValue((short)0)
                .IsRequired();

            builder.Property(x => x.CantidadHijosMayores)
                .HasDefaultValue((short)0)
                .IsRequired();

            builder.Property(x => x.TieneHijosMayoresSituacionEspecial)
                .IsRequired(false);

            builder.Property(x => x.TieneBienes)
                .IsRequired();

            builder.Property(x => x.TieneAcuerdoBienes)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.RequiereRepresentacionA)
                .IsRequired(false);

            builder.Property(x => x.RequiereRepresentacionB)
                .IsRequired(false);

            builder.Property(x => x.ObservacionCiudadano)
                .HasMaxLength(2000);

            builder.Property(x => x.MotivoCambio)
                .HasMaxLength(300);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.PreregistroId,
                x.NumeroVersion
            })
                .IsUnique();

            builder.HasIndex(x => x.CreadoPorCuentaId);

            builder.HasOne(x => x.Preregistro)
                .WithMany(x => x.Versiones)
                .HasForeignKey(x => x.PreregistroId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadoPorCuenta)
                .WithMany(x => x.VersionesPreregistroCreadas)
                .HasForeignKey(x => x.CreadoPorCuentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
