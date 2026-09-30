using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class HistorialEstadoExpedienteConfiguracion
        : IEntityTypeConfiguration<HistorialEstadoExpediente>
    {
        public void Configure(
            EntityTypeBuilder<HistorialEstadoExpediente> builder)
        {
            builder.ToTable(
                "historial_estado_expediente",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_historial_estado_secuencia",
                        "numero_secuencia > 0");

                    tabla.HasCheckConstraint(
                        "ck_historial_estado_fechas",
                        """
                        finalizado_en IS NULL
                        OR finalizado_en >= iniciado_en
                        """);

                    tabla.HasCheckConstraint(
                        "ck_historial_estado_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.HistorialEstadoExpedienteId);

            builder.Property(x => x.HistorialEstadoExpedienteId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroSecuencia)
                .IsRequired();

            builder.Property(x => x.IniciadoEn)
                .IsRequired();

            builder.Property(x => x.FinalizadoEn);

            builder.Property(x => x.RegistradoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.NumeroSecuencia
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.IniciadoEn
            });

            builder.HasIndex(x => x.EstadoExpedienteId);

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasIndex(x => x.ExpedienteId)
                .IsUnique()
                .HasFilter("finalizado_en IS NULL");

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.HistorialEstados)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EstadoExpediente)
                .WithMany(x => x.HistorialExpedientes)
                .HasForeignKey(x => x.EstadoExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.EstadosExpedienteRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}