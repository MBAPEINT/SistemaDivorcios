using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class RegistroAuditoriaConfiguracion
        : IEntityTypeConfiguration<RegistroAuditoria>
    {
        public void Configure(
            EntityTypeBuilder<RegistroAuditoria> builder)
        {
            builder.ToTable(
                "registro_auditoria",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_auditoria_actor_tipo",
                        """
                        actor_tipo_codigo IN (
                            'SISTEMA',
                            'PUBLICO',
                            'USUARIO_INTERNO',
                            'CUENTA_CIUDADANA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_auditoria_actor",
                        """
                        (
                            actor_tipo_codigo IN (
                                'SISTEMA',
                                'PUBLICO'
                            )
                            AND usuario_interno_id IS NULL
                            AND cuenta_ciudadana_id IS NULL
                        )
                        OR
                        (
                            actor_tipo_codigo = 'USUARIO_INTERNO'
                            AND usuario_interno_id IS NOT NULL
                            AND cuenta_ciudadana_id IS NULL
                        )
                        OR
                        (
                            actor_tipo_codigo = 'CUENTA_CIUDADANA'
                            AND usuario_interno_id IS NULL
                            AND cuenta_ciudadana_id IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_auditoria_accion",
                        "btrim(accion_codigo) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_auditoria_recurso",
                        "btrim(recurso_codigo) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_auditoria_recurso_id",
                        """
                        recurso_id IS NULL
                        OR btrim(recurso_id) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_auditoria_resultado",
                        """
                        resultado_codigo IN (
                            'EXITO',
                            'RECHAZADO',
                            'ERROR'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_auditoria_descripcion",
                        """
                        descripcion IS NULL
                        OR btrim(descripcion) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_auditoria_ip",
                        """
                        direccion_ip IS NULL
                        OR btrim(direccion_ip) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_auditoria_user_agent",
                        """
                        user_agent IS NULL
                        OR btrim(user_agent) <> ''
                        """);
                });

            builder.HasKey(x => x.RegistroAuditoriaId);

            builder.Property(x => x.RegistroAuditoriaId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.ActorTipoCodigo)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(x => x.AccionCodigo)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.RecursoCodigo)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(x => x.RecursoId)
                .HasMaxLength(100);

            builder.Property(x => x.ResultadoCodigo)
                .HasMaxLength(15)
                .HasDefaultValue("EXITO")
                .IsRequired();

            builder.Property(x => x.Descripcion)
                .HasMaxLength(1000);

            builder.Property(x => x.DetalleJson)
                .HasColumnType("jsonb");

            builder.Property(x => x.DireccionIp)
                .HasMaxLength(64);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            builder.Property(x => x.CorrelacionId)
                .IsRequired();

            builder.Property(x => x.RegistradoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => x.CorrelacionId);

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.RegistradoEn
            });

            builder.HasIndex(x => new
            {
                x.UsuarioInternoId,
                x.RegistradoEn
            });

            builder.HasIndex(x => new
            {
                x.CuentaCiudadanaId,
                x.RegistradoEn
            });

            builder.HasIndex(x => new
            {
                x.RecursoCodigo,
                x.RecursoId
            });

            builder.HasIndex(x => new
            {
                x.AccionCodigo,
                x.RegistradoEn
            });

            builder.HasIndex(x => new
            {
                x.ResultadoCodigo,
                x.RegistradoEn
            });

            builder.HasOne(x => x.UsuarioInterno)
                .WithMany(x => x.RegistrosAuditoria)
                .HasForeignKey(x => x.UsuarioInternoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CuentaCiudadana)
                .WithMany(x => x.RegistrosAuditoria)
                .HasForeignKey(x => x.CuentaCiudadanaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.RegistrosAuditoria)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}