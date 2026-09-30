using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class NotificacionConfiguracion
        : IEntityTypeConfiguration<Notificacion>
    {
        public void Configure(EntityTypeBuilder<Notificacion> builder)
        {
            builder.ToTable(
                "notificacion",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_notificacion_canal",
                        """
                        canal_codigo IN (
                            'WHATSAPP',
                            'LLAMADA',
                            'CORREO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_notificacion_tipo",
                        """
                        tipo_codigo IN (
                            'PRERREGISTRO_OBSERVADO',
                            'PRERREGISTRO_APROBADO',
                            'AUDIENCIA_PROGRAMADA',
                            'AUDIENCIA_REPROGRAMADA',
                            'SOLICITUD_OBSERVADA',
                            'RESOLUCION_DISPONIBLE',
                            'TRAMITE_FINALIZADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_notificacion_estado",
                        """
                        estado_codigo IN (
                            'PENDIENTE',
                            'PROCESANDO',
                            'ENVIADA',
                            'FALLIDA',
                            'CANCELADA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_notificacion_destino",
                        "btrim(destino) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_notificacion_contenido",
                        "btrim(contenido) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_notificacion_plantilla",
                        """
                        plantilla_codigo IS NULL
                        OR btrim(plantilla_codigo) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_notificacion_finalizacion",
                        """
                        (
                            estado_codigo IN (
                                'PENDIENTE',
                                'PROCESANDO'
                            )
                            AND finalizada_en IS NULL
                        )
                        OR
                        (
                            estado_codigo IN (
                                'ENVIADA',
                                'FALLIDA',
                                'CANCELADA'
                            )
                            AND finalizada_en IS NOT NULL
                            AND proximo_intento_en IS NULL
                        )
                        """);
                });

            builder.HasKey(x => x.NotificacionId);

            builder.Property(x => x.NotificacionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.CanalCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("WHATSAPP")
                .IsRequired();

            builder.Property(x => x.TipoCodigo)
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(x => x.Destino)
                .HasMaxLength(160)
                .IsRequired();

            builder.Property(x => x.PlantillaCodigo)
                .HasMaxLength(100);

            builder.Property(x => x.Contenido)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("PENDIENTE")
                .IsRequired();

            builder.Property(x => x.ProximoIntentoEn);

            builder.Property(x => x.FinalizadaEn);

            builder.Property(x => x.CreadaEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.EstadoCodigo,
                x.ProximoIntentoEn
            });

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.CreadaEn
            });

            builder.HasIndex(x => x.PersonaDestinatariaId);

            builder.HasIndex(x => x.CreadaPorUsuarioId);

            builder.HasIndex(x => x.TipoCodigo);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.Notificaciones)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PersonaDestinataria)
                .WithMany(x => x.NotificacionesRecibidas)
                .HasForeignKey(x => x.PersonaDestinatariaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadaPorUsuario)
                .WithMany(x => x.NotificacionesCreadas)
                .HasForeignKey(x => x.CreadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}