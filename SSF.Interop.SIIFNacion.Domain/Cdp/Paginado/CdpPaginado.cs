using System;

namespace SSF.Interop.SIIFNacion.Domain.Cdp
{
    /// <summary>
    /// Fila de CDP proveniente del listado paginado SIIF; mapea a <c>DYNTBLCDPPAGINADO</c>.
    /// </summary>
    public class CdpPaginado
    {
        /// <summary>Clave primaria OID (GUID string).</summary>
        public string Oid { get; set; } = null!;

        /// <summary>Versi�n de fila Gestordoc.</summary>
        public decimal? NrVersion { get; set; }

        /// <summary>Marca de creaci�n (epoch ms o convenci�n Gestordoc).</summary>
        public decimal? BnCreated { get; set; }

        /// <summary>1 activo; 0 deshabilitado por nueva carga del mismo <see cref="AnioVigencia"/>.</summary>
        public decimal? FgEnabled { get; set; }

        /// <summary>Referencia a revisi�n de formulario (Gestordoc).</summary>
        public string? OidRevisionForm { get; set; }

        /// <summary>Indicador sistema Gestordoc.</summary>
        public decimal? FgSystem { get; set; }

        /// <summary>Marca de actualizaci�n.</summary>
        public decimal? BnUpdated { get; set; }

        /// <summary>Usuario �ltima actualizaci�n.</summary>
        public string? NmUserUpdate { get; set; }

        /// <summary>C�digo de moneda.</summary>
        public string? CodigoMoneda { get; set; }

        /// <summary>Descripci�n moneda.</summary>
        public string? DescripcionMoneda { get; set; }

        /// <summary>Vigencia num�rica seg�n SIIF (columna Vigencia).</summary>
        public int? Vigencia { get; set; }

        /// <summary>A�o de vigencia de la carga; columna <c>ANIOVIGENCIA</c> (alineado a fechas de filtro).</summary>
        public string? AnioVigencia { get; set; }

        /// <summary>Usuario creaci�n registro.</summary>
        public string? UsuarioCreacion { get; set; }

        /// <summary>Fecha creaci�n en sistema origen.</summary>
        public DateTime? FechaCreacionSistema { get; set; }

        /// <summary>Usuario modificaci�n.</summary>
        public string? UsuarioModificacion { get; set; }

        /// <summary>Fecha modificaci�n.</summary>
        public DateTime? FechaModificacion { get; set; }

        /// <summary>Observaciones libres.</summary>
        public string? Observaciones { get; set; }

        /// <summary>Identificador l�gico CDP.</summary>
        public string? IdCdp { get; set; }

        /// <summary>C�digo PCI conexi�n.</summary>
        public string? CodPciConexion { get; set; }

        /// <summary>Descripci�n PCI conexi�n.</summary>
        public string? DescPciConexion { get; set; }

        /// <summary>C�digo subunidad (nombre de columna hist�rica en BD).</summary>
        public string? CodSubNidad { get; set; }

        /// <summary>Valor inicial CDP.</summary>
        public decimal? VlInicial { get; set; }

        /// <summary>Descripci�n subunidad.</summary>
        public string? DescSubUnidad { get; set; }

        /// <summary>N�mero solicitud CDP.</summary>
        public decimal? NnSolicitudCdp { get; set; }

        /// <summary>N�mero documento.</summary>
        public decimal? NnDocumento { get; set; }

        /// <summary>Fecha registro del CDP en SIIF.</summary>
        public DateTime? DtRegistro { get; set; }

        /// <summary>Fecha creaci�n del CDP en SIIF.</summary>
        public DateTime? DtCreacion { get; set; }

        /// <summary>Tipo de CDP.</summary>
        public string? TipoCdp { get; set; }

        /// <summary>Estado del CDP.</summary>
        public string? Estado { get; set; }

        /// <summary>Objeto / texto del CDP.</summary>
        public string? Objeto { get; set; }

        /// <summary>C�digo dependencia afectaci�n.</summary>
        public string? CodDepAfectacio { get; set; }

        /// <summary>Descripci�n dependencia afectaci�n.</summary>
        public string? DesDepAfectacio { get; set; }

        /// <summary>C�digo posici�n del gasto.</summary>
        public string? CodPosGasto { get; set; }

        /// <summary>Descripci�n posici�n del gasto.</summary>
        public string? DesPosGasto { get; set; }

        /// <summary>C�digo fuente.</summary>
        public string? CodFuente { get; set; }

        /// <summary>Descripci�n fuente.</summary>
        public string? DesFuente { get; set; }

        /// <summary>C�digo recurso.</summary>
        public string? CodRecurso { get; set; }

        /// <summary>Descripci�n recurso.</summary>
        public string? DesRecurso { get; set; }

        /// <summary>C�digo situaci�n.</summary>
        public string? CodigoSituacion { get; set; }

        /// <summary>Descripci�n situaci�n.</summary>
        public string? DesSituacion { get; set; }

        /// <summary>Valor operaciones.</summary>
        public decimal? VlOperaciones { get; set; }

        /// <summary>Valor actual.</summary>
        public decimal? VlActual { get; set; }

        /// <summary>Saldo por compromisos.</summary>
        public decimal? SaldoPorComp { get; set; }

        /// <summary>Valor bloqueado.</summary>
        public decimal? VlBloqueado { get; set; }

        /// <summary>Id respuesta asociada.</summary>
        public decimal? IdRespuesta { get; set; }

        /// <summary>Id reintegro.</summary>
        public decimal? IdReintegro { get; set; }

        /// <summary>Id orden de pago.</summary>
        public decimal? IdOrdenPago { get; set; }

        /// <summary>Id obligaci�n.</summary>
        public decimal? IdObligacion { get; set; }

        /// <summary>Id cuenta.</summary>
        public string? IdCuenta { get; set; }

        /// <summary>Lista compromisos serializada.</summary>
        public string? ListaCompromiso { get; set; }

        /// <summary>Lista obligaciones serializada.</summary>
        public string? ListaObligacion { get; set; }

        /// <summary>Lista reintegros serializada.</summary>
        public string? ListaReintegro { get; set; }

        /// <summary>Fecha de carga de integraci�n.</summary>
        public DateTime? FechaCarga { get; set; }

        /// <summary>Listado cuentas por pagar.</summary>
        public string? ListCuentasXpagar { get; set; }

        /// <summary>Listado �rdenes de pago.</summary>
        public string? ListaOrdenDePago { get; set; }
    }
}
