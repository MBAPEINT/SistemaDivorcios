using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class HistorialEstadoCasoConfiguracion
        : IEntityTypeConfiguration<HistorialEstadoCaso>
    {
        public void Configure(
            EntityTypeBuilder<HistorialEstadoCaso> builder)
        {
            builder.ToTable(
                "historial_estado_caso",
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

            builder.HasKey(x => x.HistorialEstadoCasoId);

            builder.Property(x => x.HistorialEstadoCasoId)
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
                x.CasoId,
                x.NumeroSecuencia
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.IniciadoEn
            });

            builder.HasIndex(x => x.EstadoCasoId);

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasIndex(x => x.CasoId)
                .IsUnique()
                .HasFilter("finalizado_en IS NULL");

            builder.HasOne(x => x.Caso)
                .WithMany(x => x.HistorialEstados)
                .HasForeignKey(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EstadoCaso)
                .WithMany(x => x.HistorialCasos)
                .HasForeignKey(x => x.EstadoCasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.EstadosCasoRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}