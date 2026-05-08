using System.Net.Http;
using System.Text.Json;

namespace SSF.Interop.SIIFNacion.Installer;

/// <summary>Obtiene URLs de descarga del SDK win-x64 y del Hosting Bundle desde el JSON de releases de .NET.</summary>
internal static class DotNetReleaseResolver
{
    private const string ReleasesIndexUrl = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";

    public sealed record DownloadPair(string SdkInstallerUrl, string HostingBundleUrl, string SdkVersion, string ReleaseVersion);

    /// <summary>Resuelve las URLs de la última versión publicada del canal indicado (p. ej. 9.0).</summary>
    public static async Task<DownloadPair> ResolveLatestAsync(string channelVersion, CancellationToken cancellationToken = default)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        await using var indexStream = await http.GetStreamAsync(new Uri(ReleasesIndexUrl), cancellationToken).ConfigureAwait(false);
        using var indexDoc = await JsonDocument.ParseAsync(indexStream, cancellationToken: cancellationToken).ConfigureAwait(false);

        string? releasesJsonUrl = null;
        foreach (var el in indexDoc.RootElement.GetProperty("releases-index").EnumerateArray())
        {
            if (el.GetProperty("channel-version").GetString() == channelVersion)
            {
                releasesJsonUrl = el.GetProperty("releases.json").GetString();
                break;
            }
        }

        if (string.IsNullOrEmpty(releasesJsonUrl))
            throw new InvalidOperationException($"No se encontró el canal {channelVersion} en releases-index.json.");

        await using var relStream = await http.GetStreamAsync(new Uri(releasesJsonUrl), cancellationToken).ConfigureAwait(false);
        using var relDoc = await JsonDocument.ParseAsync(relStream, cancellationToken: cancellationToken).ConfigureAwait(false);

        var root = relDoc.RootElement;
        var latest = root.GetProperty("latest-release").GetString()
            ?? throw new InvalidOperationException("releases.json sin latest-release.");

        JsonElement releaseBlock = default;
        var found = false;
        foreach (var rel in root.GetProperty("releases").EnumerateArray())
        {
            if (rel.GetProperty("release-version").GetString() == latest)
            {
                releaseBlock = rel;
                found = true;
                break;
            }
        }

        if (!found)
            throw new InvalidOperationException($"No se encontró la release {latest} en releases.json.");

        var sdkUrl = FindFileUrl(releaseBlock, "sdk", "dotnet-sdk-win-x64.exe");
        var hostingUrl = FindFileUrl(releaseBlock, "aspnetcore-runtime", "dotnet-hosting-win.exe");
        var sdkVer = releaseBlock.GetProperty("sdk").GetProperty("version").GetString() ?? "";

        return new DownloadPair(sdkUrl, hostingUrl, sdkVer, latest);
    }

    private static string FindFileUrl(JsonElement releaseBlock, string section, string fileName)
    {
        var files = releaseBlock.GetProperty(section).GetProperty("files");
        foreach (var f in files.EnumerateArray())
        {
            if (f.GetProperty("name").GetString() == fileName)
                return f.GetProperty("url").GetString() ?? "";
        }

        throw new InvalidOperationException($"No se encontró el archivo {fileName} en {section}.");
    }
}
