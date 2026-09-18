using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ConsultaReniecConfiguracion
        : IEntityTypeConfiguration<ConsultaReniec>
    {
        public void Configure(EntityTypeBuilder<ConsultaReniec> builder)
        {
            builder.ToTable(
                "consulta_reniec",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_consulta_reniec_dni",
                        "dni_consultado ~ '^[0-9]{8}$'");

                    tabla.HasCheckConstraint(
                        "ck_consulta_reniec_resultado",
                        "resultado_codigo IN " +
                        "('ENCONTRADO', 'NO_ENCONTRADO', 'ERROR')");

                    tabla.HasCheckConstraint(
                        "ck_consulta_reniec_expiracion",
                        "expira_en >= consultado_en");

                    tabla.HasCheckConstraint(
                        "ck_consulta_reniec_http",
                        "codigo_http IS NULL " +
                        "OR codigo_http BETWEEN 100 AND 599");

                    tabla.HasCheckConstraint(
                        "ck_consulta_reniec_datos_encontrados",
                        "resultado_codigo <> 'ENCONTRADO' OR " +
                        "(prenombres IS NOT NULL " +
                        "AND apellido_paterno IS NOT NULL " +
                        "AND apellido_materno IS NOT NULL)");
                });

            builder.HasKey(x => x.ConsultaReniecId);

            builder.Property(x => x.ConsultaReniecId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.DniConsultado)
                .HasColumnType("char(8)")
                .IsRequired();

            builder.Property(x => x.Prenombres)
                .HasMaxLength(120);

            builder.Property(x => x.ApellidoPaterno)
                .HasMaxLength(80);

            builder.Property(x => x.ApellidoMaterno)
                .HasMaxLength(80);

            builder.Property(x => x.Direccion)
                .HasMaxLength(250);

            builder.Property(x => x.ResultadoCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CodigoHttp);

            builder.Property(x => x.ConsultadoEn)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.ExpiraEn)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.DniConsultado,
                x.ConsultadoEn
            });

            builder.HasIndex(x => x.ExpiraEn);

            builder.HasOne(x => x.Persona)
                .WithMany(x => x.ConsultasReniec)
                .HasForeignKey(x => x.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}