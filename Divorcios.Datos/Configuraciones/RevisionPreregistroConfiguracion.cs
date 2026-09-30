using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class RevisionPreregistroConfiguracion
        : IEntityTypeConfiguration<RevisionPreregistro>
    {
        public void Configure(
            EntityTypeBuilder<RevisionPreregistro> builder)
        {
            builder.ToTable(
                "revision_preregistro",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_revision_preregistro_numero",
                        "numero_revision > 0");

                    tabla.HasCheckConstraint(
                        "ck_revision_preregistro_resultado",
                        """
                        resultado_codigo IN (
                            'EN_REVISION',
                            'OBSERVADO',
                            'APROBADO'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_revision_preregistro_fechas",
                        """
                        finalizada_en IS NULL
                        OR finalizada_en >= iniciada_en
                        """);

                    tabla.HasCheckConstraint(
                        "ck_revision_preregistro_finalizacion",
                        """
                        (
                            resultado_codigo = 'EN_REVISION'
                            AND finalizada_en IS NULL
                        )
                        OR
                        (
                            resultado_codigo IN (
                                'OBSERVADO',
                                'APROBADO'
                            )
                            AND finalizada_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_revision_preregistro_comentario",
                        """
                        comentario_general IS NULL
                        OR btrim(comentario_general) <> ''
                        """);
                });

            builder.HasKey(x => x.RevisionPreregistroId);

            builder.Property(x => x.RevisionPreregistroId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.NumeroRevision)
                .IsRequired();

            builder.Property(x => x.ResultadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("EN_REVISION")
                .IsRequired();

            builder.Property(x => x.ComentarioGeneral)
                .HasMaxLength(2000);

            builder.Property(x => x.IniciadaEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.FinalizadaEn);

            builder.HasIndex(x => new
            {
                x.PreregistroVersionId,
                x.NumeroRevision
            })
                .IsUnique();

            builder.HasIndex(x => x.RevisadoPorUsuarioId);

            builder.HasIndex(x => x.ResultadoCodigo);

            builder.HasOne(x => x.PreregistroVersion)
                .WithMany(x => x.Revisiones)
                .HasForeignKey(x => x.PreregistroVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RevisadoPorUsuario)
                .WithMany(x => x.RevisionesPrerregistro)
                .HasForeignKey(x => x.RevisadoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}