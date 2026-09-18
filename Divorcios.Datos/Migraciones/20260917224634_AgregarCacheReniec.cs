using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarCacheReniec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consulta_reniec",
                schema: "divorcios",
                columns: table => new
                {
                    consulta_reniec_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    dni_consultado = table.Column<string>(type: "char(8)", nullable: false),
                    prenombres = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    apellido_paterno = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    apellido_materno = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    resultado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    codigo_http = table.Column<short>(type: "smallint", nullable: true),
                    consultado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expira_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    persona_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consulta_reniec", x => x.consulta_reniec_id);
                    table.CheckConstraint("ck_consulta_reniec_datos_encontrados", "resultado_codigo <> 'ENCONTRADO' OR (prenombres IS NOT NULL AND apellido_paterno IS NOT NULL AND apellido_materno IS NOT NULL)");
                    table.CheckConstraint("ck_consulta_reniec_dni", "dni_consultado ~ '^[0-9]{8}$'");
                    table.CheckConstraint("ck_consulta_reniec_expiracion", "expira_en >= consultado_en");
                    table.CheckConstraint("ck_consulta_reniec_http", "codigo_http IS NULL OR codigo_http BETWEEN 100 AND 599");
                    table.CheckConstraint("ck_consulta_reniec_resultado", "resultado_codigo IN ('ENCONTRADO', 'NO_ENCONTRADO', 'ERROR')");
                    table.ForeignKey(
                        name: "fk_consulta_reniec_personas_persona_id",
                        column: x => x.persona_id,
                        principalSchema: "divorcios",
                        principalTable: "persona",
                        principalColumn: "persona_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_consulta_reniec_dni_consultado_consultado_en",
                schema: "divorcios",
                table: "consulta_reniec",
                columns: new[] { "dni_consultado", "consultado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_consulta_reniec_expira_en",
                schema: "divorcios",
                table: "consulta_reniec",
                column: "expira_en");

            migrationBuilder.CreateIndex(
                name: "ix_consulta_reniec_persona_id",
                schema: "divorcios",
                table: "consulta_reniec",
                column: "persona_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consulta_reniec",
                schema: "divorcios");
        }
    }
}
