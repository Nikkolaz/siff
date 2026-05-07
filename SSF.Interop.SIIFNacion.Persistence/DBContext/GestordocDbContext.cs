using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Domain.Auditoria;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Domain.Siif;

namespace SSF.Interop.SIIFNacion.Persistence.DBContext
{
    public class GestordocDbContext : DbContext
    {
        public GestordocDbContext(DbContextOptions<GestordocDbContext> options) : base(options)
        {
        }

        public DbSet<CdpExpedido> CdpExpedidos { get; set; } = default!;
        public DbSet<CdpItem> CdpItems { get; set; } = default!;
        // Obligaciones sincronizadas desde SIIF (tabla GESTORDOC.dbo.DYNTBLSIIFOBLIGAPO).
        public DbSet<SiifObligacionApo> SiifObligacionesApo { get; set; } = default!;
        // Auditoría de integraciones (tabla GESTORDOC.dbo.DYNEFWAUDITORIA).
        public DbSet<DynEfwAuditoria> AuditoriaIntegracion { get; set; } = default!;
        // Ejecución presupuestal agregada (tabla GESTORDOC.dbo.DYNTBLEJECUCIONAGR).
        public DbSet<SiifEjecucionAgregada> EjecucionesAgregadas { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CdpExpedido>(entity =>
            {
                entity.ToTable("DYNTBLCDPDETALLE", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32);
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.IdCdpDetalle).HasColumnName("IDCDPDETALLE").HasColumnType("numeric(10,0)");
                entity.Property(e => e.NnSolicitudCdp).HasColumnName("NNSOLICITUDCDP").HasColumnType("numeric(10,0)");
                entity.Property(e => e.Consecutivo).HasColumnName("CONSECUTIVO").HasColumnType("numeric(10,0)");
                entity.Property(e => e.FechaRegistrada).HasColumnName("FECHAREGISTRADA");
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(2040);
                entity.Property(e => e.CodAutoBys).HasColumnName("CODAUTOBYS").HasColumnType("numeric(10,0)");
                entity.Property(e => e.TipoSolCdp).HasColumnName("TIPOSOLCDP").HasMaxLength(2040);
                entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION");
                entity.Property(e => e.VlSaldoCerNc).HasColumnName("VLSALDOCERNC").HasColumnType("decimal(28,12)");
                entity.Property(e => e.VlTotal).HasColumnName("VLTOTAL").HasColumnType("decimal(28,12)");
                entity.Property(e => e.FechaDeCarga).HasColumnName("FECHADECARGA");

                entity.Ignore(e => e.Items);
            });

            modelBuilder.Entity<CdpItem>(entity =>
            {
                entity.ToTable("DYNTBLITEMCDP", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32);
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.IdItemCdp).HasColumnName("IDITEMCDP").HasColumnType("numeric(10,0)");
                entity.Property(e => e.NnSolicitudCdp).HasColumnName("NNSOLICITUDCDP").HasColumnType("numeric(10,0)");
                entity.Property(e => e.CodPosicionGasto).HasColumnName("CODPOSICIONGAST").HasMaxLength(400);
                entity.Property(e => e.NmPosicionGasto).HasColumnName("NMPOSICIONGASTO").HasMaxLength(2040);
                entity.Property(e => e.CodFuenteFinan).HasColumnName("CODFUENTEFINAN").HasMaxLength(400);
                entity.Property(e => e.NmFuenteFinan).HasColumnName("NMFUENTEFINAN").HasMaxLength(2040);
                entity.Property(e => e.CodRecurPptal).HasColumnName("CODRECURPPTAL").HasMaxLength(400);
                entity.Property(e => e.NmRecurPptal).HasColumnName("NMRECURPPTAL").HasMaxLength(2040);
                entity.Property(e => e.CodSituacionFon).HasColumnName("CODSITUACIONFON").HasMaxLength(400);
                entity.Property(e => e.NmSituacionFon).HasColumnName("NMSITUACIONFON").HasMaxLength(2040);
                entity.Property(e => e.CodDepAfecGasto).HasColumnName("CODDEPAFECGASTO").HasMaxLength(400);
                entity.Property(e => e.NmDepAfecGasto).HasColumnName("NMDEPAFECGASTO").HasMaxLength(2040);
                entity.Property(e => e.VlItem).HasColumnName("VLITEM").HasColumnType("decimal(28,12)");
                entity.Property(e => e.VlSaldoCopromet).HasColumnName("VLSALDOCOPROMET").HasColumnType("decimal(28,12)");
                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA");
            });

            
            // Mapeo explícito propiedad C# → columna SQL (nombres y tipos alineados al DDL de DYNTBLSIIFOBLIGAPO).
            modelBuilder.Entity<SiifObligacionApo>(entity =>
            {
                entity.ToTable("DYNTBLSIIFOBLIGAPO", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32);
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.IdObligacionPosicio).HasColumnName("IDOBLIGAPOSICIO").HasColumnType("numeric(10,0)");
                entity.Property(e => e.IdObliga).HasColumnName("IDOBLIGA").HasColumnType("numeric(10,0)");
                entity.Property(e => e.CodObligacion).HasColumnName("CODOBLIGACION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.VigenciaCod).HasColumnName("VIGENCIACOD").HasMaxLength(400);
                entity.Property(e => e.AnioVigencia).HasColumnName("ANIOVIGENCIA").HasMaxLength(400);
                entity.Property(e => e.CodDepAfectacio).HasColumnName("CODDEPAFECTACIO").HasMaxLength(400);
                entity.Property(e => e.DesDepAfectacio).HasColumnName("DESDEPAFECTACIO").HasMaxLength(2040);
                entity.Property(e => e.CodPosicionGast).HasColumnName("CODPOSICIONGAST").HasMaxLength(2040);
                entity.Property(e => e.DesPosicionGast).HasColumnName("DESPOSICIONGAST");
                entity.Property(e => e.CodFuenteFinan).HasColumnName("CODFUENTEFINAN").HasMaxLength(2040);
                entity.Property(e => e.DesFuenteFinan).HasColumnName("DESFUENTEFINAN").HasMaxLength(2040);
                entity.Property(e => e.CodRecursoPpal).HasColumnName("CODRECURSOPPAL").HasMaxLength(400);
                entity.Property(e => e.DesRecursoPpal).HasColumnName("DESRECURSOPPAL").HasMaxLength(2040);
                entity.Property(e => e.CodSituacionFon).HasColumnName("CODSITUACIONFON").HasMaxLength(400);
                entity.Property(e => e.DesSituacionFon).HasColumnName("DESSITUACIONFON").HasMaxLength(2040);
                entity.Property(e => e.VlInicialPosicion).HasColumnName("VLINICIALPOSICI").HasColumnType("decimal(28,12)");
                entity.Property(e => e.VlOperaciones).HasColumnName("VLOPERACIONES").HasColumnType("decimal(28,12)");
                entity.Property(e => e.VlActualPosicio).HasColumnName("VLACTUALPOSICIO").HasColumnType("decimal(28,12)");
                entity.Property(e => e.SaldoXUtilizar).HasColumnName("SALDOXUTILIZAR").HasColumnType("decimal(28,12)");
                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA");
            });

            modelBuilder.Entity<DynEfwAuditoria>(entity =>
            {
                entity.ToTable("DYNEFWAUDITORIA", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32);
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.OidRevisionForm).HasColumnName("OIDREVISIONFORM").HasMaxLength(32);
                entity.Property(e => e.FgSystem).HasColumnName("FGSYSTEM").HasColumnType("numeric(2,0)");
                entity.Property(e => e.BnUpdated).HasColumnName("BNUPDATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.NmUserUpdate).HasColumnName("NMUSERUPDATE").HasMaxLength(255);

                entity.Property(e => e.IdTransaccion).HasColumnName("IDTRANSACCION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.IdTramite).HasColumnName("IDTRAMITE").HasMaxLength(2040);
                entity.Property(e => e.Mensaje).HasColumnName("MENSAJE").HasMaxLength(2040);
                entity.Property(e => e.Peticion).HasColumnName("PETICION");
                entity.Property(e => e.Respuesta).HasColumnName("RESPUESTA");
                entity.Property(e => e.TiempoRq).HasColumnName("TIEMPORQ").HasMaxLength(2040);
                entity.Property(e => e.TiempoRs).HasColumnName("TIEMPORS").HasMaxLength(2040);
                entity.Property(e => e.Nivel).HasColumnName("NIVEL").HasMaxLength(2040);
                entity.Property(e => e.Excepcion).HasColumnName("EXCEPCION");
                entity.Property(e => e.PuntoDeControl).HasColumnName("PUNTODECONTROL").HasMaxLength(2040);
                entity.Property(e => e.Aplicacion).HasColumnName("APLICACION").HasMaxLength(255);
                entity.Property(e => e.Ccf).HasColumnName("CCF").HasMaxLength(2040);
                entity.Property(e => e.EstadoFinal).HasColumnName("ESTADO_FINAL").HasMaxLength(255);
            });

            // Mapeo de ejecución presupuestal agregada → tabla DYNTBLEJECUCIONAGR
            modelBuilder.Entity<SiifEjecucionAgregada>(entity =>
            {
                entity.ToTable("DYNTBLEJECUCIONAGR", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32);
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.OidRevisionForm).HasColumnName("OIDREVISIONFORM").HasMaxLength(32);
                entity.Property(e => e.FgSystem).HasColumnName("FGSYSTEM").HasColumnType("numeric(2,0)");
                entity.Property(e => e.BnUpdated).HasColumnName("BNUPDATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.AnioFiscal).HasColumnName("ANIOFISCAL").HasColumnType("numeric(10,0)");
                entity.Property(e => e.PosicionGasto).HasColumnName("POSICIONGASTO").HasColumnType("varchar(max)");
                entity.Property(e => e.ApropiacionInicial).HasColumnName("APROPIAINICIAL").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApropiacionAdicionada).HasColumnName("APROPIAADICIONA").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApropiacionReducida).HasColumnName("APROPIAREDUCIDA").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApropiacionVigente).HasColumnName("APROPIAVIGENTE").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApropiacionBloqueada).HasColumnName("APROPIABLOQUEAD").HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApropiacionDisponible).HasColumnName("APROPIADISPONIB").HasColumnType("decimal(18,2)");
                entity.Property(e => e.VlCdp).HasColumnName("VLCDP").HasColumnType("decimal(18,2)");
                entity.Property(e => e.VlCompromiso).HasColumnName("VLCOMPROMISO").HasColumnType("decimal(18,2)");
                entity.Property(e => e.VlObligacion).HasColumnName("VLOBLIGACION").HasColumnType("decimal(18,2)");
                entity.Property(e => e.VlOrdenPago).HasColumnName("VLORDENPAGO").HasColumnType("decimal(18,2)");
                entity.Property(e => e.VlPago).HasColumnName("VLPAGO").HasColumnType("decimal(18,2)");
                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA").HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
            });
        }
    }
}

