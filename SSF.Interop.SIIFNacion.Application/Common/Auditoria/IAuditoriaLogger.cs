using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Application.Common.Auditoria
{
    public interface IAuditoriaLogger
    {
        /// <param name="idTramite">Correlación del proceso (GUID, id ciclo worker, etc.).</param>
        /// <param name="puntoDeControl">Constante de <see cref="SiifPuntoDeControl"/>.</param>
        /// <param name="mensaje">Descripción legible.</param>
        /// <param name="peticion">Serialización opcional de request (ya saneada por implementación).</param>
        /// <param name="respuesta">Serialización opcional de response.</param>
        /// <param name="tiempoRq">Metadato temporal de ida.</param>
        /// <param name="tiempoRs">Duración o tiempo de vuelta (texto).</param>
        /// <param name="estadoFinal">EXITOSO/PARCIAL/FALLIDO o null si no aplica en esta línea.</param>
        /// <param name="nmUserUpdate">Usuario para columnas Gestordoc.</param>
        Task InfoAsync(
            string idTramite,
            string puntoDeControl,
            string mensaje,
            string? peticion,
            string? respuesta,
            string? tiempoRq,
            string? tiempoRs,
            string? estadoFinal,
            string? nmUserUpdate,
            CancellationToken cancellationToken);

        Task WarnAsync(
            string idTramite,
            string puntoDeControl,
            string mensaje,
            string? peticion,
            string? respuesta,
            string? tiempoRq,
            string? tiempoRs,
            string? estadoFinal,
            string? nmUserUpdate,
            CancellationToken cancellationToken);

        Task ErrorAsync(
            string idTramite,
            string puntoDeControl,
            string mensaje,
            string? peticion,
            string? respuesta,
            string? tiempoRq,
            string? tiempoRs,
            string? estadoFinal,
            string? nmUserUpdate,
            Exception? exception,
            CancellationToken cancellationToken);
    }
}
