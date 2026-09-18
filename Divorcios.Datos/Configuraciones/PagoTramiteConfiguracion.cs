using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PagoTramiteConfiguracion
        : IEntityTypeConfiguration<PagoTramite>
    {
        public void Configure(EntityTypeBuilder<PagoTramite> builder)
        {
            builder.ToTable(
                "pago_tramite",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_pago_tramite_concepto",
                        """
                        concepto_codigo IN (
                            'COPIAS_CERTIFICADAS',
                            'TASA_PROCEDIMIENTO',
                            'OTRO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_tramite_voucher",
                        "btrim(numero_voucher) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_pago_tramite_monto",
                        "monto > 0");

                    tabla.HasCheckConstraint(
                        "ck_pago_tramite_moneda",
                        "moneda_codigo = 'PEN'");

                    tabla.HasCheckConstraint(
                        "ck_pago_tramite_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_tramite_otro",
                        """
                        concepto_codigo <> 'OTRO'
                        OR (
                            observacion IS NOT NULL
                            AND btrim(observacion) <> ''
                        )
                        """);
                });

            builder.HasKey(x => x.PagoTramiteId);

            builder.Property(x => x.PagoTramiteId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.ConceptoCodigo)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.NumeroVoucher)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.Monto)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.MonedaCodigo)
                .HasMaxLength(3)
                .HasDefaultValue("PEN")
                .IsRequired();

            builder.Property(x => x.FechaPago)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.RegistradoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.NumeroVoucher,
                x.FechaPago
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.FechaPago
            });

            builder.HasIndex(x => x.DocumentoComprobanteId)
                .IsUnique();

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasIndex(x => x.ConceptoCodigo);

            builder.HasOne(x => x.Caso)
                .WithMany(x => x.Pagos)
                .HasForeignKey(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoComprobante)
                .WithOne(x => x.PagoTramite)
                .HasForeignKey<PagoTramite>(
                    x => x.DocumentoComprobanteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.PagosRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}