using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Common.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.Exceptions;
using SSF.Interop.SIIFNacion.Domain.Siif;

namespace SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones
{
    /// <summary>
    /// Handler MediatR: recorre todas las páginas del servicio paginado de obligaciones SIIF y persiste en <c>DYNTBLSIIFOBLIGAPO</c> con auditoría.
    /// </summary>
    public sealed class SincronizarListaObligacionesCommandHandler
        : IRequestHandler<SincronizarListaObligacionesCommand, SincronizarListaObligacionesResultDto>
    {
        // SIIF admite como máximo 50 registros por página en este endpoint; el bucle avanza página a página hasta cubrir Total.
        public const int ObligacionesPageSizeMax = 50;

        // Cliente de integración presupuestal (HTTP + token).
        private readonly ISiifBudgetService _siifBudgetService;

        // Persistencia masiva y deshabilitación por vigencia en tabla de obligaciones.
        private readonly IObligacionApoRepository _repository;

        // Registro best-effort en DYNEFWAUDITORIA (no debe tumbar el flujo principal).
        private readonly IAuditoriaLogger _audit;

        /// <summary>Constructor con dependencias típicas del flujo SIIF + BD.</summary>
        public SincronizarListaObligacionesCommandHandler(
            ISiifBudgetService siifBudgetService,
            IObligacionApoRepository repository,
            IAuditoriaLogger audit)
        {
            _siifBudgetService = siifBudgetService;
            _repository = repository;
            _audit = audit;
        }

        /// <summary>
        /// Ejecuta la sincronización página a página; valida año único; deshabilita por <c>ANIOVIGENCIA</c> y opcionalmente <c>VIGENCIACOD</c>.
        /// </summary>
        public async Task<SincronizarListaObligacionesResultDto> Handle(
            SincronizarListaObligacionesCommand request,
            CancellationToken cancellationToken)
        {
            // Correlación de auditoría para todas las líneas generadas en esta ejecución del comando.
            var idTramite = Guid.NewGuid().ToString();

            // Usuario que se escribe en auditoría y en columnas de negocio cuando aplica.
            var usuario = request.LoginUsuarioSiifHeader;

            // Cronómetro de muro de todo el comando (todas las páginas y esperas de red).
            var totalSw = Stopwatch.StartNew();
            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.InicioFlujo,
                "Inicio sincronización de obligaciones SIIF.",
                peticion: null,
                respuesta: null,
                tiempoRq: null,
                tiempoRs: null,
                estadoFinal: null,
                nmUserUpdate: usuario,
                cancellationToken);

            // Paso 1: Armar headers HTTP que exige SIIF (codPCI, loginUsuarioSIIF, consecutivo, hash opcional).
            var headers = new SiifRequestHeaderDto
            {
                CodPci = request.CodPciHeader,
                LoginUsuarioSiif = request.LoginUsuarioSiifHeader,
                Consecutivo = request.ConsecutivoHeader,
                Hash = request.HashHeader
            };

            // Paso 2: Filtro de negocio (mismo para todas las páginas de la misma sincronización).
            var filtro = new ListaObligacionesFiltroDto
            {
                CodPCI = request.CodPCI,
                FechaInicio = request.FechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = request.FechaFin.ToString("yyyy-MM-dd"),
                TipoGasto = request.TipoGasto,
                Rango = request.Rango,
                Vigencia = request.Vigencia,
                DetalleUsosPresupuestales = request.DetalleUsosPresupuestales
            };

            // Paso 3: Vigencia única por año calendario (ANIOVIGENCIA) a partir del rango FechaInicio / FechaFin.
            string anioVigencia;
            try
            {
                anioVigencia = SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateRange(
                    request.FechaInicio, request.FechaFin);
            }
            catch (Exception ex)
            {
                await _audit.ErrorAsync(
                    idTramite,
                    SiifPuntoDeControl.ValidacionRespuesta,
                    "Validación de vigencia falló (rango de fechas inválido).",
                    peticion: JsonSerializer.Serialize(filtro),
                    respuesta: null,
                    tiempoRq: null,
                    tiempoRs: null,
                    estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                    nmUserUpdate: usuario,
                    exception: ex,
                    cancellationToken);
                throw;
            }

            // Valores de auditoría / carga comunes a todas las filas insertadas en este lote lógico (misma corrida del comando).
            var bnCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var fechaCarga = DateTime.UtcNow;
            // Vigencia del filtro se persiste en columna VIGENCIACOD de cada fila.
            var vigenciaCod = string.IsNullOrWhiteSpace(request.Vigencia) ? null : request.Vigencia.Trim();

            var totalInsertados = 0;
            var paginas = 0;
            // Total devuelto por SIIF en Data.Total (se lee en la primera respuesta válida).
            int? totalSiif = null;
            var page = 1;
            var deshabilitoPreviosDeEstaVigencia = false;
            var iteraciones = 0;
            var totalMsConsumoSiif = 0L;

            // Paso 4: Bucle de paginación — una llamada HTTP por página hasta agotar registros o cumplir Total.
            try
            {
                while (true)
                {
                    // 4a: Cuerpo del POST con página actual y tamaño fijo (50).
                    var body = new ConsultaListaObligacionesPaginadaRequestDto
                    {
                        Pagination = new PaginationDto { Page = page, Size = ObligacionesPageSizeMax },
                        LstObliEnt = filtro
                    };

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.EnvioRequestSiif,
                        $"Envío request SIIF obligaciones (page={page}, size={ObligacionesPageSizeMax}, anioVigencia={anioVigencia}).",
                        peticion: JsonSerializer.Serialize(body),
                        respuesta: null,
                        tiempoRq: null,
                        tiempoRs: null,
                        estadoFinal: null,
                        nmUserUpdate: usuario,
                        cancellationToken);

                    var siifSw = Stopwatch.StartNew();
                    // 4b: Llamada al servicio externo SIIF (Infrastructure adapta State/Code/Message a Success).
                    var response = await _siifBudgetService.ConsultarListaObligacionesPaginadaAsync(
                        body, headers, cancellationToken);
                    siifSw.Stop();
                    iteraciones++;
                    totalMsConsumoSiif += siifSw.ElapsedMilliseconds;

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.RespuestaSiifRecibida,
                        $"Respuesta recibida SIIF obligaciones (page={page}). success={response.Success}.",
                        peticion: null,
                        respuesta: response.Data.ToString(),
                        tiempoRq: null,
                        tiempoRs: $"{siifSw.ElapsedMilliseconds} ms",
                        estadoFinal: null,
                        nmUserUpdate: usuario,
                        cancellationToken);

                    // 4c: Si la respuesta no es exitosa, no continuar paginando.
                    if (!response.Success)
                    {
                        await _audit.ErrorAsync(
                            idTramite,
                            SiifPuntoDeControl.ValidacionRespuesta,
                            $"SIIF no devolvió éxito al consultar obligaciones paginadas (página {page}). {response.Message}",
                            peticion: JsonSerializer.Serialize(body),
                            respuesta: response.Data.ToString(),
                            tiempoRq: null,
                            tiempoRs: $"{siifSw.ElapsedMilliseconds} ms",
                            estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                            nmUserUpdate: usuario,
                            exception: null,
                            cancellationToken);
                        throw new BadRequestException(
                            $"SIIF no devolvió éxito al consultar obligaciones paginadas (página {page}). {response.Message}");
                    }

                    // 4d: response.Data es el JSON raíz completo { State, Code, Message, Data, ... }.
                    var root = response.Data;
                    if (root.ValueKind != JsonValueKind.Object)
                        break;

                    // 4e: Dentro del raíz, el payload útil está en la propiedad "Data" (objeto con Total y ListaObligacionesSal).
                    if (!root.TryGetProperty("Data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Object)
                        break;

                    // 4f: Leer Total una sola vez para saber cuántas páginas faltan (page * tamaño >= Total).
                    if (totalSiif is null && dataEl.TryGetProperty("Total", out var totalEl) &&
                        totalEl.ValueKind == JsonValueKind.Number)
                    {
                        totalSiif = totalEl.TryGetInt32(out var i) ? i : (int)totalEl.GetInt64();
                    }

                    // 4g: Array de obligaciones de la página actual.
                    if (!dataEl.TryGetProperty("ListaObligacionesSal", out var listaEl) ||
                        listaEl.ValueKind != JsonValueKind.Array)
                        break;

                    var count = listaEl.GetArrayLength();
                    // 4h: Sin ítems en esta página → fin del proceso (no hay más datos).
                    if (count == 0)
                        break;

                    // Solo si hay datos de negocio: deshabilitar cargas anteriores de la misma ANIOVIGENCIA (una vez por corrida).
                    if (!deshabilitoPreviosDeEstaVigencia)
                    {
                        var swBd = Stopwatch.StartNew();
                        await _repository.DisableByAnioVigenciaAsync(anioVigencia, vigenciaCod, cancellationToken);
                        swBd.Stop();
                        deshabilitoPreviosDeEstaVigencia = true;

                        await _audit.InfoAsync(
                            idTramite,
                            SiifPuntoDeControl.ActualizacionBd,
                            $"Deshabilitación por vigencia completada (anioVigencia={anioVigencia}, vigenciaCod={(vigenciaCod ?? "(todas)")}).",
                            peticion: null,
                            respuesta: null,
                            tiempoRq: null,
                            tiempoRs: $"{swBd.ElapsedMilliseconds} ms",
                            estadoFinal: null,
                            nmUserUpdate: usuario,
                            cancellationToken);
                    }

                    // 4i: Mapear cada elemento JSON a entidad de dominio lista para EF.
                    var rows = new List<SiifObligacionApo>(count);
                    foreach (var item in listaEl.EnumerateArray())
                    {
                        rows.Add(MapItem(item, vigenciaCod, anioVigencia, bnCreated, fechaCarga));
                    }

                    // 4j: Persistir solo esta página en BD (evita acumular miles de entidades en memoria).
                    var swInsert = Stopwatch.StartNew();
                    await _repository.AddRangeAsync(rows, cancellationToken);
                    await _repository.SaveChangesAsync(cancellationToken);
                    swInsert.Stop();

                    await _audit.InfoAsync(
                        idTramite,
                        SiifPuntoDeControl.InsercionBd,
                        $"Inserción BD obligaciones completada (page={page}, count={count}).",
                        peticion: null,
                        respuesta: null,
                        tiempoRq: null,
                        tiempoRs: $"{swInsert.ElapsedMilliseconds} ms",
                        estadoFinal: null,
                        nmUserUpdate: usuario,
                        cancellationToken);

                    totalInsertados += count;
                    paginas++;

                    // 4k: Criterio de parada — ya se alcanzó el total de registros que reportó SIIF.
                    if (totalSiif.HasValue && page * ObligacionesPageSizeMax >= totalSiif.Value)
                        break;

                    // 4l: Última página “corta” (menos de 50) → no hay página siguiente con datos.
                    if (count < ObligacionesPageSizeMax)
                        break;

                    // 4m: Siguiente página.
                    page++;
                }
            }
            catch (Exception ex)
            {
                totalSw.Stop();
                await _audit.ErrorAsync(
                    idTramite,
                    SiifPuntoDeControl.FinError,
                    $"Fin flujo obligaciones con error. paginas={paginas}, insertados={totalInsertados}, iteraciones={iteraciones}, msSiif={totalMsConsumoSiif}, msTotal={totalSw.ElapsedMilliseconds}.",
                    peticion: null,
                    respuesta: null,
                    tiempoRq: null,
                    tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                    estadoFinal: SiifAuditoriaEstadoFinal.Fallido,
                    nmUserUpdate: usuario,
                    exception: ex,
                    cancellationToken);
                throw;
            }

            totalSw.Stop();
            await _audit.InfoAsync(
                idTramite,
                SiifPuntoDeControl.FinExitoso,
                $"Fin flujo obligaciones exitoso. paginas={paginas}, insertados={totalInsertados}, iteraciones={iteraciones}, msSiif={totalMsConsumoSiif}, msTotal={totalSw.ElapsedMilliseconds}.",
                peticion: null,
                respuesta: null,
                tiempoRq: null,
                tiempoRs: $"{totalSw.ElapsedMilliseconds} ms",
                estadoFinal: SiifAuditoriaEstadoFinal.Exitoso,
                nmUserUpdate: usuario,
                cancellationToken);

            // Paso 5: Resumen para el cliente HTTP (totales y páginas recorridas).
            return new SincronizarListaObligacionesResultDto
            {
                TotalReportadoPorSiif = totalSiif ?? totalInsertados,
                RegistrosInsertados = totalInsertados,
                PaginasProcesadas = paginas
            };
        }

        /// <summary>
        /// Convierte un elemento de <c>ListaObligacionesSal</c> (JSON) en <see cref="SiifObligacionApo"/> lista para <c>AddRange</c>.
        /// </summary>
        private static SiifObligacionApo MapItem(
            JsonElement item,
            string? vigenciaCod,
            string anioVigencia,
            long bnCreated,
            DateTime fechaCarga)
        {
            return new SiifObligacionApo
            {
                // PK técnica y columnas de auditoría estándar del modelo Gestordoc.
                Oid = Guid.NewGuid().ToString("N"),
                NrVersion = 1,
                BnCreated = bnCreated,
                FgEnabled = 1,
                AnioVigencia = anioVigencia,
                // Identificadores numéricos: se extraen dígitos de strings que pueden traer espacios u otros caracteres.
                //IdObligacionPosicio = TryParseDecimalLong(TrimmedString(item, "OrdenesPago")),
                IdObliga = TryParseDecimalLong(TrimmedString(item, "Obligaciones")),
                CodObligacion = TryParseDecimalLong(TrimmedString(item, "CodigoObligacion")),
                VigenciaCod = vigenciaCod,
                // Dependencia: nombre JSON puede variar (tilde); se prueba ambas variantes.
                CodDepAfectacio = TrimmedString(item, "DescripcionDependenciaAfectación")
                    ?? TrimmedString(item, "DescripcionDependenciaAfectacion"),
                DesDepAfectacio = TrimmedString(item, "DescripcionDependenciaAfectación")
                    ?? TrimmedString(item, "DescripcionDependenciaAfectacion"),
                CodPosicionGast = TrimmedString(item, "CodigoPosicionGasto"),
                DesPosicionGast = TrimmedString(item, "DescripcionPosicionGasto"),
                CodFuenteFinan = TrimmedString(item, "CodigoFuenteFinanciacion"),
                DesFuenteFinan = TrimmedString(item, "DescripcionFuenteFinanciacion"),
                CodRecursoPpal = TrimmedString(item, "CodRecursoPresupuestal"),
                DesRecursoPpal = TrimmedString(item, "DescripcionRecursoPresupuestal"),
                CodSituacionFon = TrimmedString(item, "CodigoSituacionFondos"),
                DesSituacionFon = TrimmedString(item, "DescripcionSituacionFondos"),
                // Montos: pueden venir como número JSON o como string con separador de miles (coma).
                VlInicialPosicion = ParseMoney(item, "ValorInicialPosicion"),
                VlOperaciones = ParseMoney(item, "ValorOperaciones"),
                VlActualPosicio = ParseMoney(item, "ValorActualPosicion"),
                SaldoXUtilizar = ParseMoney(item, "SaldoxUtilizar"),
                FechaCarga = fechaCarga
            };
        }

        /// <summary>Lee propiedad string del JSON y hace trim; null si no aplica.</summary>
        private static string? TrimmedString(JsonElement el, string name) =>
            el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
                ? p.GetString()?.Trim()
                : null;

        /// <summary>Extrae dígitos del texto y parsea a decimal (IDs con basura alrededor).</summary>
        private static decimal? TryParseDecimalLong(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            var cleaned = new string(s.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(cleaned))
                return null;
            return decimal.TryParse(cleaned, NumberStyles.None, CultureInfo.InvariantCulture, out var d)
                ? d
                : null;
        }

        /// <summary>Obtiene un importe desde número JSON o string con separadores de miles.</summary>
        private static decimal? ParseMoney(JsonElement el, string name)
        {
            if (!el.TryGetProperty(name, out var p))
                return null;

            if (p.ValueKind == JsonValueKind.Number)
                return p.GetDecimal();

            if (p.ValueKind == JsonValueKind.String)
                return ParseMoneyString(p.GetString());

            return null;
        }

        /// <summary>Normaliza string monetario y parsea en cultura invariante.</summary>
        private static decimal? ParseMoneyString(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            var cleaned = new string(s.Where(c => c != ',').ToArray()).Trim();
            return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                ? d
                : null;
        }
    }
}
