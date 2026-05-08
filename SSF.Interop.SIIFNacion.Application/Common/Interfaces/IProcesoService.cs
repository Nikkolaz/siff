using SSF.Interop.SIIFNacion.Domain;

namespace SSF.Interop.SIIFNacion.Application.Common.Interfaces
{
    public interface IProcesoService
    {
        Task<int> ObtenerIdEstadoByNombreAsync(string estadoCodigo);
        Task CambiarEstadoAsync(long idEjecucion, string estadoCodigo, string? mensaje = null);
        Task RegistrarArchivoAsync(long idEjecucion, string nombre, string ruta, long tamano, int registros, string nombreVista, int wk_mes, int idEstado);
    }
}
