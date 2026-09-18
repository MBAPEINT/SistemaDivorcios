using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class InicialIdentidadCaso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "divorcios");

            migrationBuilder.CreateTable(
                name: "persona",
                schema: "divorcios",
                columns: table => new
                {
                    persona_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    dni = table.Column<string>(type: "char(8)", nullable: false),
                    nombres = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    apellido_paterno = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    apellido_materno = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    celular = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    correo = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    direccion_dni = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    verificado_reniec_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persona", x => x.persona_id);
                    table.CheckConstraint("ck_persona_apellido_materno", "btrim(apellido_materno) <> ''");
                    table.CheckConstraint("ck_persona_apellido_paterno", "btrim(apellido_paterno) <> ''");
                    table.CheckConstraint("ck_persona_celular", "celular IS NULL OR celular ~ '^\\+?[0-9]{9,15}$'");
                    table.CheckConstraint("ck_persona_dni", "dni ~ '^[0-9]{8}$'");
                    table.CheckConstraint("ck_persona_nombres", "btrim(nombres) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "usuario_interno",
                schema: "divorcios",
                columns: table => new
                {
                    usuario_interno_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    login = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nombre_visible = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    rol_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_interno", x => x.usuario_interno_id);
                    table.CheckConstraint("ck_usuario_hash", "btrim(password_hash) <> ''");
                    table.CheckConstraint("ck_usuario_login", "btrim(login) <> ''");
                    table.CheckConstraint("ck_usuario_rol", "rol_codigo IN ('ADMIN', 'ABOGADA', 'ASISTENTE')");
                });

            migrationBuilder.CreateTable(
                name: "cuenta_ciudadana",
                schema: "divorcios",
                columns: table => new
                {
                    cuenta_ciudadana_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    persona_id = table.Column<long>(type: "bigint", nullable: false),
                    celular_verificado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bloqueado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_ciudadana", x => x.cuenta_ciudadana_id);
                    table.ForeignKey(
                        name: "fk_cuenta_ciudadana_personas_persona_id",
                        column: x => x.persona_id,
                        principalSchema: "divorcios",
                        principalTable: "persona",
                        principalColumn: "persona_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "caso",
                schema: "divorcios",
                columns: table => new
                {
                    caso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo_pre = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    creado_por_cuenta_id = table.Column<long>(type: "bigint", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_caso", x => x.caso_id);
                    table.CheckConstraint("ck_caso_fechas", "cerrado_en IS NULL OR cerrado_en >= creado_en");
                    table.ForeignKey(
                        name: "fk_caso_cuentas_ciudadanas_creado_por_cuenta_id",
                        column: x => x.creado_por_cuenta_id,
                        principalSchema: "divorcios",
                        principalTable: "cuenta_ciudadana",
                        principalColumn: "cuenta_ciudadana_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "validacion_identidad",
                schema: "divorcios",
                columns: table => new
                {
                    validacion_identidad_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cuenta_ciudadana_id = table.Column<long>(type: "bigint", nullable: false),
                    proveedor_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: "RENIEC"),
                    referencia_consulta = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    resultado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    validado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_validacion_identidad", x => x.validacion_identidad_id);
                    table.CheckConstraint("ck_validacion_resultado", "resultado_codigo IN ('VALIDO', 'NO_VALIDO', 'ERROR')");
                    table.ForeignKey(
                        name: "fk_validacion_identidad_cuenta_ciudadana_cuenta_ciudadana_id",
                        column: x => x.cuenta_ciudadana_id,
                        principalSchema: "divorcios",
                        principalTable: "cuenta_ciudadana",
                        principalColumn: "cuenta_ciudadana_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "caso_conyuge",
                schema: "divorcios",
                columns: table => new
                {
                    caso_conyuge_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    persona_id = table.Column<long>(type: "bigint", nullable: false),
                    posicion_codigo = table.Column<string>(type: "char(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_caso_conyuge", x => x.caso_conyuge_id);
                    table.CheckConstraint("ck_conyuge_posicion", "posicion_codigo IN ('A', 'B')");
                    table.ForeignKey(
                        name: "fk_caso_conyuge_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_caso_conyuge_personas_persona_id",
                        column: x => x.persona_id,
                        principalSchema: "divorcios",
                        principalTable: "persona",
                        principalColumn: "persona_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_caso_codigo_pre",
                schema: "divorcios",
                table: "caso",
                column: "codigo_pre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_caso_creado_por_cuenta_id",
                schema: "divorcios",
                table: "caso",
                column: "creado_por_cuenta_id");

            migrationBuilder.CreateIndex(
                name: "ix_caso_conyuge_caso_id_persona_id",
                schema: "divorcios",
                table: "caso_conyuge",
                columns: new[] { "caso_id", "persona_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_caso_conyuge_caso_id_posicion_codigo",
                schema: "divorcios",
                table: "caso_conyuge",
                columns: new[] { "caso_id", "posicion_codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_caso_conyuge_persona_id",
                schema: "divorcios",
                table: "caso_conyuge",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_ciudadana_persona_id",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persona_dni",
                schema: "divorcios",
                table: "persona",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_interno_login",
                schema: "divorcios",
                table: "usuario_interno",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_validacion_identidad_cuenta_ciudadana_id",
                schema: "divorcios",
                table: "validacion_identidad",
                column: "cuenta_ciudadana_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caso_conyuge",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "usuario_interno",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "validacion_identidad",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "caso",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "cuenta_ciudadana",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "persona",
                schema: "divorcios");
        }
    }
}
