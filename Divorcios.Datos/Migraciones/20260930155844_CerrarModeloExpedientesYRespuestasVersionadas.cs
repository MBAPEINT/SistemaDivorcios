using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CerrarModeloExpedientesYRespuestasVersionadas : Migration
    {
        // Borrador acumulado desde el esquema antiguo de Caso. No convierte datos
        // históricos: exige un esquema sin filas antes de cambiar su estructura.
        // Si hay datos, preparar otra estrategia de conservación antes de aplicar.
        private static void ExigirEsquemaSinDatos(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $revision_modelo$
                DECLARE
                    tabla RECORD;
                    hay_datos BOOLEAN;
                BEGIN
                    FOR tabla IN
                        SELECT schemaname, tablename
                        FROM pg_tables
                        WHERE schemaname = 'divorcios'
                          AND tablename <> '__EFMigrationsHistory'
                        ORDER BY tablename
                    LOOP
                        EXECUTE format(
                            'LOCK TABLE %I.%I IN ACCESS EXCLUSIVE MODE',
                            tabla.schemaname, tabla.tablename);
                        EXECUTE format(
                            'SELECT EXISTS (SELECT 1 FROM %I.%I LIMIT 1)',
                            tabla.schemaname, tabla.tablename)
                            INTO hay_datos;
                        IF hay_datos THEN
                            RAISE EXCEPTION
                                'Migracion de revision detenida: %.% contiene datos. Preparar una conversion que los conserve.',
                                tabla.schemaname, tabla.tablename;
                        END IF;
                    END LOOP;
                END;
                $revision_modelo$;
                """);
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExigirEsquemaSinDatos(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "fk_actuacion_administrativa_casos_caso_id",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropForeignKey(
                name: "fk_asistencia_audiencia_casos_conyuges_caso_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropForeignKey(
                name: "fk_audiencia_ratificacion_casos_caso_id",
                schema: "divorcios",
                table: "audiencia_ratificacion");

            migrationBuilder.DropForeignKey(
                name: "fk_documento_actuacion_administrativa_actuacion_administrativa",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropForeignKey(
                name: "fk_documento_caso_caso_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropForeignKey(
                name: "fk_expediente_caso_caso_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropForeignKey(
                name: "fk_expediente_usuarios_internos_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropForeignKey(
                name: "fk_notificacion_caso_caso_id",
                schema: "divorcios",
                table: "notificacion");

            migrationBuilder.DropForeignKey(
                name: "fk_preregistro_caso_caso_id",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropForeignKey(
                name: "fk_preregistro_requisito_preregistro_preregistro_id",
                schema: "divorcios",
                table: "preregistro_requisito");

            migrationBuilder.DropForeignKey(
                name: "fk_registro_auditoria_caso_caso_id",
                schema: "divorcios",
                table: "registro_auditoria");

            migrationBuilder.DropForeignKey(
                name: "fk_representacion_caso_conyuge_caso_conyuge_id",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropForeignKey(
                name: "fk_revision_preregistro_preregistro_preregistro_id",
                schema: "divorcios",
                table: "revision_preregistro");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitante_disolucion_caso_conyuge_caso_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitud_disolucion_caso_caso_id",
                schema: "divorcios",
                table: "solicitud_disolucion");

            migrationBuilder.DropTable(
                name: "caso_conyuge",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "pago_tramite",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "plazo_caso",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "historial_estado_caso",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "caso",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "estado_caso",
                schema: "divorcios");

            migrationBuilder.DropIndex(
                name: "ix_representacion_caso_conyuge_id",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_cantidad",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_tipo_dia",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_unidad",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_vigencia",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_preregistro_envio",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropCheckConstraint(
                name: "ck_preregistro_hijos_mayores_incapaces",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropCheckConstraint(
                name: "ck_preregistro_hijos_menores",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropIndex(
                name: "ix_expediente_caso_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropIndex(
                name: "ix_expediente_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_numero",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_observacion",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropIndex(
                name: "ix_consulta_reniec_expira_en",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_datos_encontrados",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_expiracion",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_http",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_resultado",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asistencia_inasistencia",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asistencia_ratificacion",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asistencia_verificacion",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropIndex(
                name: "ix_actuacion_administrativa_caso_id_tipo_codigo",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropIndex(
                name: "ix_actuacion_administrativa_numero_resolucion_fecha_emision",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_datos_emision",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_estado",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_fecha_registro_emision",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_tipo",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropColumn(
                name: "cantidad_hijos_mayores_incapaces",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "cantidad_hijos_menores",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "fecha_matrimonio",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "matrimonio_en_porvenir",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "mutuo_acuerdo_declarado",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "tiene_bienes_sociales",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "ultimo_domicilio_conyugal_en_porvenir",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "observacion",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                newName: "expediente_id");

            migrationBuilder.RenameIndex(
                name: "ix_solicitud_disolucion_caso_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                newName: "ix_solicitud_disolucion_expediente_id");

            migrationBuilder.RenameColumn(
                name: "caso_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                newName: "expediente_conyuge_id");

            migrationBuilder.RenameIndex(
                name: "ix_solicitante_disolucion_solicitud_disolucion_id_caso_conyuge",
                schema: "divorcios",
                table: "solicitante_disolucion",
                newName: "ix_solicitante_disolucion_solicitud_disolucion_id_expediente_c");

            migrationBuilder.RenameIndex(
                name: "ix_solicitante_disolucion_caso_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                newName: "ix_solicitante_disolucion_expediente_conyuge_id");

            migrationBuilder.RenameColumn(
                name: "preregistro_id",
                schema: "divorcios",
                table: "revision_preregistro",
                newName: "preregistro_version_id");

            migrationBuilder.RenameIndex(
                name: "ix_revision_preregistro_preregistro_id_numero_revision",
                schema: "divorcios",
                table: "revision_preregistro",
                newName: "ix_revision_preregistro_preregistro_version_id_numero_revision");

            migrationBuilder.RenameColumn(
                name: "caso_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                newName: "expediente_conyuge_id");

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "registro_auditoria",
                newName: "expediente_id");

            migrationBuilder.RenameIndex(
                name: "ix_registro_auditoria_caso_id_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                newName: "ix_registro_auditoria_expediente_id_registrado_en");

            migrationBuilder.RenameColumn(
                name: "preregistro_id",
                schema: "divorcios",
                table: "preregistro_requisito",
                newName: "preregistro_version_id");

            migrationBuilder.RenameIndex(
                name: "ix_preregistro_requisito_preregistro_id_requisito_catalogo_id",
                schema: "divorcios",
                table: "preregistro_requisito",
                newName: "ix_preregistro_requisito_preregistro_version_id_requisito_cata");

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "preregistro",
                newName: "expediente_id");

            migrationBuilder.RenameIndex(
                name: "ix_preregistro_caso_id",
                schema: "divorcios",
                table: "preregistro",
                newName: "ix_preregistro_expediente_id");

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "notificacion",
                newName: "expediente_id");

            migrationBuilder.RenameIndex(
                name: "ix_notificacion_caso_id_creada_en",
                schema: "divorcios",
                table: "notificacion",
                newName: "ix_notificacion_expediente_id_creada_en");

            migrationBuilder.RenameColumn(
                name: "registrado_en",
                schema: "divorcios",
                table: "expediente",
                newName: "fecha_inicio_digital");

            migrationBuilder.RenameColumn(
                name: "cargado_en",
                schema: "divorcios",
                table: "documento_version",
                newName: "creado_en");

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "documento",
                newName: "expediente_id");

            migrationBuilder.DropIndex(
                name: "ix_documento_actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento");

            // Son datos distintos, no un renombre con equivalencia histórica.
            migrationBuilder.DropColumn(
                name: "actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.AddColumn<long>(
                name: "creado_por_usuario_id",
                schema: "divorcios",
                table: "documento",
                type: "bigint",
                nullable: true);

            migrationBuilder.RenameIndex(
                name: "ix_documento_caso_id_etapa_codigo",
                schema: "divorcios",
                table: "documento",
                newName: "ix_documento_expediente_id_etapa_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_documento_creado_por_usuario_id",
                schema: "divorcios",
                table: "documento",
                column: "creado_por_usuario_id");

            // Son datos distintos, no un renombre con equivalencia histórica.
            migrationBuilder.DropColumn(
                name: "bloqueado_en",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_acceso_en",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "expediente_id");

            migrationBuilder.RenameIndex(
                name: "ix_audiencia_ratificacion_caso_id_numero_programacion",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "ix_audiencia_ratificacion_expediente_id_numero_programacion");

            migrationBuilder.RenameIndex(
                name: "ix_audiencia_ratificacion_caso_id_fecha_hora_programada",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "ix_audiencia_ratificacion_expediente_id_fecha_hora_programada");

            migrationBuilder.RenameIndex(
                name: "ix_audiencia_ratificacion_caso_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "ix_audiencia_ratificacion_expediente_id");

            migrationBuilder.RenameColumn(
                name: "caso_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                newName: "expediente_conyuge_id");

            migrationBuilder.RenameIndex(
                name: "ix_asistencia_audiencia_caso_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                newName: "ix_asistencia_audiencia_expediente_conyuge_id");

            migrationBuilder.RenameIndex(
                name: "ix_asistencia_audiencia_audiencia_ratificacion_id_caso_conyuge",
                schema: "divorcios",
                table: "asistencia_audiencia",
                newName: "ix_asistencia_audiencia_audiencia_ratificacion_id_expediente_c");

            migrationBuilder.RenameColumn(
                name: "caso_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                newName: "expediente_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_acceso_en",
                schema: "divorcios",
                table: "usuario_interno",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "creado_en",
                schema: "divorcios",
                table: "solicitante_disolucion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<long>(
                name: "documento_poder_id",
                schema: "divorcios",
                table: "representacion",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "estado_codigo",
                schema: "divorcios",
                table: "representacion",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "VIGENTE");

            migrationBuilder.AddColumn<string>(
                name: "tipo_poder_codigo",
                schema: "divorcios",
                table: "representacion",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ESPECIAL");

            migrationBuilder.AlterColumn<string>(
                name: "unidad_codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_dia_codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "fuente",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(45)",
                oldMaxLength: 45);

            migrationBuilder.AddColumn<bool>(
                name: "activo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "descripcion",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "evento_inicio_codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "aplica",
                schema: "divorcios",
                table: "preregistro_requisito",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "generado_en",
                schema: "divorcios",
                table: "preregistro_requisito",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueado_en",
                schema: "divorcios",
                table: "preregistro",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado_codigo",
                schema: "divorcios",
                table: "preregistro",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "BORRADOR");

            migrationBuilder.AddColumn<DateTime>(
                name: "actualizado_en",
                schema: "divorcios",
                table: "persona",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "verificado_reniec",
                schema: "divorcios",
                table: "persona",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "documento_oficio_id",
                schema: "divorcios",
                table: "oficio",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "numero_expediente",
                schema: "divorcios",
                table: "expediente",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "fecha_ingreso_mesa_partes",
                schema: "divorcios",
                table: "expediente",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<DateTime>(
                name: "cerrado_en",
                schema: "divorcios",
                table: "expediente",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_preregistro",
                schema: "divorcios",
                table: "expediente",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "creado_en",
                schema: "divorcios",
                table: "expediente",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<long>(
                name: "creado_por_cuenta_id",
                schema: "divorcios",
                table: "expediente",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "oficializado_en",
                schema: "divorcios",
                table: "expediente",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "preregistro_bloqueado_en",
                schema: "divorcios",
                table: "expediente",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "numero_version",
                schema: "divorcios",
                table: "documento_version",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<string>(
                name: "motivo_cambio",
                schema: "divorcios",
                table: "documento_version",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "creado_por_cuenta_id",
                schema: "divorcios",
                table: "documento",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado_codigo",
                schema: "divorcios",
                table: "documento",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "VIGENTE");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creado_en",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueado_hasta",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "clave_hash",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "correo_verificado_en",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estado_codigo",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVA");

            migrationBuilder.AddColumn<short>(
                name: "intentos_fallidos",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<string>(
                name: "resultado_codigo",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expira_en",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "consultado_en",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "origen_codigo",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "API");

            migrationBuilder.AddColumn<string>(
                name: "respuesta_hash",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ratifico_voluntad",
                schema: "divorcios",
                table: "asistencia_audiencia",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_codigo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(35)",
                oldMaxLength: 35);

            migrationBuilder.AddColumn<long>(
                name: "documento_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_notificacion",
                schema: "divorcios",
                table: "actuacion_administrativa",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numero_secuencia",
                schema: "divorcios",
                table: "actuacion_administrativa",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "estado_expediente",
                schema: "divorcios",
                columns: table => new
                {
                    estado_expediente_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    nombre_ciudadano = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    etapa_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    orden_visual = table.Column<short>(type: "smallint", nullable: false),
                    es_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_expediente", x => x.estado_expediente_id);
                    table.CheckConstraint("ck_estado_etapa", "etapa_codigo IN ('PRERREGISTRO', 'SEPARACION', 'ESPERA', 'DIVORCIO', 'CIERRE')");
                    table.CheckConstraint("ck_estado_orden", "orden_visual > 0");
                });

            migrationBuilder.CreateTable(
                name: "expediente_conyuge",
                schema: "divorcios",
                columns: table => new
                {
                    expediente_conyuge_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    expediente_id = table.Column<long>(type: "bigint", nullable: false),
                    persona_id = table.Column<long>(type: "bigint", nullable: false),
                    posicion_codigo = table.Column<string>(type: "char(1)", nullable: false),
                    es_iniciador = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expediente_conyuge", x => x.expediente_conyuge_id);
                    table.CheckConstraint("ck_conyuge_posicion", "posicion_codigo IN ('A', 'B')");
                    table.ForeignKey(
                        name: "fk_expediente_conyuge_expediente_expediente_id",
                        column: x => x.expediente_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente",
                        principalColumn: "expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expediente_conyuge_personas_persona_id",
                        column: x => x.persona_id,
                        principalSchema: "divorcios",
                        principalTable: "persona",
                        principalColumn: "persona_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preregistro_requisito_documento",
                schema: "divorcios",
                columns: table => new
                {
                    preregistro_requisito_id = table.Column<long>(type: "bigint", nullable: false),
                    documento_version_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_preregistro_requisito_documento", x => new { x.preregistro_requisito_id, x.documento_version_id });
                    table.ForeignKey(
                        name: "fk_preregistro_requisito_documento_documento_version_documento",
                        column: x => x.documento_version_id,
                        principalSchema: "divorcios",
                        principalTable: "documento_version",
                        principalColumn: "documento_version_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_preregistro_requisito_documento_preregistro_requisito_prere",
                        column: x => x.preregistro_requisito_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro_requisito",
                        principalColumn: "preregistro_requisito_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preregistro_version",
                schema: "divorcios",
                columns: table => new
                {
                    preregistro_version_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    preregistro_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_version = table.Column<int>(type: "integer", nullable: false),
                    fecha_matrimonio = table.Column<DateOnly>(type: "date", nullable: false),
                    matrimonio_en_porvenir = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_domicilio_conyugal_porvenir = table.Column<bool>(type: "boolean", nullable: false),
                    domicilio_conyugal = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    tiene_hijos = table.Column<bool>(type: "boolean", nullable: false),
                    cantidad_hijos_menores = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    cantidad_hijos_mayores = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    tiene_hijos_mayores_situacion_especial = table.Column<bool>(type: "boolean", nullable: true),
                    tiene_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_acuerdo_bienes = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    requiere_representacion_a = table.Column<bool>(type: "boolean", nullable: true),
                    requiere_representacion_b = table.Column<bool>(type: "boolean", nullable: true),
                    observacion_ciudadano = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    motivo_cambio = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    creado_por_cuenta_id = table.Column<long>(type: "bigint", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_preregistro_version", x => x.preregistro_version_id);
                    table.CheckConstraint("ck_preregistro_version_acuerdo_bienes", "tiene_bienes\r\nOR tiene_acuerdo_bienes = FALSE");
                    table.CheckConstraint("ck_preregistro_version_hijos_mayores", "cantidad_hijos_mayores >= 0");
                    table.CheckConstraint("ck_preregistro_version_hijos_mayores_situacion_especial", "tiene_hijos_mayores_situacion_especial IS NOT TRUE\nOR (tiene_hijos AND cantidad_hijos_mayores > 0)");
                    table.CheckConstraint("ck_preregistro_version_hijos_menores", "cantidad_hijos_menores >= 0");
                    table.CheckConstraint("ck_preregistro_version_motivo", "numero_version = 1\r\nOR (\r\n    motivo_cambio IS NOT NULL\r\n    AND btrim(motivo_cambio) <> ''\r\n)");
                    table.CheckConstraint("ck_preregistro_version_numero", "numero_version > 0");
                    table.CheckConstraint("ck_preregistro_version_sin_hijos", "tiene_hijos\r\nOR (\r\n    cantidad_hijos_menores = 0\r\n    AND cantidad_hijos_mayores = 0\r\n)");
                    table.ForeignKey(
                        name: "fk_preregistro_version_cuenta_ciudadana_creado_por_cuenta_id",
                        column: x => x.creado_por_cuenta_id,
                        principalSchema: "divorcios",
                        principalTable: "cuenta_ciudadana",
                        principalColumn: "cuenta_ciudadana_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_preregistro_version_preregistro_preregistro_id",
                        column: x => x.preregistro_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro",
                        principalColumn: "preregistro_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "revision_detalle_documento",
                schema: "divorcios",
                columns: table => new
                {
                    revision_detalle_id = table.Column<long>(type: "bigint", nullable: false),
                    documento_version_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_revision_detalle_documento", x => new { x.revision_detalle_id, x.documento_version_id });
                    table.ForeignKey(
                        name: "fk_revision_detalle_documento_documento_version_documento_vers",
                        column: x => x.documento_version_id,
                        principalSchema: "divorcios",
                        principalTable: "documento_version",
                        principalColumn: "documento_version_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_detalle_documento_revision_detalle_revision_detall",
                        column: x => x.revision_detalle_id,
                        principalSchema: "divorcios",
                        principalTable: "revision_detalle",
                        principalColumn: "revision_detalle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historial_estado_expediente",
                schema: "divorcios",
                columns: table => new
                {
                    historial_estado_expediente_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    expediente_id = table.Column<long>(type: "bigint", nullable: false),
                    estado_expediente_id = table.Column<short>(type: "smallint", nullable: false),
                    numero_secuencia = table.Column<int>(type: "integer", nullable: false),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    iniciado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finalizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historial_estado_expediente", x => x.historial_estado_expediente_id);
                    table.CheckConstraint("ck_historial_estado_fechas", "finalizado_en IS NULL\r\nOR finalizado_en >= iniciado_en");
                    table.CheckConstraint("ck_historial_estado_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_historial_estado_secuencia", "numero_secuencia > 0");
                    table.ForeignKey(
                        name: "fk_historial_estado_expediente_estado_expediente_estado_expedi",
                        column: x => x.estado_expediente_id,
                        principalSchema: "divorcios",
                        principalTable: "estado_expediente",
                        principalColumn: "estado_expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_expediente_expediente_expediente_id",
                        column: x => x.expediente_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente",
                        principalColumn: "expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_expediente_usuarios_internos_registrado_po",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pago",
                schema: "divorcios",
                columns: table => new
                {
                    pago_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    expediente_id = table.Column<long>(type: "bigint", nullable: false),
                    expediente_conyuge_pagante_id = table.Column<long>(type: "bigint", nullable: false),
                    documento_comprobante_id = table.Column<long>(type: "bigint", nullable: true),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_pago = table.Column<int>(type: "integer", nullable: false),
                    dni_pagante_snapshot = table.Column<string>(type: "char(8)", nullable: false),
                    nombre_pagante_snapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    concepto_codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    concepto_descripcion_snapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    moneda_codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    estado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDIENTE"),
                    numero_voucher = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    referencia_caja = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    solicitado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ultima_consulta_caja_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pagado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    anulado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pago", x => x.pago_id);
                    table.CheckConstraint("ck_pago_concepto", "btrim(concepto_codigo) <> ''");
                    table.CheckConstraint("ck_pago_descripcion", "btrim(concepto_descripcion_snapshot) <> ''");
                    table.CheckConstraint("ck_pago_dni", "dni_pagante_snapshot ~ '^[0-9]{8}$'");
                    table.CheckConstraint("ck_pago_estado", "estado_codigo IN (\r\n    'PENDIENTE',\r\n    'PAGADO',\r\n    'ANULADO'\r\n)");
                    table.CheckConstraint("ck_pago_fechas", "(\r\n    ultima_consulta_caja_en IS NULL\r\n    OR ultima_consulta_caja_en >= solicitado_en\r\n)\r\nAND\r\n(\r\n    pagado_en IS NULL\r\n    OR pagado_en >= solicitado_en\r\n)\r\nAND\r\n(\r\n    anulado_en IS NULL\r\n    OR anulado_en >= solicitado_en\r\n)\r\nAND\r\n(\r\n    pagado_en IS NULL\r\n    OR anulado_en IS NULL\r\n    OR anulado_en >= pagado_en\r\n)");
                    table.CheckConstraint("ck_pago_flujo", "(\r\n    estado_codigo = 'PENDIENTE'\r\n    AND pagado_en IS NULL\r\n    AND anulado_en IS NULL\r\n    AND numero_voucher IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'PAGADO'\r\n    AND pagado_en IS NOT NULL\n    AND anulado_en IS NULL\n    AND numero_voucher IS NOT NULL\n)\r\nOR\r\n(\n    estado_codigo = 'ANULADO'\n    AND anulado_en IS NOT NULL\n    AND (\n        pagado_en IS NULL\n        OR numero_voucher IS NOT NULL\n    )\n)");
                    table.CheckConstraint("ck_pago_moneda", "moneda_codigo = 'PEN'");
                    table.CheckConstraint("ck_pago_monto", "monto > 0");
                    table.CheckConstraint("ck_pago_nombre", "btrim(nombre_pagante_snapshot) <> ''");
                    table.CheckConstraint("ck_pago_numero", "numero_pago > 0");
                    table.CheckConstraint("ck_pago_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_pago_referencia_caja", "referencia_caja IS NULL\r\nOR btrim(referencia_caja) <> ''");
                    table.CheckConstraint("ck_pago_voucher", "numero_voucher IS NULL\r\nOR btrim(numero_voucher) <> ''");
                    table.ForeignKey(
                        name: "fk_pago_documento_documento_comprobante_id",
                        column: x => x.documento_comprobante_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_expediente_conyuge_expediente_conyuge_pagante_id",
                        column: x => x.expediente_conyuge_pagante_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente_conyuge",
                        principalColumn: "expediente_conyuge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_expediente_expediente_id",
                        column: x => x.expediente_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente",
                        principalColumn: "expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_usuarios_internos_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expediente_contacto_historial",
                schema: "divorcios",
                columns: table => new
                {
                    expediente_contacto_historial_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    expediente_conyuge_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_contacto_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    fuente_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    preregistro_version_origen_id = table.Column<long>(type: "bigint", nullable: true),
                    vigente_desde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    vigente_hasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expediente_contacto_historial", x => x.expediente_contacto_historial_id);
                    table.CheckConstraint("ck_expediente_contacto_fuente", "fuente_codigo IN (\r\n    'PRERREGISTRO',\r\n    'RENIEC',\r\n    'MUNICIPALIDAD'\r\n)");
                    table.CheckConstraint("ck_expediente_contacto_origen", "fuente_codigo <> 'PRERREGISTRO'\r\nOR preregistro_version_origen_id IS NOT NULL");
                    table.CheckConstraint("ck_expediente_contacto_tipo", "tipo_contacto_codigo IN (\r\n    'CELULAR',\r\n    'CORREO',\r\n    'DIRECCION'\r\n)");
                    table.CheckConstraint("ck_expediente_contacto_valor", "btrim(valor) <> ''");
                    table.CheckConstraint("ck_expediente_contacto_vigencia", "vigente_hasta IS NULL\r\nOR vigente_hasta >= vigente_desde");
                    table.ForeignKey(
                        name: "fk_expediente_contacto_historial_expedientes_conyuges_expedien",
                        column: x => x.expediente_conyuge_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente_conyuge",
                        principalColumn: "expediente_conyuge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expediente_contacto_historial_preregistros_versiones_prereg",
                        column: x => x.preregistro_version_origen_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro_version",
                        principalColumn: "preregistro_version_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expediente_contacto_historial_usuarios_internos_registrado_",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expediente_version",
                schema: "divorcios",
                columns: table => new
                {
                    expediente_version_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    expediente_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_version = table.Column<int>(type: "integer", nullable: false),
                    preregistro_version_origen_id = table.Column<long>(type: "bigint", nullable: true),
                    fecha_matrimonio = table.Column<DateOnly>(type: "date", nullable: false),
                    matrimonio_en_porvenir = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_domicilio_conyugal_porvenir = table.Column<bool>(type: "boolean", nullable: false),
                    domicilio_conyugal = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    tiene_hijos = table.Column<bool>(type: "boolean", nullable: false),
                    cantidad_hijos_menores = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    cantidad_hijos_mayores = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    tiene_hijos_mayores_situacion_especial = table.Column<bool>(type: "boolean", nullable: true),
                    tiene_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_acuerdo_bienes = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    requiere_representacion_a = table.Column<bool>(type: "boolean", nullable: true),
                    requiere_representacion_b = table.Column<bool>(type: "boolean", nullable: true),
                    motivo_cambio = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expediente_version", x => x.expediente_version_id);
                    table.CheckConstraint("ck_expediente_version_acuerdo_bienes", "tiene_bienes\r\nOR tiene_acuerdo_bienes = FALSE");
                    table.CheckConstraint("ck_expediente_version_hijos_mayores", "cantidad_hijos_mayores >= 0");
                    table.CheckConstraint("ck_expediente_version_hijos_mayores_situacion_especial", "tiene_hijos_mayores_situacion_especial IS NOT TRUE\nOR (tiene_hijos AND cantidad_hijos_mayores > 0)");
                    table.CheckConstraint("ck_expediente_version_hijos_menores", "cantidad_hijos_menores >= 0");
                    table.CheckConstraint("ck_expediente_version_motivo", "btrim(motivo_cambio) <> ''");
                    table.CheckConstraint("ck_expediente_version_numero", "numero_version > 0");
                    table.CheckConstraint("ck_expediente_version_sin_hijos", "tiene_hijos\r\nOR (\r\n    cantidad_hijos_menores = 0\r\n    AND cantidad_hijos_mayores = 0\r\n)");
                    table.ForeignKey(
                        name: "fk_expediente_version_expediente_expediente_id",
                        column: x => x.expediente_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente",
                        principalColumn: "expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expediente_version_preregistros_versiones_preregistro_versi",
                        column: x => x.preregistro_version_origen_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro_version",
                        principalColumn: "preregistro_version_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expediente_version_usuarios_internos_registrado_por_usuario",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plazo_expediente",
                schema: "divorcios",
                columns: table => new
                {
                    plazo_expediente_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    expediente_id = table.Column<long>(type: "bigint", nullable: false),
                    regla_plazo_id = table.Column<short>(type: "smallint", nullable: false),
                    historial_estado_expediente_origen_id = table.Column<long>(type: "bigint", nullable: true),
                    numero_aplicacion = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "PENDIENTE"),
                    cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plazo_expediente", x => x.plazo_expediente_id);
                    table.CheckConstraint("ck_plazo_expediente_cierre", "(\r\n    estado_codigo = 'PENDIENTE'\r\n    AND cerrado_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo IN (\r\n        'CUMPLIDO',\r\n        'CANCELADO'\r\n    )\r\n    AND cerrado_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_plazo_expediente_estado", "estado_codigo IN (\r\n    'PENDIENTE',\r\n    'CUMPLIDO',\r\n    'CANCELADO'\r\n)");
                    table.CheckConstraint("ck_plazo_expediente_estado_aplicacion", "numero_aplicacion > 0");
                    table.CheckConstraint("ck_plazo_expediente_fechas", "fecha_vencimiento >= fecha_inicio");
                    table.CheckConstraint("ck_plazo_expediente_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.ForeignKey(
                        name: "fk_plazo_expediente_expediente_expediente_id",
                        column: x => x.expediente_id,
                        principalSchema: "divorcios",
                        principalTable: "expediente",
                        principalColumn: "expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_expediente_historial_estado_expediente_historial_esta",
                        column: x => x.historial_estado_expediente_origen_id,
                        principalSchema: "divorcios",
                        principalTable: "historial_estado_expediente",
                        principalColumn: "historial_estado_expediente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_expediente_reglas_plazo_regla_plazo_id",
                        column: x => x.regla_plazo_id,
                        principalSchema: "divorcios",
                        principalTable: "regla_plazo",
                        principalColumn: "regla_plazo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_expediente_usuarios_internos_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_representacion_expediente_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                column: "expediente_conyuge_id",
                unique: true,
                filter: "estado_codigo = 'VIGENTE'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_representacion_estado",
                schema: "divorcios",
                table: "representacion",
                sql: "estado_codigo IN (\r\n    'VIGENTE',\r\n    'REVOCADA',\r\n    'VENCIDA'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_representacion_estado_cierre",
                schema: "divorcios",
                table: "representacion",
                sql: "estado_codigo = 'VIGENTE'\r\nOR vigente_hasta IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_representacion_tipo_poder",
                schema: "divorcios",
                table: "representacion",
                sql: "tipo_poder_codigo = 'ESPECIAL'");

            migrationBuilder.CreateIndex(
                name: "ix_regla_plazo_activo_evento_inicio_codigo",
                schema: "divorcios",
                table: "regla_plazo",
                columns: new[] { "activo", "evento_inicio_codigo" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_cantidad",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "cantidad > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_descripcion",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "descripcion IS NULL\r\nOR btrim(descripcion) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_evento_inicio",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "btrim(evento_inicio_codigo) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_fuente",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "btrim(fuente) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_tipo_dia",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "tipo_dia_codigo IN (\r\n    'CALENDARIO',\r\n    'HABIL',\r\n    'OPERATIVO'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_unidad",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "unidad_codigo IN ('DIA', 'MES')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_plazo_vigencia",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "vigente_hasta IS NULL\r\nOR vigente_hasta >= vigente_desde");

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_estado_codigo",
                schema: "divorcios",
                table: "preregistro",
                column: "estado_codigo");

            migrationBuilder.AddCheckConstraint(
                name: "ck_preregistro_bloqueo",
                schema: "divorcios",
                table: "preregistro",
                sql: "bloqueado_en IS NULL\r\nOR (\r\n    aprobado_en IS NOT NULL\r\n    AND bloqueado_en >= aprobado_en\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_preregistro_envio",
                schema: "divorcios",
                table: "preregistro",
                sql: "enviado_en IS NULL\r\nOR enviado_en >= creado_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_preregistro_estado",
                schema: "divorcios",
                table: "preregistro",
                sql: "estado_codigo IN (\r\n    'BORRADOR',\r\n    'ENVIADO',\r\n    'OBSERVADO',\r\n    'APROBADO',\r\n    'CANCELADO'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_persona_actualizacion",
                schema: "divorcios",
                table: "persona",
                sql: "actualizado_en IS NULL\r\nOR actualizado_en >= creado_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_persona_verificacion_reniec",
                schema: "divorcios",
                table: "persona",
                sql: "(\r\n    verificado_reniec = FALSE\r\n    AND verificado_reniec_en IS NULL\r\n)\r\nOR\r\n(\r\n    verificado_reniec = TRUE\r\n    AND verificado_reniec_en IS NOT NULL\r\n)");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_cerrado_en",
                schema: "divorcios",
                table: "expediente",
                column: "cerrado_en");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_codigo_preregistro",
                schema: "divorcios",
                table: "expediente",
                column: "codigo_preregistro",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_creado_por_cuenta_id",
                schema: "divorcios",
                table: "expediente",
                column: "creado_por_cuenta_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_bloqueo_preregistro",
                schema: "divorcios",
                table: "expediente",
                sql: "preregistro_bloqueado_en IS NULL\r\nOR preregistro_bloqueado_en >= fecha_inicio_digital");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_cierre",
                schema: "divorcios",
                table: "expediente",
                sql: "cerrado_en IS NULL\r\nOR cerrado_en >= fecha_inicio_digital");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_codigo_preregistro",
                schema: "divorcios",
                table: "expediente",
                sql: "btrim(codigo_preregistro) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_numero",
                schema: "divorcios",
                table: "expediente",
                sql: "numero_expediente IS NULL\r\nOR btrim(numero_expediente) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_oficializacion",
                schema: "divorcios",
                table: "expediente",
                sql: "(\r\n    numero_expediente IS NULL\r\n    AND fecha_ingreso_mesa_partes IS NULL\r\n    AND oficializado_en IS NULL\r\n)\r\nOR\r\n(\r\n    numero_expediente IS NOT NULL\r\n    AND fecha_ingreso_mesa_partes IS NOT NULL\r\n    AND oficializado_en IS NOT NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_documento_version_motivo",
                schema: "divorcios",
                table: "documento_version",
                sql: "numero_version = 1\r\nOR (\r\n    motivo_cambio IS NOT NULL\r\n    AND btrim(motivo_cambio) <> ''\r\n)");

            migrationBuilder.CreateIndex(
                name: "ix_documento_creado_por_cuenta_id",
                schema: "divorcios",
                table: "documento",
                column: "creado_por_cuenta_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_estado_codigo",
                schema: "divorcios",
                table: "documento",
                column: "estado_codigo");

            migrationBuilder.AddCheckConstraint(
                name: "ck_documento_creador",
                schema: "divorcios",
                table: "documento",
                sql: "NOT (\r\n    creado_por_cuenta_id IS NOT NULL\r\n    AND creado_por_usuario_id IS NOT NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_documento_estado",
                schema: "divorcios",
                table: "documento",
                sql: "estado_codigo IN (\r\n    'VIGENTE',\r\n    'REEMPLAZADO',\r\n    'ANULADO'\r\n)");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_ciudadana_estado_codigo",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                column: "estado_codigo");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cuenta_ciudadana_bloqueo",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                sql: "estado_codigo <> 'BLOQUEADA'\r\nOR bloqueado_hasta IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cuenta_ciudadana_estado",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                sql: "estado_codigo IN (\r\n    'ACTIVA',\r\n    'BLOQUEADA',\r\n    'INACTIVA'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cuenta_ciudadana_hash",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                sql: "clave_hash IS NULL\r\nOR btrim(clave_hash) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cuenta_ciudadana_intentos",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                sql: "intentos_fallidos >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_consulta_reniec_dni_consultado_expira_en",
                schema: "divorcios",
                table: "consulta_reniec",
                columns: new[] { "dni_consultado", "expira_en" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_datos_encontrados",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "resultado_codigo <> 'ENCONTRADO'\r\nOR (\r\n    prenombres IS NOT NULL\r\n    AND btrim(prenombres) <> ''\r\n    AND apellido_paterno IS NOT NULL\r\n    AND btrim(apellido_paterno) <> ''\r\n    AND apellido_materno IS NOT NULL\r\n    AND btrim(apellido_materno) <> ''\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_expiracion",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "expira_en IS NULL\r\nOR expira_en >= consultado_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_hash",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "respuesta_hash IS NULL\r\nOR btrim(respuesta_hash) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_http",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "codigo_http IS NULL\r\nOR codigo_http BETWEEN 100 AND 599");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_origen",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "origen_codigo IN (\r\n    'API',\r\n    'IMPORTACION'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_resultado",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "resultado_codigo IN (\r\n    'ENCONTRADO',\r\n    'NO_ENCONTRADO',\r\n    'ERROR'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asistencia_inasistencia",
                schema: "divorcios",
                table: "asistencia_audiencia",
                sql: "asistio IS TRUE\nOR (\n    ratifico_voluntad IS NULL\n    AND identidad_verificada_en IS NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asistencia_ratificacion",
                schema: "divorcios",
                table: "asistencia_audiencia",
                sql: "ratifico_voluntad IS NULL\nOR (\n    asistio IS TRUE\n    AND identidad_verificada_en IS NOT NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asistencia_verificacion",
                schema: "divorcios",
                table: "asistencia_audiencia",
                sql: "identidad_verificada_en IS NULL\r\nOR asistio IS TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_documento_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "documento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_expediente_id_tipo_codigo_numero_s",
                schema: "divorcios",
                table: "actuacion_administrativa",
                columns: new[] { "expediente_id", "tipo_codigo", "numero_secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_numero_resolucion",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "numero_resolucion");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_emision",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "(\r\n    estado_codigo = 'BORRADOR'\r\n    AND fecha_emision IS NULL\r\n    AND emitida_por_usuario_id IS NULL\r\n    AND emitida_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'EMITIDA'\r\n    AND fecha_emision IS NOT NULL\r\n    AND emitida_por_usuario_id IS NOT NULL\r\n    AND emitida_en IS NOT NULL\r\n)\r\nOR estado_codigo = 'ANULADA'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_estado",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "estado_codigo IN (\r\n    'BORRADOR',\r\n    'EMITIDA',\r\n    'ANULADA'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_fecha_emision",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "emitida_en IS NULL\r\nOR emitida_en >= creada_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_fecha_notificacion",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "fecha_notificacion IS NULL\r\nOR (\r\n    fecha_emision IS NOT NULL\r\n    AND fecha_notificacion >= fecha_emision\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_secuencia",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "numero_secuencia > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_tipo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "tipo_codigo IN (\r\n    'INFORME_ADMISIBILIDAD',\r\n    'RESOLUCION_ADMISIBILIDAD',\r\n    'INFORME_SEPARACION',\r\n    'RESOLUCION_SEPARACION',\r\n    'INFORME_DISOLUCION',\r\n    'RESOLUCION_DISOLUCION'\r\n)");

            migrationBuilder.CreateIndex(
                name: "ix_estado_expediente_codigo",
                schema: "divorcios",
                table: "estado_expediente",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estado_expediente_etapa_codigo_orden_visual",
                schema: "divorcios",
                table: "estado_expediente",
                columns: new[] { "etapa_codigo", "orden_visual" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_contacto_historial_expediente_conyuge_id_tipo_co",
                schema: "divorcios",
                table: "expediente_contacto_historial",
                columns: new[] { "expediente_conyuge_id", "tipo_contacto_codigo" },
                unique: true,
                filter: "vigente_hasta IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_contacto_historial_expediente_conyuge_id_tipo_co1",
                schema: "divorcios",
                table: "expediente_contacto_historial",
                columns: new[] { "expediente_conyuge_id", "tipo_contacto_codigo", "vigente_desde" });

            migrationBuilder.CreateIndex(
                name: "ix_expediente_contacto_historial_preregistro_version_origen_id",
                schema: "divorcios",
                table: "expediente_contacto_historial",
                column: "preregistro_version_origen_id");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_contacto_historial_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente_contacto_historial",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_conyuge_expediente_id_persona_id",
                schema: "divorcios",
                table: "expediente_conyuge",
                columns: new[] { "expediente_id", "persona_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_conyuge_expediente_id_posicion_codigo",
                schema: "divorcios",
                table: "expediente_conyuge",
                columns: new[] { "expediente_id", "posicion_codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_conyuge_persona_id",
                schema: "divorcios",
                table: "expediente_conyuge",
                column: "persona_id");

            migrationBuilder.CreateIndex(
                name: "ux_expediente_un_iniciador",
                schema: "divorcios",
                table: "expediente_conyuge",
                column: "expediente_id",
                unique: true,
                filter: "es_iniciador = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_version_expediente_id_numero_version",
                schema: "divorcios",
                table: "expediente_version",
                columns: new[] { "expediente_id", "numero_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_version_preregistro_version_origen_id",
                schema: "divorcios",
                table: "expediente_version",
                column: "preregistro_version_origen_id");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_version_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente_version",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_expediente_estado_expediente_id",
                schema: "divorcios",
                table: "historial_estado_expediente",
                column: "estado_expediente_id");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_expediente_expediente_id",
                schema: "divorcios",
                table: "historial_estado_expediente",
                column: "expediente_id",
                unique: true,
                filter: "finalizado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_expediente_expediente_id_iniciado_en",
                schema: "divorcios",
                table: "historial_estado_expediente",
                columns: new[] { "expediente_id", "iniciado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_expediente_expediente_id_numero_secuencia",
                schema: "divorcios",
                table: "historial_estado_expediente",
                columns: new[] { "expediente_id", "numero_secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_expediente_registrado_por_usuario_id",
                schema: "divorcios",
                table: "historial_estado_expediente",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_pago_documento_comprobante_id",
                schema: "divorcios",
                table: "pago",
                column: "documento_comprobante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pago_estado_codigo_solicitado_en",
                schema: "divorcios",
                table: "pago",
                columns: new[] { "estado_codigo", "solicitado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_pago_expediente_conyuge_pagante_id",
                schema: "divorcios",
                table: "pago",
                column: "expediente_conyuge_pagante_id");

            migrationBuilder.CreateIndex(
                name: "ix_pago_expediente_id_numero_pago",
                schema: "divorcios",
                table: "pago",
                columns: new[] { "expediente_id", "numero_pago" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pago_numero_voucher",
                schema: "divorcios",
                table: "pago",
                column: "numero_voucher",
                unique: true,
                filter: "numero_voucher IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pago_referencia_caja",
                schema: "divorcios",
                table: "pago",
                column: "referencia_caja",
                unique: true,
                filter: "referencia_caja IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pago_registrado_por_usuario_id",
                schema: "divorcios",
                table: "pago",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_expediente_creado_por_usuario_id",
                schema: "divorcios",
                table: "plazo_expediente",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_expediente_expediente_id_estado_codigo_fecha_vencimie",
                schema: "divorcios",
                table: "plazo_expediente",
                columns: new[] { "expediente_id", "estado_codigo", "fecha_vencimiento" });

            migrationBuilder.CreateIndex(
                name: "ix_plazo_expediente_expediente_id_regla_plazo_id_numero_aplica",
                schema: "divorcios",
                table: "plazo_expediente",
                columns: new[] { "expediente_id", "regla_plazo_id", "numero_aplicacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plazo_expediente_historial_estado_expediente_origen_id",
                schema: "divorcios",
                table: "plazo_expediente",
                column: "historial_estado_expediente_origen_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_expediente_regla_plazo_id",
                schema: "divorcios",
                table: "plazo_expediente",
                column: "regla_plazo_id");

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_requisito_documento_documento_version_id",
                schema: "divorcios",
                table: "preregistro_requisito_documento",
                column: "documento_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_version_creado_por_cuenta_id",
                schema: "divorcios",
                table: "preregistro_version",
                column: "creado_por_cuenta_id");

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_version_preregistro_id_numero_version",
                schema: "divorcios",
                table: "preregistro_version",
                columns: new[] { "preregistro_id", "numero_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revision_detalle_documento_documento_version_id",
                schema: "divorcios",
                table: "revision_detalle_documento",
                column: "documento_version_id");

            migrationBuilder.AddForeignKey(
                name: "fk_actuacion_administrativa_documentos_documento_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "documento_id",
                principalSchema: "divorcios",
                principalTable: "documento",
                principalColumn: "documento_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_actuacion_administrativa_expedientes_expediente_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_asistencia_audiencia_expedientes_conyuges_expediente_conyug",
                schema: "divorcios",
                table: "asistencia_audiencia",
                column: "expediente_conyuge_id",
                principalSchema: "divorcios",
                principalTable: "expediente_conyuge",
                principalColumn: "expediente_conyuge_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_audiencia_ratificacion_expedientes_expediente_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_documento_cuenta_ciudadana_creado_por_cuenta_id",
                schema: "divorcios",
                table: "documento",
                column: "creado_por_cuenta_id",
                principalSchema: "divorcios",
                principalTable: "cuenta_ciudadana",
                principalColumn: "cuenta_ciudadana_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_documento_expedientes_expediente_id",
                schema: "divorcios",
                table: "documento",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_documento_usuarios_internos_creado_por_usuario_id",
                schema: "divorcios",
                table: "documento",
                column: "creado_por_usuario_id",
                principalSchema: "divorcios",
                principalTable: "usuario_interno",
                principalColumn: "usuario_interno_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_expediente_cuenta_ciudadana_creado_por_cuenta_id",
                schema: "divorcios",
                table: "expediente",
                column: "creado_por_cuenta_id",
                principalSchema: "divorcios",
                principalTable: "cuenta_ciudadana",
                principalColumn: "cuenta_ciudadana_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_notificacion_expediente_expediente_id",
                schema: "divorcios",
                table: "notificacion",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_preregistro_expediente_expediente_id",
                schema: "divorcios",
                table: "preregistro",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_preregistro_requisito_preregistros_versiones_preregistro_ve",
                schema: "divorcios",
                table: "preregistro_requisito",
                column: "preregistro_version_id",
                principalSchema: "divorcios",
                principalTable: "preregistro_version",
                principalColumn: "preregistro_version_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_registro_auditoria_expediente_expediente_id",
                schema: "divorcios",
                table: "registro_auditoria",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_representacion_expediente_conyuge_expediente_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                column: "expediente_conyuge_id",
                principalSchema: "divorcios",
                principalTable: "expediente_conyuge",
                principalColumn: "expediente_conyuge_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_revision_preregistro_preregistro_version_preregistro_versio",
                schema: "divorcios",
                table: "revision_preregistro",
                column: "preregistro_version_id",
                principalSchema: "divorcios",
                principalTable: "preregistro_version",
                principalColumn: "preregistro_version_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitante_disolucion_expediente_conyuge_expediente_conyug",
                schema: "divorcios",
                table: "solicitante_disolucion",
                column: "expediente_conyuge_id",
                principalSchema: "divorcios",
                principalTable: "expediente_conyuge",
                principalColumn: "expediente_conyuge_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitud_disolucion_expediente_expediente_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "expediente_id",
                principalSchema: "divorcios",
                principalTable: "expediente",
                principalColumn: "expediente_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ExigirEsquemaSinDatos(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "fk_actuacion_administrativa_documentos_documento_id",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropForeignKey(
                name: "fk_actuacion_administrativa_expedientes_expediente_id",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropForeignKey(
                name: "fk_asistencia_audiencia_expedientes_conyuges_expediente_conyug",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropForeignKey(
                name: "fk_audiencia_ratificacion_expedientes_expediente_id",
                schema: "divorcios",
                table: "audiencia_ratificacion");

            migrationBuilder.DropForeignKey(
                name: "fk_documento_cuenta_ciudadana_creado_por_cuenta_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropForeignKey(
                name: "fk_documento_expedientes_expediente_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropForeignKey(
                name: "fk_documento_usuarios_internos_creado_por_usuario_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropForeignKey(
                name: "fk_expediente_cuenta_ciudadana_creado_por_cuenta_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropForeignKey(
                name: "fk_notificacion_expediente_expediente_id",
                schema: "divorcios",
                table: "notificacion");

            migrationBuilder.DropForeignKey(
                name: "fk_preregistro_expediente_expediente_id",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropForeignKey(
                name: "fk_preregistro_requisito_preregistros_versiones_preregistro_ve",
                schema: "divorcios",
                table: "preregistro_requisito");

            migrationBuilder.DropForeignKey(
                name: "fk_registro_auditoria_expediente_expediente_id",
                schema: "divorcios",
                table: "registro_auditoria");

            migrationBuilder.DropForeignKey(
                name: "fk_representacion_expediente_conyuge_expediente_conyuge_id",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropForeignKey(
                name: "fk_revision_preregistro_preregistro_version_preregistro_versio",
                schema: "divorcios",
                table: "revision_preregistro");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitante_disolucion_expediente_conyuge_expediente_conyug",
                schema: "divorcios",
                table: "solicitante_disolucion");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitud_disolucion_expediente_expediente_id",
                schema: "divorcios",
                table: "solicitud_disolucion");

            migrationBuilder.DropTable(
                name: "expediente_contacto_historial",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "expediente_version",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "pago",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "plazo_expediente",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "preregistro_requisito_documento",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "revision_detalle_documento",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "preregistro_version",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "expediente_conyuge",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "historial_estado_expediente",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "estado_expediente",
                schema: "divorcios");

            migrationBuilder.DropIndex(
                name: "ix_representacion_expediente_conyuge_id",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_representacion_estado",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_representacion_estado_cierre",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropCheckConstraint(
                name: "ck_representacion_tipo_poder",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropIndex(
                name: "ix_regla_plazo_activo_evento_inicio_codigo",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_cantidad",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_descripcion",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_evento_inicio",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_fuente",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_tipo_dia",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_unidad",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropCheckConstraint(
                name: "ck_regla_plazo_vigencia",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropIndex(
                name: "ix_preregistro_estado_codigo",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropCheckConstraint(
                name: "ck_preregistro_bloqueo",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropCheckConstraint(
                name: "ck_preregistro_envio",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropCheckConstraint(
                name: "ck_preregistro_estado",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropCheckConstraint(
                name: "ck_persona_actualizacion",
                schema: "divorcios",
                table: "persona");

            migrationBuilder.DropCheckConstraint(
                name: "ck_persona_verificacion_reniec",
                schema: "divorcios",
                table: "persona");

            migrationBuilder.DropIndex(
                name: "ix_expediente_cerrado_en",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropIndex(
                name: "ix_expediente_codigo_preregistro",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropIndex(
                name: "ix_expediente_creado_por_cuenta_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_bloqueo_preregistro",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_cierre",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_codigo_preregistro",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_numero",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expediente_oficializacion",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropCheckConstraint(
                name: "ck_documento_version_motivo",
                schema: "divorcios",
                table: "documento_version");

            migrationBuilder.DropIndex(
                name: "ix_documento_creado_por_cuenta_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropIndex(
                name: "ix_documento_estado_codigo",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropCheckConstraint(
                name: "ck_documento_creador",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropCheckConstraint(
                name: "ck_documento_estado",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropIndex(
                name: "ix_cuenta_ciudadana_estado_codigo",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cuenta_ciudadana_bloqueo",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cuenta_ciudadana_estado",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cuenta_ciudadana_hash",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cuenta_ciudadana_intentos",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropIndex(
                name: "ix_consulta_reniec_dni_consultado_expira_en",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_datos_encontrados",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_expiracion",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_hash",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_http",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_origen",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_consulta_reniec_resultado",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asistencia_inasistencia",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asistencia_ratificacion",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_asistencia_verificacion",
                schema: "divorcios",
                table: "asistencia_audiencia");

            migrationBuilder.DropIndex(
                name: "ix_actuacion_administrativa_documento_id",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropIndex(
                name: "ix_actuacion_administrativa_expediente_id_tipo_codigo_numero_s",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropIndex(
                name: "ix_actuacion_administrativa_numero_resolucion",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_emision",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_estado",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_fecha_emision",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_fecha_notificacion",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_secuencia",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropCheckConstraint(
                name: "ck_actuacion_tipo",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropColumn(
                name: "ultimo_acceso_en",
                schema: "divorcios",
                table: "usuario_interno");

            migrationBuilder.DropColumn(
                name: "creado_en",
                schema: "divorcios",
                table: "solicitante_disolucion");

            migrationBuilder.DropColumn(
                name: "estado_codigo",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropColumn(
                name: "tipo_poder_codigo",
                schema: "divorcios",
                table: "representacion");

            migrationBuilder.DropColumn(
                name: "activo",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropColumn(
                name: "descripcion",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropColumn(
                name: "evento_inicio_codigo",
                schema: "divorcios",
                table: "regla_plazo");

            migrationBuilder.DropColumn(
                name: "aplica",
                schema: "divorcios",
                table: "preregistro_requisito");

            migrationBuilder.DropColumn(
                name: "generado_en",
                schema: "divorcios",
                table: "preregistro_requisito");

            migrationBuilder.DropColumn(
                name: "bloqueado_en",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "estado_codigo",
                schema: "divorcios",
                table: "preregistro");

            migrationBuilder.DropColumn(
                name: "actualizado_en",
                schema: "divorcios",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "verificado_reniec",
                schema: "divorcios",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "cerrado_en",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "codigo_preregistro",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "creado_en",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "creado_por_cuenta_id",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "oficializado_en",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "preregistro_bloqueado_en",
                schema: "divorcios",
                table: "expediente");

            migrationBuilder.DropColumn(
                name: "motivo_cambio",
                schema: "divorcios",
                table: "documento_version");

            migrationBuilder.DropColumn(
                name: "creado_por_cuenta_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropColumn(
                name: "estado_codigo",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.DropColumn(
                name: "bloqueado_hasta",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropColumn(
                name: "clave_hash",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropColumn(
                name: "correo_verificado_en",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropColumn(
                name: "estado_codigo",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropColumn(
                name: "intentos_fallidos",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.DropColumn(
                name: "origen_codigo",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropColumn(
                name: "respuesta_hash",
                schema: "divorcios",
                table: "consulta_reniec");

            migrationBuilder.DropColumn(
                name: "documento_id",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropColumn(
                name: "fecha_notificacion",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.DropColumn(
                name: "numero_secuencia",
                schema: "divorcios",
                table: "actuacion_administrativa");

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                newName: "caso_id");

            migrationBuilder.RenameIndex(
                name: "ix_solicitud_disolucion_expediente_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                newName: "ix_solicitud_disolucion_caso_id");

            migrationBuilder.RenameColumn(
                name: "expediente_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                newName: "caso_conyuge_id");

            migrationBuilder.RenameIndex(
                name: "ix_solicitante_disolucion_solicitud_disolucion_id_expediente_c",
                schema: "divorcios",
                table: "solicitante_disolucion",
                newName: "ix_solicitante_disolucion_solicitud_disolucion_id_caso_conyuge");

            migrationBuilder.RenameIndex(
                name: "ix_solicitante_disolucion_expediente_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                newName: "ix_solicitante_disolucion_caso_conyuge_id");

            migrationBuilder.RenameColumn(
                name: "preregistro_version_id",
                schema: "divorcios",
                table: "revision_preregistro",
                newName: "preregistro_id");

            migrationBuilder.RenameIndex(
                name: "ix_revision_preregistro_preregistro_version_id_numero_revision",
                schema: "divorcios",
                table: "revision_preregistro",
                newName: "ix_revision_preregistro_preregistro_id_numero_revision");

            migrationBuilder.RenameColumn(
                name: "expediente_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                newName: "caso_conyuge_id");

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "registro_auditoria",
                newName: "caso_id");

            migrationBuilder.RenameIndex(
                name: "ix_registro_auditoria_expediente_id_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                newName: "ix_registro_auditoria_caso_id_registrado_en");

            migrationBuilder.RenameColumn(
                name: "preregistro_version_id",
                schema: "divorcios",
                table: "preregistro_requisito",
                newName: "preregistro_id");

            migrationBuilder.RenameIndex(
                name: "ix_preregistro_requisito_preregistro_version_id_requisito_cata",
                schema: "divorcios",
                table: "preregistro_requisito",
                newName: "ix_preregistro_requisito_preregistro_id_requisito_catalogo_id");

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "preregistro",
                newName: "caso_id");

            migrationBuilder.RenameIndex(
                name: "ix_preregistro_expediente_id",
                schema: "divorcios",
                table: "preregistro",
                newName: "ix_preregistro_caso_id");

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "notificacion",
                newName: "caso_id");

            migrationBuilder.RenameIndex(
                name: "ix_notificacion_expediente_id_creada_en",
                schema: "divorcios",
                table: "notificacion",
                newName: "ix_notificacion_caso_id_creada_en");

            migrationBuilder.RenameColumn(
                name: "fecha_inicio_digital",
                schema: "divorcios",
                table: "expediente",
                newName: "registrado_en");

            migrationBuilder.RenameColumn(
                name: "creado_en",
                schema: "divorcios",
                table: "documento_version",
                newName: "cargado_en");

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "documento",
                newName: "caso_id");

            migrationBuilder.DropIndex(
                name: "ix_documento_creado_por_usuario_id",
                schema: "divorcios",
                table: "documento");

            // Son datos distintos, no un renombre con equivalencia histórica.
            migrationBuilder.DropColumn(
                name: "creado_por_usuario_id",
                schema: "divorcios",
                table: "documento");

            migrationBuilder.AddColumn<long>(
                name: "actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento",
                type: "bigint",
                nullable: true);

            migrationBuilder.RenameIndex(
                name: "ix_documento_expediente_id_etapa_codigo",
                schema: "divorcios",
                table: "documento",
                newName: "ix_documento_caso_id_etapa_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_documento_actuacion_administrativa_id",
                schema: "divorcios",
                table: "documento",
                column: "actuacion_administrativa_id");

            // Son datos distintos, no un renombre con equivalencia histórica.
            migrationBuilder.DropColumn(
                name: "ultimo_acceso_en",
                schema: "divorcios",
                table: "cuenta_ciudadana");

            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueado_en",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "caso_id");

            migrationBuilder.RenameIndex(
                name: "ix_audiencia_ratificacion_expediente_id_numero_programacion",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "ix_audiencia_ratificacion_caso_id_numero_programacion");

            migrationBuilder.RenameIndex(
                name: "ix_audiencia_ratificacion_expediente_id_fecha_hora_programada",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "ix_audiencia_ratificacion_caso_id_fecha_hora_programada");

            migrationBuilder.RenameIndex(
                name: "ix_audiencia_ratificacion_expediente_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                newName: "ix_audiencia_ratificacion_caso_id");

            migrationBuilder.RenameColumn(
                name: "expediente_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                newName: "caso_conyuge_id");

            migrationBuilder.RenameIndex(
                name: "ix_asistencia_audiencia_expediente_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                newName: "ix_asistencia_audiencia_caso_conyuge_id");

            migrationBuilder.RenameIndex(
                name: "ix_asistencia_audiencia_audiencia_ratificacion_id_expediente_c",
                schema: "divorcios",
                table: "asistencia_audiencia",
                newName: "ix_asistencia_audiencia_audiencia_ratificacion_id_caso_conyuge");

            migrationBuilder.RenameColumn(
                name: "expediente_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                newName: "caso_id");

            migrationBuilder.AlterColumn<long>(
                name: "documento_poder_id",
                schema: "divorcios",
                table: "representacion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "unidad_codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_dia_codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "fuente",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "divorcios",
                table: "regla_plazo",
                type: "character varying(45)",
                maxLength: 45,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<short>(
                name: "cantidad_hijos_mayores_incapaces",
                schema: "divorcios",
                table: "preregistro",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "cantidad_hijos_menores",
                schema: "divorcios",
                table: "preregistro",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_matrimonio",
                schema: "divorcios",
                table: "preregistro",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "matrimonio_en_porvenir",
                schema: "divorcios",
                table: "preregistro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "mutuo_acuerdo_declarado",
                schema: "divorcios",
                table: "preregistro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "tiene_bienes_sociales",
                schema: "divorcios",
                table: "preregistro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ultimo_domicilio_conyugal_en_porvenir",
                schema: "divorcios",
                table: "preregistro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "documento_oficio_id",
                schema: "divorcios",
                table: "oficio",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "numero_expediente",
                schema: "divorcios",
                table: "expediente",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "fecha_ingreso_mesa_partes",
                schema: "divorcios",
                table: "expediente",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "caso_id",
                schema: "divorcios",
                table: "expediente",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "observacion",
                schema: "divorcios",
                table: "expediente",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<short>(
                name: "numero_version",
                schema: "divorcios",
                table: "documento_version",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "creado_en",
                schema: "divorcios",
                table: "cuenta_ciudadana",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "resultado_codigo",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expira_en",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "consultado_en",
                schema: "divorcios",
                table: "consulta_reniec",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "ratifico_voluntad",
                schema: "divorcios",
                table: "asistencia_audiencia",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "tipo_codigo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.CreateTable(
                name: "caso",
                schema: "divorcios",
                columns: table => new
                {
                    caso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    creado_por_cuenta_id = table.Column<long>(type: "bigint", nullable: true),
                    cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    codigo_pre = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
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
                name: "estado_caso",
                schema: "divorcios",
                columns: table => new
                {
                    estado_caso_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    codigo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    es_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    etapa_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    nombre_ciudadano = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    orden_visual = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_caso", x => x.estado_caso_id);
                    table.CheckConstraint("ck_estado_etapa", "etapa_codigo IN ('PRERREGISTRO', 'SEPARACION', 'ESPERA', 'DIVORCIO', 'CIERRE')");
                    table.CheckConstraint("ck_estado_orden", "orden_visual > 0");
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

            migrationBuilder.CreateTable(
                name: "pago_tramite",
                schema: "divorcios",
                columns: table => new
                {
                    pago_tramite_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    documento_comprobante_id = table.Column<long>(type: "bigint", nullable: true),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    concepto_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_pago = table.Column<DateOnly>(type: "date", nullable: false),
                    moneda_codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    numero_voucher = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pago_tramite", x => x.pago_tramite_id);
                    table.CheckConstraint("ck_pago_tramite_concepto", "concepto_codigo IN (\r\n    'COPIAS_CERTIFICADAS',\r\n    'TASA_PROCEDIMIENTO',\r\n    'OTRO'\r\n)");
                    table.CheckConstraint("ck_pago_tramite_moneda", "moneda_codigo = 'PEN'");
                    table.CheckConstraint("ck_pago_tramite_monto", "monto > 0");
                    table.CheckConstraint("ck_pago_tramite_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_pago_tramite_otro", "concepto_codigo <> 'OTRO'\r\nOR (\r\n    observacion IS NOT NULL\r\n    AND btrim(observacion) <> ''\r\n)");
                    table.CheckConstraint("ck_pago_tramite_voucher", "btrim(numero_voucher) <> ''");
                    table.ForeignKey(
                        name: "fk_pago_tramite_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_tramite_documento_documento_comprobante_id",
                        column: x => x.documento_comprobante_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_tramite_usuarios_internos_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historial_estado_caso",
                schema: "divorcios",
                columns: table => new
                {
                    historial_estado_caso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    estado_caso_id = table.Column<short>(type: "smallint", nullable: false),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    finalizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    iniciado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    numero_secuencia = table.Column<int>(type: "integer", nullable: false),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historial_estado_caso", x => x.historial_estado_caso_id);
                    table.CheckConstraint("ck_historial_estado_fechas", "finalizado_en IS NULL\r\nOR finalizado_en >= iniciado_en");
                    table.CheckConstraint("ck_historial_estado_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_historial_estado_secuencia", "numero_secuencia > 0");
                    table.ForeignKey(
                        name: "fk_historial_estado_caso_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_caso_estado_caso_estado_caso_id",
                        column: x => x.estado_caso_id,
                        principalSchema: "divorcios",
                        principalTable: "estado_caso",
                        principalColumn: "estado_caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_caso_usuarios_internos_registrado_por_usua",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plazo_caso",
                schema: "divorcios",
                columns: table => new
                {
                    plazo_caso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    creado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    historial_estado_caso_origen_id = table.Column<long>(type: "bigint", nullable: true),
                    regla_plazo_id = table.Column<short>(type: "smallint", nullable: false),
                    cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "PENDIENTE"),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    numero_aplicacion = table.Column<int>(type: "integer", nullable: false),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plazo_caso", x => x.plazo_caso_id);
                    table.CheckConstraint("ck_plazo_caso_aplicacion", "numero_aplicacion > 0");
                    table.CheckConstraint("ck_plazo_caso_cierre", "(\r\n    estado_codigo = 'PENDIENTE'\r\n    AND cerrado_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo IN (\r\n        'CUMPLIDO',\r\n        'CANCELADO'\r\n    )\r\n    AND cerrado_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_plazo_caso_estado", "estado_codigo IN (\r\n    'PENDIENTE',\r\n    'CUMPLIDO',\r\n    'CANCELADO'\r\n)");
                    table.CheckConstraint("ck_plazo_caso_fechas", "fecha_vencimiento >= fecha_inicio");
                    table.CheckConstraint("ck_plazo_caso_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.ForeignKey(
                        name: "fk_plazo_caso_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_caso_historial_estado_caso_historial_estado_caso_orig",
                        column: x => x.historial_estado_caso_origen_id,
                        principalSchema: "divorcios",
                        principalTable: "historial_estado_caso",
                        principalColumn: "historial_estado_caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_caso_reglas_plazo_regla_plazo_id",
                        column: x => x.regla_plazo_id,
                        principalSchema: "divorcios",
                        principalTable: "regla_plazo",
                        principalColumn: "regla_plazo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_caso_usuarios_internos_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_representacion_caso_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                column: "caso_conyuge_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_cantidad",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "cantidad > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_tipo_dia",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "tipo_dia_codigo IN ('CALENDARIO', 'HABIL', 'OPERATIVO')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_unidad",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "unidad_codigo IN ('DIA', 'MES')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_regla_vigencia",
                schema: "divorcios",
                table: "regla_plazo",
                sql: "vigente_hasta IS NULL OR vigente_hasta >= vigente_desde");

            migrationBuilder.AddCheckConstraint(
                name: "ck_preregistro_envio",
                schema: "divorcios",
                table: "preregistro",
                sql: "enviado_en IS NULL OR enviado_en >= creado_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_preregistro_hijos_mayores_incapaces",
                schema: "divorcios",
                table: "preregistro",
                sql: "cantidad_hijos_mayores_incapaces >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_preregistro_hijos_menores",
                schema: "divorcios",
                table: "preregistro",
                sql: "cantidad_hijos_menores >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_caso_id",
                schema: "divorcios",
                table: "expediente",
                column: "caso_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente",
                column: "registrado_por_usuario_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_numero",
                schema: "divorcios",
                table: "expediente",
                sql: "btrim(numero_expediente) <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expediente_observacion",
                schema: "divorcios",
                table: "expediente",
                sql: "observacion IS NULL\r\nOR btrim(observacion) <> ''");

            migrationBuilder.CreateIndex(
                name: "ix_consulta_reniec_expira_en",
                schema: "divorcios",
                table: "consulta_reniec",
                column: "expira_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_datos_encontrados",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "resultado_codigo <> 'ENCONTRADO' OR (prenombres IS NOT NULL AND apellido_paterno IS NOT NULL AND apellido_materno IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_expiracion",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "expira_en >= consultado_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_http",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "codigo_http IS NULL OR codigo_http BETWEEN 100 AND 599");

            migrationBuilder.AddCheckConstraint(
                name: "ck_consulta_reniec_resultado",
                schema: "divorcios",
                table: "consulta_reniec",
                sql: "resultado_codigo IN ('ENCONTRADO', 'NO_ENCONTRADO', 'ERROR')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asistencia_inasistencia",
                schema: "divorcios",
                table: "asistencia_audiencia",
                sql: "asistio = TRUE\r\nOR (\r\n    ratifico_voluntad = FALSE\r\n    AND identidad_verificada_en IS NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asistencia_ratificacion",
                schema: "divorcios",
                table: "asistencia_audiencia",
                sql: "ratifico_voluntad = FALSE\r\nOR (\r\n    asistio = TRUE\r\n    AND identidad_verificada_en IS NOT NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_asistencia_verificacion",
                schema: "divorcios",
                table: "asistencia_audiencia",
                sql: "identidad_verificada_en IS NULL\r\nOR asistio = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_caso_id_tipo_codigo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                columns: new[] { "caso_id", "tipo_codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_actuacion_administrativa_numero_resolucion_fecha_emision",
                schema: "divorcios",
                table: "actuacion_administrativa",
                columns: new[] { "numero_resolucion", "fecha_emision" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_datos_emision",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "(\r\n    estado_codigo = 'BORRADOR'\r\n    AND numero_resolucion IS NULL\r\n    AND fecha_emision IS NULL\r\n    AND emitida_por_usuario_id IS NULL\r\n    AND emitida_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'EMITIDA'\r\n    AND numero_resolucion IS NOT NULL\r\n    AND btrim(numero_resolucion) <> ''\r\n    AND fecha_emision IS NOT NULL\r\n    AND emitida_por_usuario_id IS NOT NULL\r\n    AND emitida_en IS NOT NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_estado",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "estado_codigo IN (\r\n    'BORRADOR',\r\n    'EMITIDA'\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_fecha_registro_emision",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "emitida_en IS NULL\r\nOR emitida_en >= creada_en");

            migrationBuilder.AddCheckConstraint(
                name: "ck_actuacion_tipo",
                schema: "divorcios",
                table: "actuacion_administrativa",
                sql: "tipo_codigo IN (\r\n    'ADMISIBILIDAD',\r\n    'SEPARACION_CONVENCIONAL',\r\n    'DISOLUCION_VINCULO'\r\n)");

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
                name: "ix_estado_caso_codigo",
                schema: "divorcios",
                table: "estado_caso",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estado_caso_etapa_codigo_orden_visual",
                schema: "divorcios",
                table: "estado_caso",
                columns: new[] { "etapa_codigo", "orden_visual" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_caso_id",
                schema: "divorcios",
                table: "historial_estado_caso",
                column: "caso_id",
                unique: true,
                filter: "finalizado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_caso_id_iniciado_en",
                schema: "divorcios",
                table: "historial_estado_caso",
                columns: new[] { "caso_id", "iniciado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_caso_id_numero_secuencia",
                schema: "divorcios",
                table: "historial_estado_caso",
                columns: new[] { "caso_id", "numero_secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_estado_caso_id",
                schema: "divorcios",
                table: "historial_estado_caso",
                column: "estado_caso_id");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_registrado_por_usuario_id",
                schema: "divorcios",
                table: "historial_estado_caso",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_caso_id_fecha_pago",
                schema: "divorcios",
                table: "pago_tramite",
                columns: new[] { "caso_id", "fecha_pago" });

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_concepto_codigo",
                schema: "divorcios",
                table: "pago_tramite",
                column: "concepto_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_documento_comprobante_id",
                schema: "divorcios",
                table: "pago_tramite",
                column: "documento_comprobante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_numero_voucher_fecha_pago",
                schema: "divorcios",
                table: "pago_tramite",
                columns: new[] { "numero_voucher", "fecha_pago" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_registrado_por_usuario_id",
                schema: "divorcios",
                table: "pago_tramite",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_caso_id_estado_codigo_fecha_vencimiento",
                schema: "divorcios",
                table: "plazo_caso",
                columns: new[] { "caso_id", "estado_codigo", "fecha_vencimiento" });

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_caso_id_regla_plazo_id_numero_aplicacion",
                schema: "divorcios",
                table: "plazo_caso",
                columns: new[] { "caso_id", "regla_plazo_id", "numero_aplicacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_creado_por_usuario_id",
                schema: "divorcios",
                table: "plazo_caso",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_historial_estado_caso_origen_id",
                schema: "divorcios",
                table: "plazo_caso",
                column: "historial_estado_caso_origen_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_regla_plazo_id",
                schema: "divorcios",
                table: "plazo_caso",
                column: "regla_plazo_id");

            migrationBuilder.AddForeignKey(
                name: "fk_actuacion_administrativa_casos_caso_id",
                schema: "divorcios",
                table: "actuacion_administrativa",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_asistencia_audiencia_casos_conyuges_caso_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                column: "caso_conyuge_id",
                principalSchema: "divorcios",
                principalTable: "caso_conyuge",
                principalColumn: "caso_conyuge_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_audiencia_ratificacion_casos_caso_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_documento_actuacion_administrativa_actuacion_administrativa",
                schema: "divorcios",
                table: "documento",
                column: "actuacion_administrativa_id",
                principalSchema: "divorcios",
                principalTable: "actuacion_administrativa",
                principalColumn: "actuacion_administrativa_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_documento_caso_caso_id",
                schema: "divorcios",
                table: "documento",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_expediente_caso_caso_id",
                schema: "divorcios",
                table: "expediente",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_expediente_usuarios_internos_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente",
                column: "registrado_por_usuario_id",
                principalSchema: "divorcios",
                principalTable: "usuario_interno",
                principalColumn: "usuario_interno_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_notificacion_caso_caso_id",
                schema: "divorcios",
                table: "notificacion",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_preregistro_caso_caso_id",
                schema: "divorcios",
                table: "preregistro",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_preregistro_requisito_preregistro_preregistro_id",
                schema: "divorcios",
                table: "preregistro_requisito",
                column: "preregistro_id",
                principalSchema: "divorcios",
                principalTable: "preregistro",
                principalColumn: "preregistro_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_registro_auditoria_caso_caso_id",
                schema: "divorcios",
                table: "registro_auditoria",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_representacion_caso_conyuge_caso_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                column: "caso_conyuge_id",
                principalSchema: "divorcios",
                principalTable: "caso_conyuge",
                principalColumn: "caso_conyuge_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_revision_preregistro_preregistro_preregistro_id",
                schema: "divorcios",
                table: "revision_preregistro",
                column: "preregistro_id",
                principalSchema: "divorcios",
                principalTable: "preregistro",
                principalColumn: "preregistro_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitante_disolucion_caso_conyuge_caso_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                column: "caso_conyuge_id",
                principalSchema: "divorcios",
                principalTable: "caso_conyuge",
                principalColumn: "caso_conyuge_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitud_disolucion_caso_caso_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "caso_id",
                principalSchema: "divorcios",
                principalTable: "caso",
                principalColumn: "caso_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
