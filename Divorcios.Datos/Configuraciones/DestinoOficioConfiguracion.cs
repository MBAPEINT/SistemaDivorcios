using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class DestinoOficioConfiguracion
        : IEntityTypeConfiguration<DestinoOficio>
    {
        public void Configure(EntityTypeBuilder<DestinoOficio> builder)
        {
            builder.ToTable(
                "destino_oficio",
                "divorcios");

            builder.HasKey(x => x.DestinoOficioId);

            builder.Property(x => x.DestinoOficioId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Codigo)
                .HasMaxLength(35)
                .IsRequired();

            builder.HasIndex(x => x.Codigo)
                .IsUnique();

            builder.Property(x => x.Nombre)
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}