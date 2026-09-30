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
                        "ck_regla_plazo_cantidad",
                        "cantidad > 0");

                    tabla.HasCheckConstraint(
                        "ck_regla_plazo_unidad",
                        "unidad_codigo IN ('DIA', 'MES')");

                    tabla.HasCheckConstraint(
                        "ck_regla_plazo_tipo_dia",
                        """
                        tipo_dia_codigo IN (
                            'CALENDARIO',
                            'HABIL',
                            'OPERATIVO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_regla_plazo_evento_inicio",
                        "btrim(evento_inicio_codigo) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_regla_plazo_vigencia",
                        """
                        vigente_hasta IS NULL
                        OR vigente_hasta >= vigente_desde
                        """);

                    tabla.HasCheckConstraint(
                        "ck_regla_plazo_descripcion",
                        """
                        descripcion IS NULL
                        OR btrim(descripcion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_regla_plazo_fuente",
                        "btrim(fuente) <> ''");
                });

            builder.HasKey(x => x.ReglaPlazoId);

            builder.Property(x => x.ReglaPlazoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Nombre)
                .HasMaxLength(160)
                .IsRequired();

            builder.Property(x => x.Descripcion)
                .HasMaxLength(1000);

            builder.Property(x => x.Cantidad)
                .IsRequired();

            builder.Property(x => x.UnidadCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.TipoDiaCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.EventoInicioCodigo)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.VigenteDesde)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.VigenteHasta)
                .HasColumnType("date");

            builder.Property(x => x.Fuente)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.Codigo,
                x.VigenteDesde
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.Activo,
                x.EventoInicioCodigo
            });
        }
    }
}