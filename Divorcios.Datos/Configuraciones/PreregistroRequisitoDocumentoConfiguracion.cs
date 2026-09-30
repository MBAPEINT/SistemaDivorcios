using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PreregistroRequisitoDocumentoConfiguracion
        : IEntityTypeConfiguration<PreregistroRequisitoDocumento>
    {
        public void Configure(
            EntityTypeBuilder<PreregistroRequisitoDocumento> builder)
        {
            builder.ToTable("preregistro_requisito_documento", "divorcios");

            builder.HasKey(x => new
            {
                x.PreregistroRequisitoId,
                x.DocumentoVersionId
            });

            builder.HasIndex(x => x.DocumentoVersionId);

            builder.HasOne(x => x.PreregistroRequisito)
                .WithMany(x => x.ArchivosPresentados)
                .HasForeignKey(x => x.PreregistroRequisitoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoVersion)
                .WithMany(x => x.PresentacionesPreregistro)
                .HasForeignKey(x => x.DocumentoVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
