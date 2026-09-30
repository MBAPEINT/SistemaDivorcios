using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ExpedienteConfiguracion
        : IEntityTypeConfiguration<Expediente>
    {
        public void Configure(EntityTypeBuilder<Expediente> builder)
        {
            builder.ToTable(
                "expediente",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_expediente_codigo_preregistro",
                        "btrim(codigo_preregistro) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_expediente_numero",
                        """
                        numero_expediente IS NULL
                        OR btrim(numero_expediente) <> ''
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_oficializacion",
                        """
                        (
                            numero_expediente IS NULL
                            AND fecha_ingreso_mesa_partes IS NULL
                            AND oficializado_en IS NULL
                        )
                        OR
                        (
                            numero_expediente IS NOT NULL
                            AND fecha_ingreso_mesa_partes IS NOT NULL
                            AND oficializado_en IS NOT NULL
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_bloqueo_preregistro",
                        """
                        preregistro_bloqueado_en IS NULL
                        OR preregistro_bloqueado_en >= fecha_inicio_digital
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_cierre",
                        """
                        cerrado_en IS NULL
                        OR cerrado_en >= fecha_inicio_digital
                        """);
                });

            builder.HasKey(x => x.ExpedienteId);

            builder.Property(x => x.ExpedienteId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.CodigoPreregistro)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.NumeroExpediente)
                .HasMaxLength(60);

            builder.Property(x => x.FechaInicioDigital)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.FechaIngresoMesaPartes)
                .HasColumnType("date");

            builder.Property(x => x.OficializadoEn);

            builder.Property(x => x.PreregistroBloqueadoEn);

            builder.Property(x => x.CerradoEn);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => x.CodigoPreregistro)
                .IsUnique();

            builder.HasIndex(x => x.NumeroExpediente)
                .IsUnique();

            builder.HasIndex(x => x.FechaIngresoMesaPartes);

            builder.HasIndex(x => x.CerradoEn);

            builder.HasIndex(x => x.CreadoPorCuentaId);

            builder.HasOne(x => x.CreadoPorCuenta)
                .WithMany(x => x.ExpedientesCreados)
                .HasForeignKey(x => x.CreadoPorCuentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}