using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ExpedienteConfiguracion
        : IEntityTypeConfiguration<Expediente>
    {
        public void Configure(EntityTypeBuilder<Expediente> builder)
        {
            builder.ToTable(
                "expediente",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_expediente_numero",
                        "btrim(numero_expediente) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_expediente_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.ExpedienteId);

            builder.Property(x => x.ExpedienteId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroExpediente)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(x => x.FechaIngresoMesaPartes)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(1000);

            builder.Property(x => x.RegistradoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => x.NumeroExpediente)
                .IsUnique();

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasIndex(x => x.FechaIngresoMesaPartes);

            builder.HasOne(x => x.Caso)
                .WithOne(x => x.Expediente)
                .HasForeignKey<Expediente>(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.ExpedientesRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}