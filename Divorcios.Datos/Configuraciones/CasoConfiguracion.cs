using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class CasoConfiguracion : IEntityTypeConfiguration<Caso>
    {
        public void Configure(EntityTypeBuilder<Caso> builder)
        {
            builder.ToTable(
                "caso",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_caso_fechas",
                        "cerrado_en IS NULL OR cerrado_en >= creado_en");
                });

            builder.HasKey(x => x.CasoId);

            builder.Property(x => x.CasoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.CodigoPre)
                .HasMaxLength(24)
                .IsRequired();

            builder.HasIndex(x => x.CodigoPre)
                .IsUnique();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.CerradoEn);

            builder.HasOne(x => x.CreadoPorCuenta)
                .WithMany(x => x.CasosCreados)
                .HasForeignKey(x => x.CreadoPorCuentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}