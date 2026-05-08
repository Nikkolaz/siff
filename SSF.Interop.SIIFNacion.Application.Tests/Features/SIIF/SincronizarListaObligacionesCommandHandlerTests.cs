using Moq;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Application.Exceptions;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Commands.SincronizarListaObligaciones;
using SSF.Interop.SIIFNacion.Application.Tests.TestHelpers;
using SSF.Interop.SIIFNacion.Domain.Siif;

namespace SSF.Interop.SIIFNacion.Application.Tests.Features.SIIF;

public class SincronizarListaObligacionesCommandHandlerTests
{
    private readonly Mock<ISiifBudgetService> _siif = new();
    private readonly Mock<IObligacionApoRepository> _repo = new();
    private readonly Mock<IAuditoriaLogger> _audit = new();
    private readonly SincronizarListaObligacionesCommandHandler _sut;

    public SincronizarListaObligacionesCommandHandlerTests()
    {
        _sut = new SincronizarListaObligacionesCommandHandler(_siif.Object, _repo.Object, _audit.Object);
    }

    [Fact]
    public async Task Handle_cruzaAniosEnComando_lanzaAntesDeLlamarSiif()
    {
        var cmd = BaseCommand();
        cmd.FechaInicio = new DateTime(2024, 12, 1);
        cmd.FechaFin = new DateTime(2025, 1, 1);

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.Handle(cmd, CancellationToken.None));

        _siif.Verify(
            s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_siifNoExitoso_lanzaBadRequestException()
    {
        _siif.Setup(s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaObligacionesPaginadaResponseDto
            {
                Success = false,
                Message = "error"
            });

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.Handle(BaseCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_listaVacia_noDeshabilitaNiInserta()
    {
        var json = SiifObligacionesTestBodies.EnvelopeLista(0, 0);
        _siif.Setup(s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaObligacionesPaginadaResponseDto
            {
                Success = true,
                Data = JsonTestHelper.Root(json)
            });

        var result = await _sut.Handle(BaseCommand(), CancellationToken.None);

        Assert.Equal(0, result.RegistrosInsertados);
        Assert.Equal(0, result.PaginasProcesadas);
        _repo.Verify(r => r.DisableByAnioVigenciaAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<SiifObligacionApo>>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_unRegistro_deshabilitaVigenciaUnaVez_eInsertaConAnioYFgEnabled()
    {
        var json = SiifObligacionesTestBodies.EnvelopeLista(1, 1);
        _siif.Setup(s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaObligacionesPaginadaResponseDto
            {
                Success = true,
                Data = JsonTestHelper.Root(json)
            });

        var result = await _sut.Handle(BaseCommand(), CancellationToken.None);

        Assert.Equal(1, result.RegistrosInsertados);
        Assert.Equal(1, result.PaginasProcesadas);
        _repo.Verify(r => r.DisableByAnioVigenciaAsync("2025", "v", It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<SiifObligacionApo>>(rows =>
                    rows.Count() == 1
                    && rows.First().AnioVigencia == "2025"
                    && rows.First().FgEnabled == 1
                    && rows.First().VigenciaCod == "v"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_dosPaginasTotal55_deshabilitaUnaVez_yRecorrePaginas1y2()
    {
        var paginas = new List<int>();
        var respuestas = new Queue<ConsultaListaObligacionesPaginadaResponseDto>();
        respuestas.Enqueue(new ConsultaListaObligacionesPaginadaResponseDto
        {
            Success = true,
            Data = JsonTestHelper.Root(SiifObligacionesTestBodies.EnvelopeLista(55, 50))
        });
        respuestas.Enqueue(new ConsultaListaObligacionesPaginadaResponseDto
        {
            Success = true,
            Data = JsonTestHelper.Root(SiifObligacionesTestBodies.EnvelopeLista(55, 5))
        });

        _siif.Setup(s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .Returns<ConsultaListaObligacionesPaginadaRequestDto, SiifRequestHeaderDto, CancellationToken>(
                (body, _, _) =>
                {
                    paginas.Add(body.Pagination.Page);
                    return Task.FromResult(respuestas.Dequeue());
                });

        var result = await _sut.Handle(BaseCommand(), CancellationToken.None);

        Assert.Equal(55, result.RegistrosInsertados);
        Assert.Equal(2, result.PaginasProcesadas);
        Assert.Equal(new[] { 1, 2 }, paginas);
        _repo.Verify(r => r.DisableByAnioVigenciaAsync("2025", "v", It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_vigenciaUno_llamaDeshabilitarConVigenciaCod1()
    {
        var json = SiifObligacionesTestBodies.EnvelopeLista(1, 1);
        _siif.Setup(s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaObligacionesPaginadaResponseDto
            {
                Success = true,
                Data = JsonTestHelper.Root(json)
            });

        var cmd = BaseCommand();
        cmd.Vigencia = "1";

        await _sut.Handle(cmd, CancellationToken.None);

        _repo.Verify(r => r.DisableByAnioVigenciaAsync("2025", "1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_sinVigenciaEnComando_deshabilitaSoloPorAnio()
    {
        var json = SiifObligacionesTestBodies.EnvelopeLista(1, 1);
        _siif.Setup(s => s.ConsultarListaObligacionesPaginadaAsync(
                It.IsAny<ConsultaListaObligacionesPaginadaRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultaListaObligacionesPaginadaResponseDto
            {
                Success = true,
                Data = JsonTestHelper.Root(json)
            });

        var cmd = BaseCommand();
        cmd.Vigencia = "";

        await _sut.Handle(cmd, CancellationToken.None);

        _repo.Verify(r => r.DisableByAnioVigenciaAsync("2025", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SincronizarListaObligacionesCommand BaseCommand() => new()
    {
        CodPciHeader = "h1",
        LoginUsuarioSiifHeader = "h2",
        ConsecutivoHeader = "h3",
        CodPCI = "pci",
        FechaInicio = new DateTime(2025, 1, 1),
        FechaFin = new DateTime(2025, 6, 30),
        TipoGasto = "tg",
        Rango = "r",
        Vigencia = "v",
        DetalleUsosPresupuestales = "d"
    };
}
