using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class SolicitanteDisolucionConfiguracion
        : IEntityTypeConfiguration<SolicitanteDisolucion>
    {
        public void Configure(
            EntityTypeBuilder<SolicitanteDisolucion> builder)
        {
            builder.ToTable(
                "solicitante_disolucion",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_solicitante_disolucion_modalidad",
                        """
                        modalidad_codigo IN (
                            'DIRECTA',
                            'APODERADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_solicitante_disolucion_representacion",
                        """
                        (
                            modalidad_codigo = 'DIRECTA'
                            AND representacion_id IS NULL
                        )
                        OR
                        (
                            modalidad_codigo = 'APODERADO'
                            AND representacion_id IS NOT NULL
                        )
                        """);
                });

            builder.HasKey(x => x.SolicitanteDisolucionId);

            builder.Property(x => x.SolicitanteDisolucionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.ModalidadCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("DIRECTA")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.SolicitudDisolucionId,
                x.CasoConyugeId
            })
                .IsUnique();

            builder.HasIndex(x => x.CasoConyugeId);

            builder.HasIndex(x => x.RepresentacionId);

            builder.HasOne(x => x.SolicitudDisolucion)
                .WithMany(x => x.Solicitantes)
                .HasForeignKey(x => x.SolicitudDisolucionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CasoConyuge)
                .WithMany(x => x.SolicitudesDisolucionPresentadas)
                .HasForeignKey(x => x.CasoConyugeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Representacion)
                .WithMany(x => x.SolicitudesDisolucionPresentadas)
                .HasForeignKey(x => x.RepresentacionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}