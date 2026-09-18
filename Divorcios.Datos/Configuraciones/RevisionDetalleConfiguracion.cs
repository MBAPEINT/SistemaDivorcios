using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class RevisionDetalleConfiguracion
        : IEntityTypeConfiguration<RevisionDetalle>
    {
        public void Configure(
            EntityTypeBuilder<RevisionDetalle> builder)
        {
            builder.ToTable(
                "revision_detalle",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_revision_detalle_resultado",
                        """
                        resultado_codigo IN (
                            'CONFORME',
                            'OBSERVADO',
                            'NO_APLICA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_revision_detalle_observacion_vacia",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_revision_detalle_observado",
                        """
                        resultado_codigo <> 'OBSERVADO'
                        OR (
                            observacion IS NOT NULL
                            AND btrim(observacion) <> ''
                        )
                        """);
                });

            builder.HasKey(x => x.RevisionDetalleId);

            builder.Property(x => x.RevisionDetalleId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.ResultadoCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.RevisionPreregistroId,
                x.PreregistroRequisitoId
            })
                .IsUnique();

            builder.HasIndex(x => x.PreregistroRequisitoId);

            builder.HasIndex(x => x.ResultadoCodigo);

            builder.HasOne(x => x.RevisionPreregistro)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.RevisionPreregistroId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreregistroRequisito)
                .WithMany(x => x.DetallesRevision)
                .HasForeignKey(x => x.PreregistroRequisitoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}