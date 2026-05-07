namespace SSF.Interop.SIIFNacion.Application.Common.Auditoria
{
    public static class SiifAuditoriaNivel
    {
        /// <summary>Traza informativa (flujo normal).</summary>
        public const string Info = "INFO";

        /// <summary>Advertencia: situación anómala pero no necesariamente fallo.</summary>
        public const string Warn = "WARN";

        /// <summary>Error: fallo de validación, SIIF o excepción no controlada en el flujo.</summary>
        public const string Error = "ERROR";
    }

    /// <summary>
    /// Valores para columna ESTADO_FINAL: resultado de negocio del tramo auditado.
    /// </summary>
    public static class SiifAuditoriaEstadoFinal
    {
        /// <summary>El trámite o flujo terminó correctamente.</summary>
        public const string Exitoso = "EXITOSO";

        /// <summary>Fallo explícito (validación, SIIF, BD, etc.).</summary>
        public const string Fallido = "FALLIDO";

        /// <summary>Parte del trabajo OK y parte no (p. ej. worker con varios jobs en paralelo).</summary>
        public const string Parcial = "PARCIAL";
    }

    /// <summary>
    /// Valores estándar de PUNTODECONTROL en auditoría: permiten filtrar en BD por etapa del pipeline.
    /// </summary>
    public static class SiifPuntoDeControl
    {
        /// <summary>Primera línea al entrar a un handler de integración SIIF.</summary>
        public const string InicioFlujo = "INICIO_FLUJO_SIIF";

        /// <summary>Construcción del DTO de petición antes del HTTP (si aplica).</summary>
        public const string ConstruccionRequest = "CONSTRUCCION_REQUEST";

        /// <summary>Justo antes de enviar la petición HTTP a SIIF.</summary>
        public const string EnvioRequestSiif = "ENVIO_REQUEST_SIIF";

        /// <summary>Respuesta HTTP recibida (cuerpo o metadatos relevantes).</summary>
        public const string RespuestaSiifRecibida = "RESPUESTA_SIIF_RECIBIDA";

        /// <summary>Validación de State/Code/Message o reglas de negocio sobre la respuesta.</summary>
        public const string ValidacionRespuesta = "VALIDACION_RESPUESTA";

        /// <summary>Operación de actualización masiva en BD (p. ej. FGENABLED = 0 por vigencia).</summary>
        public const string ActualizacionBd = "ACTUALIZACION_BD";

        /// <summary>Inserción de nuevas filas (lote o página).</summary>
        public const string InsercionBd = "INSERCION_BD";

        /// <summary>Cierre exitoso del handler.</summary>
        public const string FinExitoso = "FIN_FLUJO_EXITOSO";

        /// <summary>Cierre con error registrado.</summary>
        public const string FinError = "FIN_FLUJO_ERROR";

        /// <summary>Inicio de un ciclo completo del Worker (antes del paralelismo de jobs).</summary>
        public const string WorkerCicloInicio = "WORKER_SIIF_CICLO_INICIO";

        /// <summary>Fin de ciclo del Worker con resumen de tiempos y estado agregado.</summary>
        public const string WorkerCicloFin = "WORKER_SIIF_CICLO_FIN";

        /// <summary>Inicio de un job individual dentro del Worker (CDP, CDP paginado, obligaciones por vigencia).</summary>
        public const string WorkerFlujoInicio = "WORKER_SIIF_FLUJO_INICIO";

        /// <summary>Fin exitoso de un job individual del Worker.</summary>
        public const string WorkerFlujoFin = "WORKER_SIIF_FLUJO_FIN";

        /// <summary>Error aislado en un job del Worker (los demás jobs paralelos pueden haber continuado).</summary>
        public const string WorkerFlujoError = "WORKER_SIIF_FLUJO_ERROR";
    }
}
