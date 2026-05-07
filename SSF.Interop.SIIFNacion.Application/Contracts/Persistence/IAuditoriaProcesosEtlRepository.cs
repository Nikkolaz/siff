using SSF.Interop.SIIFNacion.Service.Domain.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Service.Application.Contracts.Persistence
{
    public interface IAuditoriaProcesosEtlRepository
    {

        Task<AuditoriaProcesosEtl> AddAsync(AuditoriaProcesosEtl entity);
    }
}
