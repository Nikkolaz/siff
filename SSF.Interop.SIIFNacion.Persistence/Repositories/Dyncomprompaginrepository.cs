using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

namespace SSF.Interop.SIIFNacion.Persistence.Repositories.GenericRepositories
{
    /// <summary>
    /// Repositorio para la tabla GESTORDOC.dbo.DYNTBLCOMPROMPAGIN.
    /// Persiste los registros de la lista paginada de compromisos RP devuelta por SIIF.
    /// </summary>
    public class DynCompromPaginRepository : IDynCompromPaginRepository
    {
        private readonly CompromisoDynContext _context;

        public DynCompromPaginRepository(CompromisoDynContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task SaveRangeAsync(IEnumerable<DynTblCompromPagin> registros, CancellationToken cancellationToken)
        {
            var lista = registros.ToList();
            if (lista.Count == 0) return;

            await _context.CompromPagin.AddRangeAsync(lista, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(decimal codCompromiso, string anioVigencia, CancellationToken cancellationToken)
        {
            return await _context.CompromPagin
                .AnyAsync(
                    c => c.CodCompromiso.HasValue
                        && c.CodCompromiso.Value == codCompromiso
                        && c.AnioVigencia == anioVigencia,
                    cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteByAnioVigenciaAsync(string anioVigencia, CancellationToken cancellationToken)
        {
            var registros = await _context.CompromPagin
                .Where(c => c.AnioVigencia == anioVigencia)
                .ToListAsync(cancellationToken);

            if (registros.Count > 0)
            {
                _context.CompromPagin.RemoveRange(registros);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task UpsertCompromisosAsync(IEnumerable<DynTblCompromPagin> registros, CancellationToken cancellationToken)
        {
            var listaEntrante = registros.ToList();
            if (listaEntrante.Count == 0) return;

            var anioVigencia = listaEntrante.First().AnioVigencia;
            var llaveExistentes = await _context.CompromPagin
                .Where(c => c.AnioVigencia == anioVigencia)
                .Select(c => new
                {
                    c.Oid,
                    c.IdPci,
                    c.CodCompromiso,
                    c.CodDependencia,
                    c.CodPGastos,
                    c.CodFuente,
                    c.CodSituacion,
                    c.CodRecurso
                })
                .ToListAsync(cancellationToken);

            var registrosNuevos = new List<DynTblCompromPagin>();
            var actualizaciones = new Dictionary<string, DynTblCompromPagin>();

            foreach (var entrante in listaEntrante)
            {
                var match = llaveExistentes.FirstOrDefault(e =>
                    e.IdPci == entrante.IdPci &&
                    e.CodCompromiso == entrante.CodCompromiso &&
                    e.CodDependencia == entrante.CodDependencia &&
                    e.CodPGastos == entrante.CodPGastos &&
                    e.CodFuente == entrante.CodFuente &&
                    e.CodSituacion == entrante.CodSituacion &&
                    e.CodRecurso == entrante.CodRecurso);

                if (match == null)
                {
                    registrosNuevos.Add(entrante);
                }
                else
                {
                    actualizaciones[match.Oid] = entrante;
                }
            }

            if (actualizaciones.Count > 0)
            {
                var oidsToUpdate = actualizaciones.Keys.ToList();
                var entidadesAActualizar = new List<DynTblCompromPagin>();
                const int chunkSize = 1500;

                for (int i = 0; i < oidsToUpdate.Count; i += chunkSize)
                {
                    var chunk = oidsToUpdate.Skip(i).Take(chunkSize).ToList();
                    var registrosChunk = await _context.CompromPagin
                        .Where(c => chunk.Contains(c.Oid))
                        .ToListAsync(cancellationToken);
                    entidadesAActualizar.AddRange(registrosChunk);
                }

                foreach (var existente in entidadesAActualizar)
                {
                    var entrante = actualizaciones[existente.Oid];
                    
                    existente.VlInicial     = entrante.VlInicial;
                    existente.VlOperaciones = entrante.VlOperaciones;
                    existente.VlActual      = entrante.VlActual;
                    existente.SaldoUtilizar = entrante.SaldoUtilizar;
                    existente.Estado        = entrante.Estado;
                    existente.FechaCarga    = entrante.FechaCarga;
                    existente.BnUpdated     = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }

            if (registrosNuevos.Count > 0)
                await _context.CompromPagin.AddRangeAsync(registrosNuevos, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}