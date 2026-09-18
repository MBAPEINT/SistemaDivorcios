using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class IntentoNotificacionConfiguracion
        : IEntityTypeConfiguration<IntentoNotificacion>
    {
        public void Configure(
            EntityTypeBuilder<IntentoNotificacion> builder)
        {
            builder.ToTable(
                "intento_notificacion",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_numero",
                        "numero_intento > 0");

                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_resultado",
                        """
                        resultado_codigo IN (
                            'EN_PROCESO',
                            'EXITOSO',
                            'ERROR'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_http",
                        """
                        codigo_http IS NULL
                        OR codigo_http BETWEEN 100 AND 599
                        """);

                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_proveedor",
                        """
                        proveedor_mensaje_id IS NULL
                        OR btrim(proveedor_mensaje_id) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_error_vacio",
                        """
                        error IS NULL
                        OR btrim(error) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_fechas",
                        """
                        finalizado_en IS NULL
                        OR finalizado_en >= iniciado_en
                        """);

                    tabla.HasCheckConstraint(
                        "ck_intento_notificacion_finalizacion",
                        """
                        (
                            resultado_codigo = 'EN_PROCESO'
                            AND finalizado_en IS NULL
                            AND codigo_http IS NULL
                            AND proveedor_mensaje_id IS NULL
                            AND error IS NULL
                        )
                        OR
                        (
                            resultado_codigo = 'EXITOSO'
                            AND finalizado_en IS NOT NULL
                            AND error IS NULL
                        )
                        OR
                        (
                            resultado_codigo = 'ERROR'
                            AND finalizado_en IS NOT NULL
                            AND error IS NOT NULL
                            AND btrim(error) <> ''
                        )
                        """);
                });

            builder.HasKey(x => x.IntentoNotificacionId);

            builder.Property(x => x.IntentoNotificacionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroIntento)
                .IsRequired();

            builder.Property(x => x.ResultadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("EN_PROCESO")
                .IsRequired();

            builder.Property(x => x.ProveedorMensajeId)
                .HasMaxLength(200);

            builder.Property(x => x.Error)
                .HasMaxLength(2000);

            builder.Property(x => x.IniciadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.FinalizadoEn);

            builder.HasIndex(x => new
            {
                x.NotificacionId,
                x.NumeroIntento
            })
                .IsUnique();

            builder.HasIndex(x => x.EjecutadoPorUsuarioId);

            builder.HasIndex(x => x.ResultadoCodigo);

            builder.HasOne(x => x.Notificacion)
                .WithMany(x => x.Intentos)
                .HasForeignKey(x => x.NotificacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EjecutadoPorUsuario)
                .WithMany(x =>
                    x.IntentosNotificacionEjecutados)
                .HasForeignKey(x => x.EjecutadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}