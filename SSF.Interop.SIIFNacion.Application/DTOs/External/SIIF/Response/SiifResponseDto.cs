using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response
{
    public class SiifResponseDto<TData>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TData? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class SIIFCompromisoResponseDto
    {
        // --- Identificadores y fechas principales ---
        [JsonPropertyName("Pci")]
        public string? Pci { get; set; }

        [JsonPropertyName("Codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [JsonPropertyName("Estado")]
        public string? Estado { get; set; }

        [JsonPropertyName("CodigoCdp")]
        public int CodigoCdp { get; set; }

        [JsonPropertyName("FechaCdp")]
        public DateTime FechaCdp { get; set; }

        // --- Moneda ---
        [JsonPropertyName("CodigoMoneda")]
        public int CodigoMoneda { get; set; }

        [JsonPropertyName("NombreMoneda")]
        public string? NombreMoneda { get; set; }

        [JsonPropertyName("ValorTasa")]
        public decimal ValorTasa { get; set; }

        // --- Vigencia y descripción ---
        [JsonPropertyName("Vigencia")]
        public string? Vigencia { get; set; }

        [JsonPropertyName("Descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("Objeto")]
        public string? Objeto { get; set; }

        // --- Valores monetarios ---
        [JsonPropertyName("ValorInicial")]
        public decimal ValorInicial { get; set; }

        [JsonPropertyName("ValorInicialOriginalMoneda")]
        public decimal ValorInicialOriginalMoneda { get; set; }

        [JsonPropertyName("ValorTotalOperacion")]
        public decimal ValorTotalOperacion { get; set; }

        [JsonPropertyName("ValorActual")]
        public decimal ValorActual { get; set; }

        [JsonPropertyName("SaldoPorObligar")]
        public decimal SaldoPorObligar { get; set; }

        [JsonPropertyName("SaldoMoneda")]
        public decimal SaldoMoneda { get; set; }

        // --- Tercero / pagador ---
        [JsonPropertyName("Tercero")]
        public TerceroDto? Tercero { get; set; }

        [JsonPropertyName("MedioPago")]
        public string? MedioPago { get; set; }

        [JsonPropertyName("DetalleCuentaBancaria")]
        public DetalleCuentaBancariaDto? DetalleCuentaBancaria { get; set; }

        [JsonPropertyName("DetalleOrdenadorGasto")]
        public DetalleOrdenadorGastoDto? DetalleOrdenadorGasto { get; set; }

        [JsonPropertyName("CajaMenor")]
        public object? CajaMenor { get; set; }

        [JsonPropertyName("DatosAdministrativos")]
        public DatosAdministrativosDto? DatosAdministrativos { get; set; }

        // ─────────────────────────────────────────────────────────────────────────────
        // SIIF puede devolver los ítems de afectación bajo distintos nombres según la
        // versión del servicio.  Se aceptan TODOS mediante aliases JsonPropertyName:
        //   • "ListadoItemsAfectacion"   — nombre documentado en la guía oficial
        //   • "ItemsCDP"                 — nombre observado en respuestas reales
        //
        // Estrategia: deserializar ambos campos y unirlos en la propiedad consolidada.
        // ─────────────────────────────────────────────────────────────────────────────

        [JsonPropertyName("ListadoItemsAfectacion")]
        public List<ItemAfectacionDto>? ListadoItemsAfectacionRaw { get; set; }

        [JsonPropertyName("ItemsCDP")]
        public List<ItemAfectacionDto>? ItemsCDPRaw { get; set; }

        [JsonIgnore]
        public List<ItemAfectacionDto> ListadoItemsAfectacion
        {
            get
            {
                var lista = new List<ItemAfectacionDto>();
                if (ListadoItemsAfectacionRaw?.Count > 0) lista.AddRange(ListadoItemsAfectacionRaw);
                if (ItemsCDPRaw?.Count > 0) lista.AddRange(ItemsCDPRaw);
                return lista;
            }
            set
            {
                // Permite asignación directa (p. ej. desde el handler para null-safety)
                ListadoItemsAfectacionRaw = value;
            }
        }


        [JsonPropertyName("ListadoPlanesPago")]
        public List<PlanPagoDto>? ListadoPlanesPagoRaw { get; set; }

        [JsonIgnore]
        public List<PlanPagoDto> ListadoPlanesPago
        {
            get => ListadoPlanesPagoRaw ?? new List<PlanPagoDto>();
            set => ListadoPlanesPagoRaw = value;
        }
    }

    public class TerceroDto
    {
        [JsonPropertyName("TipoDocumento")]
        public string? TipoDocumento { get; set; }

        [JsonPropertyName("NumeroDocumento")]
        public string? NumeroDocumento { get; set; }

        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; }
    }

    public class DetalleCuentaBancariaDto
    {
        [JsonPropertyName("Numero")]
        public string? Numero { get; set; }

        [JsonPropertyName("EntidadFinanciera")]
        public string? EntidadFinanciera { get; set; }

        [JsonPropertyName("TipoCuenta")]
        public string? TipoCuenta { get; set; }

        [JsonPropertyName("Estado")]
        public string? Estado { get; set; }
    }

    public class DetalleOrdenadorGastoDto
    {
        [JsonPropertyName("TipoDocumento")]
        public string? TipoDocumento { get; set; }

        [JsonPropertyName("NumeroDocumento")]
        public string? NumeroDocumento { get; set; }

        [JsonPropertyName("Nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("Consecutivo")]
        public int Consecutivo { get; set; }

        [JsonPropertyName("CodigoCargo")]
        public string? CodigoCargo { get; set; }

        [JsonPropertyName("NombreCargo")]
        public string? NombreCargo { get; set; }
    }

    public class DatosAdministrativosDto
    {
        [JsonPropertyName("NumeroDocumentoSoporte")]
        public string? NumeroDocumentoSoporte { get; set; }

        [JsonPropertyName("TipoDocumentoSoporte")]
        public string? TipoDocumentoSoporte { get; set; }

        [JsonPropertyName("Fecha")]
        public DateTime Fecha { get; set; }
    }

    public class ItemAfectacionDto
    {
        [JsonPropertyName("CodigoDependenciaAfectacion")]
        public string? CodigoDependenciaAfectacion { get; set; }

        [JsonPropertyName("NombreDependenciaAfectacion")]
        public string? NombreDependenciaAfectacion { get; set; }

        [JsonPropertyName("CodigoPosicionGasto")]
        public string? CodigoPosicionGasto { get; set; }

        [JsonPropertyName("NombrePosicionGasto")]
        public string? NombrePosicionGasto { get; set; }

        [JsonPropertyName("CodigoFuenteFinanciacion")]
        public string? CodigoFuenteFinanciacion { get; set; }

        [JsonPropertyName("NombreFuenteFinanciacion")]
        public string? NombreFuenteFinanciacion { get; set; }

        [JsonPropertyName("CodigoRecursoPresupuestal")]
        public string? CodigoRecursoPresupuestal { get; set; }

        // Alias adicional que SIIF también usa a veces: "CodRecursoPresupuestal"
        [JsonPropertyName("CodRecursoPresupuestal")]
        public string? CodRecursoPresupuestalAlias
        {
            get => null;
            set { if (!string.IsNullOrWhiteSpace(value)) CodigoRecursoPresupuestal ??= value; }
        }

        [JsonPropertyName("NombreRecursoPresupuestal")]
        public string? NombreRecursoPresupuestal { get; set; }

        [JsonPropertyName("CodigoSituacionFondos")]
        public string? CodigoSituacionFondos { get; set; }

        [JsonPropertyName("NombreSituacionFondos")]
        public string? NombreSituacionFondos { get; set; }

        [JsonPropertyName("ValorInicial")]
        public decimal ValorInicial { get; set; }

        [JsonPropertyName("ValorOperaciones")]
        public decimal ValorOperaciones { get; set; }

        [JsonPropertyName("ValorActual")]
        public decimal ValorActual { get; set; }

        [JsonPropertyName("Saldo")]
        public decimal Saldo { get; set; }

        [JsonPropertyName("ListadoOperaciones")]
        public List<object> ListadoOperaciones { get; set; } = new();
    }

    public class PlanPagoDto
    {
        [JsonPropertyName("FechaPago")]
        public DateTime FechaPago { get; set; }

        [JsonPropertyName("CodigoDependenciaAfectacionPAC")]
        public string? CodigoDependenciaAfectacionPAC { get; set; }

        [JsonPropertyName("NombreDependenciaAfectacionPAC")]
        public string? NombreDependenciaAfectacionPAC { get; set; }

        [JsonPropertyName("CodigoPosicionCatalogoPAC")]
        public string? CodigoPosicionCatalogoPAC { get; set; }

        [JsonPropertyName("NombrePosicionCatalogoPAC")]
        public string? NombrePosicionCatalogoPAC { get; set; }

        [JsonPropertyName("Valor")]
        public decimal Valor { get; set; }

        [JsonPropertyName("SaldoPorObligar")]
        public decimal SaldoPorObligar { get; set; }

        [JsonPropertyName("CodigoLineaPago")]
        public string? CodigoLineaPago { get; set; }

        [JsonPropertyName("NombreLineaPago")]
        public string? NombreLineaPago { get; set; }
    }

  public class ListaCompromisoDto
    {
       public List<ListaCompromisoItemDto> consultaCompromisoSal { get; set; } = new List<ListaCompromisoItemDto>();
    }


    public class ListaCompromisoItemDto
   
    {
        [JsonPropertyName("CodigoPCI")]
        public string? CodigoPCI { get; set; }
        [JsonPropertyName("IdPCI")]
        public int? IdPCI { get; set; }
        [JsonPropertyName("DescripcionPCI")]
        public string? DescripcionPCI { get; set; }
        [JsonPropertyName("CodigoCompromiso")]
        public string? CodigoCompromiso { get; set; }
        [JsonPropertyName("FechaRegistro")]
        public string? FechaRegistro { get; set; }
        [JsonPropertyName("FechaCreacion")]
        public string? FechaCreacion { get; set; }
        [JsonPropertyName("Estado")]
        public string? Estado { get; set; }
        [JsonPropertyName("CodigoDependencia")]
        public string? CodigoDependencia { get; set; }
        [JsonPropertyName("DescripcionDependencia")]
        public string? DescripcionDependencia { get; set; }
        [JsonPropertyName("CodigoPosicionGastos")]
        public string? CodigoPosicionGastos { get; set; }
        [JsonPropertyName("DescripcionPosicionGastos")]
        public string? DescripcionPosicionGastos { get; set; }
        [JsonPropertyName("CodigoFuente")]
        public string? CodigoFuente { get; set; }
        [JsonPropertyName("Fuente")]
        public string? Fuente { get; set; }
        [JsonPropertyName("CodigoSituacion")]
        public string? CodigoSituacion { get; set; }
        [JsonPropertyName("Situacion")]
        public string? Situacion { get; set; }
        [JsonPropertyName("CodigoRecurso")]
        public string? CodigoRecurso { get; set; }
        [JsonPropertyName("Recursos")]
        public string? Recursos { get; set; }
        [JsonPropertyName("ValorInicial")]
        public string? ValorInicial { get; set; }
        [JsonPropertyName("ValorOperaciones")]
        public string? ValorOperaciones { get; set; }
        [JsonPropertyName("ValorActual")]
        public string? ValorActual { get; set; }
        [JsonPropertyName("SaldoUtilizar")]
        public string? SaldoUtilizar { get; set; }
        [JsonPropertyName("TipoIdentificacion")]
        public string? TipoIdentificacion { get; set; }
        [JsonPropertyName("NumeroIdentificacion")]
        public string? NumeroIdentificacion { get; set; }
        [JsonPropertyName("NombreRazonSocial")]
        public string? NombreRazonSocial { get; set; }
        [JsonPropertyName("MedioPago")]
        public string? MedioPago { get; set; }
        [JsonPropertyName("TipoCuenta")]
        public string? TipoCuenta { get; set; }
        [JsonPropertyName("NumeroCuenta")]
        public string? NumeroCuenta { get; set; }
        [JsonPropertyName("EstadoCuenta")]
        public string? EstadoCuenta { get; set; }
        [JsonPropertyName("NitEntidadFinanciera")]
        public string? NitEntidadFinanciera { get; set; }
        [JsonPropertyName("DescripcionEntidadFinanciera")]
        public string? DescripcionEntidadFinanciera { get; set; }
        [JsonPropertyName("SolicitudCDP")]
        public string? SolicitudCDP { get; set; }
        [JsonPropertyName("CDP")]
        public string? CDP { get; set; }
        [JsonPropertyName("CuentasPagar")]
        public string? CuentasPagar { get; set; }
        [JsonPropertyName("Obligaciones")]
        public string? Obligaciones { get; set; }
        [JsonPropertyName("OrdenesPago")]
        public string? OrdenesPago { get; set; }
        [JsonPropertyName("Reintegros")]
        public string? Reintegros { get; set; }
        [JsonPropertyName("FechaDocumentoSoporte")]
        public string? FechaDocumentoSoporte { get; set; }
        [JsonPropertyName("TipoDocumentoSoporte")]
        public string? TipoDocumentoSoporte { get; set; }
        [JsonPropertyName("NumeroDocumentoSoporte")]
        public string? NumeroDocumentoSoporte { get; set; }
         [JsonPropertyName("Observaciones")]
        public string? Observaciones { get; set; }
        
    }

}
