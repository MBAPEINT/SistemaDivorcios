using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarActuacionesAdministrativas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "actuacion_administrativa",
                schema: "divorcios",
                columns: table => new
                {
                    actuacion_administrativa_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_codigo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "BORRADOR"),
                    numero_resolucion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: true),
                    creada_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    creada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    emitida_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    emitida_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actuacion_administrativa", x => x.actuacion_administrativa_id);
                    table.CheckConstraint("ck_actuacion_datos_emision", "(\r\n    estado_codigo = 'BORRADOR'\r\n    AND numero_resolucion IS NULL\r\n    AND fecha_emision IS NULL\r\n    AND emitida_por_usuario_id IS NULL\r\n    AND emitida_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'EMITIDA'\r\n    AND numero_resolucion IS NOT NULL\r\n    AND btrim(numero_resolucion) <> ''\r\n    AND fecha_emision IS NOT NULL\r\n    AND emitida_por_usuario_id IS NOT NULL\r\n    AND emitida_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_actuacion_estado", "estado_codigo IN (\r\n    'BORRADOR',\r\n    'EMITIDA'\r\n)");
                    table.CheckConstraint("ck_actuacion_fecha_registro_emision", "emitida_en IS NULL\r\nOR emitida_en >= creada_en");
                    table.CheckConstraint("ck_actuacion_numero_resolucion", "numero_resolucion IS NULL\r\nOR btrim(numero_resolucion) <> ''");
                    table.CheckConstraint("ck_actuacion_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_actuacion_tipo", "tipo_codigo IN (\r\n    'ADMISIBILIDAD',\r\n    'SEPARACION_CONVENCIONAL',\r\n    'DISOLUCION_VINCULO'\r\n)");
                    table.ForeignKey(
                        name: "fk_actuacion_administrativa_casos_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actuacion_administrativa_usuarios_internos_creada_por_usuar",
                        column: x => x.creada_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actuacion_administrativa_usuarios_internos_emitida_por_usua",
                        column: x => x.emitida_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documento_actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento",
                column: "actuacion_administrativa_id");

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_caso_id_tipo_codigo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                columns: new[] { "caso_id", "tipo_codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_creada_por_usuario_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "creada_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_emitida_por_usuario_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "emitida_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_estado_codigo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "estado_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_numero_resolucion_fecha_emision",
                schema: "divorcios",
                table: "actuacion_administrativa",
                columns: new[] { "numero_resolucion", "fecha_emision" });

            migrationBuilder.AddForeignKey(
                name: "fk_documento_actuacion_administrativa_actuacion_administrativa",
                schema: "divorcios",
                table: "documento",
                column: "actuacion_administrativa_id",
                principalSchema: "divorcios",
                principalTable: "actuacion_administrativa",
                principalColumn: "actuacion_administrativa_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documento_actuacion_administrativa_actuacion_administrativa",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropTable(
                name: "actuacion_administrativa",
                schema: "divorcios");

            migrationBuilder.DropIndex(
                name: "ix_documento_actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropColumn(
                name: "actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento");
        }
    }
}
