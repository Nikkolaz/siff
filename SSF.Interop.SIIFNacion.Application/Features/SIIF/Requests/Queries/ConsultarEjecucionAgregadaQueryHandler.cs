using MediatR;
using System.Text.Json;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Domain.Siif;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarEjecucionAgregadaQueryHandler
        : IRequestHandler<ConsultarEjecucionAgregadaQuery, ConsultarEjecucionAgregadaResponseDto>
    {
        private readonly ISiifBudgetService _siifBudgetService;
        private readonly IEjecucionAgregadaRepository _ejecucionRepository;

        public ConsultarEjecucionAgregadaQueryHandler(
            ISiifBudgetService siifBudgetService,
            IEjecucionAgregadaRepository ejecucionRepository)
        {
            _siifBudgetService = siifBudgetService;
            _ejecucionRepository = ejecucionRepository;
        }

        public async Task<ConsultarEjecucionAgregadaResponseDto> Handle(
            ConsultarEjecucionAgregadaQuery request,
            CancellationToken cancellationToken)
        {
            var headers = new SiifRequestHeaderDto
            {
                CodPci = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo = request.ConsecutivoHeader,
                Hash = request.HashHeader
            };

            var body = new ConsultarEjecucionAgregadaRequestDto
            {
                IdentificacionPCI = request.IdentificacionPCI,
                NivelInstitucional = request.NivelInstitucional,
                ValorInstitucional = request.ValorInstitucional,
                AnioFiscal = request.AnioFiscal,
                Mes = request.Mes,
                TipoReporte = request.TipoReporte,
                Vigencia = request.Vigencia,
                NivelNormativo = request.NivelNormativo,
                PosicionGastos = request.PosicionGastos,
                Usuario = request.Usuario
            };

            // Paso 1: Consultar SIIF
            var response = await _siifBudgetService.ConsultarEjecucionAgregadaAsync(
                body, headers, cancellationToken);

            // Paso 2: Navegar response.Data (JsonElement del objeto raíz) → "data" → array → "pLista"
            // Estructura real: { "success": false, "data": [ { "pLista": [ {...}, {...} ] } ] }
            if (response.Data.ValueKind == JsonValueKind.Object)
            {
                // Buscar la propiedad "data" (case-insensitive manualmente por si acaso)
                JsonElement dataEl = default;
                bool tieneData = response.Data.TryGetProperty("data", out dataEl) ||
                                 response.Data.TryGetProperty("Data", out dataEl);

                if (tieneData && dataEl.ValueKind == JsonValueKind.Array)
                {
                    var registros = MapearDesdeDataArray(dataEl, request.AnioFiscal);
                    if (registros.Any())
                        await _ejecucionRepository.SaveRangeAsync(registros, cancellationToken);
                }
            }

            return response;
        }

       
        private static List<SiifEjecucionAgregada> MapearDesdeDataArray(JsonElement dataArray, int anioFiscal)
        {
            var lista = new List<SiifEjecucionAgregada>();
            var ahora = DateTime.UtcNow;

            foreach (var dataItem in dataArray.EnumerateArray())
            {
                // Buscar pLista dentro de cada elemento del array
                JsonElement pLista = default;
                bool tieneLista = dataItem.TryGetProperty("pLista", out pLista) ||
                                  dataItem.TryGetProperty("PLista", out pLista);

                if (!tieneLista || pLista.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var item in pLista.EnumerateArray())
                {
                    var entidad = new SiifEjecucionAgregada
                    {
                        Oid        = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                        NrVersion  = 1,
                        BnCreated  = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        FgEnabled  = 1,
                        FgSystem   = 0,
                        BnUpdated  = 0,
                        FechaCarga = ahora,
                        AnioFiscal = anioFiscal
                    };

                    if (item.TryGetProperty("CodPosicionGasto", out var cpg))
                        entidad.PosicionGasto = cpg.GetString();

                    if (item.TryGetProperty("ApropiacionInicial", out var ai) && ai.ValueKind == JsonValueKind.Number)
                        entidad.ApropiacionInicial = ai.GetDecimal();

                    if (item.TryGetProperty("ApropiacionAdicionada", out var aa) && aa.ValueKind == JsonValueKind.Number)
                        entidad.ApropiacionAdicionada = aa.GetDecimal();

                    if (item.TryGetProperty("ApropiacionReducida", out var ar) && ar.ValueKind == JsonValueKind.Number)
                        entidad.ApropiacionReducida = ar.GetDecimal();

                    if (item.TryGetProperty("ApropiacionVigente", out var av) && av.ValueKind == JsonValueKind.Number)
                        entidad.ApropiacionVigente = av.GetDecimal();

                    if (item.TryGetProperty("ApropiacionBloqueada", out var ab) && ab.ValueKind == JsonValueKind.Number)
                        entidad.ApropiacionBloqueada = ab.GetDecimal();

                    if (item.TryGetProperty("ApropiacionDisponible", out var ad) && ad.ValueKind == JsonValueKind.Number)
                        entidad.ApropiacionDisponible = ad.GetDecimal();

                    if (item.TryGetProperty("ValorCDP", out var vcdp) && vcdp.ValueKind == JsonValueKind.Number)
                        entidad.VlCdp = vcdp.GetDecimal();

                    if (item.TryGetProperty("ValorCompromiso", out var vcomp) && vcomp.ValueKind == JsonValueKind.Number)
                        entidad.VlCompromiso = vcomp.GetDecimal();

                    if (item.TryGetProperty("ValorObligacion", out var vobl) && vobl.ValueKind == JsonValueKind.Number)
                        entidad.VlObligacion = vobl.GetDecimal();

                    if (item.TryGetProperty("ValorOrdenPago", out var vop) && vop.ValueKind == JsonValueKind.Number)
                        entidad.VlOrdenPago = vop.GetDecimal();

                    if (item.TryGetProperty("ValorPago", out var vp) && vp.ValueKind == JsonValueKind.Number)
                        entidad.VlPago = vp.GetDecimal();

                    lista.Add(entidad);
                }
            }

            return lista;
        }
    }
}
