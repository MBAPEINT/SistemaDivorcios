using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class CuentaCiudadanaConfiguracion
        : IEntityTypeConfiguration<CuentaCiudadana>
    {
        public void Configure(
            EntityTypeBuilder<CuentaCiudadana> builder)
        {
            builder.ToTable(
                "cuenta_ciudadana",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_cuenta_ciudadana_estado",
                        """
                        estado_codigo IN (
                            'ACTIVA',
                            'BLOQUEADA',
                            'INACTIVA'
                        )
                        """);

                    tabla.HasCheckConstraint(
                        "ck_cuenta_ciudadana_intentos",
                        "intentos_fallidos >= 0");

                    tabla.HasCheckConstraint(
                        "ck_cuenta_ciudadana_bloqueo",
                        """
                        estado_codigo <> 'BLOQUEADA'
                        OR bloqueado_hasta IS NOT NULL
                        """);

                    tabla.HasCheckConstraint(
                        "ck_cuenta_ciudadana_hash",
                        """
                        clave_hash IS NULL
                        OR btrim(clave_hash) <> ''
                        """);
                });

            builder.HasKey(x => x.CuentaCiudadanaId);

            builder.Property(x => x.CuentaCiudadanaId)
                .UseIdentityAlwaysColumn();

            builder.Property(x => x.PersonaId)
                .IsRequired();

            builder.Property(x => x.CelularVerificadoEn);
            builder.Property(x => x.CorreoVerificadoEn);

            builder.Property(x => x.ClaveHash)
                .HasMaxLength(255);

            builder.Property(x => x.EstadoCodigo)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVA")
                .IsRequired();

            builder.Property(x => x.IntentosFallidos)
                .HasDefaultValue((short)0)
                .IsRequired();

            builder.Property(x => x.BloqueadoHasta);
            builder.Property(x => x.UltimoAccesoEn);

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.HasIndex(x => x.PersonaId)
                .IsUnique();

            builder.HasIndex(x => x.EstadoCodigo);

            builder.HasOne(x => x.Persona)
                .WithOne(x => x.CuentaCiudadana)
                .HasForeignKey<CuentaCiudadana>(x => x.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}