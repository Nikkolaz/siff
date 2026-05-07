namespace SSF.Interop.SIIFNacion.Domain.Cdp
{
    /// <summary>
    /// Entidad que mapea la tabla DYNTBLCCOMPPTAL (nueva versión producción de tblccompptal).
    /// PK: OID varchar(32) — mismo patrón Gestordoc que el resto del sistema.
    /// </summary>
    public class DynTblCCompPtal
    {
        public string Oid { get; set; } = default!;
        public decimal NrVersion { get; set; } = 1;
        public long BnCreated { get; set; }
        public decimal FgEnabled { get; set; } = 1;
        public string? OidRevisionForm { get; set; }
        public decimal FgSystem { get; set; } = 0;
        public long BnUpdated { get; set; } = 0;
        public string? NmUserUpdate { get; set; }

        // 🔑 Identificadores del compromiso
        public decimal? IdCompDetalle { get; set; }
        public decimal? IdCompromiso { get; set; }

        // 📅 Datos principales
        public string? VigenciaNm { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string? Estado { get; set; }
        public decimal? CodCdp { get; set; }
        public DateTime? FechaCdp { get; set; }

        // 💱 Moneda
        public decimal? CodMoneda { get; set; }
        public string? NmMoneda { get; set; }
        public decimal? ValorTasa { get; set; }

        // 📝 Texto
        public string? Descripcion { get; set; }
        public string? Objeto { get; set; }

        // 💵 Valores
        public decimal? ValorInicial { get; set; }
        public decimal? VlIniOriMoneda { get; set; }
        public decimal? VlTOperacion { get; set; }
        public decimal? ValorActual { get; set; }
        public decimal? SaldoXObligar { get; set; }
        public decimal? SaldoMoneda { get; set; }

        // 📄 Documento
        public string? TtDocumento { get; set; }
        public string? TnDocumento { get; set; }

        // 👤 Tercero
        public string? TerceroNm { get; set; }

        // 💳 Pago
        public string? MedioPago { get; set; }

        // 🏦 Cuenta bancaria
        public string? CuentaNn { get; set; }
        public string? CuentaEntFinan { get; set; }
        public string? CuentaTipo { get; set; }
        public string? CuentaEstado { get; set; }

        // 🧑‍💼 Ordenador del gasto
        public string? OrdenadorTDoc { get; set; }
        public string? OrdenadorNDoc { get; set; }
        public string? OrdenadorNm { get; set; }
        public decimal? OrdenadorConsec { get; set; }
        public string? OrdenadorCodCar { get; set; }
        public string? OrdenadorNmCarg { get; set; }

        // 🧾 Otros
        public string? CajaMenor { get; set; }
        public string? NnDocSoporte { get; set; }
        public string? TDocSoporte { get; set; }
        public DateTime? DtDocSoporte { get; set; }

        // ⏱️ Control
        public DateTime? FechaCarga { get; set; }
        public string? AnioVigencia { get; set; }

        // 🔗 Colección de ítems (relación 1:N)
        public List<DynTblListItemsAfe> Items { get; set; } = new();
    }

    /// <summary>
    /// Entidad que mapea la tabla DYNTBLLISTITEMSAFE (nueva versión producción de tblccompptal_item).
    /// </summary>
    public class DynTblListItemsAfe
    {
        public string Oid { get; set; } = default!;
        public decimal NrVersion { get; set; } = 1;
        public long BnCreated { get; set; }
        public decimal FgEnabled { get; set; } = 1;
        public string? OidRevisionForm { get; set; }
        public decimal FgSystem { get; set; } = 0;
        public long BnUpdated { get; set; } = 0;
        public string? NmUserUpdate { get; set; }

        // 🔑 Identificadores
        public decimal? IdItem { get; set; }
        public decimal? IdCompromiso { get; set; }// FK → DynTblCCompPtal.Oid

        // 🏢 Dependencia
        public string? CodDepAfecta { get; set; }
        public string? NmDepAfecta { get; set; }

        // 💸 Posición de gasto
        public string? CodPGasto { get; set; }
        public string? NmPGasto { get; set; }

        // 🏦 Fuente de financiación
        public string? CodFFinan { get; set; }
        public string? NmFFinan { get; set; }

        // 📊 Recurso presupuestal
        public string? CodRPPtal { get; set; }
        public string? NmRPPtal { get; set; }

        // 💰 Situación de fondos
        public string? CodSFondo { get; set; }
        public string? NmSFondo { get; set; }

        // 💵 Valores
        public decimal? VlInicial { get; set; }
        public decimal? VlOperaciones { get; set; }
        public decimal? VlActual { get; set; }
        public decimal? Saldo { get; set; }

        // ⏱️ Control
        public DateTime? FechaCarga { get; set; }
        public string? AnioVigencia { get; set; }
    }
}
