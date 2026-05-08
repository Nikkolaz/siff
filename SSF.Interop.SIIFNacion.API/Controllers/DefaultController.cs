using Microsoft.AspNetCore.Mvc;

namespace SSF.Interop.SIIFNacion.Controllers
{
    [Route("/")]
    [ApiController]
    public class DefaultController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration p_Configuracion = configuration;
        [HttpGet]
        public string Index()
        {
            return $"Running ... New Env {p_Configuracion["Environment"]}";
        }
    }
}
