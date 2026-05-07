using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Persistence.DBContext
{
    public class DbContextCdp : DbContext
    {
        public DbContextCdp(DbContextOptions<DbContextCdp> options) : base(options) { }

        public DbSet<CdpPaginado> Cdps { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CdpPaginado>(entity =>
            {
                entity.ToTable("DYNTBLCDPPAGINADO", "dbo");

                entity.HasKey(e => e.Oid);

                // Default
                entity.Property(e => e.FechaCreacionSistema).HasDefaultValueSql("getdate()");

                // Columnas principales
                entity.Property(e => e.Oid).HasColumnName("OID").HasMaxLength(36);
                entity.Property(e => e.NrVersion).HasColumnName("NRVERSION");
                entity.Property(e => e.BnCreated).HasColumnName("BNCREATED");
                entity.Property(e => e.FgEnabled).HasColumnName("FGENABLED");
                entity.Property(e => e.OidRevisionForm).HasColumnName("OIDREVISIONFORM").HasMaxLength(32);
                entity.Property(e => e.FgSystem).HasColumnName("FGSYSTEM");
                entity.Property(e => e.BnUpdated).HasColumnName("BNUPDATED");
                entity.Property(e => e.NmUserUpdate).HasColumnName("NMUSERUPDATE").HasMaxLength(255);
                entity.Property(e => e.CodigoMoneda).HasColumnName("CodigoMoneda").HasMaxLength(10);
                entity.Property(e => e.DescripcionMoneda).HasColumnName("DescripcionMoneda").HasMaxLength(50);
                entity.Property(e => e.Vigencia).HasColumnName("Vigencia");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("UsuarioCreacion").HasMaxLength(50);
                entity.Property(e => e.UsuarioModificacion).HasColumnName("UsuarioModificacion").HasMaxLength(50);
                entity.Property(e => e.FechaModificacion).HasColumnName("FechaModificacion");
                entity.Property(e => e.Observaciones).HasColumnName("Observaciones").HasMaxLength(500);

                // Datos de CDP
                entity.Property(e => e.IdCdp).HasColumnName("IDCDP").HasMaxLength(2040);
                entity.Property(e => e.CodPciConexion).HasColumnName("CODPCICONEXION").HasMaxLength(400);
                entity.Property(e => e.DescPciConexion).HasColumnName("DESCPCICONEXION").HasMaxLength(2040);
                entity.Property(e => e.CodSubNidad).HasColumnName("CODSUBNIDAD").HasMaxLength(400);
                entity.Property(e => e.VlInicial).HasColumnName("VLINICIAL").HasColumnType("numeric(18,2)");
                entity.Property(e => e.DescSubUnidad).HasColumnName("DESCSUBUNIDAD").HasMaxLength(2040);
                entity.Property(e => e.NnSolicitudCdp).HasColumnName("NNSOLICITUDCDP");
                entity.Property(e => e.NnDocumento).HasColumnName("NNDOCUMENTO");
                entity.Property(e => e.DtRegistro).HasColumnName("DTREGISTRO");
                entity.Property(e => e.DtCreacion).HasColumnName("DTCREACION");
                entity.Property(e => e.TipoCdp).HasColumnName("TIPOCDP").HasMaxLength(400);
                entity.Property(e => e.Estado).HasColumnName("ESTADO").HasMaxLength(400);
                entity.Property(e => e.Objeto).HasColumnName("OBJETO");
                entity.Property(e => e.CodDepAfectacio).HasColumnName("CODDEPAFECTACIO").HasMaxLength(400);
                entity.Property(e => e.DesDepAfectacio).HasColumnName("DESDEPAFECTACIO").HasMaxLength(2040);
                entity.Property(e => e.CodPosGasto).HasColumnName("CODPOSGASTO").HasMaxLength(400);
                entity.Property(e => e.DesPosGasto).HasColumnName("DESPOSGASTO");
                entity.Property(e => e.CodFuente).HasColumnName("CODFUENTE").HasMaxLength(400);
                entity.Property(e => e.DesFuente).HasColumnName("DESFUENTE").HasMaxLength(2040);
                entity.Property(e => e.CodRecurso).HasColumnName("CODRECURSO").HasMaxLength(400);
                entity.Property(e => e.DesRecurso).HasColumnName("DESRECURSO").HasMaxLength(2040);
                entity.Property(e => e.CodigoSituacion).HasColumnName("CODIGOSITUACION").HasMaxLength(400);
                entity.Property(e => e.DesSituacion).HasColumnName("DESSITUACION").HasMaxLength(400);
                entity.Property(e => e.VlOperaciones).HasColumnName("VLOPERACIONES").HasColumnType("numeric(18,2)");
                entity.Property(e => e.VlActual).HasColumnName("VLACTUAL").HasColumnType("numeric(18,2)");
                entity.Property(e => e.SaldoPorComp).HasColumnName("SALDOPORCOMP").HasColumnType("numeric(18,2)");
                entity.Property(e => e.VlBloqueado).HasColumnName("VLBLOQUEADO").HasColumnType("numeric(18,2)");

                // IDs relacionados
                entity.Property(e => e.IdRespuesta).HasColumnName("IDRESPUESTA");
                entity.Property(e => e.IdReintegro).HasColumnName("IDREINTEGRO");
                entity.Property(e => e.IdOrdenPago).HasColumnName("IDORDENPAGO");
                entity.Property(e => e.IdObligacion).HasColumnName("IDOBLIGACION");
                entity.Property(e => e.IdCuenta).HasColumnName("IDCUENTA").HasMaxLength(2040);

                // Listas
                entity.Property(e => e.ListaCompromiso).HasColumnName("LISTACOMPROMISO");
                entity.Property(e => e.ListCuentasXpagar).HasColumnName("LISTCUENTASXPAG");
                entity.Property(e => e.ListaObligacion).HasColumnName("LISTAOBLIGACION");
                entity.Property(e => e.ListaOrdenDePago).HasColumnName("LISTORDENDEPAGA");
                entity.Property(e => e.ListaReintegro).HasColumnName("LISTAREINTEGRO");

                // Fecha de carga
                entity.Property(e => e.FechaCarga).HasColumnName("FECHACARGA");
            });
        }
    }
}