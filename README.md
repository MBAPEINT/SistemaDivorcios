# Guía de creación de endpoints y CRUD — SistemaDivorcios

Fecha de revisión: 1 de octubre de 2026.

El contrato propuesto de endpoints del módulo de prerregistro se comparte como documento Word por separado. Distingue rutas existentes, operaciones por desarrollar y decisiones pendientes.

Esta guía sirve para que el equipo agregue operaciones siguiendo la arquitectura actual: **API → Negocio → Datos**, con entidades compartidas de **Dominio**. Se utiliza TipoDocumento como ejemplo porque sus consultas y su creación ya están en el proyecto.

**Estado comprobado:** la solución compila con 0 errores y 0 advertencias. Hay doce GET de catálogos y un POST para tipos de documento. PUT, PATCH y DELETE se explican como patrones para desarrollar después; esta guía no los implementa ni autoriza borrar registros.

La revisión de este documento incluyó lectura del código y compilación. No se ejecutaron altas, actualizaciones, eliminaciones, migraciones ni cargas de prueba. La autenticación, los permisos administrativos y la auditoría efectiva todavía requieren su implementación en los bloques correspondientes.

## 1. Cómo se distribuye el trabajo entre las capas

~~~mermaid
flowchart LR
    C["Cliente o Swagger"] --> A["API: controlador"]
    A --> N["Negocio: servicio"]
    N --> D["Datos: repositorio"]
    D --> E["EF: DbContext"]
    E --> P["PostgreSQL"]
~~~

La petición entra al controlador. Este llama a un servicio. El servicio valida las reglas de la operación y llama al repositorio. El repositorio utiliza EF para consultar o guardar. En la respuesta, el servicio transforma las entidades en DTOs y el controlador entrega el estado HTTP.

| Capa | Responsabilidad | Ejemplo actual |
| --- | --- | --- |
| Dominio | Define las entidades y sus relaciones. | TipoDocumento, Documento. |
| Datos | Configura tablas, restricciones y relaciones; realiza consultas y guardados; reconoce errores de PostgreSQL. | DivorciosDbContext, CatalogosRepositorio. |
| Negocio | Aplica reglas de la operación, coordina persistencia y transforma entidades en DTOs. | CatalogosServicio. |
| API | Recibe parámetros y JSON, llama a Negocio y produce respuestas HTTP. | CatalogosController. |

Las referencias actuales son:

~~~text
Divorcios.Api     → Divorcios.Negocio
Divorcios.Negocio → Divorcios.Datos y Divorcios.Dominio
Divorcios.Datos   → Divorcios.Dominio
Divorcios.Dominio → ningún otro proyecto de la solución
~~~

La API registra Negocio en el arranque. La extensión de Negocio delega en Datos el registro de repositorios y DbContext. La API referencia directamente sólo Negocio.

Los controladores utilizan interfaces de servicio. Los servicios utilizan interfaces de repositorio. EF y Npgsql se utilizan en Datos. Las reglas del trámite pertenecen a Negocio.

## 2. Archivos reales que sirven como referencia

Los enlaces apuntan a archivos del repositorio mediante rutas relativas a este README. Se pueden abrir desde GitHub y consultar en cualquier equipo que conserve la estructura de la solución.

| Para aprender o implementar | Archivo de referencia | Qué revisar |
| --- | --- | --- |
| Entidad | [Divorcios.Dominio/Entidades/TipoDocumento.cs](Divorcios.Dominio/Entidades/TipoDocumento.cs) | Campos persistentes, identificador y navegaciones. |
| Configuración de la tabla | [Divorcios.Datos/Configuraciones/TipoDocumentoConfiguracion.cs](Divorcios.Datos/Configuraciones/TipoDocumentoConfiguracion.cs) | Longitudes, índice único, identidad y CHECK del origen. |
| DbContext | [Divorcios.Datos/Contexto/DivorciosDbContext.cs](Divorcios.Datos/Contexto/DivorciosDbContext.cs) | DbSet TiposDocumento y configuración del modelo. |
| DTO de salida | [Divorcios.Negocio/DTOs/Catalogos/TipoDocumentoDto.cs](Divorcios.Negocio/DTOs/Catalogos/TipoDocumentoDto.cs) | Campos que se exponen en la respuesta. |
| DTO de entrada | [Divorcios.Negocio/DTOs/Catalogos/CrearTipoDocumentoDto.cs](Divorcios.Negocio/DTOs/Catalogos/CrearTipoDocumentoDto.cs) | Required, StringLength y RegularExpression. |
| Contrato de Datos | [Divorcios.Datos/Interfaces/ICatalogosRepositorio.cs](Divorcios.Datos/Interfaces/ICatalogosRepositorio.cs) | Firmas de consulta, existencia y creación. |
| Implementación de Datos | [Divorcios.Datos/Repositorios/CatalogosRepositorio.cs](Divorcios.Datos/Repositorios/CatalogosRepositorio.cs) | AsNoTracking, AnyAsync, Add y SaveChangesAsync. |
| Error de persistencia | [Divorcios.Datos/Excepciones/CodigoDuplicadoPersistenciaException.cs](Divorcios.Datos/Excepciones/CodigoDuplicadoPersistenciaException.cs) | Error técnico reconocido y causa interna conservada. |
| Contrato de Negocio | [Divorcios.Negocio/Interfaces/ICatalogosServicio.cs](Divorcios.Negocio/Interfaces/ICatalogosServicio.cs) | DTOs de entrada y salida de la operación. |
| Implementación de Negocio | [Divorcios.Negocio/Servicios/CatalogosServicio.cs](Divorcios.Negocio/Servicios/CatalogosServicio.cs) | Validación, duplicados, creación y Mapear. |
| Conflicto de Negocio | [Divorcios.Negocio/Excepciones/ConflictoNegocioException.cs](Divorcios.Negocio/Excepciones/ConflictoNegocioException.cs) | Mensaje funcional y excepción interna opcional. |
| Controlador | [Divorcios.Api/Controllers/CatalogosController.cs](Divorcios.Api/Controllers/CatalogosController.cs) | GET, POST, rutas, 201, 400, 404 y 409. |
| Registro de Datos | [Divorcios.Datos/Extensiones/InyeccionDependencias.cs](Divorcios.Datos/Extensiones/InyeccionDependencias.cs) | AddDbContext y AddScoped del repositorio. |
| Registro de Negocio | [Divorcios.Negocio/Extensiones/InyeccionDependencias.cs](Divorcios.Negocio/Extensiones/InyeccionDependencias.cs) | AddScoped del servicio. |
| Arranque y Swagger | [Divorcios.Api/Program.cs](Divorcios.Api/Program.cs) | AddControllers, MapControllers, OpenAPI y Swagger UI. |
| Perfiles de ejecución | [Divorcios.Api/Properties/launchSettings.json](Divorcios.Api/Properties/launchSettings.json) | Puertos y apertura de Swagger. |

Se conserva el estilo de namespaces con llaves que ya utiliza el proyecto.

## 3. Orden exacto para agregar una operación

### Paso 1. Definir lo que hará el endpoint

Antes del código, acordar:

- Recurso y operación: consultar, crear, editar, desactivar o eliminar.
- Verbo y ruta.
- Parámetros de ruta, filtros y cuerpo JSON.
- Datos que se devolverán y estados HTTP.
- Campos editables, reglas de negocio y permisos.
- Efectos en registros relacionados, versiones e historial.

Ejemplo confirmado: crear un tipo de documento con código, nombre y origen. La base genera el identificador y el servicio lo crea activo.

No deducir que una tabla necesita las cuatro operaciones sólo porque existe.

### Paso 2. Revisar el modelo existente

Revisar entidad, configuración y DbContext. Confirmar tipos, nulabilidad, longitudes, índices y relaciones.

Agregar una operación sobre campos existentes normalmente no cambia el esquema. DTOs, métodos y controladores no requieren una migración por sí mismos. Si se descubre una necesidad de cambiar columnas o relaciones, tratarla como un bloque separado de modelado y persistencia.

### Paso 3. Definir los DTOs en Negocio

Crear o reutilizar:

~~~text
Divorcios.Negocio/DTOs/<Modulo>/
    <Recurso>Dto.cs
    Crear<Recurso>Dto.cs
    Actualizar<Recurso>Dto.cs
~~~

El DTO de salida representa lo que el cliente puede conocer. Los DTOs de entrada representan lo que puede solicitar modificar.

No recibir una entidad completa de EF en el controlador. Eso permitiría enviar identificadores, navegaciones u otros campos que no forman parte del contrato.

Los DTOs de creación y actualización pueden tener reglas diferentes; se definen según cada operación.

### Paso 4. Declarar las operaciones en la interfaz de Datos

Agregar las firmas en la interfaz del repositorio del módulo. Datos trabaja con entidades o resultados de persistencia, sin depender de los DTOs de Negocio.

~~~csharp
Task<TipoDocumento?> ObtenerTipoDocumentoAsync(
    int id, CancellationToken cancellationToken);

Task<bool> ExisteCodigoTipoDocumentoAsync(
    string codigo, CancellationToken cancellationToken);

Task<TipoDocumento> CrearTipoDocumentoAsync(
    TipoDocumento tipoDocumento,
    CancellationToken cancellationToken);
~~~

El signo de interrogación en TipoDocumento? permite devolver null cuando la consulta no encuentra el registro.

### Paso 5. Implementar el repositorio en Datos

Implementar las firmas utilizando el DbContext existente:

- Lectura: consultas LINQ y métodos asíncronos de EF.
- Creación: Add y SaveChangesAsync.
- Edición: cargar una entidad con seguimiento, modificar campos autorizados y guardar.
- Eliminación física permitida: Remove y SaveChangesAsync.
- Desactivación: modificar Activo y guardar.

Datos reconoce las restricciones técnicas esperadas. Debe comprobar el código de error y la restricción concreta para traducirlas; otros fallos continúan hacia el manejo general.

### Paso 6. Declarar la operación en la interfaz de Negocio

La interfaz del servicio utiliza DTOs:

~~~csharp
Task<TipoDocumentoDto> CrearTipoDocumentoAsync(
    CrearTipoDocumentoDto datos,
    CancellationToken cancellationToken);
~~~

La API conocerá esta interfaz. No llamará directamente al repositorio.

### Paso 7. Implementar el servicio de Negocio

En el servicio:

1. Validar la entrada.
2. Comprobar permisos, pertenencia y estado permitido cuando la operación los requiera.
3. Consultar información necesaria para las reglas.
4. Construir o modificar la entidad mediante campos explícitos.
5. Pedir al repositorio que guarde.
6. Traducir los errores técnicos esperados a errores de Negocio.
7. Convertir el resultado en el DTO de salida.

Para operaciones con varios cambios relacionados, coordinar también su atomicidad y auditoría. La pertenencia de una FK al mismo expediente no queda garantizada únicamente porque ese identificador exista.

### Paso 8. Agregar la acción en API

La acción:

1. Define verbo y ruta.
2. Recibe parámetros y DTO.
3. Llama al servicio.
4. Devuelve el estado HTTP y el DTO.
5. Traduce los errores conocidos a ProblemDetails o ValidationProblemDetails.

Agregar EndpointSummary y ProducesResponseType para que Swagger describa el contrato. Esas anotaciones documentan; no implementan validaciones ni permisos.

### Paso 9. Revisar el registro de dependencias

Si se crean un servicio y un repositorio nuevos, registrar sus implementaciones:

~~~csharp
// En la extensión de Datos:
servicios.AddScoped<IRecursoRepositorio, RecursoRepositorio>();

// En la extensión de Negocio:
servicios.AddScoped<IRecursoServicio, RecursoServicio>();
~~~

Son nombres de plantilla para un módulo nuevo; no existen actualmente en el proyecto.

Si sólo se agregan métodos a CatalogosServicio y CatalogosRepositorio, los registros existentes ya sirven. Las firmas nuevas sí deben agregarse a sus interfaces.

### Paso 10. Compilar y verificar el contrato

Compilar, revisar Swagger y comprobar los casos relevantes. No ejecutar escrituras en la base de desarrollo hasta que se acuerde una carga o prueba concreta.

## 4. GET: consultar datos

### 4.1. Listado existente

~~~http
GET /api/catalogos/tipos-documento
GET /api/catalogos/tipos-documento?activo=true
GET /api/catalogos/tipos-documento?activo=false
~~~

Orden de trabajo:

1. Reutilizar TipoDocumentoDto.
2. Agregar ListarTiposDocumentoAsync a la interfaz de Datos.
3. Implementar filtros y orden en Datos.
4. Agregar el método equivalente a la interfaz del servicio.
5. En Negocio, convertir cada entidad en DTO.
6. En API, recibir filtros y devolver Ok.
7. Comprobar lista vacía y filtros.

Consulta de referencia en Datos:

~~~csharp
return await contexto.TiposDocumento.AsNoTracking()
    .Where(x => !activo.HasValue || x.Activo == activo.Value)
    .OrderBy(x => x.Codigo)
    .ThenBy(x => x.TipoDocumentoId)
    .ToListAsync(cancellationToken);
~~~

AsNoTracking es apropiado para la lectura. El filtro nullable significa: sin filtro devuelve todos; true o false selecciona ese indicador. Activo no equivale automáticamente a vigencia o aplicabilidad.

Una lista sin resultados devuelve 200 con []. Para consultas que puedan crecer mucho, definir paginación y orden estable antes de cargar todos los registros.

### 4.2. Detalle existente

~~~http
GET /api/catalogos/tipos-documento/{id}
~~~

El repositorio devuelve una entidad o null. El servicio transforma la entidad en DTO. El controlador devuelve 200 o 404.

La acción actual valida que el identificador esté entre 1 y short.MaxValue. La ruta usa la restricción int: una URL que no satisface esa restricción no coincide y devuelve 404. Un número entero que coincide con la ruta pero falla Range produce 400.

**GET también valida entradas.** Valida identificadores y filtros. El DTO de salida no lleva Required para validar una petición porque se utiliza para la respuesta.

Guía de código: métodos ListarTiposDocumento y ObtenerTipoDocumento del controlador, y sus equivalentes Async en servicio y repositorio.

## 5. POST: crear un registro

Ruta existente:

~~~http
POST /api/catalogos/tipos-documento
~~~

Orden de archivos:

1. CrearTipoDocumentoDto.
2. ICatalogosRepositorio: ExisteCodigoTipoDocumentoAsync y CrearTipoDocumentoAsync.
3. CatalogosRepositorio: consulta de existencia y guardado.
4. CodigoDuplicadoPersistenciaException en Datos.
5. ICatalogosServicio: firma con DTO de entrada y salida.
6. CatalogosServicio: validación, normalización, duplicado, creación y mapeo.
7. ConflictoNegocioException en Negocio.
8. CatalogosController: acción POST y respuestas HTTP.

El DTO actual acepta Codigo, Nombre y OrigenCodigo. Las longitudes son 40, 160 y 20. El origen admite CIUDADANO, MUNICIPALIDAD o AMBOS porque esas opciones ya están en el CHECK del modelo. TipoDocumentoId lo genera PostgreSQL y el servicio establece Activo = true.

El servicio verifica las anotaciones mediante Validator.ValidateObject. [ApiController] también valida la petición antes de ejecutar la acción. Se conserva la validación en Negocio para otros posibles consumidores del servicio. [Referencia oficial de validación de controladores](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0#automatic-http-400-responses).

El servicio elimina espacios exteriores del código y del nombre. No se ha definido conversión automática de códigos a mayúsculas ni una equivalencia entre códigos con distinta capitalización.

Add marca la entidad como nueva. SaveChangesAsync realiza el guardado. Si termina correctamente, EF incorpora el identificador generado.

La respuesta actual utiliza:

~~~csharp
return CreatedAtAction(
    nameof(ObtenerTipoDocumento),
    new { id = creado.TipoDocumentoId },
    creado);
~~~

Devuelve el DTO, estado 201 y cabecera Location para consultar el registro mediante el GET existente.

La consulta previa de existencia mejora el mensaje, pero el índice único sigue siendo la garantía ante concurrencia. El repositorio reconoce UniqueViolation sólo para ix_tipo_documento_codigo, retira del seguimiento la inserción fallida y lanza su excepción de persistencia. El servicio la transforma en un conflicto; la API devuelve 409.

Este POST no crea automáticamente requisitos oficiales, formatos ni documentos del trámite.

## 6. Edición: PUT y PATCH

**Estado: patrones pendientes de implementación.** Los archivos y métodos nuevos de esta sección son propuestas de contrato, no operaciones disponibles.

Editar exige definir primero qué campos se pueden modificar, qué actor puede hacerlo y qué consecuencias tiene para los registros que ya usan ese catálogo. El tipo de documento es referenciado por Documento; cambiar su significado requiere revisar el efecto histórico.

PUT representa el envío de la representación completa de los campos editables acordados. PATCH expresa un cambio parcial mediante un contrato definido. Un campo omitido no debe confundirse automáticamente con false o null. [Semántica de PUT](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.4), [semántica de PATCH](https://www.rfc-editor.org/rfc/rfc5789.html).

### 6.1. Pasos para una actualización con PUT

Ejemplo de ruta futura:

~~~http
PUT /api/catalogos/tipos-documento/{id}
~~~

1. En Negocio/DTOs/Catalogos, definir ActualizarTipoDocumentoDto con los campos realmente editables.
2. En Datos/Interfaces/ICatalogosRepositorio, declarar la carga para edición y el guardado.
3. En Datos/Repositorios/CatalogosRepositorio, implementar la carga con seguimiento.
4. En Negocio/Interfaces/ICatalogosServicio, declarar ActualizarTipoDocumentoAsync.
5. En Negocio/Servicios/CatalogosServicio, comprobar existencia y reglas, asignar campos explícitos, guardar y mapear.
6. En API/Controllers/CatalogosController, agregar HttpPut y devolver 200 con DTO o 404 si no existe.
7. Traducir validación a 400 y conflictos a 409.
8. Comprobar que los campos no editables y las relaciones históricas permanecen intactos.

Firmas orientativas de Datos:

~~~csharp
Task<TipoDocumento?> ObtenerTipoDocumentoParaEdicionAsync(
    int id, CancellationToken cancellationToken);

Task GuardarCambiosAsync(
    CancellationToken cancellationToken);
~~~

Carga orientativa del repositorio:

~~~csharp
public Task<TipoDocumento?> ObtenerTipoDocumentoParaEdicionAsync(
    int id, CancellationToken cancellationToken)
{
    return contexto.TiposDocumento.AsTracking()
        .SingleOrDefaultAsync(
            x => x.TipoDocumentoId == id,
            cancellationToken);
}
~~~

GuardarCambiosAsync utilizaría SaveChangesAsync sobre ese mismo contexto.

Firma orientativa del servicio:

~~~csharp
Task<TipoDocumentoDto?> ActualizarTipoDocumentoAsync(
    int id,
    ActualizarTipoDocumentoDto datos,
    CancellationToken cancellationToken);
~~~

El retorno nullable permite comunicar ausencia al controlador. ActualizarTipoDocumentoDto todavía debe diseñarse; no existe actualmente.

Algoritmo del servicio:

~~~text
Validar DTO
    → cargar entidad para edición
    → si no existe, devolver null
    → validar permiso y cambios admitidos
    → asignar sólo campos autorizados
    → guardar
    → devolver DTO
~~~

No construir una entidad completa desde un JSON y marcar todas sus propiedades como modificadas. Ese procedimiento podría sobrescribir identificadores, estados o valores que el cliente no debería controlar.

Si se permitirá editar el código, la comprobación de duplicados debe excluir el identificador actual. Reutilizar sin cambios el método ExisteCodigoTipoDocumentoAsync marcaría como duplicado el código del propio registro.

La política de campos editables y el control de concurrencia de edición siguen pendientes. No introducir un token de concurrencia o una nueva columna sin revisar su necesidad.

### 6.2. Ejemplo de cambio parcial: activar o desactivar

Para catálogos reutilizados, una operación específica sobre Activo puede conservar el registro y sus referencias. Debe acordarse su significado antes de implementarla.

Contrato educativo:

~~~http
PATCH /api/catalogos/tipos-documento/{id}/activo
~~~

Posible DTO futuro:

~~~csharp
using System.ComponentModel.DataAnnotations;

namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed class CambiarActivoTipoDocumentoDto
    {
        [Required(ErrorMessage = "Debe indicar el valor de activo.")]
        public bool? Activo { get; set; }
    }
}
~~~

Se utiliza bool? para distinguir un valor omitido de false. Required acepta false y rechaza null. Tras validar, el servicio puede utilizar Activo.Value.

Este ejemplo describe un cuerpo JSON específico con un único campo. El formato y el contrato de PATCH se documentan expresamente; no requiere asumir que exista una implementación de JSON Patch.

El servicio cargaría el catálogo, validaría la operación, modificaría Activo y guardaría. Desactivar no borra documentos ni asociaciones históricas. La política de consultas debe aclarar cómo se muestran los inactivos; nuestros GET actuales ya permiten consultarlos.

## 7. DELETE: eliminar cuando el recurso lo permita

**Estado: patrón educativo, pendiente de implementación y de política de eliminación.** No ejecutar este ejemplo contra la base actual.

Ejemplo de ruta física futura:

~~~http
DELETE /api/catalogos/tipos-documento/{id}
~~~

### 7.1. Decidir eliminación física o desactivación

Eliminar físicamente retira la fila. Desactivar conserva la fila y cambia su disponibilidad para operaciones nuevas.

En este proyecto:

- Documento referencia a TipoDocumento con DeleteBehavior.Restrict.
- Un tipo utilizado no puede borrarse físicamente mientras conserve esa referencia.
- Las versiones, evaluaciones, auditoría e históricos se conservan; no se les aplica un CRUD de borrado libre.
- Una corrección documental crea nuevas versiones y mantiene los vínculos anteriores.

La regla para catálogos sin uso debe acordarse. La existencia de un método DELETE no autoriza borrar registros relacionados ni cambiar Restrict por Cascade.

Guía de la relación real: [Divorcios.Datos/Configuraciones/DocumentoConfiguracion.cs](Divorcios.Datos/Configuraciones/DocumentoConfiguracion.cs).

### 7.2. Orden de trabajo para una eliminación permitida

1. Definir en el contrato qué se elimina y quién tiene permiso.
2. Declarar en la interfaz de Datos la consulta de uso y la eliminación.
3. En Datos, consultar referencias y preparar Remove.
4. Declarar la operación en la interfaz de Negocio.
5. En Negocio, cargar el registro, comprobar permiso y reglas, y solicitar el borrado.
6. En API, agregar HttpDelete y convertir el resultado en 204, 404 o 409.
7. Reconocer en Datos las restricciones de referencia esperadas si aparecen al guardar.
8. Verificar que los registros relacionados no se alteren.

Consulta de referencia para este ejemplo:

~~~csharp
return contexto.Documentos.AnyAsync(
    x => x.TipoDocumentoId == id,
    cancellationToken);
~~~

Fragmento del repositorio, una vez que la eliminación haya sido admitida por Negocio:

~~~csharp
contexto.TiposDocumento.Remove(tipoDocumento);
await contexto.SaveChangesAsync(cancellationToken);
~~~

Remove prepara el borrado; SaveChangesAsync lo ejecuta. Estos fragmentos no constituyen un endpoint terminado.

Algoritmo de Negocio:

~~~text
Cargar registro
    → si no existe, comunicar ausencia
    → comprobar actor y política de eliminación
    → comprobar referencias
    → si el borrado no está permitido, lanzar conflicto
    → solicitar eliminación
~~~

Una referencia puede aparecer después de la consulta previa. La FK de PostgreSQL sigue siendo la garantía final. Reconocer la violación de la FK esperada, traducirla a un error de persistencia y después a un conflicto de Negocio. La excepción actual de código duplicado sólo cubre el índice único; no cubre errores de FK.

Contrato orientativo de respuesta: 204 sin cuerpo al eliminar, 404 al no encontrar y 409 cuando la regla o una referencia impiden hacerlo. Una repetición de DELETE puede recibir otro estado manteniendo el mismo efecto final; documentar el comportamiento elegido. [Referencia de DELETE](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.5).

## 8. Validaciones y excepciones: dónde poner cada cosa

| Comprobación | Ubicación | Ejemplo |
| --- | --- | --- |
| Campo obligatorio o longitud | DTO de entrada en Negocio. | Required y StringLength. |
| Valor permitido del cuerpo | DTO; coherente con el modelo. | OrigenCodigo actual. |
| Forma y rango del identificador | Parámetro de API. | Restricción int y Range. |
| Regla de la operación | Servicio de Negocio. | Código duplicado, estado permitido, representación válida. |
| Pertenencia y autorización | Servicio/caso de uso, con actor autenticado; la API aplica además políticas cuando existan. | Documento perteneciente al expediente y permiso para consultarlo. |
| Índice único, FK o CHECK | Configuración y PostgreSQL; Datos reconoce errores esperados. | Código único y referencia documental. |
| Respuesta HTTP | API. | 400, 404 o 409. |

**Required no sustituye una regla de negocio.** Una cadena con longitud correcta puede no ser un número municipal válido. Una FK existente puede pertenecer a otro expediente.

### 8.1. Dos carpetas de excepciones con responsabilidades diferentes

~~~mermaid
flowchart LR
    P["Error reconocido de PostgreSQL"] --> D["Excepción de Datos"]
    D --> N["Excepción de Negocio"]
    N --> A["ProblemDetails en API"]
~~~

- Datos/Excepciones: describe un problema de persistencia reconocido.
- Negocio/Excepciones: describe por qué la operación solicitada no puede completarse.
- API: convierte ese resultado en una respuesta para el cliente.

Negocio ya depende de Datos. Referenciar Negocio desde Datos produciría una dependencia circular. Por eso el repositorio no lanza directamente ConflictoNegocioException.

No hace falta crear una clase distinta para cada frase de error. Usar tipos cuando el consumidor necesite distinguir tratamientos, como un conflicto esperado frente a un fallo técnico inesperado.

### 8.2. Ejemplo existente de traducción

En Datos, el catch combina UniqueViolation con el nombre ix_tipo_documento_codigo. No transforma todas las DbUpdateException en duplicados. SqlState y ConstraintName son propiedades de PostgresException. [Referencia Npgsql](https://www.npgsql.org/doc/api/Npgsql.PostgresException.html).

En Negocio, CodigoDuplicadoPersistenciaException se transforma en ConflictoNegocioException, conservando la causa interna.

En API, el POST captura ConflictoNegocioException y devuelve 409. Captura ValidationException para devolver ValidationProblemDetails.

Los errores inesperados continúan al manejo general configurado mediante AddProblemDetails y UseExceptionHandler. La API no devuelve SQL, credenciales, stack traces ni Exception.ToString como mensaje de negocio.

La ausencia en un GET se maneja actualmente con null y 404. Una lista vacía se maneja con [] y 200; no necesita una excepción.

## 9. Registro de dependencias y funciones de los archivos

Registro actual de Datos:

~~~csharp
servicios.AddScoped<ICatalogosRepositorio, CatalogosRepositorio>();
~~~

Registro actual de Negocio:

~~~csharp
servicios.AddScoped<ICatalogosServicio, CatalogosServicio>();
~~~

AddScoped registra una relación entre interfaz e implementación para el ámbito de la petición. La misma instancia scoped se reutiliza cuando se solicita de nuevo dentro de ese ámbito.

**Se registra la clase, no cada método.** Agregar un GET, POST, PUT o DELETE a estas clases no requiere repetir AddScoped. Sus métodos sí deben existir en ambas interfaces e implementaciones.

Los DTOs y excepciones que se crean con new no necesitan registrarse en DI. Los controladores se descubren con AddControllers y se publican con MapControllers.

Para un módulo nuevo, crear sus clases e interfaces y agregar sus registros a las extensiones de la capa correspondiente. No es necesario agregar una referencia de API a Datos para usar ese módulo.

## 10. Guardado, transacciones, historial y concurrencia

La creación actual de TipoDocumento usa un único SaveChangesAsync en su repositorio. Los cambios de una llamada a SaveChanges se ejecutan en una transacción cuando el proveedor la soporta. [Transacciones en EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions).

Una operación del trámite puede necesitar guardar expediente, participantes, versión e historial juntos. No asumir atomicidad entre varios SaveChanges independientes. Negocio debe coordinar la operación mediante una abstracción de persistencia/transacción de Datos. Esa coordinación general todavía no está implementada; se diseñará con el caso de uso concreto.

Las restricciones de BD protegen invariantes, pero una consulta previa no elimina las condiciones de concurrencia. El POST de tipos de documento ya traduce el rechazo del índice único. Edición, secuencias y estados requieren su estrategia correspondiente.

No reutilizar la actualización genérica para sobrescribir una encuesta enviada, una versión documental o una evaluación cerrada. Conservar las evidencias y agregar las versiones/correcciones que correspondan.

Un método de CRUD no debe aprobar un expediente, ratificar una audiencia, registrar un pago efectuado o cerrar una revisión por modificar libremente una entidad. Esas son operaciones del flujo con reglas propias.

## 11. Respuestas HTTP y Swagger

| Resultado | Estado habitual | Uso en esta guía |
| --- | --- | --- |
| Consulta o actualización con DTO | 200 | GET existentes; actualización propuesta. |
| Registro creado | 201 | POST existente, con Location. |
| Operación completada sin cuerpo | 204 | Eliminación propuesta. |
| Entrada inválida | 400 | DTOs, identificadores y filtros. |
| Falta de autenticación | 401 | Aplicable cuando se implemente autenticación. |
| Falta de permiso | 403 | Aplicable cuando se definan políticas. |
| Recurso ausente | 404 | GET existente; edición/eliminación propuestas. |
| Conflicto esperado | 409 | Duplicado actual; conflictos de edición/eliminación futuros. |
| Fallo inesperado | 500 | Manejo general. |

No convertir todos los errores a 400 o 409. Identificar su causa y mantener un formato consistente.

Documentar 200/201 con application/json. Documentar ProblemDetails y ValidationProblemDetails con application/problem+json. Evitar una anotación global que fuerce application/json sobre las respuestas ProblemDetails.

Swagger actual:

~~~text
Perfil http:  http://localhost:5151/swagger
Perfil https: https://localhost:7243/swagger
Documento:    /openapi/v1.json
~~~

OpenAPI y Swagger sólo se publican en Development. Swagger describe y permite invocar los endpoints; no añade reglas, autenticación ni auditoría por sí mismo.

## 12. Comprobaciones antes de entregar un endpoint

1. Entender la operación y sus efectos.
2. Comprobar DTO de entrada y salida.
3. Confirmar los métodos de ambas interfaces y sus implementaciones.
4. Revisar que el controlador llame a Negocio y el servicio a Datos.
5. Confirmar validaciones, ausencia y errores esperados.
6. Revisar restricciones y concurrencia.
7. Confirmar registros DI si hay clases nuevas.
8. Compilar sin errores ni advertencias.
9. Comprobar ruta, verbos y formatos en OpenAPI/Swagger.
10. Ejecutar las pruebas autorizadas y documentar sus límites.

Comandos de compilación desde la carpeta de la solución:

~~~powershell
dotnet build SistemaDivorcios.slnx
dotnet run --project Divorcios.Api --launch-profile http
~~~

Casos que se deben cubrir cuando se autorice la preparación de datos:

| Operación | Casos |
| --- | --- |
| GET | Lista vacía/poblada; detalle existente/ausente; filtros; identificador inválido; acceso permitido. |
| POST | Entrada inválida; creación; identificador y Location; código duplicado; duplicado concurrente. |
| PUT/PATCH | Ausencia; validación; campos permitidos; histórico intacto; permisos; concurrencia. |
| DELETE/desactivación | Ausencia; operación permitida; referencias; efecto histórico; repetición y concurrencia. |

La autorización vigente no incluye cargar datos de prueba. Esta guía no la cambia. Las simulaciones en memoria no equivalen a probar el guardado real, las FK o la concurrencia real de PostgreSQL.

## 13. Lista rápida para repartir el trabajo del grupo

Para cada operación, dejar escrito:

~~~text
Operación y ruta:
Actor y permisos:
DTO de entrada:
DTO de salida:
Métodos nuevos en la interfaz de Datos:
Implementación de consultas/guardado:
Métodos nuevos en la interfaz de Negocio:
Reglas del servicio:
Excepciones de persistencia y de Negocio:
Acción del controlador:
Respuestas HTTP:
Registro DI, si hay clases nuevas:
Pruebas realizadas y pendientes:
Efecto en historial, documentos y referencias:
~~~

Trabajar por operaciones completas. El responsable debe revisar los archivos compartidos del módulo para no duplicar métodos, rutas o registros. Mantener los cambios del modelo y las migraciones como decisiones explícitas separadas del CRUD.

Esta guía permite continuar el código; no determina requisitos oficiales, tarifas, plazos, integraciones ni políticas funcionales que aún están pendientes.
