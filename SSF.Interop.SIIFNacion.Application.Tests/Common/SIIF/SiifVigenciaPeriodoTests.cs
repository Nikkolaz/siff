using SSF.Interop.SIIFNacion.Application.Common.SIIF;
using SSF.Interop.SIIFNacion.Application.Exceptions;

namespace SSF.Interop.SIIFNacion.Application.Tests.Common.SIIF;

public class SiifVigenciaPeriodoTests
{
    [Fact]
    public void ResolveAnioVigenciaFromDateRange_mismoAnio_devuelveCadenaDelAnio()
    {
        var anio = SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateRange(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 12, 31));

        Assert.Equal("2025", anio);
    }

    [Fact]
    public void ResolveAnioVigenciaFromDateRange_aniosDistintos_lanzaBadRequestException()
    {
        var ex = Assert.Throws<BadRequestException>(() =>
            SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateRange(
                new DateTime(2024, 12, 1),
                new DateTime(2025, 1, 1)));

        Assert.Contains("único año", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveAnioVigenciaFromDateStrings_isoMismaVigencia_ok()
    {
        var anio = SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateStrings(
            "2025-01-01",
            "2025-06-30",
            "Ini",
            "Fin");

        Assert.Equal("2025", anio);
    }

    [Fact]
    public void ResolveAnioVigenciaFromDateStrings_cruzaAnios_lanza()
    {
        Assert.Throws<BadRequestException>(() =>
            SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateStrings(
                "2024-12-01",
                "2025-01-01",
                "Ini",
                "Fin"));
    }

    [Theory]
    [InlineData(null, "2025-01-01")]
    [InlineData("2025-01-01", "")]
    [InlineData("   ", "2025-01-01")]
    public void ResolveAnioVigenciaFromDateStrings_fechasObligatorias_lanza(string? ini, string? fin)
    {
        Assert.Throws<BadRequestException>(() =>
            SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateStrings(ini, fin, "A", "B"));
    }

    [Fact]
    public void ResolveAnioVigenciaFromDateStrings_textoNoFecha_lanza()
    {
        Assert.Throws<BadRequestException>(() =>
            SiifVigenciaPeriodo.ResolveAnioVigenciaFromDateStrings(
                "no-es-una-fecha",
                "2025-01-01",
                "Ini",
                "Fin"));
    }
}
