# Validación de PostgreSQL y migraciones

Fecha: 30 de septiembre de 2026. Este informe documenta la etapa de validación aislada anterior a la autorización de aplicación local. En esa etapa la migración nueva no se aplicó a desarrollo. Estado posterior: la cadena completa ya está aplicada en la nueva base local verificada; véase APLICACION_DESARROLLO_LOCAL.md. La evidencia y los hashes de este informe corresponden a la revisión previa.

## Base local y límite de la auditoría

El usuario aclaró que la base anterior estaba en otra PC y autorizó crear el contenedor en esta. Antes de crearlo, los contextos locales de Docker no tenían contenedores, imágenes ni volúmenes, ni había PostgreSQL escuchando en 5433.

Se creó `divorcios_postgres` usando el Compose existente. Base `divorcios_db`, PostgreSQL 17.11, puerto 5433, volumen nuevo `sistemadivorcios_divorcios_postgres_data`. No se recreó ni auditó la base de la otra PC, cuyo contenido sigue desconocido.

Se ejecutó `AuditarBaseSoloLectura.sql` en una transacción `REPEATABLE READ READ ONLY`. Antes y después de las pruebas aisladas se obtuvieron resultados idénticos:

| Comprobación | Resultado |
| --- | --- |
| transaction_read_only | on |
| Historial __EFMigrationsHistory | No existe |
| Migraciones aplicadas en la base local nueva | Ninguna |
| Tablas de aplicación en cualquier esquema de usuario | Ninguna |
| Registros de aplicación o catálogo | No hay tablas donde almacenarlos |

No confundir ausencia de una tabla con un conteo de cero sobre una tabla existente. En particular, los catálogos del modelo tampoco existen todavía:

| Tabla de catálogo | Estado local |
| --- | --- |
| estado_expediente | No existe |
| tipo_documento | No existe |
| requisito_catalogo | No existe |
| regla_plazo | No existe |
| destino_oficio | No existe |
| dia_no_laborable | No existe |

La auditoría evita consultar contenido personal: sólo metadatos, identificadores de migraciones y cantidades exactas. Estos resultados no permiten deducir las migraciones ni registros de la otra PC.

## Aislamiento de las pruebas

Se usó un contenedor independiente `divorcios-validacion-20260930`, con la misma imagen PostgreSQL 17 Alpine que desarrollo:

- Red `none`, sin puertos publicados.
- Datos en `tmpfs` en `/var/lib/postgresql/data`.
- Sin bind mounts ni volúmenes Docker, y sin acceso al volumen de desarrollo.
- Bases separadas para cadena completa, modelo de referencia y cada prueba de rechazo.

Se retiró únicamente este contenedor después de guardar los resultados. No se vaciaron tablas ni se borraron registros para permitir una migración. Las filas sintéticas de las pruebas permanecieron presentes después de cada rechazo, hasta finalizar el contenedor temporal.

## Cadena completa y comparación con el modelo

Se generó el SQL de todas las migraciones desde 0 hasta `20260930155844_CerrarModeloExpedientesYRespuestasVersionadas` y se ejecutó con `psql -v ON_ERROR_STOP=1`.

La primera ejecución reveló tres diferencias frente al modelo: defaults `0` o cadena vacía introducidos por EF en `actuacion_administrativa.numero_secuencia`, `expediente.codigo_preregistro` y `regla_plazo.evento_inicio_codigo`. Se retiraron de la migración, se compiló con 0 errores y 0 advertencias y se regeneraron los SQL.

La validación final se repitió desde cero en otra base temporal nueva, sin reutilizar ni vaciar la primera. Las **18 migraciones** se ejecutaron correctamente y quedaron registradas en su orden esperado.

Para comprobar el esquema, se creó otra base temporal independiente mediante el SQL generado por `Database.GenerateCreateScript()` del modelo actual. Se compararon catálogos PostgreSQL de ambas bases, ordenados por nombre y sin depender de OID o del orden físico de columnas:

| Elemento | Cadena completa | Modelo de referencia | Diferencias finales |
| --- | ---: | ---: | ---: |
| Tablas | 37 | 37 | 0 |
| Columnas | 358 | 358 | 0 |
| Restricciones | 278 | 278 | 0 |
| Índices | 159 | 159 | 0 |

La comparación incluye tipos y precisión, nulabilidad, identidad, defaults, definiciones de PK/UNIQUE/FK/CHECK, validación y diferibilidad de restricciones, índices únicos y filtros. Las restricciones incluyen 165 CHECK y 76 FK. El esquema generado por la cadena coincide exactamente con esa referencia del modelo.

## Bloqueo ante datos existentes

Cada prueba Up partió de una base temporal diferente con las 17 migraciones históricas aplicadas. Se insertó un único registro sintético y se intentó la migración nueva:

| Prueba | Registro presente | Resultado | Verificación posterior |
| --- | --- | --- | --- |
| Up: catálogo | requisito_catalogo: 1 | Rechazada por el control previo | Fila conservada; mismas 17 migraciones, esquema y conteos. |
| Up: registro operativo | persona: 1 | Rechazada por el control previo | Fila conservada; mismas 17 migraciones, esquema y conteos. |
| Down: catálogo en modelo final | requisito_catalogo: 1 | Rechazada por el control previo | Fila conservada; mismas 18 migraciones y esquema final. |

Los errores recibidos identificaron la tabla poblada y el mensaje `Migracion de revision detenida`. Se verificaron códigos de salida fallidos, historial de migraciones intacto y catálogos de estructura iguales antes/después. En las pruebas Up también coincidieron todos los conteos antes/después.

Esto demuestra que ni el DDL ni el registro de la migración se ejecutaron después del rechazo. El historial de migraciones existente por sí solo no bloqueó la cadena vacía: queda excluido intencionadamente del control.

## Archivos para revisión y decisión siguiente

- `20260930155844_CierreModelo_Revision.sql`: SQL incremental actualizado tras retirar los tres defaults.
- `AuditarBaseSoloLectura.sql`: auditoría reutilizable sin cambios de estructura o datos.
- `Resultados_Validacion_PostgreSQL.json`: resultados, migraciones verificadas, hashes del esquema/SQL, aislamiento y mensajes de rechazo.
- `LEEME_CierreModelo.md`: alcance y restricciones del borrador.

En el momento de esta auditoría la base local era nueva y no requería borrar ni reiniciar nada. Se comprobó que necesitaba la cadena completa: el SQL incremental no bastaba para una base sin esquema previo. Esa inicialización se realizó posteriormente con autorización expresa, conservando datos, volumen y cadena histórica; véase APLICACION_DESARROLLO_LOCAL.md.

Si se desea conservar o trasladar la base de la otra PC, antes debe auditarse esa instancia y definirse una conversión/importación que preserve sus datos. La prueba con un esquema vacío no autoriza usar el borrador sobre registros antiguos ni quitar el bloqueo.

La validación cubre la estructura y el bloqueo en PostgreSQL 17.11 aislado. No implementa servicios de Negocio ni certifica contratos externos, autorización, auditoría operativa o funcionamiento sobre una base antigua con datos.
