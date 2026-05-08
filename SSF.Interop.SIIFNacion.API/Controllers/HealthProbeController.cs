using Microsoft.AspNetCore.Mvc;

namespace SSF.Interop.SIIFNacion.API.Controllers;

/// <summary>
/// Comprobación mínima de disponibilidad (liveness): no consulta base de datos ni SIIF.
/// Útil para balanceadores, probes de Kubernetes/IIS o monitoreo que solo necesita saber si el proceso HTTP responde.
/// </summary>
[ApiController]
[Route("api/health")]
public sealed class HealthProbeController : ControllerBase
{
    /// <summary>Devuelve 200 si la API está en ejecución.</summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live([FromServices] IHostEnvironment host, [FromServices] IConfiguration configuration)
    {
        return Ok(new
        {
            status = "up",
            environment = host.EnvironmentName,
            deployment = configuration["Environment"],
            utc = DateTime.UtcNow
        });
    }
}
