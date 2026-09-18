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
                            'ADMISIBILIDAD',
                            'SEPARACION_CONVENCIONAL',
                            'DISOLUCION_VINCULO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_estado",
                        """
                        estado_codigo IN (
                            'BORRADOR',
                            'EMITIDA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_numero_resolucion",
                        """
                        numero_resolucion IS NULL
                        OR btrim(numero_resolucion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_datos_emision",
                        """
                        (
                            estado_codigo = 'BORRADOR'
                            AND numero_resolucion IS NULL
                            AND fecha_emision IS NULL
                            AND emitida_por_usuario_id IS NULL
                            AND emitida_en IS NULL
                        )
                        OR
                        (
                            estado_codigo = 'EMITIDA'
                            AND numero_resolucion IS NOT NULL
                            AND btrim(numero_resolucion) <> ''
                            AND fecha_emision IS NOT NULL
                            AND emitida_por_usuario_id IS NOT NULL
                            AND emitida_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_actuacion_fecha_registro_emision",
                        """
                        emitida_en IS NULL
                        OR emitida_en >= creada_en
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
                .HasMaxLength(35)
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("BORRADOR")
                .IsRequired();

            builder.Property(x => x.NumeroResolucion)
                .HasMaxLength(80);

            builder.Property(x => x.FechaEmision)
                .HasColumnType("date");

            builder.Property(x => x.CreadaEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.EmitidaEn);

            builder.Property(x => x.Observacion)
                .HasMaxLength(2000);

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.TipoCodigo
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.NumeroResolucion,
                x.FechaEmision
            });

            builder.HasIndex(x => x.CreadaPorUsuarioId);

            builder.HasIndex(x => x.EmitidaPorUsuarioId);

            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.Caso)
                .WithMany(x => x.ActuacionesAdministrativas)
                .HasForeignKey(x => x.CasoId)
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