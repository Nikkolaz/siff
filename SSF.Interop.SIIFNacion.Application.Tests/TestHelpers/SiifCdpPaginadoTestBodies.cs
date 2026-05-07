namespace SSF.Interop.SIIFNacion.Application.Tests.TestHelpers;

/// <summary>Objetos mínimos compatibles con <c>MapCdpItem</c> (todos los <c>GetProperty</c> requeridos).</summary>
internal static class SiifCdpPaginadoTestBodies
{
    public const string ItemMinimoJson =
        """{"CodigoPCIConexion":"PCI","DescripcionPCIConexion":"DPCI","CodigoSubunidad":"SU","DescripcionSubunidad":"DSU","NumeroSolicitudCDP":1,"NumeroDocumento":2,"FechaRegistro":"2025-03-01T00:00:00","FechaCreacion":"2025-03-02T00:00:00","TipoCDP":"T","Estado":"E","Objeto":"O","CodigoDependenciaAfectacion":"DA","DescripcionDependenciaAfectacion":"DDA","CodigoPosicionGasto":"PG","DescripcionPosicionGasto":"DPG","CodigoFuente":"F","DescripcionFuente":"DF","CodigoRecurso":"R","DescripcionRecurso":"DR","CodigoSituacion":"S","DescripcionSituacion":"DS","ValorOperaciones":1.1,"ValorActual":2.2,"SaldoporComp":3.3,"ValorBloqueado":4.4,"ListaCuentasPorPagar":null,"ListaObligaciones":null,"ListaOrdenDePago":null,"ListaReintegro":null,"ValorInicial":5.5}""";

    public static string EnvelopeAnidado(int total, int cantidadItems)
    {
        var items = string.Join(",", Enumerable.Repeat(ItemMinimoJson, cantidadItems));
        return $@"{{""State"":true,""Code"":200,""Data"":{{""Total"":{total},""ConsultaCDPSal"":[{items}]}}}}";
    }

    /// <summary>Payload sin nodo <c>Data</c>: <c>ConsultaCDPSal</c> y <c>Total</c> en la raíz.</summary>
    public static string EnvelopePlanoEnRaiz(int total, int cantidadItems)
    {
        var items = string.Join(",", Enumerable.Repeat(ItemMinimoJson, cantidadItems));
        return $@"{{""State"":true,""Code"":200,""Total"":{total},""ConsultaCDPSal"":[{items}]}}";
    }
}
