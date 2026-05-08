using Moq;
using SSF.Interop.SIIFNacion.Application.Common.Auditoria;
using SSF.Interop.SIIFNacion.Application.Common.Interfaces.ExternalServices.SIIF;
using SSF.Interop.SIIFNacion.Application.Contracts.Persistence;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Common;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Contracts;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response;
using SSF.Interop.SIIFNacion.Application.DTOs.External.SIIF.Response.Contracts;
using SSF.Interop.SIIFNacion.Application.Features.SIIF.Requests.Queries;
using SSF.Interop.SIIFNacion.Domain.Cdp;

namespace SSF.Interop.SIIFNacion.Application.Tests.Features.SIIF;

/// <summary>
/// 🧪 Tests unitarios para ConsultarCompromisoPresupuestalRPQueryHandler.
/// 
/// 📐 Convención de nombres: Handle_[escenario]_[resultado esperado]
/// 🔧 Stack: xUnit + Moq. Mismo patrón que ConsultarCdpPaginadaQueryHandlerTests.
/// ✅ Cobertura: respuesta vacía, persistencia en ambas tablas, idempotencia y error SIIF.
/// </summary>
public class ConsultarCompromisoPresupuestalRPQueryHandlerTests
{
    // 🤖 Mocks de las dependencias inyectadas
    private readonly Mock<ISiifBudgetService>    _siif       = new();
    private readonly Mock<ICdpCompromisoRepository> _legacyRepo = new();
    private readonly Mock<IDynCompromisoRepository> _dynRepo    = new();
    private readonly Mock<IAuditoriaLogger>      _audit      = new();

    // 🎯 System Under Test
    private readonly ConsultarCompromisoPresupuestalRPQueryHandler _sut;

    public ConsultarCompromisoPresupuestalRPQueryHandlerTests()
    {
        // 📌 IAuditoriaLogger: configurado para no lanzar excepciones en ningún método
        // (best-effort por diseño — la auditoría no debe romper el flujo principal).
        _audit.Setup(a => a.InfoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _audit.Setup(a => a.WarnAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _audit.Setup(a => a.ErrorAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<Exception?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new ConsultarCompromisoPresupuestalRPQueryHandler(
            _siif.Object, _legacyRepo.Object, _dynRepo.Object, _audit.Object);
    }

    // ────────────────────────────────────────────────────────────────────────────────
    // 🧪 Test 1: Respuesta nula → no se persiste nada
    // ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_siifDevuelveNull_noGuardaNingúnRepositorio()
    {
        // 🎭 Arrange: SIIF devuelve null (situación real cuando el compromiso no existe)
        _siif.Setup(s => s.ConsultarCompromisoPptalAsync(
                It.IsAny<ConsultarCompromisoRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsultarCompromisoResponseDto?)null);

        // ▶️ Act
        var result = await _sut.Handle(BaseQuery(), CancellationToken.None);

        // ✅ Assert: se devuelve un DTO vacío y NO se tocan los repositorios
        Assert.NotNull(result);

        _legacyRepo.Verify(r => r.SaveAsync(
            It.IsAny<CdpCompromiso>(), It.IsAny<CancellationToken>()), Times.Never);

        _dynRepo.Verify(r => r.SaveAsync(
            It.IsAny<DynTblCCompPtal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────────────────────
    // 🧪 Test 2: Codigo = 0 → no se persiste nada
    // ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_codigoCero_noGuardaNingúnRepositorio()
    {
        // 🎭 Arrange: SIIF devuelve objeto con Codigo=0 (compromiso no encontrado)
        _siif.Setup(s => s.ConsultarCompromisoPptalAsync(
                It.IsAny<ConsultarCompromisoRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultarCompromisoResponseDto { Codigo = 0 });

        // ▶️ Act
        await _sut.Handle(BaseQuery(), CancellationToken.None);

        // ✅ Assert: repositorios no fueron tocados
        _legacyRepo.Verify(r => r.SaveAsync(
            It.IsAny<CdpCompromiso>(), It.IsAny<CancellationToken>()), Times.Never);

        _dynRepo.Verify(r => r.SaveAsync(
            It.IsAny<DynTblCCompPtal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────────────────────
    // 🧪 Test 3: Compromiso nuevo → se guarda en legacy Y en DYN
    // ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_compromisoNuevo_guardaEnLegacyYEnDyn()
    {
        // 🎭 Arrange: SIIF devuelve compromiso válido; ambas tablas reportan que no existe
        var siifResponse = CompromisoValido(codigo: 9999);

        _siif.Setup(s => s.ConsultarCompromisoPptalAsync(
                It.IsAny<ConsultarCompromisoRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(siifResponse);

        // 🔑 IdCompromiso=9999 no existe → se debe insertar
        _legacyRepo.Setup(r => r.ExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _dynRepo.Setup(r => r.ExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _legacyRepo.Setup(r => r.ResetIdentityIfEmpty())
            .Returns(Task.CompletedTask);

        // ▶️ Act
        var result = await _sut.Handle(BaseQuery(), CancellationToken.None);

        // ✅ Assert: ambas tablas guardaron exactamente una vez
        Assert.Equal(9999, result.Codigo);

        _legacyRepo.Verify(r => r.SaveAsync(
            It.Is<CdpCompromiso>(c => c.IdCompromiso == 9999),
            It.IsAny<CancellationToken>()), Times.Once);

        _dynRepo.Verify(r => r.SaveAsync(
            It.Is<DynTblCCompPtal>(d => d.IdCompromiso == 9999 && d.FgEnabled == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────────────────────
    // 🧪 Test 4: Compromiso ya existe → idempotencia (no guarda de nuevo)
    // ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_compromisoYaExiste_noGuardaNingúnRepositorio()
    {
        // 🎭 Arrange: SIIF devuelve compromiso válido; ambas tablas reportan que ya existe
        _siif.Setup(s => s.ConsultarCompromisoPptalAsync(
                It.IsAny<ConsultarCompromisoRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CompromisoValido(codigo: 1234));

        // ♻️ Ya existe en ambas tablas → idempotencia garantizada
        _legacyRepo.Setup(r => r.ExistsAsync(1234, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _dynRepo.Setup(r => r.ExistsAsync(1234, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // ▶️ Act
        await _sut.Handle(BaseQuery(), CancellationToken.None);

        // ✅ Assert: SaveAsync no fue llamado en ninguna tabla
        _legacyRepo.Verify(r => r.SaveAsync(
            It.IsAny<CdpCompromiso>(), It.IsAny<CancellationToken>()), Times.Never);

        _dynRepo.Verify(r => r.SaveAsync(
            It.IsAny<DynTblCCompPtal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ────────────────────────────────────────────────────────────────────────────────
    // 🧪 Test 5: SIIF lanza excepción → se propaga y se registra en auditoría
    // ────────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_siifLanzaExcepcion_propagaYRegistraErrorEnAuditoria()
    {
        // 🎭 Arrange: SIIF falla con excepción de red (timeout, etc.)
        _siif.Setup(s => s.ConsultarCompromisoPptalAsync(
                It.IsAny<ConsultarCompromisoRequestDto>(),
                It.IsAny<SiifRequestHeaderDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection timeout"));

        // ▶️ Act & Assert: la excepción debe propagarse al caller
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _sut.Handle(BaseQuery(), CancellationToken.None));

        // 🔍 Verificar que se registró exactamente un error en auditoría
        _audit.Verify(a => a.ErrorAsync(
            It.IsAny<string>(),
            SiifPuntoDeControl.FinError,
            It.Is<string>(msg => msg.Contains("💥")),
            It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<string?>(),
            SiifAuditoriaEstadoFinal.Fallido,
            It.IsAny<string?>(),
            It.IsAny<Exception>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 🏗️ Construye un query base válido para todos los tests.
    /// </summary>
    private static ConsultarCompromisoPresupuestalRPQuery BaseQuery() => new()
    {
        CodPciHeader           = "36-01-07",
        LoginUsuarioSiifHeader = "MHwsprueba",
        ConsecutivoHeader      = "3",
        HashHeader             = "",
        Pci                    = "36-01-07",
        CodCompromisoPptalGastos = 9999,
        Vigencia               = "2025"
    };

    /// <summary>
    /// 🏗️ Construye un ConsultarCompromisoResponseDto con datos mínimos válidos.
    /// </summary>
    private static ConsultarCompromisoResponseDto CompromisoValido(int codigo) => new()
    {
        Codigo        = codigo,
        Vigencia      = "2025",
        FechaRegistro = DateTime.Today,
        FechaCdp      = DateTime.Today,
        Estado        = "VIGENTE",
        Descripcion   = "Test compromiso",
        Objeto        = "Test objeto",
        // 💰 Valores monetarios con redondeo estándar
        ValorInicial              = 1_000_000m,
        ValorInicialOriginalMoneda = 1_000_000m,
        ValorTotalOperacion       = 1_000_000m,
        ValorActual               = 900_000m,
        SaldoPorObligar           = 100_000m,
        // 📋 Listas vacías para evitar NullReferenceException en el mapper
        ListadoItemsAfectacionRaw = new(),
        ListadoPlanesPagoRaw      = new()
    };
}
