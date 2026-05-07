using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries
{
    public sealed class ConsultarCdpQueryHandler : IRequestHandler<ConsultarCdpQuery, ConsultarCdpResponseDto>
    {
        // Cliente HTTP de alto nivel hacia endpoints presupuestales SIIF (token inyectado por handlers delegados).
        private readonly ISiifBudgetService _siifBudgetService;

        // Abstracci�n de persistencia para tablas de detalle CDP (cabecera expedida + �tems).
        private readonly ICdpRepository _cdpRepository;

        public ConsultarCdpQueryHandler(
            ISiifBudgetService siifBudgetService,
            ICdpRepository cdpRepository)
        {
            _siifBudgetService = siifBudgetService;
            _cdpRepository = cdpRepository;
        }

        public async Task<ConsultarCdpResponseDto> Handle(ConsultarCdpQuery request, CancellationToken cancellationToken)
        {
            // Traducci�n directa de la query a DTOs que entiende la capa Infrastructure.
            var headers = new SiifRequestHeaderDto
            {
                CodPci = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo = request.ConsecutivoHeader,
                Hash = request.HashHeader
            };

            var body = new ConsultarCdpRequestDto
            {
                IdentificacionPCI = request.IdentificacionPCI,
                ConsecutivoCDP = request.ConsecutivoCDP
            };

            // Invocaci�n del endpoint SIIF; la respuesta tipada expone JsonElement para tolerar variaciones menores del payload.
            var response = await _siifBudgetService.ConsultarCdpAsync(body, headers, cancellationToken);

            // Ra�z JSON devuelta por SIIF (puede incluir State, Data, mensajes, etc., seg�n servicio).
            var data = response.Data;

            // Solo intentamos persistir si hay cuerpo JSON utilizable.
            if (data.ValueKind != JsonValueKind.Undefined && data.ValueKind != JsonValueKind.Null)
            {
                // El contrato funcional espera el objeto expedici�n bajo la propiedad "expedicionCDPSal".
                if (data.TryGetProperty("expedicionCDPSal", out JsonElement expedicion))
                {
                    // Timestamp de negocio �nico para todas las filas insertadas en esta respuesta.
                    var ahora = DateTime.UtcNow;

                    // BNCREATED en modelo Gestordoc: milisegundos Unix (alineado a otros handlers).
                    var bnCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    // Campos escalares de la cabecera de expedici�n (nombres JSON fijados por SIIF).
                    var codSolCdp = ParseMonedaSiif(GetString(expedicion, "CodSolCDP"));
                    var consecutivo = ParseMonedaSiif(GetString(expedicion, "Consecutivo"));

                    // Fecha opcional en SIIF: si no viene o es null, dejamos null en entidad.
                    var fechaRegistrada = expedicion.TryGetProperty("FechaRegistrada", out var fr) && fr.ValueKind != JsonValueKind.Null
                        ? fr.GetDateTime()
                        : (DateTime?)null;

                    var codAutoBys = ParseMonedaSiif(GetString(expedicion, "CodAutorizacionByS"));

                    // Entidad de cabecera (una fila por consulta exitosa con expedici�n).
                    var cabecera = new CdpExpedido
                    {
                        Oid = Guid.NewGuid().ToString("N"),
                        NrVersion = 1,
                        BnCreated = bnCreated,
                        FgEnabled = 1,
                        IdCdpDetalle = codSolCdp,
                        NnSolicitudCdp = codSolCdp,
                        Consecutivo = consecutivo,
                        FechaRegistrada = fechaRegistrada,
                        Estado = expedicion.GetProperty("Estado").GetString(),
                        CodAutoBys = codAutoBys,
                        TipoSolCdp = expedicion.GetProperty("TipoSolCDP").GetString(),
                        Descripcion = expedicion.GetProperty("Descripcion").GetString(),
                        VlSaldoCerNc = ParseMonedaSiif(GetString(expedicion, "SaldoCertificadoNC")),
                        VlTotal = ParseMonedaSiif(GetString(expedicion, "Valortotal")),
                        FechaDeCarga = ahora
                    };

                    // Lista de �tems presupuestales asociados al CDP (puede venir vac�a).
                    var items = new List<CdpItem>();

                    // "ItemsCDP" es un arreglo de l�neas de detalle.
                    if (expedicion.TryGetProperty("ItemsCDP", out JsonElement itemsElement) &&
                        itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        var indice = 0;

                        // Cada elemento del arreglo se mapea a una fila hija.
                        foreach (var item in itemsElement.EnumerateArray())
                        {
                            indice++;
                            items.Add(new CdpItem
                            {
                                Oid = Guid.NewGuid().ToString("N"),
                                NrVersion = 1,
                                BnCreated = bnCreated,
                                FgEnabled = 1,
                                IdItemCdp = indice,
                                NnSolicitudCdp = codSolCdp,
                                CodPosicionGasto = GetString(item, "CodPosicionGasto"),
                                NmPosicionGasto = GetString(item, "NomPosicionGasto"),
                                CodFuenteFinan = GetString(item, "CodFuenteFinanciacion"),
                                NmFuenteFinan = GetString(item, "NomFuenteFinanciacion"),
                                CodRecurPptal = GetString(item, "CodRecursoPresupuestal"),
                                NmRecurPptal = GetString(item, "NomRecursoPresupuestal"),
                                CodSituacionFon = GetString(item, "CodSituacionFondos"),
                                NmSituacionFon = GetString(item, "NomSituacionFondos"),
                                CodDepAfecGasto = GetString(item, "CodDepAfectacionGasto"),
                                NmDepAfecGasto = GetString(item, "NomDepAfectacionGasto"),
                                VlItem = ParseMonedaSiif(GetString(item, "ValorItem")),
                                VlSaldoCopromet = ParseMonedaSiif(GetString(item, "SaldoComprometer")),
                                FechaCarga = ahora
                            });
                        }
                    }

                    // Una transacci�n de aplicaci�n (implementada en el repositorio) persiste cabecera + detalle.
                    await _cdpRepository.SaveCdpAsync(cabecera, items, cancellationToken);
                }
            }

            // Devolvemos el DTO de respuesta crudo/estructurado al llamador (API o tests) aunque no haya habido persistencia.
            return response;
        }

        private static string? GetString(JsonElement el, string name) =>
            el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString()
                : (p.ValueKind == JsonValueKind.Number ? p.GetRawText() : null);

        private static decimal ParseMonedaSiif(string? valorMoneda)
        {
            if (string.IsNullOrWhiteSpace(valorMoneda)) return 0m;
            string valorLimpio = valorMoneda.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(valorLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal res))
                return res;
            return 0m;
        }
    }
}
