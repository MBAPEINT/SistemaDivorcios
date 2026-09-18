using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class RequisitoCatalogoConfiguracion
        : IEntityTypeConfiguration<RequisitoCatalogo>
    {
        public void Configure(
            EntityTypeBuilder<RequisitoCatalogo> builder)
        {
            builder.ToTable(
                "requisito_catalogo",
                "divorcios");

            builder.HasKey(x => x.RequisitoCatalogoId);

            builder.Property(x => x.RequisitoCatalogoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Codigo)
                .HasMaxLength(40)
                .IsRequired();

            builder.HasIndex(x => x.Codigo)
                .IsUnique();

            builder.Property(x => x.Nombre)
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(x => x.Descripcion)
                .HasColumnType("text");

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}