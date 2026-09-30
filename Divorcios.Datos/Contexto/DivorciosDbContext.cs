using Divorcios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Divorcios.Datos.Contexto
{
    public class DivorciosDbContext : DbContext
    {
        public DivorciosDbContext(
            DbContextOptions<DivorciosDbContext> options)
            : base(options)
        {
        }

        public DbSet<Persona> Personas 
            => Set<Persona>();
        public DbSet<UsuarioInterno> UsuariosInternos
            => Set<UsuarioInterno>();
        public DbSet<CuentaCiudadana> CuentasCiudadanas
            => Set<CuentaCiudadana>();
        public DbSet<ValidacionIdentidad> ValidacionesIdentidad
            => Set<ValidacionIdentidad>();
        public DbSet<ExpedienteConyuge> ExpedientesConyuges
            => Set<ExpedienteConyuge>();
        public DbSet<TipoDocumento> TiposDocumento
            => Set<TipoDocumento>();
        public DbSet<RequisitoCatalogo> RequisitosCatalogo
            => Set<RequisitoCatalogo>();
        public DbSet<EstadoExpediente> EstadosExpediente
            => Set<EstadoExpediente>();
        public DbSet<ReglaPlazo> ReglasPlazo
            => Set<ReglaPlazo>();
        public DbSet<DestinoOficio> DestinosOficio
            => Set<DestinoOficio>();
        public DbSet<ConsultaReniec> ConsultasReniec
            => Set<ConsultaReniec>();
        public DbSet<Preregistro> Preregistros 
            => Set<Preregistro>();
        public DbSet<PreregistroRequisito> PreregistrosRequisitos
            => Set<PreregistroRequisito>();
        public DbSet<PreregistroRequisitoDocumento> PreregistrosRequisitosDocumentos
            => Set<PreregistroRequisitoDocumento>();
        public DbSet<Documento> Documentos 
            => Set<Documento>();
        public DbSet<DocumentoVersion> DocumentosVersiones
            => Set<DocumentoVersion>();
        public DbSet<RevisionPreregistro> RevisionesPrerregistro
            => Set<RevisionPreregistro>();
        public DbSet<RevisionDetalle> RevisionesDetalle
            => Set<RevisionDetalle>();
        public DbSet<RevisionDetalleDocumento> RevisionesDetalleDocumentos
            => Set<RevisionDetalleDocumento>();
        public DbSet<Representacion> Representaciones
            => Set<Representacion>();
        public DbSet<Expediente> Expedientes
            => Set<Expediente>();
        public DbSet<HistorialEstadoExpediente> HistorialEstadosExpediente
            => Set<HistorialEstadoExpediente>();
        public DbSet<PlazoExpediente> PlazosExpediente
            => Set<PlazoExpediente>();
        public DbSet<AudienciaRatificacion> AudienciasRatificacion
            => Set<AudienciaRatificacion>();
        public DbSet<AsistenciaAudiencia> AsistenciasAudiencia
            => Set<AsistenciaAudiencia>();
        public DbSet<SolicitudDisolucion> SolicitudesDisolucion
            => Set<SolicitudDisolucion>();
        public DbSet<SolicitanteDisolucion> SolicitantesDisolucion
            => Set<SolicitanteDisolucion>();
        public DbSet<ActuacionAdministrativa> ActuacionesAdministrativas
            => Set<ActuacionAdministrativa>();
        public DbSet<Oficio> Oficios
            => Set<Oficio>();
        public DbSet<Pago> Pagos
            => Set<Pago>();
        public DbSet<Notificacion> Notificaciones
            => Set<Notificacion>();
        public DbSet<IntentoNotificacion> IntentosNotificacion
            => Set<IntentoNotificacion>();
        public DbSet<DiaNoLaborable> DiasNoLaborables
            => Set<DiaNoLaborable>();
        public DbSet<RegistroAuditoria> RegistrosAuditoria
            => Set<RegistroAuditoria>();
        public DbSet<PreregistroVersion> PreregistrosVersiones
            => Set<PreregistroVersion>();
        public DbSet<ExpedienteVersion> ExpedientesVersiones
            => Set<ExpedienteVersion>();
        public DbSet<ExpedienteContactoHistorial> ExpedientesContactosHistorial
            => Set<ExpedienteContactoHistorial>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("divorcios");

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DivorciosDbContext).Assembly);
        }
    }
}
