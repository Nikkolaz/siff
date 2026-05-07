namespace SSF.Interop.SIIFNacion.Application.Tests.TestHelpers;

/// <summary>Respuestas mínimas compatibles con <c>SincronizarListaObligacionesCommandHandler</c> (State/Code/Data/ListaObligacionesSal).</summary>
internal static class SiifObligacionesTestBodies
{
    private const string ObligacionItemMinima = """{"Obligaciones":"99","CodigoObligacion":"100"}""";

    public static string EnvelopeLista(int totalReportado, int cantidadItemsEnPagina)
    {
        var items = string.Join(",", Enumerable.Repeat(ObligacionItemMinima, cantidadItemsEnPagina));
        return $@"{{""State"":true,""Code"":200,""Data"":{{""Total"":{totalReportado},""ListaObligacionesSal"":[{items}]}}}}";
    }
}
