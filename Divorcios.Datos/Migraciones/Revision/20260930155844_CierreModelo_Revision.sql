-- SQL INCREMENTAL DE REVISION. Cadena completa aplicada a la nueva base local autorizada.
-- Cadena validada en PostgreSQL temporal aislado.
-- Rechaza tablas de divorcios con datos; ver VALIDACION_POSTGRESQL.md.

START TRANSACTION;
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

ALTER TABLE divorcios.actuacion_administrativa DROP CONSTRAINT fk_actuacion_administrativa_casos_caso_id;

ALTER TABLE divorcios.asistencia_audiencia DROP CONSTRAINT fk_asistencia_audiencia_casos_conyuges_caso_conyuge_id;

ALTER TABLE divorcios.audiencia_ratificacion DROP CONSTRAINT fk_audiencia_ratificacion_casos_caso_id;

ALTER TABLE divorcios.documento DROP CONSTRAINT fk_documento_actuacion_administrativa_actuacion_administrativa;

ALTER TABLE divorcios.documento DROP CONSTRAINT fk_documento_caso_caso_id;

ALTER TABLE divorcios.expediente DROP CONSTRAINT fk_expediente_caso_caso_id;

ALTER TABLE divorcios.expediente DROP CONSTRAINT fk_expediente_usuarios_internos_registrado_por_usuario_id;

ALTER TABLE divorcios.notificacion DROP CONSTRAINT fk_notificacion_caso_caso_id;

ALTER TABLE divorcios.preregistro DROP CONSTRAINT fk_preregistro_caso_caso_id;

ALTER TABLE divorcios.preregistro_requisito DROP CONSTRAINT fk_preregistro_requisito_preregistro_preregistro_id;

ALTER TABLE divorcios.registro_auditoria DROP CONSTRAINT fk_registro_auditoria_caso_caso_id;

ALTER TABLE divorcios.representacion DROP CONSTRAINT fk_representacion_caso_conyuge_caso_conyuge_id;

ALTER TABLE divorcios.revision_preregistro DROP CONSTRAINT fk_revision_preregistro_preregistro_preregistro_id;

ALTER TABLE divorcios.solicitante_disolucion DROP CONSTRAINT fk_solicitante_disolucion_caso_conyuge_caso_conyuge_id;

ALTER TABLE divorcios.solicitud_disolucion DROP CONSTRAINT fk_solicitud_disolucion_caso_caso_id;

DROP TABLE divorcios.caso_conyuge;

DROP TABLE divorcios.pago_tramite;

DROP TABLE divorcios.plazo_caso;

DROP TABLE divorcios.historial_estado_caso;

DROP TABLE divorcios.caso;

DROP TABLE divorcios.estado_caso;

DROP INDEX divorcios.ix_representacion_caso_conyuge_id;

ALTER TABLE divorcios.regla_plazo DROP CONSTRAINT ck_regla_cantidad;

ALTER TABLE divorcios.regla_plazo DROP CONSTRAINT ck_regla_tipo_dia;

ALTER TABLE divorcios.regla_plazo DROP CONSTRAINT ck_regla_unidad;

ALTER TABLE divorcios.regla_plazo DROP CONSTRAINT ck_regla_vigencia;

ALTER TABLE divorcios.preregistro DROP CONSTRAINT ck_preregistro_envio;

ALTER TABLE divorcios.preregistro DROP CONSTRAINT ck_preregistro_hijos_mayores_incapaces;

ALTER TABLE divorcios.preregistro DROP CONSTRAINT ck_preregistro_hijos_menores;

DROP INDEX divorcios.ix_expediente_caso_id;

DROP INDEX divorcios.ix_expediente_registrado_por_usuario_id;

ALTER TABLE divorcios.expediente DROP CONSTRAINT ck_expediente_numero;

ALTER TABLE divorcios.expediente DROP CONSTRAINT ck_expediente_observacion;

DROP INDEX divorcios.ix_consulta_reniec_expira_en;

ALTER TABLE divorcios.consulta_reniec DROP CONSTRAINT ck_consulta_reniec_datos_encontrados;

ALTER TABLE divorcios.consulta_reniec DROP CONSTRAINT ck_consulta_reniec_expiracion;

ALTER TABLE divorcios.consulta_reniec DROP CONSTRAINT ck_consulta_reniec_http;

ALTER TABLE divorcios.consulta_reniec DROP CONSTRAINT ck_consulta_reniec_resultado;

ALTER TABLE divorcios.asistencia_audiencia DROP CONSTRAINT ck_asistencia_inasistencia;

ALTER TABLE divorcios.asistencia_audiencia DROP CONSTRAINT ck_asistencia_ratificacion;

ALTER TABLE divorcios.asistencia_audiencia DROP CONSTRAINT ck_asistencia_verificacion;

DROP INDEX divorcios.ix_actuacion_administrativa_caso_id_tipo_codigo;

DROP INDEX divorcios.ix_actuacion_administrativa_numero_resolucion_fecha_emision;

ALTER TABLE divorcios.actuacion_administrativa DROP CONSTRAINT ck_actuacion_datos_emision;

ALTER TABLE divorcios.actuacion_administrativa DROP CONSTRAINT ck_actuacion_estado;

ALTER TABLE divorcios.actuacion_administrativa DROP CONSTRAINT ck_actuacion_fecha_registro_emision;

ALTER TABLE divorcios.actuacion_administrativa DROP CONSTRAINT ck_actuacion_tipo;

ALTER TABLE divorcios.preregistro DROP COLUMN cantidad_hijos_mayores_incapaces;

ALTER TABLE divorcios.preregistro DROP COLUMN cantidad_hijos_menores;

ALTER TABLE divorcios.preregistro DROP COLUMN fecha_matrimonio;

ALTER TABLE divorcios.preregistro DROP COLUMN matrimonio_en_porvenir;

ALTER TABLE divorcios.preregistro DROP COLUMN mutuo_acuerdo_declarado;

ALTER TABLE divorcios.preregistro DROP COLUMN tiene_bienes_sociales;

ALTER TABLE divorcios.preregistro DROP COLUMN ultimo_domicilio_conyugal_en_porvenir;

ALTER TABLE divorcios.expediente DROP COLUMN caso_id;

ALTER TABLE divorcios.expediente DROP COLUMN observacion;

ALTER TABLE divorcios.expediente DROP COLUMN registrado_por_usuario_id;

ALTER TABLE divorcios.solicitud_disolucion RENAME COLUMN caso_id TO expediente_id;

ALTER INDEX divorcios.ix_solicitud_disolucion_caso_id RENAME TO ix_solicitud_disolucion_expediente_id;

ALTER TABLE divorcios.solicitante_disolucion RENAME COLUMN caso_conyuge_id TO expediente_conyuge_id;

ALTER INDEX divorcios.ix_solicitante_disolucion_solicitud_disolucion_id_caso_conyuge RENAME TO ix_solicitante_disolucion_solicitud_disolucion_id_expediente_c;

ALTER INDEX divorcios.ix_solicitante_disolucion_caso_conyuge_id RENAME TO ix_solicitante_disolucion_expediente_conyuge_id;

ALTER TABLE divorcios.revision_preregistro RENAME COLUMN preregistro_id TO preregistro_version_id;

ALTER INDEX divorcios.ix_revision_preregistro_preregistro_id_numero_revision RENAME TO ix_revision_preregistro_preregistro_version_id_numero_revision;

ALTER TABLE divorcios.representacion RENAME COLUMN caso_conyuge_id TO expediente_conyuge_id;

ALTER TABLE divorcios.registro_auditoria RENAME COLUMN caso_id TO expediente_id;

ALTER INDEX divorcios.ix_registro_auditoria_caso_id_registrado_en RENAME TO ix_registro_auditoria_expediente_id_registrado_en;

ALTER TABLE divorcios.preregistro_requisito RENAME COLUMN preregistro_id TO preregistro_version_id;

ALTER INDEX divorcios.ix_preregistro_requisito_preregistro_id_requisito_catalogo_id RENAME TO ix_preregistro_requisito_preregistro_version_id_requisito_cata;

ALTER TABLE divorcios.preregistro RENAME COLUMN caso_id TO expediente_id;

ALTER INDEX divorcios.ix_preregistro_caso_id RENAME TO ix_preregistro_expediente_id;

ALTER TABLE divorcios.notificacion RENAME COLUMN caso_id TO expediente_id;

ALTER INDEX divorcios.ix_notificacion_caso_id_creada_en RENAME TO ix_notificacion_expediente_id_creada_en;

ALTER TABLE divorcios.expediente RENAME COLUMN registrado_en TO fecha_inicio_digital;

ALTER TABLE divorcios.documento_version RENAME COLUMN cargado_en TO creado_en;

ALTER TABLE divorcios.documento RENAME COLUMN caso_id TO expediente_id;

DROP INDEX divorcios.ix_documento_actuacion_administrativa_id;

ALTER TABLE divorcios.documento DROP COLUMN actuacion_administrativa_id;

ALTER TABLE divorcios.documento ADD creado_por_usuario_id bigint;

ALTER INDEX divorcios.ix_documento_caso_id_etapa_codigo RENAME TO ix_documento_expediente_id_etapa_codigo;

CREATE INDEX ix_documento_creado_por_usuario_id ON divorcios.documento (creado_por_usuario_id);

ALTER TABLE divorcios.cuenta_ciudadana DROP COLUMN bloqueado_en;

ALTER TABLE divorcios.cuenta_ciudadana ADD ultimo_acceso_en timestamp with time zone;

ALTER TABLE divorcios.audiencia_ratificacion RENAME COLUMN caso_id TO expediente_id;

ALTER INDEX divorcios.ix_audiencia_ratificacion_caso_id_numero_programacion RENAME TO ix_audiencia_ratificacion_expediente_id_numero_programacion;

ALTER INDEX divorcios.ix_audiencia_ratificacion_caso_id_fecha_hora_programada RENAME TO ix_audiencia_ratificacion_expediente_id_fecha_hora_programada;

ALTER INDEX divorcios.ix_audiencia_ratificacion_caso_id RENAME TO ix_audiencia_ratificacion_expediente_id;

ALTER TABLE divorcios.asistencia_audiencia RENAME COLUMN caso_conyuge_id TO expediente_conyuge_id;

ALTER INDEX divorcios.ix_asistencia_audiencia_caso_conyuge_id RENAME TO ix_asistencia_audiencia_expediente_conyuge_id;

ALTER INDEX divorcios.ix_asistencia_audiencia_audiencia_ratificacion_id_caso_conyuge RENAME TO ix_asistencia_audiencia_audiencia_ratificacion_id_expediente_c;

ALTER TABLE divorcios.actuacion_administrativa RENAME COLUMN caso_id TO expediente_id;

ALTER TABLE divorcios.usuario_interno ADD ultimo_acceso_en timestamp with time zone;

ALTER TABLE divorcios.solicitante_disolucion ADD creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE divorcios.representacion ALTER COLUMN documento_poder_id DROP NOT NULL;

ALTER TABLE divorcios.representacion ADD estado_codigo character varying(20) NOT NULL DEFAULT 'VIGENTE';

ALTER TABLE divorcios.representacion ADD tipo_poder_codigo character varying(30) NOT NULL DEFAULT 'ESPECIAL';

ALTER TABLE divorcios.regla_plazo ALTER COLUMN unidad_codigo TYPE character varying(20);

ALTER TABLE divorcios.regla_plazo ALTER COLUMN tipo_dia_codigo TYPE character varying(20);

ALTER TABLE divorcios.regla_plazo ALTER COLUMN nombre TYPE character varying(160);

ALTER TABLE divorcios.regla_plazo ALTER COLUMN fuente TYPE character varying(250);

ALTER TABLE divorcios.regla_plazo ALTER COLUMN codigo TYPE character varying(50);

ALTER TABLE divorcios.regla_plazo ADD activo boolean NOT NULL DEFAULT TRUE;

ALTER TABLE divorcios.regla_plazo ADD descripcion character varying(1000);

ALTER TABLE divorcios.regla_plazo ADD evento_inicio_codigo character varying(50) NOT NULL;

ALTER TABLE divorcios.preregistro_requisito ADD aplica boolean NOT NULL DEFAULT TRUE;

ALTER TABLE divorcios.preregistro_requisito ADD generado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE divorcios.preregistro ADD bloqueado_en timestamp with time zone;

ALTER TABLE divorcios.preregistro ADD estado_codigo character varying(20) NOT NULL DEFAULT 'BORRADOR';

ALTER TABLE divorcios.persona ADD actualizado_en timestamp with time zone;

ALTER TABLE divorcios.persona ADD verificado_reniec boolean NOT NULL DEFAULT FALSE;

ALTER TABLE divorcios.oficio ALTER COLUMN documento_oficio_id DROP NOT NULL;

ALTER TABLE divorcios.expediente ALTER COLUMN numero_expediente DROP NOT NULL;

ALTER TABLE divorcios.expediente ALTER COLUMN fecha_ingreso_mesa_partes DROP NOT NULL;

ALTER TABLE divorcios.expediente ADD cerrado_en timestamp with time zone;

ALTER TABLE divorcios.expediente ADD codigo_preregistro character varying(30) NOT NULL;

ALTER TABLE divorcios.expediente ADD creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE divorcios.expediente ADD creado_por_cuenta_id bigint;

ALTER TABLE divorcios.expediente ADD oficializado_en timestamp with time zone;

ALTER TABLE divorcios.expediente ADD preregistro_bloqueado_en timestamp with time zone;

ALTER TABLE divorcios.documento_version ALTER COLUMN numero_version TYPE integer;

ALTER TABLE divorcios.documento_version ADD motivo_cambio character varying(300);

ALTER TABLE divorcios.documento ADD creado_por_cuenta_id bigint;

ALTER TABLE divorcios.documento ADD estado_codigo character varying(20) NOT NULL DEFAULT 'VIGENTE';

ALTER TABLE divorcios.cuenta_ciudadana ALTER COLUMN creado_en SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE divorcios.cuenta_ciudadana ADD bloqueado_hasta timestamp with time zone;

ALTER TABLE divorcios.cuenta_ciudadana ADD clave_hash character varying(255);

ALTER TABLE divorcios.cuenta_ciudadana ADD correo_verificado_en timestamp with time zone;

ALTER TABLE divorcios.cuenta_ciudadana ADD estado_codigo character varying(20) NOT NULL DEFAULT 'ACTIVA';

ALTER TABLE divorcios.cuenta_ciudadana ADD intentos_fallidos smallint NOT NULL DEFAULT 0;

ALTER TABLE divorcios.consulta_reniec ALTER COLUMN resultado_codigo TYPE character varying(30);

ALTER TABLE divorcios.consulta_reniec ALTER COLUMN expira_en DROP NOT NULL;

ALTER TABLE divorcios.consulta_reniec ALTER COLUMN consultado_en SET DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE divorcios.consulta_reniec ADD origen_codigo character varying(20) NOT NULL DEFAULT 'API';

ALTER TABLE divorcios.consulta_reniec ADD respuesta_hash character varying(128);

ALTER TABLE divorcios.asistencia_audiencia ALTER COLUMN ratifico_voluntad DROP NOT NULL;
ALTER TABLE divorcios.asistencia_audiencia ALTER COLUMN ratifico_voluntad DROP DEFAULT;

ALTER TABLE divorcios.actuacion_administrativa ALTER COLUMN tipo_codigo TYPE character varying(40);

ALTER TABLE divorcios.actuacion_administrativa ADD documento_id bigint;

ALTER TABLE divorcios.actuacion_administrativa ADD fecha_notificacion date;

ALTER TABLE divorcios.actuacion_administrativa ADD numero_secuencia integer NOT NULL;

CREATE TABLE divorcios.estado_expediente (
    estado_expediente_id smallint GENERATED ALWAYS AS IDENTITY,
    codigo character varying(45) NOT NULL,
    nombre_ciudadano character varying(180) NOT NULL,
    etapa_codigo character varying(25) NOT NULL,
    orden_visual smallint NOT NULL,
    es_final boolean NOT NULL DEFAULT FALSE,
    activo boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT pk_estado_expediente PRIMARY KEY (estado_expediente_id),
    CONSTRAINT ck_estado_etapa CHECK (etapa_codigo IN ('PRERREGISTRO', 'SEPARACION', 'ESPERA', 'DIVORCIO', 'CIERRE')),
    CONSTRAINT ck_estado_orden CHECK (orden_visual > 0)
);

CREATE TABLE divorcios.expediente_conyuge (
    expediente_conyuge_id bigint GENERATED ALWAYS AS IDENTITY,
    expediente_id bigint NOT NULL,
    persona_id bigint NOT NULL,
    posicion_codigo char(1) NOT NULL,
    es_iniciador boolean NOT NULL DEFAULT FALSE,
    creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT pk_expediente_conyuge PRIMARY KEY (expediente_conyuge_id),
    CONSTRAINT ck_conyuge_posicion CHECK (posicion_codigo IN ('A', 'B')),
    CONSTRAINT fk_expediente_conyuge_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_expediente_conyuge_personas_persona_id FOREIGN KEY (persona_id) REFERENCES divorcios.persona (persona_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.preregistro_requisito_documento (
    preregistro_requisito_id bigint NOT NULL,
    documento_version_id bigint NOT NULL,
    CONSTRAINT pk_preregistro_requisito_documento PRIMARY KEY (preregistro_requisito_id, documento_version_id),
    CONSTRAINT fk_preregistro_requisito_documento_documento_version_documento FOREIGN KEY (documento_version_id) REFERENCES divorcios.documento_version (documento_version_id) ON DELETE RESTRICT,
    CONSTRAINT fk_preregistro_requisito_documento_preregistro_requisito_prere FOREIGN KEY (preregistro_requisito_id) REFERENCES divorcios.preregistro_requisito (preregistro_requisito_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.preregistro_version (
    preregistro_version_id bigint GENERATED ALWAYS AS IDENTITY,
    preregistro_id bigint NOT NULL,
    numero_version integer NOT NULL,
    fecha_matrimonio date NOT NULL,
    matrimonio_en_porvenir boolean NOT NULL,
    ultimo_domicilio_conyugal_porvenir boolean NOT NULL,
    domicilio_conyugal character varying(250),
    tiene_hijos boolean NOT NULL,
    cantidad_hijos_menores smallint NOT NULL DEFAULT 0,
    cantidad_hijos_mayores smallint NOT NULL DEFAULT 0,
    tiene_hijos_mayores_situacion_especial boolean,
    tiene_bienes boolean NOT NULL,
    tiene_acuerdo_bienes boolean NOT NULL DEFAULT FALSE,
    requiere_representacion_a boolean,
    requiere_representacion_b boolean,
    observacion_ciudadano character varying(2000),
    motivo_cambio character varying(300),
    creado_por_cuenta_id bigint NOT NULL,
    creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT pk_preregistro_version PRIMARY KEY (preregistro_version_id),
    CONSTRAINT ck_preregistro_version_acuerdo_bienes CHECK (tiene_bienes
OR tiene_acuerdo_bienes = FALSE),
    CONSTRAINT ck_preregistro_version_hijos_mayores CHECK (cantidad_hijos_mayores >= 0),
    CONSTRAINT ck_preregistro_version_hijos_mayores_situacion_especial CHECK (tiene_hijos_mayores_situacion_especial IS NOT TRUE
OR (tiene_hijos AND cantidad_hijos_mayores > 0)),
    CONSTRAINT ck_preregistro_version_hijos_menores CHECK (cantidad_hijos_menores >= 0),
    CONSTRAINT ck_preregistro_version_motivo CHECK (numero_version = 1
OR (
    motivo_cambio IS NOT NULL
    AND btrim(motivo_cambio) <> ''
)),
    CONSTRAINT ck_preregistro_version_numero CHECK (numero_version > 0),
    CONSTRAINT ck_preregistro_version_sin_hijos CHECK (tiene_hijos
OR (
    cantidad_hijos_menores = 0
    AND cantidad_hijos_mayores = 0
)),
    CONSTRAINT fk_preregistro_version_cuenta_ciudadana_creado_por_cuenta_id FOREIGN KEY (creado_por_cuenta_id) REFERENCES divorcios.cuenta_ciudadana (cuenta_ciudadana_id) ON DELETE RESTRICT,
    CONSTRAINT fk_preregistro_version_preregistro_preregistro_id FOREIGN KEY (preregistro_id) REFERENCES divorcios.preregistro (preregistro_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.revision_detalle_documento (
    revision_detalle_id bigint NOT NULL,
    documento_version_id bigint NOT NULL,
    CONSTRAINT pk_revision_detalle_documento PRIMARY KEY (revision_detalle_id, documento_version_id),
    CONSTRAINT fk_revision_detalle_documento_documento_version_documento_vers FOREIGN KEY (documento_version_id) REFERENCES divorcios.documento_version (documento_version_id) ON DELETE RESTRICT,
    CONSTRAINT fk_revision_detalle_documento_revision_detalle_revision_detall FOREIGN KEY (revision_detalle_id) REFERENCES divorcios.revision_detalle (revision_detalle_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.historial_estado_expediente (
    historial_estado_expediente_id bigint GENERATED ALWAYS AS IDENTITY,
    expediente_id bigint NOT NULL,
    estado_expediente_id smallint NOT NULL,
    numero_secuencia integer NOT NULL,
    registrado_por_usuario_id bigint,
    iniciado_en timestamp with time zone NOT NULL,
    finalizado_en timestamp with time zone,
    registrado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    observacion character varying(2000),
    CONSTRAINT pk_historial_estado_expediente PRIMARY KEY (historial_estado_expediente_id),
    CONSTRAINT ck_historial_estado_fechas CHECK (finalizado_en IS NULL
OR finalizado_en >= iniciado_en),
    CONSTRAINT ck_historial_estado_observacion CHECK (observacion IS NULL
OR btrim(observacion) <> ''),
    CONSTRAINT ck_historial_estado_secuencia CHECK (numero_secuencia > 0),
    CONSTRAINT fk_historial_estado_expediente_estado_expediente_estado_expedi FOREIGN KEY (estado_expediente_id) REFERENCES divorcios.estado_expediente (estado_expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_historial_estado_expediente_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_historial_estado_expediente_usuarios_internos_registrado_po FOREIGN KEY (registrado_por_usuario_id) REFERENCES divorcios.usuario_interno (usuario_interno_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.pago (
    pago_id bigint GENERATED ALWAYS AS IDENTITY,
    expediente_id bigint NOT NULL,
    expediente_conyuge_pagante_id bigint NOT NULL,
    documento_comprobante_id bigint,
    registrado_por_usuario_id bigint NOT NULL,
    numero_pago integer NOT NULL,
    dni_pagante_snapshot char(8) NOT NULL,
    nombre_pagante_snapshot character varying(200) NOT NULL,
    concepto_codigo character varying(40) NOT NULL,
    concepto_descripcion_snapshot character varying(200) NOT NULL,
    monto numeric(10,2) NOT NULL,
    moneda_codigo character varying(3) NOT NULL DEFAULT 'PEN',
    estado_codigo character varying(20) NOT NULL DEFAULT 'PENDIENTE',
    numero_voucher character varying(80),
    referencia_caja character varying(120),
    solicitado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ultima_consulta_caja_en timestamp with time zone,
    pagado_en timestamp with time zone,
    anulado_en timestamp with time zone,
    observacion character varying(2000),
    creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT pk_pago PRIMARY KEY (pago_id),
    CONSTRAINT ck_pago_concepto CHECK (btrim(concepto_codigo) <> ''),
    CONSTRAINT ck_pago_descripcion CHECK (btrim(concepto_descripcion_snapshot) <> ''),
    CONSTRAINT ck_pago_dni CHECK (dni_pagante_snapshot ~ '^[0-9]{8}$'),
    CONSTRAINT ck_pago_estado CHECK (estado_codigo IN (
    'PENDIENTE',
    'PAGADO',
    'ANULADO'
)),
    CONSTRAINT ck_pago_fechas CHECK ((
    ultima_consulta_caja_en IS NULL
    OR ultima_consulta_caja_en >= solicitado_en
)
AND
(
    pagado_en IS NULL
    OR pagado_en >= solicitado_en
)
AND
(
    anulado_en IS NULL
    OR anulado_en >= solicitado_en
)
AND
(
    pagado_en IS NULL
    OR anulado_en IS NULL
    OR anulado_en >= pagado_en
)),
    CONSTRAINT ck_pago_flujo CHECK ((
    estado_codigo = 'PENDIENTE'
    AND pagado_en IS NULL
    AND anulado_en IS NULL
    AND numero_voucher IS NULL
)
OR
(
    estado_codigo = 'PAGADO'
    AND pagado_en IS NOT NULL
    AND anulado_en IS NULL
    AND numero_voucher IS NOT NULL
)
OR
(
    estado_codigo = 'ANULADO'
    AND anulado_en IS NOT NULL
    AND (
        pagado_en IS NULL
        OR numero_voucher IS NOT NULL
    )
)),
    CONSTRAINT ck_pago_moneda CHECK (moneda_codigo = 'PEN'),
    CONSTRAINT ck_pago_monto CHECK (monto > 0),
    CONSTRAINT ck_pago_nombre CHECK (btrim(nombre_pagante_snapshot) <> ''),
    CONSTRAINT ck_pago_numero CHECK (numero_pago > 0),
    CONSTRAINT ck_pago_observacion CHECK (observacion IS NULL
OR btrim(observacion) <> ''),
    CONSTRAINT ck_pago_referencia_caja CHECK (referencia_caja IS NULL
OR btrim(referencia_caja) <> ''),
    CONSTRAINT ck_pago_voucher CHECK (numero_voucher IS NULL
OR btrim(numero_voucher) <> ''),
    CONSTRAINT fk_pago_documento_documento_comprobante_id FOREIGN KEY (documento_comprobante_id) REFERENCES divorcios.documento (documento_id) ON DELETE RESTRICT,
    CONSTRAINT fk_pago_expediente_conyuge_expediente_conyuge_pagante_id FOREIGN KEY (expediente_conyuge_pagante_id) REFERENCES divorcios.expediente_conyuge (expediente_conyuge_id) ON DELETE RESTRICT,
    CONSTRAINT fk_pago_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_pago_usuarios_internos_registrado_por_usuario_id FOREIGN KEY (registrado_por_usuario_id) REFERENCES divorcios.usuario_interno (usuario_interno_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.expediente_contacto_historial (
    expediente_contacto_historial_id bigint GENERATED ALWAYS AS IDENTITY,
    expediente_conyuge_id bigint NOT NULL,
    tipo_contacto_codigo character varying(20) NOT NULL,
    valor character varying(250) NOT NULL,
    fuente_codigo character varying(30) NOT NULL,
    preregistro_version_origen_id bigint,
    vigente_desde timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    vigente_hasta timestamp with time zone,
    registrado_por_usuario_id bigint,
    CONSTRAINT pk_expediente_contacto_historial PRIMARY KEY (expediente_contacto_historial_id),
    CONSTRAINT ck_expediente_contacto_fuente CHECK (fuente_codigo IN (
    'PRERREGISTRO',
    'RENIEC',
    'MUNICIPALIDAD'
)),
    CONSTRAINT ck_expediente_contacto_origen CHECK (fuente_codigo <> 'PRERREGISTRO'
OR preregistro_version_origen_id IS NOT NULL),
    CONSTRAINT ck_expediente_contacto_tipo CHECK (tipo_contacto_codigo IN (
    'CELULAR',
    'CORREO',
    'DIRECCION'
)),
    CONSTRAINT ck_expediente_contacto_valor CHECK (btrim(valor) <> ''),
    CONSTRAINT ck_expediente_contacto_vigencia CHECK (vigente_hasta IS NULL
OR vigente_hasta >= vigente_desde),
    CONSTRAINT fk_expediente_contacto_historial_expedientes_conyuges_expedien FOREIGN KEY (expediente_conyuge_id) REFERENCES divorcios.expediente_conyuge (expediente_conyuge_id) ON DELETE RESTRICT,
    CONSTRAINT fk_expediente_contacto_historial_preregistros_versiones_prereg FOREIGN KEY (preregistro_version_origen_id) REFERENCES divorcios.preregistro_version (preregistro_version_id) ON DELETE RESTRICT,
    CONSTRAINT fk_expediente_contacto_historial_usuarios_internos_registrado_ FOREIGN KEY (registrado_por_usuario_id) REFERENCES divorcios.usuario_interno (usuario_interno_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.expediente_version (
    expediente_version_id bigint GENERATED ALWAYS AS IDENTITY,
    expediente_id bigint NOT NULL,
    numero_version integer NOT NULL,
    preregistro_version_origen_id bigint,
    fecha_matrimonio date NOT NULL,
    matrimonio_en_porvenir boolean NOT NULL,
    ultimo_domicilio_conyugal_porvenir boolean NOT NULL,
    domicilio_conyugal character varying(250),
    tiene_hijos boolean NOT NULL,
    cantidad_hijos_menores smallint NOT NULL DEFAULT 0,
    cantidad_hijos_mayores smallint NOT NULL DEFAULT 0,
    tiene_hijos_mayores_situacion_especial boolean,
    tiene_bienes boolean NOT NULL,
    tiene_acuerdo_bienes boolean NOT NULL DEFAULT FALSE,
    requiere_representacion_a boolean,
    requiere_representacion_b boolean,
    motivo_cambio character varying(300) NOT NULL,
    registrado_por_usuario_id bigint NOT NULL,
    creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT pk_expediente_version PRIMARY KEY (expediente_version_id),
    CONSTRAINT ck_expediente_version_acuerdo_bienes CHECK (tiene_bienes
OR tiene_acuerdo_bienes = FALSE),
    CONSTRAINT ck_expediente_version_hijos_mayores CHECK (cantidad_hijos_mayores >= 0),
    CONSTRAINT ck_expediente_version_hijos_mayores_situacion_especial CHECK (tiene_hijos_mayores_situacion_especial IS NOT TRUE
OR (tiene_hijos AND cantidad_hijos_mayores > 0)),
    CONSTRAINT ck_expediente_version_hijos_menores CHECK (cantidad_hijos_menores >= 0),
    CONSTRAINT ck_expediente_version_motivo CHECK (btrim(motivo_cambio) <> ''),
    CONSTRAINT ck_expediente_version_numero CHECK (numero_version > 0),
    CONSTRAINT ck_expediente_version_sin_hijos CHECK (tiene_hijos
OR (
    cantidad_hijos_menores = 0
    AND cantidad_hijos_mayores = 0
)),
    CONSTRAINT fk_expediente_version_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_expediente_version_preregistros_versiones_preregistro_versi FOREIGN KEY (preregistro_version_origen_id) REFERENCES divorcios.preregistro_version (preregistro_version_id) ON DELETE RESTRICT,
    CONSTRAINT fk_expediente_version_usuarios_internos_registrado_por_usuario FOREIGN KEY (registrado_por_usuario_id) REFERENCES divorcios.usuario_interno (usuario_interno_id) ON DELETE RESTRICT
);

CREATE TABLE divorcios.plazo_expediente (
    plazo_expediente_id bigint GENERATED ALWAYS AS IDENTITY,
    expediente_id bigint NOT NULL,
    regla_plazo_id smallint NOT NULL,
    historial_estado_expediente_origen_id bigint,
    numero_aplicacion integer NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_vencimiento date NOT NULL,
    estado_codigo character varying(15) NOT NULL DEFAULT 'PENDIENTE',
    cerrado_en timestamp with time zone,
    creado_por_usuario_id bigint,
    creado_en timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    observacion character varying(2000),
    CONSTRAINT pk_plazo_expediente PRIMARY KEY (plazo_expediente_id),
    CONSTRAINT ck_plazo_expediente_cierre CHECK ((
    estado_codigo = 'PENDIENTE'
    AND cerrado_en IS NULL
)
OR
(
    estado_codigo IN (
        'CUMPLIDO',
        'CANCELADO'
    )
    AND cerrado_en IS NOT NULL
)),
    CONSTRAINT ck_plazo_expediente_estado CHECK (estado_codigo IN (
    'PENDIENTE',
    'CUMPLIDO',
    'CANCELADO'
)),
    CONSTRAINT ck_plazo_expediente_estado_aplicacion CHECK (numero_aplicacion > 0),
    CONSTRAINT ck_plazo_expediente_fechas CHECK (fecha_vencimiento >= fecha_inicio),
    CONSTRAINT ck_plazo_expediente_observacion CHECK (observacion IS NULL
OR btrim(observacion) <> ''),
    CONSTRAINT fk_plazo_expediente_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_plazo_expediente_historial_estado_expediente_historial_esta FOREIGN KEY (historial_estado_expediente_origen_id) REFERENCES divorcios.historial_estado_expediente (historial_estado_expediente_id) ON DELETE RESTRICT,
    CONSTRAINT fk_plazo_expediente_reglas_plazo_regla_plazo_id FOREIGN KEY (regla_plazo_id) REFERENCES divorcios.regla_plazo (regla_plazo_id) ON DELETE RESTRICT,
    CONSTRAINT fk_plazo_expediente_usuarios_internos_creado_por_usuario_id FOREIGN KEY (creado_por_usuario_id) REFERENCES divorcios.usuario_interno (usuario_interno_id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX ix_representacion_expediente_conyuge_id ON divorcios.representacion (expediente_conyuge_id) WHERE estado_codigo = 'VIGENTE';

ALTER TABLE divorcios.representacion ADD CONSTRAINT ck_representacion_estado CHECK (estado_codigo IN (
    'VIGENTE',
    'REVOCADA',
    'VENCIDA'
));

ALTER TABLE divorcios.representacion ADD CONSTRAINT ck_representacion_estado_cierre CHECK (estado_codigo = 'VIGENTE'
OR vigente_hasta IS NOT NULL);

ALTER TABLE divorcios.representacion ADD CONSTRAINT ck_representacion_tipo_poder CHECK (tipo_poder_codigo = 'ESPECIAL');

CREATE INDEX ix_regla_plazo_activo_evento_inicio_codigo ON divorcios.regla_plazo (activo, evento_inicio_codigo);

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_cantidad CHECK (cantidad > 0);

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_descripcion CHECK (descripcion IS NULL
OR btrim(descripcion) <> '');

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_evento_inicio CHECK (btrim(evento_inicio_codigo) <> '');

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_fuente CHECK (btrim(fuente) <> '');

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_tipo_dia CHECK (tipo_dia_codigo IN (
    'CALENDARIO',
    'HABIL',
    'OPERATIVO'
));

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_unidad CHECK (unidad_codigo IN ('DIA', 'MES'));

ALTER TABLE divorcios.regla_plazo ADD CONSTRAINT ck_regla_plazo_vigencia CHECK (vigente_hasta IS NULL
OR vigente_hasta >= vigente_desde);

CREATE INDEX ix_preregistro_estado_codigo ON divorcios.preregistro (estado_codigo);

ALTER TABLE divorcios.preregistro ADD CONSTRAINT ck_preregistro_bloqueo CHECK (bloqueado_en IS NULL
OR (
    aprobado_en IS NOT NULL
    AND bloqueado_en >= aprobado_en
));

ALTER TABLE divorcios.preregistro ADD CONSTRAINT ck_preregistro_envio CHECK (enviado_en IS NULL
OR enviado_en >= creado_en);

ALTER TABLE divorcios.preregistro ADD CONSTRAINT ck_preregistro_estado CHECK (estado_codigo IN (
    'BORRADOR',
    'ENVIADO',
    'OBSERVADO',
    'APROBADO',
    'CANCELADO'
));

ALTER TABLE divorcios.persona ADD CONSTRAINT ck_persona_actualizacion CHECK (actualizado_en IS NULL
OR actualizado_en >= creado_en);

ALTER TABLE divorcios.persona ADD CONSTRAINT ck_persona_verificacion_reniec CHECK ((
    verificado_reniec = FALSE
    AND verificado_reniec_en IS NULL
)
OR
(
    verificado_reniec = TRUE
    AND verificado_reniec_en IS NOT NULL
));

CREATE INDEX ix_expediente_cerrado_en ON divorcios.expediente (cerrado_en);

CREATE UNIQUE INDEX ix_expediente_codigo_preregistro ON divorcios.expediente (codigo_preregistro);

CREATE INDEX ix_expediente_creado_por_cuenta_id ON divorcios.expediente (creado_por_cuenta_id);

ALTER TABLE divorcios.expediente ADD CONSTRAINT ck_expediente_bloqueo_preregistro CHECK (preregistro_bloqueado_en IS NULL
OR preregistro_bloqueado_en >= fecha_inicio_digital);

ALTER TABLE divorcios.expediente ADD CONSTRAINT ck_expediente_cierre CHECK (cerrado_en IS NULL
OR cerrado_en >= fecha_inicio_digital);

ALTER TABLE divorcios.expediente ADD CONSTRAINT ck_expediente_codigo_preregistro CHECK (btrim(codigo_preregistro) <> '');

ALTER TABLE divorcios.expediente ADD CONSTRAINT ck_expediente_numero CHECK (numero_expediente IS NULL
OR btrim(numero_expediente) <> '');

ALTER TABLE divorcios.expediente ADD CONSTRAINT ck_expediente_oficializacion CHECK ((
    numero_expediente IS NULL
    AND fecha_ingreso_mesa_partes IS NULL
    AND oficializado_en IS NULL
)
OR
(
    numero_expediente IS NOT NULL
    AND fecha_ingreso_mesa_partes IS NOT NULL
    AND oficializado_en IS NOT NULL
));

ALTER TABLE divorcios.documento_version ADD CONSTRAINT ck_documento_version_motivo CHECK (numero_version = 1
OR (
    motivo_cambio IS NOT NULL
    AND btrim(motivo_cambio) <> ''
));

CREATE INDEX ix_documento_creado_por_cuenta_id ON divorcios.documento (creado_por_cuenta_id);

CREATE INDEX ix_documento_estado_codigo ON divorcios.documento (estado_codigo);

ALTER TABLE divorcios.documento ADD CONSTRAINT ck_documento_creador CHECK (NOT (
    creado_por_cuenta_id IS NOT NULL
    AND creado_por_usuario_id IS NOT NULL
));

ALTER TABLE divorcios.documento ADD CONSTRAINT ck_documento_estado CHECK (estado_codigo IN (
    'VIGENTE',
    'REEMPLAZADO',
    'ANULADO'
));

CREATE INDEX ix_cuenta_ciudadana_estado_codigo ON divorcios.cuenta_ciudadana (estado_codigo);

ALTER TABLE divorcios.cuenta_ciudadana ADD CONSTRAINT ck_cuenta_ciudadana_bloqueo CHECK (estado_codigo <> 'BLOQUEADA'
OR bloqueado_hasta IS NOT NULL);

ALTER TABLE divorcios.cuenta_ciudadana ADD CONSTRAINT ck_cuenta_ciudadana_estado CHECK (estado_codigo IN (
    'ACTIVA',
    'BLOQUEADA',
    'INACTIVA'
));

ALTER TABLE divorcios.cuenta_ciudadana ADD CONSTRAINT ck_cuenta_ciudadana_hash CHECK (clave_hash IS NULL
OR btrim(clave_hash) <> '');

ALTER TABLE divorcios.cuenta_ciudadana ADD CONSTRAINT ck_cuenta_ciudadana_intentos CHECK (intentos_fallidos >= 0);

CREATE INDEX ix_consulta_reniec_dni_consultado_expira_en ON divorcios.consulta_reniec (dni_consultado, expira_en);

ALTER TABLE divorcios.consulta_reniec ADD CONSTRAINT ck_consulta_reniec_datos_encontrados CHECK (resultado_codigo <> 'ENCONTRADO'
OR (
    prenombres IS NOT NULL
    AND btrim(prenombres) <> ''
    AND apellido_paterno IS NOT NULL
    AND btrim(apellido_paterno) <> ''
    AND apellido_materno IS NOT NULL
    AND btrim(apellido_materno) <> ''
));

ALTER TABLE divorcios.consulta_reniec ADD CONSTRAINT ck_consulta_reniec_expiracion CHECK (expira_en IS NULL
OR expira_en >= consultado_en);

ALTER TABLE divorcios.consulta_reniec ADD CONSTRAINT ck_consulta_reniec_hash CHECK (respuesta_hash IS NULL
OR btrim(respuesta_hash) <> '');

ALTER TABLE divorcios.consulta_reniec ADD CONSTRAINT ck_consulta_reniec_http CHECK (codigo_http IS NULL
OR codigo_http BETWEEN 100 AND 599);

ALTER TABLE divorcios.consulta_reniec ADD CONSTRAINT ck_consulta_reniec_origen CHECK (origen_codigo IN (
    'API',
    'IMPORTACION'
));

ALTER TABLE divorcios.consulta_reniec ADD CONSTRAINT ck_consulta_reniec_resultado CHECK (resultado_codigo IN (
    'ENCONTRADO',
    'NO_ENCONTRADO',
    'ERROR'
));

ALTER TABLE divorcios.asistencia_audiencia ADD CONSTRAINT ck_asistencia_inasistencia CHECK (asistio IS TRUE
OR (
    ratifico_voluntad IS NULL
    AND identidad_verificada_en IS NULL
));

ALTER TABLE divorcios.asistencia_audiencia ADD CONSTRAINT ck_asistencia_ratificacion CHECK (ratifico_voluntad IS NULL
OR (
    asistio IS TRUE
    AND identidad_verificada_en IS NOT NULL
));

ALTER TABLE divorcios.asistencia_audiencia ADD CONSTRAINT ck_asistencia_verificacion CHECK (identidad_verificada_en IS NULL
OR asistio IS TRUE);

CREATE UNIQUE INDEX ix_actuacion_administrativa_documento_id ON divorcios.actuacion_administrativa (documento_id);

CREATE UNIQUE INDEX ix_actuacion_administrativa_expediente_id_tipo_codigo_numero_s ON divorcios.actuacion_administrativa (expediente_id, tipo_codigo, numero_secuencia);

CREATE INDEX ix_actuacion_administrativa_numero_resolucion ON divorcios.actuacion_administrativa (numero_resolucion);

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT ck_actuacion_emision CHECK ((
    estado_codigo = 'BORRADOR'
    AND fecha_emision IS NULL
    AND emitida_por_usuario_id IS NULL
    AND emitida_en IS NULL
)
OR
(
    estado_codigo = 'EMITIDA'
    AND fecha_emision IS NOT NULL
    AND emitida_por_usuario_id IS NOT NULL
    AND emitida_en IS NOT NULL
)
OR estado_codigo = 'ANULADA');

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT ck_actuacion_estado CHECK (estado_codigo IN (
    'BORRADOR',
    'EMITIDA',
    'ANULADA'
));

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT ck_actuacion_fecha_emision CHECK (emitida_en IS NULL
OR emitida_en >= creada_en);

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT ck_actuacion_fecha_notificacion CHECK (fecha_notificacion IS NULL
OR (
    fecha_emision IS NOT NULL
    AND fecha_notificacion >= fecha_emision
));

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT ck_actuacion_secuencia CHECK (numero_secuencia > 0);

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT ck_actuacion_tipo CHECK (tipo_codigo IN (
    'INFORME_ADMISIBILIDAD',
    'RESOLUCION_ADMISIBILIDAD',
    'INFORME_SEPARACION',
    'RESOLUCION_SEPARACION',
    'INFORME_DISOLUCION',
    'RESOLUCION_DISOLUCION'
));

CREATE UNIQUE INDEX ix_estado_expediente_codigo ON divorcios.estado_expediente (codigo);

CREATE UNIQUE INDEX ix_estado_expediente_etapa_codigo_orden_visual ON divorcios.estado_expediente (etapa_codigo, orden_visual);

CREATE UNIQUE INDEX ix_expediente_contacto_historial_expediente_conyuge_id_tipo_co ON divorcios.expediente_contacto_historial (expediente_conyuge_id, tipo_contacto_codigo) WHERE vigente_hasta IS NULL;

CREATE INDEX ix_expediente_contacto_historial_expediente_conyuge_id_tipo_co1 ON divorcios.expediente_contacto_historial (expediente_conyuge_id, tipo_contacto_codigo, vigente_desde);

CREATE INDEX ix_expediente_contacto_historial_preregistro_version_origen_id ON divorcios.expediente_contacto_historial (preregistro_version_origen_id);

CREATE INDEX ix_expediente_contacto_historial_registrado_por_usuario_id ON divorcios.expediente_contacto_historial (registrado_por_usuario_id);

CREATE UNIQUE INDEX ix_expediente_conyuge_expediente_id_persona_id ON divorcios.expediente_conyuge (expediente_id, persona_id);

CREATE UNIQUE INDEX ix_expediente_conyuge_expediente_id_posicion_codigo ON divorcios.expediente_conyuge (expediente_id, posicion_codigo);

CREATE INDEX ix_expediente_conyuge_persona_id ON divorcios.expediente_conyuge (persona_id);

CREATE UNIQUE INDEX ux_expediente_un_iniciador ON divorcios.expediente_conyuge (expediente_id) WHERE es_iniciador = TRUE;

CREATE UNIQUE INDEX ix_expediente_version_expediente_id_numero_version ON divorcios.expediente_version (expediente_id, numero_version);

CREATE INDEX ix_expediente_version_preregistro_version_origen_id ON divorcios.expediente_version (preregistro_version_origen_id);

CREATE INDEX ix_expediente_version_registrado_por_usuario_id ON divorcios.expediente_version (registrado_por_usuario_id);

CREATE INDEX ix_historial_estado_expediente_estado_expediente_id ON divorcios.historial_estado_expediente (estado_expediente_id);

CREATE UNIQUE INDEX ix_historial_estado_expediente_expediente_id ON divorcios.historial_estado_expediente (expediente_id) WHERE finalizado_en IS NULL;

CREATE INDEX ix_historial_estado_expediente_expediente_id_iniciado_en ON divorcios.historial_estado_expediente (expediente_id, iniciado_en);

CREATE UNIQUE INDEX ix_historial_estado_expediente_expediente_id_numero_secuencia ON divorcios.historial_estado_expediente (expediente_id, numero_secuencia);

CREATE INDEX ix_historial_estado_expediente_registrado_por_usuario_id ON divorcios.historial_estado_expediente (registrado_por_usuario_id);

CREATE UNIQUE INDEX ix_pago_documento_comprobante_id ON divorcios.pago (documento_comprobante_id);

CREATE INDEX ix_pago_estado_codigo_solicitado_en ON divorcios.pago (estado_codigo, solicitado_en);

CREATE INDEX ix_pago_expediente_conyuge_pagante_id ON divorcios.pago (expediente_conyuge_pagante_id);

CREATE UNIQUE INDEX ix_pago_expediente_id_numero_pago ON divorcios.pago (expediente_id, numero_pago);

CREATE UNIQUE INDEX ix_pago_numero_voucher ON divorcios.pago (numero_voucher) WHERE numero_voucher IS NOT NULL;

CREATE UNIQUE INDEX ix_pago_referencia_caja ON divorcios.pago (referencia_caja) WHERE referencia_caja IS NOT NULL;

CREATE INDEX ix_pago_registrado_por_usuario_id ON divorcios.pago (registrado_por_usuario_id);

CREATE INDEX ix_plazo_expediente_creado_por_usuario_id ON divorcios.plazo_expediente (creado_por_usuario_id);

CREATE INDEX ix_plazo_expediente_expediente_id_estado_codigo_fecha_vencimie ON divorcios.plazo_expediente (expediente_id, estado_codigo, fecha_vencimiento);

CREATE UNIQUE INDEX ix_plazo_expediente_expediente_id_regla_plazo_id_numero_aplica ON divorcios.plazo_expediente (expediente_id, regla_plazo_id, numero_aplicacion);

CREATE INDEX ix_plazo_expediente_historial_estado_expediente_origen_id ON divorcios.plazo_expediente (historial_estado_expediente_origen_id);

CREATE INDEX ix_plazo_expediente_regla_plazo_id ON divorcios.plazo_expediente (regla_plazo_id);

CREATE INDEX ix_preregistro_requisito_documento_documento_version_id ON divorcios.preregistro_requisito_documento (documento_version_id);

CREATE INDEX ix_preregistro_version_creado_por_cuenta_id ON divorcios.preregistro_version (creado_por_cuenta_id);

CREATE UNIQUE INDEX ix_preregistro_version_preregistro_id_numero_version ON divorcios.preregistro_version (preregistro_id, numero_version);

CREATE INDEX ix_revision_detalle_documento_documento_version_id ON divorcios.revision_detalle_documento (documento_version_id);

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT fk_actuacion_administrativa_documentos_documento_id FOREIGN KEY (documento_id) REFERENCES divorcios.documento (documento_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.actuacion_administrativa ADD CONSTRAINT fk_actuacion_administrativa_expedientes_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.asistencia_audiencia ADD CONSTRAINT fk_asistencia_audiencia_expedientes_conyuges_expediente_conyug FOREIGN KEY (expediente_conyuge_id) REFERENCES divorcios.expediente_conyuge (expediente_conyuge_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.audiencia_ratificacion ADD CONSTRAINT fk_audiencia_ratificacion_expedientes_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.documento ADD CONSTRAINT fk_documento_cuenta_ciudadana_creado_por_cuenta_id FOREIGN KEY (creado_por_cuenta_id) REFERENCES divorcios.cuenta_ciudadana (cuenta_ciudadana_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.documento ADD CONSTRAINT fk_documento_expedientes_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.documento ADD CONSTRAINT fk_documento_usuarios_internos_creado_por_usuario_id FOREIGN KEY (creado_por_usuario_id) REFERENCES divorcios.usuario_interno (usuario_interno_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.expediente ADD CONSTRAINT fk_expediente_cuenta_ciudadana_creado_por_cuenta_id FOREIGN KEY (creado_por_cuenta_id) REFERENCES divorcios.cuenta_ciudadana (cuenta_ciudadana_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.notificacion ADD CONSTRAINT fk_notificacion_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.preregistro ADD CONSTRAINT fk_preregistro_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.preregistro_requisito ADD CONSTRAINT fk_preregistro_requisito_preregistros_versiones_preregistro_ve FOREIGN KEY (preregistro_version_id) REFERENCES divorcios.preregistro_version (preregistro_version_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.registro_auditoria ADD CONSTRAINT fk_registro_auditoria_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.representacion ADD CONSTRAINT fk_representacion_expediente_conyuge_expediente_conyuge_id FOREIGN KEY (expediente_conyuge_id) REFERENCES divorcios.expediente_conyuge (expediente_conyuge_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.revision_preregistro ADD CONSTRAINT fk_revision_preregistro_preregistro_version_preregistro_versio FOREIGN KEY (preregistro_version_id) REFERENCES divorcios.preregistro_version (preregistro_version_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.solicitante_disolucion ADD CONSTRAINT fk_solicitante_disolucion_expediente_conyuge_expediente_conyug FOREIGN KEY (expediente_conyuge_id) REFERENCES divorcios.expediente_conyuge (expediente_conyuge_id) ON DELETE RESTRICT;

ALTER TABLE divorcios.solicitud_disolucion ADD CONSTRAINT fk_solicitud_disolucion_expediente_expediente_id FOREIGN KEY (expediente_id) REFERENCES divorcios.expediente (expediente_id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260930155844_CerrarModeloExpedientesYRespuestasVersionadas', '10.0.12');

COMMIT;

