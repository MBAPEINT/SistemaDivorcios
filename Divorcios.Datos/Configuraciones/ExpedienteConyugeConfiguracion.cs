using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class ExpedienteConyugeConfiguracion
        : IEntityTypeConfiguration<ExpedienteConyuge>
    {
        public void Configure(EntityTypeBuilder<ExpedienteConyuge> builder)
        {
            builder.ToTable(
                "expediente_conyuge",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_conyuge_posicion",
                        "posicion_codigo IN ('A', 'B')");
                });

            builder.HasKey(x => x.ExpedienteConyugeId);

            builder.Property(x => x.ExpedienteConyugeId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.ExpedienteId)
                .IsRequired();

            builder.Property(x => x.PersonaId)
                .IsRequired();

            builder.Property(x => x.PosicionCodigo)
                .HasColumnType("char(1)")
                .IsRequired();

            builder.Property(x => x.EsIniciador)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.PosicionCodigo
            })
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.ExpedienteId,
                x.PersonaId
            })
                .IsUnique();

            builder.HasIndex(x => x.PersonaId);

            builder.HasOne(x => x.Expediente)
                .WithMany(x => x.Conyuges)
                .HasForeignKey(x => x.ExpedienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Persona)
                .WithMany(x => x.ParticipacionesExpediente)
                .HasForeignKey(x => x.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ExpedienteId)
                .IsUnique()
                .HasFilter("es_iniciador = TRUE")
                .HasDatabaseName("ux_expediente_un_iniciador");
        }
    }
}