using System;
using System.Collections.Generic;

namespace SSF.Interop.SIIFNacion.Domain.Cdp
{
    public class CdpCompromiso
    {
        public long? idcompdetalle { get; set; }
        public long? IdCompromiso { get; set; }
        public string? vigencianm { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string? Estado { get; set; }
        public int? codcdp { get; set; }
        public DateTime? FechaCdp { get; set; }
        public int? codmoneda { get; set; }
        public string? nmmoneda { get; set; }
        public decimal? ValorTasa { get; set; }
        public string? Descripcion { get; set; }
        public decimal? ValorInicial { get; set; }

        public decimal? vliniorimoneda { get; set; }
        public decimal? vltoperacion { get; set; }
        public decimal? ValorActual { get; set; }
        public decimal? SaldoxObligar { get; set; }
        public decimal? saldomoneda { get; set; }

        public string? Objeto { get; set; }

        public string? ttdocumento { get; set; }
        public string? tndocumento { get; set; }
        public string? terceronm { get; set; }

        public string? mediopago { get; set; }

        public string? cuentann { get; set; }
        public string? cuentaentfinan { get; set; }
        public string? CuentaTipo { get; set; }
        public string? CuentaEstado { get; set; }

        public string? ordenadortdoc { get; set; }
        public string? ordenadorndoc { get; set; }
        public string? ordenadornm { get; set; }
        public int? ordenadorconsec { get; set; }
        public string? ordenadorcodcar { get; set; }
        public string? ordenadornmcarg { get; set; }

        public string? cajamenor { get; set; }

        public string? nndocsoporte { get; set; }
        public string? tdocsoporte { get; set; }
        public DateTime? dtdocsoporte { get; set; }

        public DateTime? fechacarga { get; set; }

        // 👇 colección de hijos
        public List<CdpCompromisoItem> Items { get; set; } = new();
        public List<CdpCompromisoPlanPago> PlanesPago { get; set; } = new();

    }

    public class CdpCompromisoItem
    {
        public long? iditem { get; set; }

        public long idcompromiso { get; set; }

        public string? coddepafecta { get; set; }
        public string? nmdepafecta { get; set; }

        public string? codpgasto { get; set; }
        public string? nmpgasto { get; set; }

        public string? codffinan { get; set; }
        public string? nmffinan { get; set; }

        public string? codrpptal { get; set; }
        public string? nmrpptal { get; set; }

        public string? codsfondo { get; set; }
        public string? nmsfondo { get; set; }

        public decimal? vlinicial { get; set; }
        public decimal? vloperaciones { get; set; }
        public decimal? vlactual { get; set; }
        public decimal? saldo { get; set; }

        public DateTime? fechacarga { get; set; }

        // navegación
        public CdpCompromiso Compromiso { get; set; }
    }

    public class CdpCompromisoPlanPago
    {
        public long IdPlanPago { get; set; }

        public long idcompromiso { get; set; }

        public DateTime? FechaPago { get; set; }

        public string? coddepafepac { get; set; }
        public string? nmdepafepac { get; set; }

        public string? codposipac { get; set; }
        public string? nmposipac { get; set; }

        public decimal? Valor { get; set; }
        public decimal? SaldoPorObligar { get; set; }

        public string? codlineapago { get; set; }
        public string? nmlineapago { get; set; }

        public DateTime? FechaCarga { get; set; }

        // 🔗 navegación
        public CdpCompromiso? Compromiso { get; set; }
    }
}