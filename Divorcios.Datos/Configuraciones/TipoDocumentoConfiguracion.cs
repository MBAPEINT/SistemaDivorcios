using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class TipoDocumentoConfiguracion
        : IEntityTypeConfiguration<TipoDocumento>
    {
        public void Configure(EntityTypeBuilder<TipoDocumento> builder)
        {
            builder.ToTable(
                "tipo_documento",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_tipo_documento_origen",
                        "origen_codigo IN ('CIUDADANO', 'MUNICIPALIDAD', 'AMBOS')");
                });

            builder.HasKey(x => x.TipoDocumentoId);

            builder.Property(x => x.TipoDocumentoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Codigo)
                .HasMaxLength(40)
                .IsRequired();

            builder.HasIndex(x => x.Codigo)
                .IsUnique();

            builder.Property(x => x.Nombre)
                .HasMaxLength(160)
                .IsRequired();

            builder.Property(x => x.OrigenCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}