using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ReglaPlazoConfiguracion
        : IEntityTypeConfiguration<ReglaPlazo>
    {
        public void Configure(EntityTypeBuilder<ReglaPlazo> builder)
        {
            builder.ToTable(
                "regla_plazo",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_regla_cantidad",
                        "cantidad > 0");

                    tabla.HasCheckConstraint(
                        "ck_regla_unidad",
                        "unidad_codigo IN ('DIA', 'MES')");

                    tabla.HasCheckConstraint(
                        "ck_regla_tipo_dia",
                        "tipo_dia_codigo IN " +
                        "('CALENDARIO', 'HABIL', 'OPERATIVO')");

                    tabla.HasCheckConstraint(
                        "ck_regla_vigencia",
                        "vigente_hasta IS NULL " +
                        "OR vigente_hasta >= vigente_desde");
                });

            builder.HasKey(x => x.ReglaPlazoId);

            builder.Property(x => x.ReglaPlazoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Codigo)
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(x => x.Nombre)
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(x => x.Cantidad)
                .IsRequired();

            builder.Property(x => x.UnidadCodigo)
                .HasMaxLength(12)
                .IsRequired();

            builder.Property(x => x.TipoDiaCodigo)
                .HasMaxLength(12)
                .IsRequired();

            builder.Property(x => x.VigenteDesde)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.VigenteHasta)
                .HasColumnType("date");

            builder.Property(x => x.Fuente)
                .HasMaxLength(180)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.Codigo,
                x.VigenteDesde
            })
                .IsUnique();
        }
    }
}