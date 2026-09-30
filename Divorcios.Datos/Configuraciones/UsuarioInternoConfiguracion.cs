using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Divorcios.Datos.Configuraciones
{
    public class UsuarioInternoConfiguracion
        : IEntityTypeConfiguration<UsuarioInterno>
    {
        public void Configure(EntityTypeBuilder<UsuarioInterno> builder)
        {
            builder.ToTable(
                "usuario_interno",
                "divorcios",
                tabla =>
                {
                    tabla.HasCheckConstraint(
                        "ck_usuario_rol",
                        "rol_codigo IN ('ADMIN', 'ABOGADA', 'ASISTENTE')");

                    tabla.HasCheckConstraint(
                        "ck_usuario_login",
                        "btrim(login) <> ''");

                    tabla.HasCheckConstraint(
                        "ck_usuario_hash",
                        "btrim(password_hash) <> ''");
                });

            builder.HasKey(x => x.UsuarioInternoId);

            builder.Property(x => x.UsuarioInternoId)
                .UseIdentityAlwaysColumn();
            builder.Property(x => x.UltimoAccesoEn);

            builder.Property(x => x.Login)
                .HasMaxLength(80)
                .IsRequired();

            builder.HasIndex(x => x.Login)
                .IsUnique();

            builder.Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.NombreVisible)
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(x => x.RolCodigo)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Activo)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreadoEn)
                .HasDefaultValueSql("now()")
                .IsRequired();
        }
    }
}