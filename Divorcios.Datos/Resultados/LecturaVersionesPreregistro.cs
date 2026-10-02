using Divorcios.Dominio.Entidades;

namespace Divorcios.Datos.Resultados
{
    public sealed record LecturaVersionesPreregistro(CabeceraPreregistro Cabecera,
        IReadOnlyList<PreregistroVersion> Versiones, IReadOnlyList<ExpedienteContactoHistorial> Contactos,
        IReadOnlyList<RegistroAuditoria> Eventos);
}
