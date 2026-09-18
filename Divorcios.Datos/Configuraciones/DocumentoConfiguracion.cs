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

            builder.HasIndex(x => new
            {
                x.CasoId,
                x.EtapaCodigo
            });

            builder.HasIndex(x => x.TipoDocumentoId);

            builder.HasIndex(x => x.PreregistroRequisitoId);

            builder.HasIndex(x => x.ActuacionAdministrativaId);

            builder.HasOne(x => x.Caso)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.CasoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TipoDocumento)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.TipoDocumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreregistroRequisito)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.PreregistroRequisitoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ActuacionAdministrativa)
                .WithMany(x => x.Documentos)
                .HasForeignKey(x => x.ActuacionAdministrativaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}