using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PreregistroConfiguracion
        : IEntityTypeConfiguration<Preregistro>
    {
        public void Configure(EntityTypeBuilder<Preregistro> builder)
        {
            builder.ToTable(
                "preregistro",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_preregistro_estado",
                        """
                        estado_codigo IN (
                            'BORRADOR',
                            'ENVIADO',
                            'OBSERVADO',
                            'APROBADO',
                            'CANCELADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_preregistro_envio",
                        """
                        enviado_en IS NULL
                        OR enviado_en >= creado_en
                        """);

                    tabla.HasCheckConstraint(
                        "ck_preregistro_aprobacion",
                        """
                        aprobado_en IS NULL
                        OR (
                            enviado_en IS NOT NULL
                            AND aprobado_en >= enviado_en
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_preregistro_bloqueo",
                        """
                        bloqueado_en IS NULL
                        OR (
                            aprobado_en IS NOT NULL
                            AND bloqueado_en >= aprobado_en
                        )
                        """);
                });

            builder.HasKey(x => x.PreregistroId);

            builder.Property(x => x.PreregistroId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("BORRADOR")
                .IsRequired();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.EnviadoEn);
            builder.Property(x => x.AprobadoEn);
            builder.Property(x => x.BloqueadoEn);

            builder.HasIndex(x => x.ExpedienteId)
                .IsUnique();

            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.Expediente)
                .WithOne(x => x.Preregistro)
                .HasForeignKey<Preregistro>(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}