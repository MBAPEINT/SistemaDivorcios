using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarOficios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "oficio",
                schema: "divorcios",
                columns: table => new
                {
                    oficio_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    actuacion_administrativa_id = table.Column<long>(type: "bigint", nullable: false),
                    destino_oficio_id = table.Column<short>(type: "smallint", nullable: false),
                    documento_oficio_id = table.Column<long>(type: "bigint", nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "BORRADOR"),
                    numero_oficio = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: true),
                    enviado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    recibido_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_oficio", x => x.oficio_id);
                    table.CheckConstraint("ck_oficio_estado", "estado_codigo IN (\r\n    'BORRADOR',\r\n    'EMITIDO',\r\n    'ENVIADO',\r\n    'RECIBIDO'\r\n)");
                    table.CheckConstraint("ck_oficio_fechas", "recibido_en IS NULL\r\nOR (\r\n    enviado_en IS NOT NULL\r\n    AND recibido_en >= enviado_en\r\n)");
                    table.CheckConstraint("ck_oficio_flujo", "(\r\n    estado_codigo = 'BORRADOR'\r\n    AND numero_oficio IS NULL\r\n    AND fecha_emision IS NULL\r\n    AND enviado_en IS NULL\r\n    AND recibido_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'EMITIDO'\r\n    AND numero_oficio IS NOT NULL\r\n    AND btrim(numero_oficio) <> ''\r\n    AND fecha_emision IS NOT NULL\r\n    AND enviado_en IS NULL\r\n    AND recibido_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'ENVIADO'\r\n    AND numero_oficio IS NOT NULL\r\n    AND btrim(numero_oficio) <> ''\r\n    AND fecha_emision IS NOT NULL\r\n    AND enviado_en IS NOT NULL\r\n    AND recibido_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'RECIBIDO'\r\n    AND numero_oficio IS NOT NULL\r\n    AND btrim(numero_oficio) <> ''\r\n    AND fecha_emision IS NOT NULL\r\n    AND enviado_en IS NOT NULL\r\n    AND recibido_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_oficio_numero", "numero_oficio IS NULL\r\nOR btrim(numero_oficio) <> ''");
                    table.CheckConstraint("ck_oficio_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.ForeignKey(
                        name: "fk_oficio_actuacion_administrativa_actuacion_administrativa_id",
                        column: x => x.actuacion_administrativa_id,
                        principalSchema: "divorcios",
                        principalTable: "actuacion_administrativa",
                        principalColumn: "actuacion_administrativa_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_oficio_destino_oficio_destino_oficio_id",
                        column: x => x.destino_oficio_id,
                        principalSchema: "divorcios",
                        principalTable: "destino_oficio",
                        principalColumn: "destino_oficio_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_oficio_documento_documento_oficio_id",
                        column: x => x.documento_oficio_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_oficio_usuarios_internos_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_oficio_actuacion_administrativa_id_destino_oficio_id",
                schema: "divorcios",
                table: "oficio",
                columns: new[] { "actuacion_administrativa_id", "destino_oficio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_oficio_destino_oficio_id",
                schema: "divorcios",
                table: "oficio",
                column: "destino_oficio_id");

            migrationBuilder.CreateIndex(
                name: "ix_oficio_documento_oficio_id",
                schema: "divorcios",
                table: "oficio",
                column: "documento_oficio_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_oficio_estado_codigo",
                schema: "divorcios",
                table: "oficio",
                column: "estado_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_oficio_numero_oficio_fecha_emision",
                schema: "divorcios",
                table: "oficio",
                columns: new[] { "numero_oficio", "fecha_emision" });

            migrationBuilder.CreateIndex(
                name: "ix_oficio_registrado_por_usuario_id",
                schema: "divorcios",
                table: "oficio",
                column: "registrado_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "oficio",
                schema: "divorcios");
        }
    }
}
