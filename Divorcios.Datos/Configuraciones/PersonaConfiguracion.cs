using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class PersonaConfiguracion : IEntityTypeConfiguration<Persona>
    {
        public void Configure(EntityTypeBuilder<Persona> builder)
        {
            builder.ToTable("persona", "divorcios", tabla =>
            {
                tabla.HasCheckConstraint(
                    "ck_persona_dni",
                    "dni ~ '^[0-9]{8}$'");

                tabla.HasCheckConstraint(
                    "ck_persona_celular",
                    "celular IS NULL OR celular ~ '^\\+?[0-9]{9,15}$'");

                tabla.HasCheckConstraint(
                    "ck_persona_nombres",
                    "btrim(nombres) <> ''");

                tabla.HasCheckConstraint(
                    "ck_persona_apellido_paterno",
                    "btrim(apellido_paterno) <> ''");

                tabla.HasCheckConstraint(
                    "ck_persona_apellido_materno",
                    "btrim(apellido_materno) <> ''");

                tabla.HasCheckConstraint(
                    "ck_persona_verificacion_reniec",
                    """
                    (
                        verificado_reniec = FALSE
                        AND verificado_reniec_en IS NULL
                    )
                    OR
                    (
                        verificado_reniec = TRUE
                        AND verificado_reniec_en IS NOT NULL
                    )
                    """);

                tabla.HasCheckConstraint(
                    "ck_persona_actualizacion",
                    """
                    actualizado_en IS NULL
                    OR actualizado_en >= creado_en
                    """);
            });

            builder.HasKey(x => x.PersonaId);

            builder.Property(x => x.PersonaId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.Dni)
                .HasColumnType("char(8)")
                .IsRequired();

            builder.HasIndex(x => x.Dni)
                .IsUnique();

            builder.Property(x => x.Nombres)
                .HasMaxLength(120)
                .IsRequired();

            builder.Property(x => x.ApellidoPaterno)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.ApellidoMaterno)
                .HasMaxLength(80)
                .IsRequired();

            builder.Property(x => x.Celular)
                .HasMaxLength(15);

            builder.Property(x => x.Correo)
                .HasMaxLength(160);

            builder.Property(x => x.DireccionDni)
                .HasMaxLength(250);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("now()")
                .IsRequired();
            builder.Property(x => x.VerificadoReniec)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.VerificadoReniecEn);

            builder.Property(x => x.ActualizadoEn);
        }
    }
}
