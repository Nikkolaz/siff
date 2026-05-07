using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Domain.Auditoria;

namespace SSF.Interop.SIIFNacion.Application.Common.Auditoria
{
    public sealed class DynEfwAuditoriaLogger : IAuditoriaLogger
    {
        private const int MaxPayloadChars = 200_000; // evita filas gigantes por respuestas masivas
        private const string AplicacionDefault = "SSF.Interop.SIIFNacion.API";

        private readonly IAuditoriaRepository _repository;

        public DynEfwAuditoriaLogger(IAuditoriaRepository repository)
        {
            _repository = repository;
        }

        public Task InfoAsync(
            string idTramite,
            string puntoDeControl,
            string mensaje,
            string? peticion,
            string? respuesta,
            string? tiempoRq,
            string? tiempoRs,
            string? estadoFinal,
            string? nmUserUpdate,
            CancellationToken cancellationToken) =>
            WriteAsync(
                nivel: SiifAuditoriaNivel.Info,
                idTramite,
                puntoDeControl,
                mensaje,
                peticion,
                respuesta,
                tiempoRq,
                tiempoRs,
                estadoFinal,
                nmUserUpdate,
                exception: null,
                cancellationToken);

        public Task WarnAsync(
            string idTramite,
            string puntoDeControl,
            string mensaje,
            string? peticion,
            string? respuesta,
            string? tiempoRq,
            string? tiempoRs,
            string? estadoFinal,
            string? nmUserUpdate,
            CancellationToken cancellationToken) =>
            WriteAsync(
                nivel: SiifAuditoriaNivel.Warn,
                idTramite,
                puntoDeControl,
                mensaje,
                peticion,
                respuesta,
                tiempoRq,
                tiempoRs,
                estadoFinal,
                nmUserUpdate,
                exception: null,
                cancellationToken);

        public Task ErrorAsync(
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
            CancellationToken cancellationToken) =>
            WriteAsync(
                nivel: SiifAuditoriaNivel.Error,
                idTramite,
                puntoDeControl,
                mensaje,
                peticion,
                respuesta,
                tiempoRq,
                tiempoRs,
                estadoFinal,
                nmUserUpdate,
                exception,
                cancellationToken);

        private async Task WriteAsync(
            string nivel,
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
            CancellationToken cancellationToken)
        {
            try
            {
                var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var row = new DynEfwAuditoria
                {
                    Oid = Guid.NewGuid().ToString("N"),
                    NrVersion = 1,
                    BnCreated = nowMs,
                    FgEnabled = 1,
                    FgSystem = 0,
                    BnUpdated = nowMs,
                    NmUserUpdate = string.IsNullOrWhiteSpace(nmUserUpdate) ? "UsuarioIntegracion" : nmUserUpdate.Trim(),
                    IdTransaccion = 0,
                    IdTramite = idTramite,
                    Mensaje = AuditoriaSanitizer.Truncate(mensaje, 2040),
                    Peticion = AuditoriaSanitizer.Truncate(AuditoriaSanitizer.Sanitize(peticion), MaxPayloadChars),
                    Respuesta = AuditoriaSanitizer.Truncate(AuditoriaSanitizer.Sanitize(respuesta), MaxPayloadChars),
                    TiempoRq = AuditoriaSanitizer.Truncate(tiempoRq, 2040),
                    TiempoRs = AuditoriaSanitizer.Truncate(tiempoRs, 2040),
                    Nivel = nivel,
                    Excepcion = exception?.ToString(),
                    PuntoDeControl = puntoDeControl,
                    Aplicacion = AplicacionDefault,
                    Ccf = null,
                    EstadoFinal = estadoFinal
                };

                await _repository.AddAsync(row, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // best-effort: nunca romper el flujo principal por fallos de auditoría
            }
        }
    }
}

