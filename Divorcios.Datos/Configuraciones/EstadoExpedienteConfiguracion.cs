using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class EstadoExpedienteConfiguracion
        : IEntityTypeConfiguration<EstadoExpediente>
    {
        public void Configure(EntityTypeBuilder<EstadoExpediente> builder)
        {
            builder.ToTable(
                "estado_expediente",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_estado_etapa",
                        "etapa_codigo IN " +
                        "('PRERREGISTRO', 'SEPARACION', 'ESPERA', " +
                        "'DIVORCIO', 'CIERRE')");

                    tabla.HasCheckConstraint(
                        "ck_estado_orden",
                        "orden_visual > 0");
                });

            builder.HasKey(x => x.EstadoExpedienteId);

            builder.Property(x => x.EstadoExpedienteId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Codigo)
                .HasMaxLength(45)
                .IsRequired();

            builder.HasIndex(x => x.Codigo)
                .IsUnique();

            builder.Property(x => x.NombreCiudadano)
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(x => x.EtapaCodigo)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(x => x.OrdenVisual)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.EtapaCodigo,
                x.OrdenVisual
            })
                .IsUnique();

            builder.Property(x => x.EsFinal)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}