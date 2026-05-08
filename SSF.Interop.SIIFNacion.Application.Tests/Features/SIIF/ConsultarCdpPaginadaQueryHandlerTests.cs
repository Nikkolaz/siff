using Moq;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Application.Exceptions;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries;
using SSF.Interop.SIIFNacion.Application.Tests.TestHelpers;
using SSF.Interop.SIIFNacion.Domain.Cdp;
using System.Text.Json;

namespace SSF.Interop.SIIFNacion.Application.Tests.Features.SIIF;

public class ConsultarCdpPaginadaQueryHandlerTests
{
    private readonly Mock<ISiifBudgetService> _siif = new();
    private readonly Mock<ICdpPaginadoRepository> _repo = new();
    private readonly Mock<IAuditoriaLogger> _audit = new();
    private readonly ConsultarCdpPaginadaQueryHandler _sut;

    public ConsultarCdpPaginadaQueryHandlerTests()
    {
        _sut = new ConsultarCdpPaginadaQueryHandler(_siif.Object, _repo.Object, _audit.Object);
    }

    [Fact]
    public async Task Handle_siifNoExitoso_NoInserta()
    {
        _siif.Setup(s => s.ConsultarListaCdpPaginadaAsync(
                It.IsAny<ConsultaListaCdpPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaCdpPaginadaResponseDto { Success = false, Message = "x" });

        await _sut.Handle(BaseQuery(), CancellationToken.None);

        _repo.Verify(r => r.SaveRangeAsync(It.IsAny<IEnumerable<CdpPaginado>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_consultaSinRegistros_noInserta()
    {
        var json = "{\"Success\":true,\"Data\":{\"ConsultaCDPSal\":[]}}";
        _siif.Setup(s => s.ConsultarListaCdpPaginadaAsync(
                It.IsAny<ConsultaListaCdpPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaCdpPaginadaResponseDto { Success = true, Data = JsonDocument.Parse(json).RootElement.GetProperty("Data") });

        await _sut.Handle(BaseQuery(), CancellationToken.None);

        _repo.Verify(r => r.SaveRangeAsync(It.IsAny<IEnumerable<CdpPaginado>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_unCdp_eInsertaConFgEnabled()
    {
        var json = "{\"Success\":true,\"Data\":{\"ConsultaCDPSal\":[{\"CodigoPCIConexion\":\"PCI\",\"NumeroSolicitudCDP\":123,\"DescripcionPCIConexion\":\"D\",\"CodigoSubunidad\":\"S\",\"DescripcionSubunidad\":\"DS\",\"NumeroDocumento\":456,\"FechaRegistro\":\"2025-01-01\",\"FechaCreacion\":\"2025-01-01\",\"TipoCDP\":\"T\",\"Estado\":\"E\",\"Objeto\":\"O\",\"CodigoDependenciaAfectacion\":\"DA\",\"DescripcionDependenciaAfectacion\":\"DDA\",\"CodigoPosicionGasto\":\"CPG\",\"DescripcionPosicionGasto\":\"DPG\",\"CodigoFuente\":\"CF\",\"DescripcionFuente\":\"DF\",\"CodigoRecurso\":\"CR\",\"DescripcionRecurso\":\"DR\",\"CodigoSituacion\":\"CS\",\"DescripcionSituacion\":\"DSIT\",\"ValorOperaciones\":100.0,\"ValorActual\":100.0,\"SaldoporComp\":50.0,\"ValorBloqueado\":10.0,\"ListaCuentasPorPagar\":\"LCP\",\"ListaObligaciones\":\"LO\",\"ListaOrdenDePago\":\"LOP\",\"ListaReintegro\":\"LR\",\"ValorInicial\":100.0}]}}";
        
        _siif.Setup(s => s.ConsultarListaCdpPaginadaAsync(
                It.IsAny<ConsultaListaCdpPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaCdpPaginadaResponseDto { Success = true, Data = JsonDocument.Parse(json).RootElement.GetProperty("Data") });

        await _sut.Handle(BaseQuery(), CancellationToken.None);

        _repo.Verify(
            r => r.SaveRangeAsync(
                It.Is<IEnumerable<CdpPaginado>>(rows =>
                    rows.Count() == 1
                    && rows.First().FgEnabled == 1
                    && rows.First().CodPciConexion == "PCI"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ConsultarCdpPaginadaQuery BaseQuery() => new()
    {
        CodPciHeader = "a",
        LoginUsuarioSiifHeader = "b",
        ConsecutivoHeader = "c",
        FechaRegistroIni = "2025-01-01",
        FechaRegistroFin = "2025-06-30",
        PCIConsulta = "pci",
        PCISubUnidades = null,
        TipoGasto = "tg",
        Rango = "r"
    };
}
