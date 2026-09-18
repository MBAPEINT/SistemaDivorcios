using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class DocumentoVersionConfiguracion
        : IEntityTypeConfiguration<DocumentoVersion>
    {
        public void Configure(
            EntityTypeBuilder<DocumentoVersion> builder)
        {
            builder.ToTable(
                "documento_version",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_documento_version_numero",
                        "numero_version > 0");

                    tabla.HasCheckConstraint(
                        "ck_documento_version_tamano",
                        "tamano_bytes > 0");

                    tabla.HasCheckConstraint(
                        "ck_documento_version_nombre",
                        "btrim(nombre_archivo) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_documento_version_mime",
                        "btrim(mime_type) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_documento_version_clave",
                        "btrim(almacenamiento_clave) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_documento_version_sha256",
                        "sha256 ~ '^[0-9a-f]{64}$'");

                    tabla.HasCheckConstraint(
                        "ck_documento_version_cargador",
                        """
                        (
                            cargado_por_cuenta_id IS NOT NULL
                            AND cargado_por_usuario_id IS NULL
                        )
                        OR
                        (
                            cargado_por_cuenta_id IS NULL
                            AND cargado_por_usuario_id IS NOT NULL
                        )
                        """);
                });

            builder.HasKey(x => x.DocumentoVersionId);

            builder.Property(x => x.DocumentoVersionId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroVersion)
                .IsRequired();

            builder.Property(x => x.NombreArchivo)
                .HasMaxLength(240)
                .IsRequired();

            builder.Property(x => x.MimeType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.TamanoBytes)
                .IsRequired();

            builder.Property(x => x.AlmacenamientoClave)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Sha256)
                .HasColumnType("character(64)")
                .IsRequired();

            builder.Property(x => x.CargadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.DocumentoId,
                x.NumeroVersion
            })
                .IsUnique();

            builder.HasIndex(x => x.AlmacenamientoClave)
                .IsUnique();

            builder.HasIndex(x => x.Sha256);

            builder.HasIndex(x => x.CargadoPorCuentaId);

            builder.HasIndex(x => x.CargadoPorUsuarioId);

            builder.HasOne(x => x.Documento)
                .WithMany(x => x.Versiones)
                .HasForeignKey(x => x.DocumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CargadoPorCuenta)
                .WithMany(x => x.VersionesDocumentoCargadas)
                .HasForeignKey(x => x.CargadoPorCuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CargadoPorUsuario)
                .WithMany(x => x.VersionesDocumentoCargadas)
                .HasForeignKey(x => x.CargadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}