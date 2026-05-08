using System.IO;
using System.Text.Json;

namespace SSF.Interop.SIIFNacion.Installer;

internal static class JsonFormatting
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    /// <summary>Intenta parsear y devolver JSON con sangría; si falla, devuelve el texto original.</summary>
    public static string TryFormat(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc.RootElement, Indented);
        }
        catch (JsonException)
        {
            return raw;
        }
    }

    /// <summary>Valida que el texto sea JSON válido.</summary>
    public static bool TryValidate(string raw, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "El contenido está vacío.";
            return false;
        }

        try
        {
            JsonDocument.Parse(raw);
            return true;
        }
        catch (JsonException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static string ReadAllTextUtf8(string path) => File.ReadAllText(path);

    public static void WriteAllTextUtf8(string path, string content) =>
        File.WriteAllText(path, content, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}
