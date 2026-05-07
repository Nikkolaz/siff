using System.Text.Json;

namespace SSF.Interop.SIIFNacion.Application.Tests.TestHelpers;

/// <summary>Serializa JSON de prueba a <see cref="JsonElement"/> (misma forma que consume SIIF en handlers).</summary>
internal static class JsonTestHelper
{
    public static JsonElement Root(string json) =>
        JsonDocument.Parse(json).RootElement.Clone();
}
