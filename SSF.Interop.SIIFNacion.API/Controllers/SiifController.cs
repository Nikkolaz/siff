using MediatR;
using Microsoft.AspNetCore.Mvc;
using SSF.Interop.SIIFNacion.API.Models.SIIF;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.CoordinadorSincronizacionRp;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarDetalleCompromisoRP;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;

namespace SSF.Interop.SIIFNacion.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiifController : ControllerBase
    {

        private readonly IMediator _mediator;

        public SiifController(IMediator mediator)
       

        {
            _mediator = mediator;
            
        }

        [HttpPost("consultar-cdp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarCdp([FromBody] ConsultarCdpApiRequest request, CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");

            var query = new ConsultarCdpQuery
            {
                CodPciHeader           = request.CodPciHeader,
                LoginUsuarioSiifHeader = request.LoginUsuarioSiifHeader,
                ConsecutivoHeader      = request.ConsecutivoHeader,
                HashHeader             = request.HashHeader,
                IdentificacionPCI      = request.IdentificacionPCI,
                ConsecutivoCDP         = request.ConsecutivoCDP
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("consultar-cdp-paginada")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarCdpPaginada([FromBody] ConsultarCdpPaginadaApiRequest request, CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");

            var query = new ConsultarCdpPaginadaQuery
            {
                CodPciHeader           = request.CodPciHeader,
                LoginUsuarioSiifHeader = request.LoginUsuarioSiifHeader,
                ConsecutivoHeader      = request.ConsecutivoHeader,
                HashHeader             = request.HashHeader,
                Page                   = request.Page,
                Size                   = request.Size,
                PCIConsulta            = request.PCIConsulta,
                PCISubUnidades         = request.PCISubUnidades,
                FechaRegistroIni       = request.FechaRegistroIni,
                FechaRegistroFin       = request.FechaRegistroFin,
                TipoGasto              = request.TipoGasto,
                Rango                  = request.Rango
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("consulta-compromisopresupuestal-RP")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultaCompromisoPresupuestalRP(
            [FromBody] ConsultarCompromisoPresupuestalRPApiRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");

            var query = new ConsultarCompromisoPresupuestalRPQuery
            {
                CodPciHeader             = request.CodPciHeader,
                LoginUsuarioSiifHeader   = request.LoginUsuarioSiifHeader,
                ConsecutivoHeader        = request.ConsecutivoHeader,
                HashHeader               = request.HashHeader,
                Pci                      = request.Pci,
                CodCompromisoPptalGastos = request.CodCompromisoPptalGastos,
                Vigencia                 = request.Vigencia
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

     
    [HttpPost("lista-compromisopresupuestal-RP")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListaCompromisoPresupuestalRP(
        [FromHeader(Name = "codPCI")] string? codPciHeader,
        [FromHeader(Name = "loginUsuarioSIIF")] string? loginUsuarioSiifHeader,
        [FromHeader(Name = "consecutivo")] string? consecutivoHeader,
        [FromHeader(Name = "hash")] string? hashHeader,
        [FromBody] ListaCompromisoPresupuestalRPApiRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("La solicitud no puede ser nula.");

        var query = new ConsultarCompromisoPaginadoQuery
        {
            CodPciHeader           = codPciHeader,
            LoginUsuarioSiifHeader = loginUsuarioSiifHeader,
            ConsecutivoHeader      = consecutivoHeader,
            HashHeader             = hashHeader,

            Page        = request.PaginationDto.Page,
            Size        = request.PaginationDto.Size,
            PCI         = request.ConsultaCompromiso.PCI,
            FechaInicio = request.ConsultaCompromiso.FechaInicio,
            FechaFin    = request.ConsultaCompromiso.FechaFin,
            TipoGasto   = request.ConsultaCompromiso.TipoGasto,
            Rango       = request.ConsultaCompromiso.Rango,
            Vigencia    = request.ConsultaCompromiso.Vigencia
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }



        [HttpPost("consultar-ejecucion-agregada")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarEjecucionAgregada(
            [FromBody] ConsultarEjecucionAgregadaApiRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");

            var query = new ConsultarEjecucionAgregadaQuery
            {
                CodPciHeader           = request.CodPciHeader,
                LoginUsuarioSiifHeader = request.LoginUsuarioSiifHeader,
                ConsecutivoHeader      = request.ConsecutivoHeader,
                HashHeader             = request.HashHeader,
                IdentificacionPCI      = request.IdentificacionPCI,
                NivelInstitucional     = request.NivelInstitucional,
                ValorInstitucional     = request.ValorInstitucional,
                AnioFiscal             = request.AnioFiscal,
                Mes                    = request.Mes,
                TipoReporte            = request.TipoReporte,
                Vigencia               = request.Vigencia,
                NivelNormativo         = request.NivelNormativo,
                PosicionGastos         = request.PosicionGastos,
                Usuario                = request.Usuario
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("sincronizar-lista-obligaciones")]
        [ProducesResponseType(typeof(SincronizarListaObligacionesResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SincronizarListaObligaciones(
            [FromBody] SincronizarListaObligacionesApiRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");

            var command = new SincronizarListaObligacionesCommand
            {
                CodPciHeader              = request.CodPciHeader,
                LoginUsuarioSiifHeader    = request.LoginUsuarioSiifHeader,
                ConsecutivoHeader         = request.ConsecutivoHeader,
                HashHeader                = request.HashHeader,
                CodPCI                    = request.CodPCI,
                FechaInicio               = request.FechaInicio,
                FechaFin                  = request.FechaFin,
                TipoGasto                 = request.TipoGasto,
                Rango                     = request.Rango,
                Vigencia                  = request.Vigencia,
                DetalleUsosPresupuestales = request.DetalleUsosPresupuestales
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("ejecutar-actualizacion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EjecutarProcesoActualizacion(
            [FromHeader(Name = "codPCI")] string codPciHeader,
            [FromHeader(Name = "loginUsuarioSIIF")] string loginUsuarioSiifHeader,
            [FromHeader(Name = "consecutivo")] string consecutivoHeader,
            [FromHeader(Name = "hash")] string hashHeader,
            [FromBody] EjecutarProcesoActualizacionApiRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");
            
            request.CodPciHeader = codPciHeader;
            request.LoginUsuarioSiifHeader = loginUsuarioSiifHeader;
            request.ConsecutivoHeader = consecutivoHeader;
            request.HashHeader = hashHeader;

            return Ok(new { estado = "OK", mensaje = "Ejecución iniciada" });
        }

      
      
        [HttpPost("coordinar-sincronizacion-rp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CoordinarSincronizacionRp(
            [FromHeader(Name = "codPCI")] string? codPciHeader,
            [FromHeader(Name = "loginUsuarioSIIF")] string? loginUsuarioSiifHeader,
            [FromHeader(Name = "consecutivo")] string? consecutivoHeader,
            [FromHeader(Name = "hash")] string? hashHeader,
            [FromBody] CoordinarSincronizacionRpApiRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                return BadRequest("La solicitud no puede ser nula.");

            if (request.Anio < 2000 || request.Anio > 2100)
                return BadRequest($"El año '{request.Anio}' no es válido. Debe estar entre 2000 y 2100.");

            var command = new CoordinarSincronizacionRpCommand
            {
                Anio                   = request.Anio,
                CodPciHeader           = codPciHeader,
                LoginUsuarioSiifHeader = loginUsuarioSiifHeader,
                ConsecutivoHeader      = consecutivoHeader,
                HashHeader             = hashHeader
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("sincronizar-detalle-rp")]
        [ProducesResponseType(typeof(SincronizarDetalleCompromisoRPResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SincronizarDetalleRp(
            [FromHeader(Name = "codPCI")] string? codPciHeader,
            [FromHeader(Name = "loginUsuarioSIIF")] string? loginUsuarioSiifHeader,
            [FromHeader(Name = "consecutivo")] string? consecutivoHeader,
            [FromHeader(Name = "hash")] string? hashHeader,
            [FromBody] SincronizarDetalleCompromisoRPApiRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("La solicitud no puede ser nula.");

            var command = new SincronizarDetalleCompromisoRPCommand
            {
                CodPciHeader = codPciHeader ?? request.CodPciHeader,
                LoginUsuarioSiifHeader = loginUsuarioSiifHeader ?? request.LoginUsuarioSiifHeader,
                ConsecutivoHeader = consecutivoHeader ?? request.ConsecutivoHeader,
                HashHeader = hashHeader ?? request.HashHeader,
                Pci = request.Pci,
                Vigencia = request.Vigencia
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
