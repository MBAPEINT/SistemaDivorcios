using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class RepresentacionConfiguracion
        : IEntityTypeConfiguration<Representacion>
    {
        public void Configure(
            EntityTypeBuilder<Representacion> builder)
        {
            builder.ToTable(
                "representacion",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_representacion_tipo_poder",
                        "tipo_poder_codigo = 'ESPECIAL'");

                    tabla.HasCheckConstraint(
                        "ck_representacion_estado",
                        """
                        estado_codigo IN (
                            'VIGENTE',
                            'REVOCADA',
                            'VENCIDA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_representacion_vigencia",
                        """
                        vigente_hasta IS NULL
                        OR vigente_hasta >= vigente_desde
                        """);

                    tabla.HasCheckConstraint(
                        "ck_representacion_estado_cierre",
                        """
                        estado_codigo = 'VIGENTE'
                        OR vigente_hasta IS NOT NULL
                        """);
                });

            builder.HasKey(x => x.RepresentacionId);

            builder.Property(x => x.RepresentacionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.TipoPoderCodigo)
                .HasMaxLength(30)
                .HasDefaultValue("ESPECIAL")
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("VIGENTE")
                .IsRequired();

            builder.Property(x => x.VigenteDesde)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.VigenteHasta)
                .HasColumnType("date");

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => x.ExpedienteConyugeId)
                .IsUnique()
                .HasFilter("estado_codigo = 'VIGENTE'");

            builder.HasIndex(x => x.DocumentoPoderId)
                .IsUnique();

            builder.HasIndex(x => x.RepresentantePersonaId);

            builder.HasOne(x => x.ExpedienteConyuge)
                .WithMany(x => x.Representaciones)
                .HasForeignKey(x => x.ExpedienteConyugeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RepresentantePersona)
                .WithMany(x => x.RepresentacionesComoApoderado)
                .HasForeignKey(x => x.RepresentantePersonaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoPoder)
                .WithOne(x => x.RepresentacionPoder)
                .HasForeignKey<Representacion>(
                    x => x.DocumentoPoderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
