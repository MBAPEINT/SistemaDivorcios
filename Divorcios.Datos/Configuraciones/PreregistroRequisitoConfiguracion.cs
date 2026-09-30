using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PreregistroRequisitoConfiguracion
        : IEntityTypeConfiguration<PreregistroRequisito>
    {
        public void Configure(
            EntityTypeBuilder<PreregistroRequisito> builder)
        {
            builder.ToTable(
                "preregistro_requisito",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_preregistro_requisito_estado",
                        """
                        estado_codigo IN (
                            'PENDIENTE',
                            'CARGADO',
                            'CONFORME',
                            'OBSERVADO',
                            'NO_APLICA'
                        )
                        """);
                });

            builder.HasKey(x => x.PreregistroRequisitoId);

            builder.Property(x => x.PreregistroRequisitoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Obligatorio)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("PENDIENTE")
                .IsRequired();

            builder.Property(x => x.Aplica)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.GeneradoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.PreregistroVersionId,
                x.RequisitoCatalogoId
            })
                .IsUnique();

            builder.HasIndex(x => x.RequisitoCatalogoId);

            builder.HasOne(x => x.PreregistroVersion)
                .WithMany(x => x.Requisitos)
                .HasForeignKey(x => x.PreregistroVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequisitoCatalogo)
                .WithMany(x => x.PrerregistrosRequisitos)
                .HasForeignKey(x => x.RequisitoCatalogoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}