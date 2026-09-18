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
                        "ck_preregistro_hijos_menores",
                        "cantidad_hijos_menores >= 0");

                    tabla.HasCheckConstraint(
                        "ck_preregistro_hijos_mayores_incapaces",
                        "cantidad_hijos_mayores_incapaces >= 0");

                    tabla.HasCheckConstraint(
                        "ck_preregistro_envio",
                        "enviado_en IS NULL OR enviado_en >= creado_en");

                    tabla.HasCheckConstraint(
                        "ck_preregistro_aprobacion",
                        """
                        aprobado_en IS NULL
                        OR (
                            enviado_en IS NOT NULL
                            AND aprobado_en >= enviado_en
                        )
                        """);
                });

            builder.HasKey(x => x.PreregistroId);

            builder.Property(x => x.PreregistroId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.FechaMatrimonio)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.MatrimonioEnPorvenir)
                .IsRequired();

            builder.Property(x => x.UltimoDomicilioConyugalEnPorvenir)
                .IsRequired();

            builder.Property(x => x.MutuoAcuerdoDeclarado)
                .IsRequired();

            builder.Property(x => x.CantidadHijosMenores)
                .HasDefaultValue((short)0)
                .IsRequired();

            builder.Property(x => x.CantidadHijosMayoresIncapaces)
                .HasDefaultValue((short)0)
                .IsRequired();

            builder.Property(x => x.TieneBienesSociales)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.EnviadoEn);

            builder.Property(x => x.AprobadoEn);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasOne(x => x.Caso)
                .WithOne(x => x.Preregistro)
                .HasForeignKey<Preregistro>(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}