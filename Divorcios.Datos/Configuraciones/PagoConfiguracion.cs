using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PagoConfiguracion
        : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable(
                "pago",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_pago_numero",
                        "numero_pago > 0");

                    tabla.HasCheckConstraint(
                        "ck_pago_dni",
                        "dni_pagante_snapshot ~ '^[0-9]{8}$'");

                    tabla.HasCheckConstraint(
                        "ck_pago_nombre",
                        "btrim(nombre_pagante_snapshot) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_pago_concepto",
                        "btrim(concepto_codigo) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_pago_descripcion",
                        "btrim(concepto_descripcion_snapshot) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_pago_monto",
                        "monto > 0");

                    tabla.HasCheckConstraint(
                        "ck_pago_moneda",
                        "moneda_codigo = 'PEN'");

                    tabla.HasCheckConstraint(
                        "ck_pago_estado",
                        """
                        estado_codigo IN (
                            'PENDIENTE',
                            'PAGADO',
                            'ANULADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_voucher",
                        """
                        numero_voucher IS NULL
                        OR btrim(numero_voucher) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_referencia_caja",
                        """
                        referencia_caja IS NULL
                        OR btrim(referencia_caja) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_flujo",
                        """
                        (
                            estado_codigo = 'PENDIENTE'
                            AND pagado_en IS NULL
                            AND anulado_en IS NULL
                            AND numero_voucher IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'PAGADO'
                            AND pagado_en IS NOT NULL
                            AND anulado_en IS NULL
                            AND numero_voucher IS NOT NULL
                        )
                        OR
                        (
                            estado_codigo = 'ANULADO'
                            AND anulado_en IS NOT NULL
                            AND (
                                pagado_en IS NULL
                                OR numero_voucher IS NOT NULL
                            )
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_fechas",
                        """
                        (
                            ultima_consulta_caja_en IS NULL
                            OR ultima_consulta_caja_en >= solicitado_en
                        )
                        AND
                        (
                            pagado_en IS NULL
                            OR pagado_en >= solicitado_en
                        )
                        AND
                        (
                            anulado_en IS NULL
                            OR anulado_en >= solicitado_en
                        )
                        AND
                        (
                            pagado_en IS NULL
                            OR anulado_en IS NULL
                            OR anulado_en >= pagado_en
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_pago_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.PagoId);

            builder.Property(x => x.PagoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroPago)
                .IsRequired();

            builder.Property(x => x.DniPaganteSnapshot)
                .HasColumnType("char(8)")
                .IsRequired();

            builder.Property(x => x.NombrePaganteSnapshot)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.ConceptoCodigo)
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(x => x.ConceptoDescripcionSnapshot)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Monto)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.MonedaCodigo)
                .HasMaxLength(3)
                .HasDefaultValue("PEN")
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("PENDIENTE")
                .IsRequired();

            builder.Property(x => x.NumeroVoucher)
                .HasMaxLength(80);

            builder.Property(x => x.ReferenciaCaja)
                .HasMaxLength(120);

            builder.Property(x => x.SolicitadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.UltimaConsultaCajaEn);
            builder.Property(x => x.PagadoEn);
            builder.Property(x => x.AnuladoEn);

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.NumeroPago
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.EstadoCodigo,
                x.SolicitadoEn
            });

            builder.HasIndex(x => x.ExpedienteConyugePaganteId);

            builder.HasIndex(x => x.NumeroVoucher)
                .IsUnique()
                .HasFilter("numero_voucher IS NOT NULL");

            builder.HasIndex(x => x.ReferenciaCaja)
                .IsUnique()
                .HasFilter("referencia_caja IS NOT NULL");

            builder.HasIndex(x => x.DocumentoComprobanteId)
                .IsUnique();

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.Pagos)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExpedienteConyugePagante)
                .WithMany(x => x.PagosRealizados)
                .HasForeignKey(x => x.ExpedienteConyugePaganteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoComprobante)
                .WithOne(x => x.Pago)
                .HasForeignKey<Pago>(x => x.DocumentoComprobanteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.PagosRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
