using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ExpedienteVersionConfiguracion
        : IEntityTypeConfiguration<ExpedienteVersion>
    {
        public void Configure(
            EntityTypeBuilder<ExpedienteVersion> builder)
        {
            builder.ToTable(
                "expediente_version",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_expediente_version_numero",
                        "numero_version > 0");

                    tabla.HasCheckConstraint(
                        "ck_expediente_version_hijos_menores",
                        "cantidad_hijos_menores >= 0");

                    tabla.HasCheckConstraint(
                        "ck_expediente_version_hijos_mayores",
                        "cantidad_hijos_mayores >= 0");

                    tabla.HasCheckConstraint(
                        "ck_expediente_version_hijos_mayores_situacion_especial",
                        """
                        tiene_hijos_mayores_situacion_especial IS NOT TRUE
                        OR (tiene_hijos AND cantidad_hijos_mayores > 0)
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_version_sin_hijos",
                        """
                        tiene_hijos
                        OR (
                            cantidad_hijos_menores = 0
                            AND cantidad_hijos_mayores = 0
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_version_acuerdo_bienes",
                        """
                        tiene_bienes
                        OR tiene_acuerdo_bienes = FALSE
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_version_motivo",
                        "btrim(motivo_cambio) <> ''");
                });

            builder.HasKey(x => x.ExpedienteVersionId);

            builder.Property(x => x.ExpedienteVersionId)
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

            builder.Property(x => x.MotivoCambio)
                .HasMaxLength(300)
                .IsRequired();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.NumeroVersion
            })
                .IsUnique();

            builder.HasIndex(x => x.PreregistroVersionOrigenId);

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.Versiones)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreregistroVersionOrigen)
                .WithMany(x => x.VersionesExpedienteOriginadas)
                .HasForeignKey(x => x.PreregistroVersionOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.VersionesExpedienteRegistradas)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
