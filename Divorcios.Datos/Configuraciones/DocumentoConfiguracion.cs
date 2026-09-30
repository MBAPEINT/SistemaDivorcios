using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class DocumentoConfiguracion
        : IEntityTypeConfiguration<Documento>
    {
        public void Configure(EntityTypeBuilder<Documento> builder)
        {
            builder.ToTable(
                "documento",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_documento_titulo",
                        "btrim(titulo) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_documento_etapa",
                        """
                        etapa_codigo IN (
                            'PRERREGISTRO',
                            'SEPARACION',
                            'DIVORCIO',
                            'CIERRE'
                        )
                        """);
                    tabla.HasCheckConstraint(
                        "ck_documento_estado",
                        """
                        estado_codigo IN (
                            'VIGENTE',
                            'REEMPLAZADO',
                            'ANULADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_documento_creador",
                        """
                        NOT (
                            creado_por_cuenta_id IS NOT NULL
                            AND creado_por_usuario_id IS NOT NULL
                        )
                        """);
                });

            builder.HasKey(x => x.DocumentoId);

            builder.Property(x => x.DocumentoId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Titulo)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.EtapaCodigo)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("VIGENTE")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.EtapaCodigo
            });

            builder.HasIndex(x => x.TipoDocumentoId);

            builder.HasIndex(x => x.PreregistroRequisitoId);

            builder.HasIndex(x => x.CreadoPorCuentaId);

            builder.HasIndex(x => x.CreadoPorUsuarioId);

            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TipoDocumento)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.TipoDocumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreregistroRequisito)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.PreregistroRequisitoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadoPorCuenta)
                .WithMany(x => x.DocumentosCreados)
                .HasForeignKey(x => x.CreadoPorCuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreadoPorUsuario)
                .WithMany(x => x.DocumentosCreados)
                .HasForeignKey(x => x.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}