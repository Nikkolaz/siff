namespace SSF.Interop.SIIFNacion.Worker
{
    /// <summary>
    /// Raíz de opciones del Worker leídas desde configuración (<c>SiifWorker</c> en appsettings).
    /// Agrupa cron, zona horaria y parámetros por tipo de job; los encabezados SIIF van en cada job (<see cref="SiifWorkerCdpJob.Headers"/>, etc.).
    /// </summary>
    public sealed class SiifWorkerOptions
    {
        /// <summary>Nombre de la sección en appsettings.json / variables de entorno.</summary>
        public const string? SectionName = "SiifWorker";

        /// <summary>
        /// Interruptor maestro: en <c>false</c> el <see cref="SiifScheduledIntegrationWorker"/> no programa ejecuciones.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Si es <c>true</c>, al arrancar el proceso se ejecuta un ciclo completo antes de entrar al bucle del cron.
        /// </summary>
        public bool RunOnStartup { get; set; }

        /// <summary>
        /// Expresión cron de 5 campos: minuto, hora, día del mes, mes, día de semana.
        /// Ejemplo <c>0 2 * * *</c> = todos los días a las 02:00 en la zona de <see cref="TimeZoneId"/>.
        /// </summary>
        public string? CronExpression { get; set; } = "0 2 * * *";

        /// <summary>
        /// Identificador de zona horaria del sistema operativo para calcular la “fecha de hoy” del YTD y la ocurrencia del cron.
        /// En Linux suele usarse IANA (ej. <c>America/Bogota</c>); en Windows, nombres como <c>SA Pacific Standard Time</c>.
        /// </summary>
        public string? TimeZoneId { get; set; } = "UTC";

        /// <summary>Parámetros del job <see cref="ConsultarCdpQuery"/> (consulta de un CDP por identificadores).</summary>
        public SiifWorkerCdpJob Cdp { get; set; } = new();

        /// <summary>Parámetros del job <see cref="Application.Features.SIIF.Requests.Queries.ConsultarCdpPaginadaQuery"/> (lista paginada + persistencia).</summary>
        public SiifWorkerCdpPaginadoJob CdpPaginado { get; set; } = new();

        /// <summary>Parámetros del job <see cref="Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones.SincronizarListaObligacionesCommand"/> (sin Vigencia: el orquestador envía 1, 2 y 3).</summary>
        public SiifWorkerObligacionesJob Obligaciones { get; set; } = new();
    }

    /// <summary>
    /// Valores que viajan en cabeceras de las peticiones SIIF (equivalente a los campos *Header de la API REST).
    /// </summary>
    public sealed class SiifWorkerHeaders
    {
        /// <summary>Código PCI en cabecera (trazabilidad SIIF).</summary>
        public string? CodPciHeader { get; set; } = "";

        /// <summary>Usuario técnico o de integración reconocido por SIIF.</summary>
        public string? LoginUsuarioSiifHeader { get; set; } = "";

        /// <summary>Consecutivo de petición; en escenarios paralelos puede requerir unicidad según reglas del entorno SIIF.</summary>
        public string? ConsecutivoHeader { get; set; } = "";

        /// <summary>Hash opcional si el ambiente SIIF lo exige.</summary>
        public string? HashHeader { get; set; }
    }

    /// <summary>Configuración del flujo “consultar CDP” (un solo documento / expedición).</summary>
    public sealed class SiifWorkerCdpJob
    {
        /// <summary>Si es <c>false</c>, no se encola la tarea paralela de CDP en el ciclo.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Encabezados HTTP para el endpoint consultar CDP (pueden diferir de CDP paginado u obligaciones).</summary>
        public SiifWorkerHeaders Headers { get; set; } = new();

        /// <summary>Identificador de PCI en el cuerpo del servicio consultar CDP.</summary>
        public string? IdentificacionPCI { get; set; } = "";

        /// <summary>Consecutivo del CDP a consultar.</summary>
        public string? ConsecutivoCDP { get; set; } = "";
    }

    /// <summary>
    /// Configuración del flujo CDP paginado. Las fechas de registro no se configuran aquí: el orquestador las calcula YTD.
    /// </summary>
    public sealed class SiifWorkerCdpPaginadoJob
    {
        /// <summary>Si es <c>false</c>, se omite la tarea de CDP paginado en el ciclo.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Encabezados HTTP para lista CDP paginada.</summary>
        public SiifWorkerHeaders Headers { get; set; } = new();

        /// <summary>Filtro PCI de consulta (según contrato SIIF).</summary>
        public string? PCIConsulta { get; set; }

        /// <summary>Subunidades PCI (según contrato SIIF).</summary>
        public string? PCISubUnidades { get; set; }

        /// <summary>Tipo de gasto del filtro.</summary>
        public string? TipoGasto { get; set; }

        /// <summary>Rango del filtro.</summary>
        public string? Rango { get; set; }
    }

    /// <summary>
    /// Parte del filtro de obligaciones que es común a las tres ejecuciones (Vigencia 1, 2 y 3).
    /// Fechas: calculadas automáticamente (año actual del 01/01 a hoy).
    /// </summary>
    public sealed class SiifWorkerObligacionesJob
    {
        /// <summary>Si es <c>false</c>, no se lanzan las tres tareas de obligaciones.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Encabezados HTTP para obligaciones paginadas.</summary>
        public SiifWorkerHeaders Headers { get; set; } = new();

        /// <summary>Código PCI del cuerpo LstObliEnt.</summary>
        public string? CodPCI { get; set; } = "";

        /// <summary>Tipo de gasto del filtro.</summary>
        public string? TipoGasto { get; set; } = "";

        /// <summary>Rango del filtro.</summary>
        public string? Rango { get; set; } = "";

        /// <summary>Detalle de usos presupuestales (según contrato SIIF).</summary>
        public string? DetalleUsosPresupuestales { get; set; } = "";
    }
}
