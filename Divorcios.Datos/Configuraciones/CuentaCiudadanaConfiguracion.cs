using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class CuentaCiudadanaConfiguracion
        : IEntityTypeConfiguration<CuentaCiudadana>
    {
        public void Configure(EntityTypeBuilder<CuentaCiudadana> builder)
        {
            builder.ToTable("cuenta_ciudadana", "divorcios");

            builder.HasKey(x => x.CuentaCiudadanaId);

            builder.Property(x => x.CuentaCiudadanaId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.PersonaId)
                .IsRequired();

            builder.HasIndex(x => x.PersonaId)
                .IsUnique();

            builder.Property(x => x.CelularVerificadoEn);

            builder.Property(x => x.BloqueadoEn);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasOne(x => x.Persona)
                .WithOne(x => x.CuentaCiudadana)
                .HasForeignKey<CuentaCiudadana>(x => x.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}