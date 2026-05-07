using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Persistence.DBContext
{
    public class CompromisoDBContext : DbContext
    {
        public CompromisoDBContext(DbContextOptions<CompromisoDBContext> options) : base(options) { }

        public DbSet<CdpCompromiso> Ccompptals { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CdpCompromiso>(entity =>
            {
                entity.ToTable("tblccompptal", "dbo");

                // 🔑 Primary Key
                entity.HasKey(e => e.idcompdetalle);

                entity.Property(e => e.idcompdetalle)
                    .HasColumnName("idcompdetalle")
                    .ValueGeneratedOnAdd();

                // 📌 Campos principales
                entity.Property(e => e.IdCompromiso)
                    .IsRequired()
                    .HasColumnName("IdCompromiso");

                entity.Property(e => e.vigencianm)
                    .HasMaxLength(100);

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime");

                entity.Property(e => e.Estado)
                    .HasMaxLength(100);

                entity.Property(e => e.codcdp);

                entity.Property(e => e.FechaCdp)
                    .HasColumnType("datetime");

                // 💰 Moneda
                entity.Property(e => e.codmoneda);

                entity.Property(e => e.nmmoneda)
                    .HasMaxLength(100);

                entity.Property(e => e.ValorTasa)
                    .HasColumnType("decimal(18,6)");

                // 📝 Texto
                entity.Property(e => e.Descripcion)
                    .HasColumnType("varchar(max)");

                entity.Property(e => e.Objeto)
                    .HasColumnType("varchar(max)");

                // 💵 Valores
                entity.Property(e => e.ValorInicial)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.vliniorimoneda)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.vltoperacion)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ValorActual)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SaldoxObligar)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.saldomoneda)
                    .HasColumnType("decimal(18,2)");

                // 👤 Tercero
                entity.Property(e => e.ttdocumento)
                    .HasMaxLength(100);

                entity.Property(e => e.tndocumento)
                    .HasMaxLength(50);

                entity.Property(e => e.terceronm)
                    .HasMaxLength(255);

                // 💳 Pago
                entity.Property(e => e.mediopago)
                    .HasMaxLength(100);

                // 🏦 Cuenta
                entity.Property(e => e.cuentann)
                    .HasMaxLength(50);

                entity.Property(e => e.cuentaentfinan)
                    .HasMaxLength(255);

                entity.Property(e => e.CuentaTipo)
                    .HasMaxLength(50);

                entity.Property(e => e.CuentaEstado)
                    .HasMaxLength(50);

                // 🧑‍💼 Ordenador
                entity.Property(e => e.ordenadortdoc)
                    .HasMaxLength(100);

                entity.Property(e => e.ordenadorndoc)
                    .HasMaxLength(50);

                entity.Property(e => e.ordenadornm)
                    .HasMaxLength(255);

                entity.Property(e => e.ordenadorconsec);

                entity.Property(e => e.ordenadorcodcar)
                    .HasMaxLength(50);

                entity.Property(e => e.ordenadornmcarg)
                    .HasMaxLength(255);

                // 🧾 Otros
                entity.Property(e => e.cajamenor)
                    .HasMaxLength(255);

                entity.Property(e => e.nndocsoporte)
                    .HasMaxLength(100);

                entity.Property(e => e.tdocsoporte)
                    .HasMaxLength(255);

                entity.Property(e => e.dtdocsoporte)
                    .HasColumnType("datetime");

                // ⏱️ Fecha carga con default
                entity.Property(e => e.fechacarga)
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<CdpCompromisoItem>(entity =>
            {
                entity.ToTable("tblccompptal_item", "dbo");

                entity.HasKey(e => e.iditem);

                entity.Property(e => e.iditem)
                    .HasColumnName("iditem")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.idcompromiso)
                    .HasColumnName("idcompromiso")
                    .IsRequired();

                entity.Property(e => e.coddepafecta).HasMaxLength(20);
                entity.Property(e => e.nmdepafecta).HasMaxLength(255);

                entity.Property(e => e.codpgasto).HasMaxLength(100);
                entity.Property(e => e.nmpgasto).HasColumnType("varchar(max)");

                entity.Property(e => e.codffinan).HasMaxLength(20);
                entity.Property(e => e.nmffinan).HasMaxLength(100);

                entity.Property(e => e.codrpptal).HasMaxLength(20);
                entity.Property(e => e.nmrpptal).HasMaxLength(255);

                entity.Property(e => e.codsfondo).HasMaxLength(20);
                entity.Property(e => e.nmsfondo).HasMaxLength(100);

                entity.Property(e => e.vlinicial).HasColumnType("decimal(18,2)");
                entity.Property(e => e.vloperaciones).HasColumnType("decimal(18,2)");
                entity.Property(e => e.vlactual).HasColumnType("decimal(18,2)");
                entity.Property(e => e.saldo).HasColumnType("decimal(18,2)");

                entity.Property(e => e.fechacarga)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                // 🔗 relación
                entity.HasOne(d => d.Compromiso)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.idcompromiso);
            });

            modelBuilder.Entity<CdpCompromisoPlanPago>(entity =>
            {
                entity.ToTable("tblccompptal_planpago", "dbo");

                entity.HasKey(e => e.IdPlanPago);

                entity.Property(e => e.IdPlanPago)
                    .HasColumnName("IdPlanPago")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.idcompromiso)
                    .HasColumnName("idcompromiso")
                    .IsRequired();

                entity.Property(e => e.FechaPago)
                    .HasColumnType("datetime");

                entity.Property(e => e.coddepafepac).HasMaxLength(20);
                entity.Property(e => e.nmdepafepac).HasMaxLength(255);

                entity.Property(e => e.codposipac).HasMaxLength(50);
                entity.Property(e => e.nmposipac).HasMaxLength(255);

                entity.Property(e => e.Valor)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SaldoPorObligar)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.codlineapago).HasMaxLength(20);
                entity.Property(e => e.nmlineapago).HasMaxLength(100);

                entity.Property(e => e.FechaCarga)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Compromiso)
                    .WithMany(p => p.PlanesPago)
                    .HasForeignKey(d => d.idcompromiso);
            });

        }
    }

}