using Microsoft.EntityFrameworkCore;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using SSF.Interop.SIIFNacion.Persistence.DBContext;

public class CdpCompromisoRepository : ICdpCompromisoRepository
{
    private readonly CompromisoDBContext _context;

    public CdpCompromisoRepository(CompromisoDBContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(CdpCompromiso comp, CancellationToken cancellationToken)
    {
        await _context.Ccompptals.AddAsync(comp, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// BUG FIX: usa IdCompromiso (el código de SIIF) en lugar de ValorInicial.
    /// </summary>
    public async Task<bool> ExistsAsync(long idCompromiso, CancellationToken cancellationToken)
    {
        return await _context.Ccompptals
            .AnyAsync(c => c.IdCompromiso == idCompromiso, cancellationToken);
    }

    public async Task ResetIdentityIfEmpty()
    {
        var count = await _context.Ccompptals.CountAsync();
        if (count == 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('tblccompptal', RESEED, 0);");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('tblccompptal_item', RESEED, 0);");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('tblccompptal_planpago', RESEED, 0);");
        }
    }
}