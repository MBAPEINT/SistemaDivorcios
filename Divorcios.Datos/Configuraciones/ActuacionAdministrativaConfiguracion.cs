using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ActuacionAdministrativaConfiguracion
        : IEntityTypeConfiguration<ActuacionAdministrativa>
    {
        public void Configure(
            EntityTypeBuilder<ActuacionAdministrativa> builder)
        {
            builder.ToTable(
                "actuacion_administrativa",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_actuacion_tipo",
                        """
                        tipo_codigo IN (
                            'INFORME_ADMISIBILIDAD',
                            'RESOLUCION_ADMISIBILIDAD',
                            'INFORME_SEPARACION',
                            'RESOLUCION_SEPARACION',
                            'INFORME_DISOLUCION',
                            'RESOLUCION_DISOLUCION'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_secuencia",
                        "numero_secuencia > 0");

                    tabla.HasCheckConstraint(
                        "ck_actuacion_estado",
                        """
                        estado_codigo IN (
                            'BORRADOR',
                            'EMITIDA',
                            'ANULADA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_numero_resolucion",
                        """
                        numero_resolucion IS NULL
                        OR btrim(numero_resolucion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_emision",
                        """
                        (
                            estado_codigo = 'BORRADOR'
                            AND fecha_emision IS NULL
                            AND emitida_por_usuario_id IS NULL
                            AND emitida_en IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'EMITIDA'
                            AND fecha_emision IS NOT NULL
                            AND emitida_por_usuario_id IS NOT NULL
                            AND emitida_en IS NOT NULL
                        )
                        OR estado_codigo = 'ANULADA'
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_fecha_emision",
                        """
                        emitida_en IS NULL
                        OR emitida_en >= creada_en
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_fecha_notificacion",
                        """
                        fecha_notificacion IS NULL
                        OR (
                            fecha_emision IS NOT NULL
                            AND fecha_notificacion >= fecha_emision
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_observacion",
                        """
                        observacion IS NULL
                        OR btrim(observacion) <> ''
                        """);
                });

            builder.HasKey(x => x.ActuacionAdministrativaId);

            builder.Property(x => x.ActuacionAdministrativaId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.TipoCodigo)
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(x => x.NumeroSecuencia)
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("BORRADOR")
                .IsRequired();

            builder.Property(x => x.NumeroResolucion)
                .HasMaxLength(80);

            builder.Property(x => x.FechaEmision)
                .HasColumnType("date");

            builder.Property(x => x.FechaNotificacion)
                .HasColumnType("date");

            builder.Property(x => x.CreadaEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.EmitidaEn);

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.TipoCodigo,
                x.NumeroSecuencia
            })
                .IsUnique();

            builder.HasIndex(x => x.DocumentoId)
                .IsUnique();

            builder.HasIndex(x => x.NumeroResolucion);

            builder.HasIndex(x => x.CreadaPorUsuarioId);
            builder.HasIndex(x => x.EmitidaPorUsuarioId);
            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.ActuacionesAdministrativas)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Documento)
                .WithOne(x => x.ActuacionAdministrativa)
                .HasForeignKey<ActuacionAdministrativa>(
                    x => x.DocumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadaPorUsuario)
                .WithMany(x =>
                    x.ActuacionesAdministrativasCreadas)
                .HasForeignKey(x => x.CreadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EmitidaPorUsuario)
                .WithMany(x =>
                    x.ActuacionesAdministrativasEmitidas)
                .HasForeignKey(x => x.EmitidaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}