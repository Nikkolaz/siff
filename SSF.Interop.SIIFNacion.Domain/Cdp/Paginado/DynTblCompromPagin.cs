namespace SSF.Interop.SIIFNacion.Domain.Cdp
{
    /// <summary>
    /// Entidad que mapea la tabla GESTORDOC.dbo.DYNTBLCOMPROMPAGIN.
    /// Almacena cada ítem de la lista paginada de compromisos RP devuelta por SIIF
    /// (servicio ConsultaListaCompromisospaginado / WSREPG039C_001).
    /// PK: OID varchar(32) — mismo patrón Gestordoc.
    /// </summary>
    public class DynTblCompromPagin
    {
        // ── Control Gestordoc ────────────────────────────────────────────────────
        public string Oid { get; set; } = default!;
        public decimal NrVersion { get; set; } = 1;
        public long BnCreated { get; set; }
        public decimal FgEnabled { get; set; } = 1;
        public string? OidRevisionForm { get; set; }
        public decimal FgSystem { get; set; } = 0;
        public long BnUpdated { get; set; } = 0;
        public string? NmUserUpdate { get; set; }

        // ── Identificadores del compromiso ───────────────────────────────────────
        /// <summary>IDCOMPROMISO — código texto de la PCI + código compromiso</summary>
        public string? IdCompromiso { get; set; }

        /// <summary>IDPCI — IdPCI numérico devuelto por SIIF</summary>
        public decimal? IdPci { get; set; }

        /// <summary>DESCRIPCIONPCI — Descripción de la PCI</summary>
        public string? DescripcionPci { get; set; }

        /// <summary>CODCOMPROMISO — CodigoCompromiso numérico</summary>
        public decimal? CodCompromiso { get; set; }

        /// <summary>VIGENCIACOD — código de vigencia: "1", "2", "3"</summary>
        public string? VigenciaCod { get; set; }

        /// <summary>VIGENCIANM — descripción de la vigencia: "Actual", "Reserva Presupuestal"</summary>
        public string? VigenciaNm { get; set; }

        // ── Fechas ───────────────────────────────────────────────────────────────
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaCreacion { get; set; }

        // ── Estado ───────────────────────────────────────────────────────────────
        public string? Estado { get; set; }

        // ── Clasificación presupuestal ───────────────────────────────────────────
        public string? CodDependencia { get; set; }
        public string? DescripcionDep { get; set; }
        public string? CodPGastos { get; set; }
        public string? DesPGastos { get; set; }
        public string? CodFuente { get; set; }
        public string? Fuente { get; set; }
        public string? CodSituacion { get; set; }
        public string? Situacion { get; set; }
        public string? CodRecurso { get; set; }
        public string? Recursos { get; set; }

        // ── Valores monetarios ───────────────────────────────────────────────────
        public decimal? VlInicial { get; set; }
        public decimal? VlOperaciones { get; set; }
        public decimal? VlActual { get; set; }
        public decimal? SaldoUtilizar { get; set; }

        // ── Tercero / beneficiario ────────────────────────────────────────────────
        public string? TipoIdentificac { get; set; }
        public string? NnIdentificacio { get; set; }
        public string? NmRazonSocial { get; set; }

        // ── Pago ─────────────────────────────────────────────────────────────────
        public string? MedioPago { get; set; }
        public string? TipoCuenta { get; set; }
        public string? NnCuenta { get; set; }
        public string? EstadoCuenta { get; set; }
        public string? NitEntFinan { get; set; }
        public string? DesEntFinan { get; set; }

        // ── Referencias cruzadas ─────────────────────────────────────────────────
        public decimal? CodCdp { get; set; }
        public string? CuentasPagar { get; set; }
        public string? Obligaciones { get; set; }
        public string? OrdenesPago { get; set; }
        public string? Reintegros { get; set; }

        // ── Documento soporte ─────────────────────────────────────────────────────
        public DateTime? FechaDocSoporte { get; set; }
        public string? TipoDocSoporte { get; set; }
        public string? NumeroDocSoport { get; set; }
        public string? Observaciones { get; set; }

        // ── Control de carga ─────────────────────────────────────────────────────
        public DateTime? FechaCarga { get; set; }

        /// <summary>ANIOVIGENCIA — año fiscal de la consulta (p.ej. "2025")</summary>
        public string? AnioVigencia { get; set; }
    }
}