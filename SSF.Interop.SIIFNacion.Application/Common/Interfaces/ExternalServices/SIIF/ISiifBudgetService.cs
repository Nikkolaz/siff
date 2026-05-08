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
    public interface ISiifBudgetService
    {

        Task<ConsultarCdpResponseDto> ConsultarCdpAsync(
           ConsultarCdpRequestDto request,
           SiifRequestHeaderDto headers,
           CancellationToken cancellationToken = default);

        Task<ConsultaListaCdpPaginadaResponseDto> ConsultarListaCdpPaginadaAsync(
            ConsultaListaCdpPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default);

        Task<ConsultarCompromisoResponseDto> ConsultarCompromisoPptalAsync(
            ConsultarCompromisoRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default);

        Task<ConsultaListaCompromisoPaginadaResponseDto> ConsultarListaCompromisoPaginadaAsync(
            ConsultaListaCompromisoPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default);

        Task<ConsultaListaObligacionesPaginadaResponseDto> ConsultarListaObligacionesPaginadaAsync(
            ConsultaListaObligacionesPaginadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default);

        Task<ConsultarEjecucionAgregadaResponseDto> ConsultarEjecucionAgregadaAsync(
            ConsultarEjecucionAgregadaRequestDto request,
            SiifRequestHeaderDto headers,
            CancellationToken cancellationToken = default);
    }
}
