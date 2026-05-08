using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    /// <summary>
    /// Repositorio para DYNTBLCCOMPPTAL y DYNTBLLISTITEMSAFE.
    /// Inserta el compromiso padre primero y luego los ítems por separado
    /// para evitar errores de FK con EF Core.
    /// </summary>
    public class DynCompromisoRepository : IDynCompromisoRepository
    {
        private readonly CompromisoDynContext _context;

        public DynCompromisoRepository(CompromisoDynContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(DynTblCCompPtal compromiso, CancellationToken cancellationToken)
        {
            // Extraer ítems antes de insertar el padre (EF los ignora en el mapeo)
            var items = compromiso.Items.ToList();
            compromiso.Items.Clear();

            // 1. Insertar el compromiso padre
            await _context.Compromisos.AddAsync(compromiso, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 2. Insertar los ítems con el OidCompromiso ya confirmado en BD
            if (items.Any())
            {
                await _context.Items.AddRangeAsync(items, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(decimal idCompromiso, CancellationToken cancellationToken)
        {
            return await _context.Compromisos
                .AnyAsync(c => c.IdCompromiso.HasValue && c.IdCompromiso.Value == idCompromiso, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpsertDetailAsync(DynTblCCompPtal compromiso, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Buscamos por la llave de negocio: IdCompromiso (SIIF Code)
                // Nota: El prompt menciona PCI + CodCompromiso + Vigencia, pero en este esquema 
                // IdCompromiso ya es el identificador único que viene de SIIF.
                var existente = await _context.Compromisos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.IdCompromiso == compromiso.IdCompromiso, cancellationToken);

                if (existente == null)
                {
                    // INSERT
                    compromiso.Oid = Guid.NewGuid().ToString("N").ToUpperInvariant();
                    compromiso.BnCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    compromiso.NrVersion = 1;

                    var items = compromiso.Items.ToList();
                    compromiso.Items = new List<DynTblListItemsAfe>();

                    await _context.Compromisos.AddAsync(compromiso, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    foreach (var item in items)
                    {
                        item.Oid = Guid.NewGuid().ToString("N").ToUpperInvariant();
                        item.IdCompromiso = compromiso.IdCompromiso; // Referencia por Id de Negocio
                        item.BnCreated = compromiso.BnCreated;
                    }
                    await _context.Items.AddRangeAsync(items, cancellationToken);
                }
                else
                {
                    // UPDATE
                    existente.BnUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    existente.NrVersion++;
                    
                    // Actualizar campos financieros y administrativos
                    existente.Estado = compromiso.Estado;
                    existente.Descripcion = compromiso.Descripcion;
                    existente.Objeto = compromiso.Objeto;
                    existente.ValorInicial = compromiso.ValorInicial;
                    existente.VlIniOriMoneda = compromiso.VlIniOriMoneda;
                    existente.VlTOperacion = compromiso.VlTOperacion;
                    existente.ValorActual = compromiso.ValorActual;
                    existente.SaldoXObligar = compromiso.SaldoXObligar;
                    existente.SaldoMoneda = compromiso.SaldoMoneda;
                    existente.FechaCarga = compromiso.FechaCarga;
                    
                    // Tercero y Banco
                    existente.TerceroNm = compromiso.TerceroNm;
                    existente.CuentaNn = compromiso.CuentaNn;
                    existente.CuentaEntFinan = compromiso.CuentaEntFinan;
                    existente.CuentaTipo = compromiso.CuentaTipo;
                    existente.CuentaEstado = compromiso.CuentaEstado;

                    // Reemplazar items (Borrar y volver a insertar para simplicidad en detalle variable)
                    if (existente.Items.Any())
                    {
                        _context.Items.RemoveRange(existente.Items);
                    }

                    foreach (var item in compromiso.Items)
                    {
                        item.Oid = Guid.NewGuid().ToString("N").ToUpperInvariant();
                        item.IdCompromiso = existente.IdCompromiso;
                        item.BnCreated = existente.BnCreated;
                        item.BnUpdated = existente.BnUpdated;
                        _context.Items.Add(item);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task WipeByVigenciaAsync(string vigencia, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Obtener los OID de los compromisos de esta vigencia para borrar sus ítems
                var oidsCompromiso = await _context.Compromisos
                    .Where(c => c.AnioVigencia == vigencia)
                    .Select(c => c.IdCompromiso)
                    .ToListAsync(cancellationToken);

                // 2. Borrar ítems (hijos) primero para evitar violación de FK
                if (oidsCompromiso.Any())
                {
                    var itemsAEliminar = await _context.Items
                        .Where(i => oidsCompromiso.Contains(i.IdCompromiso))
                        .ToListAsync(cancellationToken);

                    if (itemsAEliminar.Any())
                        _context.Items.RemoveRange(itemsAEliminar);
                }

                // 3. Borrar compromisos (padres)
                var compromisosAEliminar = await _context.Compromisos
                    .Where(c => c.AnioVigencia == vigencia)
                    .ToListAsync(cancellationToken);

                if (compromisosAEliminar.Any())
                    _context.Compromisos.RemoveRange(compromisosAEliminar);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
