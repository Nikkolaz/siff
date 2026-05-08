using System;
using System.Collections.Generic;

namespace SSF.Interop.SIIFNacion.Domain.Cdp
{
    public class CdpExpedido
    {
        public string Oid { get; set; } = default!;

        /// <summary>NRVERSION: constante 1 al insertar.</summary>
        public decimal NrVersion { get; set; } = 1;

        /// <summary>BNCREATED: marca de tiempo (numeric en BD).</summary>
        public long BnCreated { get; set; }

        /// <summary>FGENABLED: constante 1 al insertar.</summary>
        public decimal FgEnabled { get; set; } = 1;

        public decimal? IdCdpDetalle { get; set; }
        public decimal? NnSolicitudCdp { get; set; }
        public decimal? Consecutivo { get; set; }
        public DateTime? FechaRegistrada { get; set; }
        public string? Estado { get; set; }
        public decimal? CodAutoBys { get; set; }
        public string? TipoSolCdp { get; set; }
        public string? Descripcion { get; set; }
        public decimal? VlSaldoCerNc { get; set; }
        public decimal? VlTotal { get; set; }

        /// <summary>FECHADECARGA: fecha actual del sistema al insertar.</summary>
        public DateTime FechaDeCarga { get; set; }

        public ICollection<CdpItem> Items { get; set; } = new List<CdpItem>();
    }
    
    /// <summary>
    /// Ítem de CDP persistido en <c>DYNTBLITEMCDP</c>.
    /// </summary>
    public class CdpItem
    {
        public string Oid { get; set; } = default!;

        public decimal NrVersion { get; set; } = 1;
        public long BnCreated { get; set; }
        public decimal FgEnabled { get; set; } = 1;

        public decimal? IdItemCdp { get; set; }
        public decimal? NnSolicitudCdp { get; set; }

        public string? CodPosicionGasto { get; set; }
        public string? NmPosicionGasto { get; set; }
        public string? CodFuenteFinan { get; set; }
        public string? NmFuenteFinan { get; set; }
        public string? CodRecurPptal { get; set; }
        public string? NmRecurPptal { get; set; }
        public string? CodSituacionFon { get; set; }
        public string? NmSituacionFon { get; set; }
        public string? CodDepAfecGasto { get; set; }
        public string? NmDepAfecGasto { get; set; }

        public decimal? VlItem { get; set; }
        public decimal? VlSaldoCopromet { get; set; }

        public DateTime FechaCarga { get; set; }
    }

}

