using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class DiaNoLaborableConfiguracion
        : IEntityTypeConfiguration<DiaNoLaborable>
    {
        public void Configure(
            EntityTypeBuilder<DiaNoLaborable> builder)
        {
            builder.ToTable(
                "dia_no_laborable",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_dia_no_laborable_nombre",
                        "btrim(nombre) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_dia_no_laborable_tipo",
                        """
                        tipo_codigo IN (
                            'FERIADO',
                            'NO_LABORABLE',
                            'CIERRE_INSTITUCIONAL'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_dia_no_laborable_ambito",
                        """
                        ambito_codigo IN (
                            'NACIONAL',
                            'REGIONAL',
                            'MUNICIPAL'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_dia_no_laborable_aplicacion",
                        """
                        excluye_dia_habil = TRUE
                        OR excluye_dia_operativo = TRUE
                        """);
                });

            builder.HasKey(x => x.DiaNoLaborableId);

            builder.Property(x => x.DiaNoLaborableId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Fecha)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Nombre)
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(x => x.TipoCodigo)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(x => x.AmbitoCodigo)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.ExcluyeDiaHabil)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.ExcluyeDiaOperativo)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => x.Fecha)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.Activo,
                x.Fecha
            });

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x =>
                    x.DiasNoLaborablesRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}