using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class RevisionDetalleDocumentoConfiguracion
        : IEntityTypeConfiguration<RevisionDetalleDocumento>
    {
        public void Configure(
            EntityTypeBuilder<RevisionDetalleDocumento> builder)
        {
            builder.ToTable("revision_detalle_documento", "divorcios");

            // El par identifica la evidencia sin duplicar DocumentoId ni los datos del archivo.
            builder.HasKey(x => new
            {
                x.RevisionDetalleId,
                x.DocumentoVersionId
            });

            builder.HasIndex(x => x.DocumentoVersionId);

            builder.HasOne(x => x.RevisionDetalle)
                .WithMany(x => x.DocumentosEvaluados)
                .HasForeignKey(x => x.RevisionDetalleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoVersion)
                .WithMany(x => x.Evaluaciones)
                .HasForeignKey(x => x.DocumentoVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
