using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF
{
    public interface ISiifCatalogService
    {
        Task<ConsultarCatalogoInstitucionalResponseDto> ConsultarCatalogoInstitucionalAsync(
            ConsultarCatalogoInstitucionalRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default);
    }
}
