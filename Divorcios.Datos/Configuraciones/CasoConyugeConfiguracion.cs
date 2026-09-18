using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class CasoConyugeConfiguracion
        : IEntityTypeConfiguration<CasoConyuge>
    {
        public void Configure(EntityTypeBuilder<CasoConyuge> builder)
        {
            builder.ToTable(
                "caso_conyuge",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_conyuge_posicion",
                        "posicion_codigo IN ('A', 'B')");
                });

            builder.HasKey(x => x.CasoConyugeId);

            builder.Property(x => x.CasoConyugeId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.CasoId)
                .IsRequired();

            builder.Property(x => x.PersonaId)
                .IsRequired();

            builder.Property(x => x.PosicionCodigo)
                .HasColumnType("char(1)")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.PosicionCodigo
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.PersonaId
            })
                .IsUnique();

            builder.HasIndex(x => x.PersonaId);

            builder.HasOne(x => x.Caso)
                .WithMany(x => x.Conyuges)
                .HasForeignKey(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Persona)
                .WithMany(x => x.CasosConyuge)
                .HasForeignKey(x => x.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}