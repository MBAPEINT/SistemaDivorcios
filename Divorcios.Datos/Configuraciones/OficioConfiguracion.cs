using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class OficioConfiguracion
        : IEntityTypeConfiguration<Oficio>
    {
        public void Configure(EntityTypeBuilder<Oficio> builder)
        {
            builder.ToTable(
                "oficio",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_oficio_estado",
                        """
                        estado_codigo IN (
                            'BORRADOR',
                            'EMITIDO',
                            'ENVIADO',
                            'RECIBIDO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_oficio_numero",
                        """
                        numero_oficio IS NULL
                        OR btrim(numero_oficio) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_oficio_flujo",
                        """
                        (
                            estado_codigo = 'BORRADOR'
                            AND numero_oficio IS NULL
                            AND fecha_emision IS NULL
                            AND enviado_en IS NULL
                            AND recibido_en IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'EMITIDO'
                            AND numero_oficio IS NOT NULL
                            AND btrim(numero_oficio) <> ''
                            AND fecha_emision IS NOT NULL
                            AND enviado_en IS NULL
                            AND recibido_en IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'ENVIADO'
                            AND numero_oficio IS NOT NULL
                            AND btrim(numero_oficio) <> ''
                            AND fecha_emision IS NOT NULL
                            AND enviado_en IS NOT NULL
                            AND recibido_en IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'RECIBIDO'
                            AND numero_oficio IS NOT NULL
                            AND btrim(numero_oficio) <> ''
                            AND fecha_emision IS NOT NULL
                            AND enviado_en IS NOT NULL
                            AND recibido_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_oficio_fechas",
                        """
                        recibido_en IS NULL
                        OR (
                            enviado_en IS NOT NULL
                            AND recibido_en >= enviado_en
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_oficio_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.OficioId);

            builder.Property(x => x.OficioId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("BORRADOR")
                .IsRequired();

            builder.Property(x => x.NumeroOficio)
                .HasMaxLength(80);

            builder.Property(x => x.FechaEmision)
                .HasColumnType("date");

            builder.Property(x => x.EnviadoEn);

            builder.Property(x => x.RecibidoEn);

            builder.Property(x => x.RegistradoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.ActuacionAdministrativaId,
                x.DestinoOficioId
            })
                .IsUnique();

            builder.HasIndex(x => x.DocumentoOficioId)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.NumeroOficio,
                x.FechaEmision
            });

            builder.HasIndex(x => x.DestinoOficioId);

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.ActuacionAdministrativa)
                .WithMany(x => x.Oficios)
                .HasForeignKey(x => x.ActuacionAdministrativaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DestinoOficio)
                .WithMany(x => x.Oficios)
                .HasForeignKey(x => x.DestinoOficioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DocumentoOficio)
                .WithOne(x => x.Oficio)
                .HasForeignKey<Oficio>(x => x.DocumentoOficioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.OficiosRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}