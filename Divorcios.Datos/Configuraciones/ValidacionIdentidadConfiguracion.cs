using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ValidacionIdentidadConfiguracion
        : IEntityTypeConfiguration<ValidacionIdentidad>
    {
        public void Configure(
            EntityTypeBuilder<ValidacionIdentidad> builder)
        {
            builder.ToTable(
                "validacion_identidad",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_validacion_resultado",
                        "resultado_codigo IN ('VALIDO', 'NO_VALIDO', 'ERROR')");
                });

            builder.HasKey(x => x.ValidacionIdentidadId);

            builder.Property(x => x.ValidacionIdentidadId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.CuentaCiudadanaId)
                .IsRequired();

            builder.Property(x => x.ProveedorCodigo)
                .HasMaxLength(25)
                .HasDefaultValue("RENIEC")
                .IsRequired();

            builder.Property(x => x.ReferenciaConsulta)
                .HasMaxLength(120);

            builder.Property(x => x.ResultadoCodigo)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.ValidadoEn)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasOne(x => x.CuentaCiudadana)
                .WithMany(x => x.ValidacionesIdentidad)
                .HasForeignKey(x => x.CuentaCiudadanaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}