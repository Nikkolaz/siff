using SSF.Interop.SIIFNacion.Application.Common.SIIF;

namespace SSF.Interop.SIIFNacion.Application.Tests.Common.SIIF;

public class SiifWorkerPeriodoTests
{
    [Fact]
    public void YearToDateLocal_desdeAbril_vaDesdeEneroPrimeroHastaEseDia()
    {
        var hoy = new DateTime(2026, 4, 9, 15, 30, 0, DateTimeKind.Unspecified);
        var (ini, fin) = SiifWorkerPeriodo.YearToDateLocal(hoy);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), ini);
        Assert.Equal(new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Unspecified), fin);
    }

    [Fact]
    public void YearToDateIsoStrings_formatoInvariante()
    {
        var hoy = new DateTime(2026, 4, 9);
        var (a, b) = SiifWorkerPeriodo.YearToDateIsoStrings(hoy);
        Assert.Equal("2026-01-01", a);
        Assert.Equal("2026-04-09", b);
    }
}
