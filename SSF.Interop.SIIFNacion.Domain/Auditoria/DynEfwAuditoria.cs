using System;

namespace SSF.Interop.SIIFNacion.Domain.Auditoria
{
    /// <summary>
    /// Entidad de dominio que refleja una fila de <c>GESTORDOC.dbo.DYNEFWAUDITORIA</c> (trazas de integraciones).
    /// </summary>
    public sealed class DynEfwAuditoria
    {
        /// <summary>Clave primaria OID.</summary>
        public string Oid { get; set; } = default!;

        /// <summary>Versión de fila Gestordoc.</summary>
        public decimal? NrVersion { get; set; }

        /// <summary>Marca de creación (epoch ms).</summary>
        public long? BnCreated { get; set; }

        /// <summary>1 = registro activo de auditoría.</summary>
        public decimal? FgEnabled { get; set; }

        /// <summary>Referencia opcional a revisión de formulario.</summary>
        public string? OidRevisionForm { get; set; }

        /// <summary>Indicador sistema.</summary>
        public decimal? FgSystem { get; set; }

        /// <summary>Marca de actualización.</summary>
        public long? BnUpdated { get; set; }

        /// <summary>Usuario que originó o está asociado al evento.</summary>
        public string? NmUserUpdate { get; set; }

        /// <summary>Identificador numérico de transacción (convención Gestordoc).</summary>
        public decimal? IdTransaccion { get; set; }

        /// <summary>Identificador de trámite / correlación libre (GUID de flujo, worker, etc.).</summary>
        public string? IdTramite { get; set; }

        /// <summary>Texto descriptivo del evento (truncado al persistir si aplica).</summary>
        public string? Mensaje { get; set; }

        /// <summary>Payload de petición serializado (sensible enmascarado en capa de aplicación).</summary>
        public string? Peticion { get; set; }

        /// <summary>Payload de respuesta serializado (sensible enmascarado).</summary>
        public string? Respuesta { get; set; }

        /// <summary>Tiempo de request o metadato temporal de ida (texto libre).</summary>
        public string? TiempoRq { get; set; }

        /// <summary>Tiempo de respuesta o duración (p. ej. "123 ms").</summary>
        public string? TiempoRs { get; set; }

        /// <summary>Nivel: INFO, WARN, ERROR (ver <c>SiifAuditoriaNivel</c>).</summary>
        public string? Nivel { get; set; }

        /// <summary>Texto de excepción si hubo error.</summary>
        public string? Excepcion { get; set; }

        /// <summary>Etapa del flujo (ver <c>SiifPuntoDeControl</c>).</summary>
        public string? PuntoDeControl { get; set; }

        /// <summary>Nombre de aplicación emisora del log.</summary>
        public string? Aplicacion { get; set; }

        /// <summary>Campo CCF / contexto adicional según negocio.</summary>
        public string? Ccf { get; set; }

        /// <summary>Estado final del tramo: EXITOSO, FALLIDO, PARCIAL, etc.</summary>
        public string? EstadoFinal { get; set; }
    }
}
