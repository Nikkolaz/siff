using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Persistence.DBContext
{
    /// <summary>
    /// DbContext para las tablas nuevas de producción DYNTBLCCOMPPTAL y DYNTBLLISTITEMSAFE.
    /// No se usa navegación EF entre padre e ítems — se insertan por separado para evitar conflictos de FK.
    /// </summary>
    public class CompromisoDynContext : DbContext
    {
        public CompromisoDynContext(DbContextOptions<CompromisoDynContext> options) : base(options) { }

        public DbSet<DynTblCCompPtal> Compromisos { get; set; } = default!;
        public DbSet<DynTblListItemsAfe> Items { get; set; } = default!;

        /// <summary>Lista paginada de compromisos RP → tabla DYNTBLCOMPROMPAGIN.</summary>
        public DbSet<DynTblCompromPagin> CompromPagin { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DynTblCCompPtal>(entity =>
            {
                entity.ToTable("DYNTBLCCOMPPTAL", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32).IsRequired();
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.OidRevisionForm).HasColumnName("OIDREVISIONFORM").HasMaxLength(32);
                entity.Property(e => e.FgSystem).HasColumnName("FGSYSTEM").HasColumnType("numeric(2,0)");
                entity.Property(e => e.BnUpdated).HasColumnName("BNUPDATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.NmUserUpdate).HasColumnName("NMUSERUPDATE").HasMaxLength(255);
                entity.Property(e => e.IdCompDetalle).HasColumnName("IDCOMPDETALLE").HasColumnType("numeric(10,0)");
                entity.Property(e => e.IdCompromiso).HasColumnName("IDCOMPROMISO").HasColumnType("numeric(10,0)");
                entity.Property(e => e.VigenciaNm).HasColumnName("VIGENCIANM").HasMaxLength(2040);
                entity.Property(e => e.FechaRegistro).HasColumnName("FECHAREGISTRO").HasColumnType("datetime");
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(2040);
                entity.Property(e => e.CodCdp).HasColumnName("CODCDP").HasColumnType("numeric(10,0)");
                entity.Property(e => e.FechaCdp).HasColumnName("FECHACDP").HasColumnType("datetime");
                entity.Property(e => e.CodMoneda).HasColumnName("CODMONEDA").HasColumnType("numeric(10,0)");
                entity.Property(e => e.NmMoneda).HasColumnName("NMMONEDA").HasMaxLength(2040);
                entity.Property(e => e.ValorTasa).HasColumnName("VALORTASA").HasColumnType("numeric(28,12)");
                entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION").HasColumnType("varchar(max)");
                entity.Property(e => e.ValorInicial).HasColumnName("VALORINICIAL").HasColumnType("numeric(28,12)");
                entity.Property(e => e.VlIniOriMoneda).HasColumnName("VLINIORIMONEDA").HasColumnType("numeric(28,12)");
                entity.Property(e => e.VlTOperacion).HasColumnName("VLTOPERACION").HasColumnType("numeric(28,12)");
                entity.Property(e => e.ValorActual).HasColumnName("VALORACTUAL").HasColumnType("numeric(28,12)");
                entity.Property(e => e.SaldoXObligar).HasColumnName("SALDOXOBLIGAR").HasColumnType("numeric(28,12)");
                entity.Property(e => e.SaldoMoneda).HasColumnName("SALDOMONEDA").HasColumnType("numeric(28,12)");
                entity.Property(e => e.Objeto).HasColumnName("OBJETO").HasColumnType("varchar(max)");
                entity.Property(e => e.TtDocumento).HasColumnName("TTDOCUMENTO").HasMaxLength(2040);
                entity.Property(e => e.TnDocumento).HasColumnName("TNDOCUMENTO").HasMaxLength(400);
                entity.Property(e => e.TerceroNm).HasColumnName("TERCERONM").HasMaxLength(2040);
                entity.Property(e => e.MedioPago).HasColumnName("MEDIOPAGO").HasMaxLength(2040);
                entity.Property(e => e.CuentaNn).HasColumnName("CUENTANN").HasMaxLength(400);
                entity.Property(e => e.CuentaEntFinan).HasColumnName("CUENTAENTFINAN").HasMaxLength(2040);
                entity.Property(e => e.CuentaTipo).HasColumnName("CUENTATIPO").HasMaxLength(400);
                entity.Property(e => e.CuentaEstado).HasColumnName("CUENTAESTADO").HasMaxLength(400);
                entity.Property(e => e.OrdenadorTDoc).HasColumnName("ORDENADORTDOC").HasMaxLength(2040);
                entity.Property(e => e.OrdenadorNDoc).HasColumnName("ORDENADORNDOC").HasMaxLength(400);
                entity.Property(e => e.OrdenadorNm).HasColumnName("ORDENADORNM").HasMaxLength(2040);
                entity.Property(e => e.OrdenadorConsec).HasColumnName("ORDENADORCONSEC").HasColumnType("numeric(10,0)");
                entity.Property(e => e.OrdenadorCodCar).HasColumnName("ORDENADORCODCAR").HasMaxLength(400);
                entity.Property(e => e.OrdenadorNmCarg).HasColumnName("ORDENADORNMCARG").HasMaxLength(2040);
                entity.Property(e => e.CajaMenor).HasColumnName("CAJAMENOR").HasMaxLength(2040);
                entity.Property(e => e.NnDocSoporte).HasColumnName("NNDOCSOPORTE").HasMaxLength(2040);
                entity.Property(e => e.TDocSoporte).HasColumnName("TDOCSOPORTE").HasMaxLength(2040);
                entity.Property(e => e.DtDocSoporte).HasColumnName("DTDOCSOPORTE").HasColumnType("datetime");
                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA").HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.AnioVigencia).HasColumnName("ANIOVIGENCIA").HasMaxLength(400);

                // Sin relación HasMany — los ítems se insertan por separado en el repositorio
                entity.Ignore(e => e.Items);
            });

            modelBuilder.Entity<DynTblListItemsAfe>(entity =>
            {
                entity.ToTable("DYNTBLLISTITEMSAFE", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32).IsRequired();
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.OidRevisionForm).HasColumnName("OIDREVISIONFORM").HasMaxLength(32);
                entity.Property(e => e.FgSystem).HasColumnName("FGSYSTEM").HasColumnType("numeric(2,0)");
                entity.Property(e => e.BnUpdated).HasColumnName("BNUPDATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.NmUserUpdate).HasColumnName("NMUSERUPDATE").HasMaxLength(255);
                entity.Property(e => e.IdItem).HasColumnName("IDITEM").HasColumnType("numeric(10,0)");
                entity.Property(e => e.IdCompromiso).HasColumnName("IDCOMPROMISO").HasColumnType("numeric(10,0)");
                entity.Property(e => e.CodDepAfecta).HasColumnName("CODDEPAFECTA").HasMaxLength(400);
                entity.Property(e => e.NmDepAfecta).HasColumnName("NMDEPAFECTA").HasMaxLength(2040);
                entity.Property(e => e.CodPGasto).HasColumnName("CODPGASTO").HasMaxLength(400);
                entity.Property(e => e.NmPGasto).HasColumnName("NMPGASTO").HasColumnType("varchar(max)");
                entity.Property(e => e.CodFFinan).HasColumnName("CODFFINAN").HasMaxLength(400);
                entity.Property(e => e.NmFFinan).HasColumnName("NMFFINAN").HasMaxLength(2040);
                entity.Property(e => e.CodRPPtal).HasColumnName("CODRPPTAL").HasMaxLength(400);
                entity.Property(e => e.NmRPPtal).HasColumnName("NMRPPTAL").HasMaxLength(2040);
                entity.Property(e => e.CodSFondo).HasColumnName("CODSFONDO").HasMaxLength(400);
                entity.Property(e => e.NmSFondo).HasColumnName("NMSFONDO").HasMaxLength(2040);
                entity.Property(e => e.VlInicial).HasColumnName("VLINICIAL").HasColumnType("numeric(28,12)");
                entity.Property(e => e.VlOperaciones).HasColumnName("VLOPERACIONES").HasColumnType("numeric(28,12)");
                entity.Property(e => e.VlActual).HasColumnName("VLACTUAL").HasColumnType("numeric(28,12)");
                entity.Property(e => e.Saldo).HasColumnName("SALDO").HasColumnType("numeric(28,12)");
                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA").HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.AnioVigencia).HasColumnName("ANIOVIGENCIA").HasMaxLength(400);
            });

            // ── DYNTBLCOMPROMPAGIN: lista paginada de compromisos RP ─────────────
            modelBuilder.Entity<DynTblCompromPagin>(entity =>
            {
                entity.ToTable("DYNTBLCOMPROMPAGIN", "dbo");
                entity.HasKey(e => e.Oid);

                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(32).IsRequired();
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION").HasColumnType("numeric(10,0)");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED").HasColumnType("numeric(2,0)");
                entity.Property(e => e.OidRevisionForm).HasColumnName("OIDREVISIONFORM").HasMaxLength(32);
                entity.Property(e => e.FgSystem).HasColumnName("FGSYSTEM").HasColumnType("numeric(2,0)");
                entity.Property(e => e.BnUpdated).HasColumnName("BNUPDATED").HasColumnType("numeric(19,0)");
                entity.Property(e => e.NmUserUpdate).HasColumnName("NMUSERUPDATE").HasMaxLength(255);

                entity.Property(e => e.IdCompromiso).HasColumnName("IDCOMPROMISO").HasMaxLength(2040);
                entity.Property(e => e.IdPci).HasColumnName("IDPCI").HasColumnType("numeric(10,0)");
                entity.Property(e => e.DescripcionPci).HasColumnName("DESCRIPCIONPCI").HasMaxLength(2040);
                entity.Property(e => e.CodCompromiso).HasColumnName("CODCOMPROMISO").HasColumnType("numeric(10,0)");
                entity.Property(e => e.VigenciaCod).HasColumnName("VIGENCIACOD").HasMaxLength(400);
                entity.Property(e => e.VigenciaNm).HasColumnName("VIGENCIANM").HasMaxLength(2040);
                entity.Property(e => e.FechaRegistro).HasColumnName("FECHAREGISTRO").HasColumnType("datetime");
                entity.Property(e => e.FechaCreacion).HasColumnName("FECHACREACION").HasColumnType("datetime");
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(2040);

                entity.Property(e => e.CodDependencia).HasColumnName("CODDEPENDENCIA").HasMaxLength(400);
                entity.Property(e => e.DescripcionDep).HasColumnName("DESCRIPCIONDEP").HasMaxLength(2040);
                entity.Property(e => e.CodPGastos).HasColumnName("CODPGASTOS").HasMaxLength(2040);
                entity.Property(e => e.DesPGastos).HasColumnName("DESPGASTOS").HasColumnType("varchar(MAX)");
                entity.Property(e => e.CodFuente).HasColumnName("CODFUENTE").HasMaxLength(400);
                entity.Property(e => e.Fuente).HasColumnName("FUENTE").HasMaxLength(2040);
                entity.Property(e => e.CodSituacion).HasColumnName("CODSITUACION").HasMaxLength(400);
                entity.Property(e => e.Situacion).HasColumnName("SITUACION").HasMaxLength(2040);
                entity.Property(e => e.CodRecurso).HasColumnName("CODRECURSO").HasMaxLength(400);
                entity.Property(e => e.Recursos).HasColumnName("RECURSOS").HasMaxLength(2040);

                entity.Property(e => e.VlInicial).HasColumnName("VLINICIAL").HasColumnType("numeric(28,12)");
                entity.Property(e => e.VlOperaciones).HasColumnName("VLOPERACIONES").HasColumnType("numeric(28,12)");
                entity.Property(e => e.VlActual).HasColumnName("VLACTUAL").HasColumnType("numeric(28,12)");
                entity.Property(e => e.SaldoUtilizar).HasColumnName("SALDOUTILIZAR").HasColumnType("numeric(28,12)");

                entity.Property(e => e.TipoIdentificac).HasColumnName("TIPOIDENTIFICAC").HasMaxLength(2040);
                entity.Property(e => e.NnIdentificacio).HasColumnName("NNIDENTIFICACIO").HasMaxLength(400);
                entity.Property(e => e.NmRazonSocial).HasColumnName("NMRAZONSOCIAL").HasMaxLength(2040);
                entity.Property(e => e.MedioPago).HasColumnName("MEDIOPAGO").HasMaxLength(2040);
                entity.Property(e => e.TipoCuenta).HasColumnName("TIPOCUENTA").HasMaxLength(400);
                entity.Property(e => e.NnCuenta).HasColumnName("NNCUENTA").HasMaxLength(400);
                entity.Property(e => e.EstadoCuenta).HasColumnName("ESTADOCUENTA").HasMaxLength(400);
                entity.Property(e => e.NitEntFinan).HasColumnName("NITENTFINAN").HasMaxLength(400);
                entity.Property(e => e.DesEntFinan).HasColumnName("DESENTFINAN").HasMaxLength(2040);

                entity.Property(e => e.CodCdp).HasColumnName("CODCDP").HasColumnType("numeric(10,0)");
                entity.Property(e => e.CuentasPagar).HasColumnName("CUENTASPAGAR").HasColumnType("varchar(MAX)");
                entity.Property(e => e.Obligaciones).HasColumnName("OBLIGACIONES").HasColumnType("varchar(MAX)");
                entity.Property(e => e.OrdenesPago).HasColumnName("ORDENESPAGO").HasColumnType("varchar(MAX)");
                entity.Property(e => e.Reintegros).HasColumnName("REINTEGROS").HasColumnType("varchar(MAX)");

                entity.Property(e => e.FechaDocSoporte).HasColumnName("FECHADOCSOPORTE").HasColumnType("datetime");
                entity.Property(e => e.TipoDocSoporte).HasColumnName("TIPODOCSOPORTE").HasMaxLength(2040);
                entity.Property(e => e.NumeroDocSoport).HasColumnName("NUMERODOCSOPORT").HasMaxLength(2040);
                entity.Property(e => e.Observaciones).HasColumnName("OBSERVACIONES").HasColumnType("varchar(MAX)");

                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA").HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.AnioVigencia).HasColumnName("ANIOVIGENCIA").HasMaxLength(400);
            });
        }
    }
}