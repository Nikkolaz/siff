using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts
{
    public sealed class ConsultarCdpResponseDto : SiifResponseDto<JsonElement> { }
    public sealed class ConsultaListaCdpPaginadaResponseDto : SiifResponseDto<JsonElement> { }
    public sealed class ConsultarCompromisoResponseDto : SIIFCompromisoResponseDto { }
    public sealed class ConsultaListaCompromisoPaginadaResponseDto : ListaCompromisoDto { }
    public sealed class ConsultaListaObligacionesPaginadaResponseDto : SiifResponseDto<JsonElement> { }
    public sealed class ConsultarEjecucionAgregadaResponseDto : SiifResponseDto<JsonElement> { }
    public sealed class ConsultarCatalogoInstitucionalResponseDto : SiifResponseDto<JsonElement> { }
}
