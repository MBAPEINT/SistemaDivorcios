using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ExpedienteContactoHistorialConfiguracion
        : IEntityTypeConfiguration<ExpedienteContactoHistorial>
    {
        public void Configure(
            EntityTypeBuilder<ExpedienteContactoHistorial> builder)
        {
            builder.ToTable(
                "expediente_contacto_historial",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_expediente_contacto_tipo",
                        """
                        tipo_contacto_codigo IN (
                            'CELULAR',
                            'CORREO',
                            'DIRECCION'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_contacto_valor",
                        "btrim(valor) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_expediente_contacto_fuente",
                        """
                        fuente_codigo IN (
                            'PRERREGISTRO',
                            'RENIEC',
                            'MUNICIPALIDAD'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_contacto_origen",
                        """
                        fuente_codigo <> 'PRERREGISTRO'
                        OR preregistro_version_origen_id IS NOT NULL
                        """);

                    tabla.HasCheckConstraint(
                        "ck_expediente_contacto_vigencia",
                        """
                        vigente_hasta IS NULL
                        OR vigente_hasta >= vigente_desde
                        """);
                });

            builder.HasKey(x => x.ExpedienteContactoHistorialId);

            builder.Property(x => x.ExpedienteContactoHistorialId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.TipoContactoCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Valor)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.FuenteCodigo)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.VigenteDesde)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.VigenteHasta);

            builder.HasIndex(x => new
            {
                x.ExpedienteConyugeId,
                x.TipoContactoCodigo,
                x.VigenteDesde
            });

            // Solo puede existir un contacto vigente de cada tipo
            // para un mismo participante del expediente.
            builder.HasIndex(x => new
            {
                x.ExpedienteConyugeId,
                x.TipoContactoCodigo
            })
                .IsUnique()
                .HasFilter("vigente_hasta IS NULL");

            builder.HasIndex(x => x.PreregistroVersionOrigenId);

            builder.HasIndex(x => x.RegistradoPorUsuarioId);

            builder.HasOne(x => x.ExpedienteConyuge)
                .WithMany(x => x.ContactosHistorial)
                .HasForeignKey(x => x.ExpedienteConyugeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreregistroVersionOrigen)
                .WithMany(x => x.ContactosExpedienteOriginados)
                .HasForeignKey(x => x.PreregistroVersionOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistradoPorUsuario)
                .WithMany(x => x.ContactosExpedienteRegistrados)
                .HasForeignKey(x => x.RegistradoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}