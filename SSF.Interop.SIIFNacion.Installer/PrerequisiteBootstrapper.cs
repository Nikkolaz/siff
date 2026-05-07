using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;

namespace SSF.Interop.SIIFNacion.Installer;

/// <summary>Descarga instaladores oficiales y lanza un script PowerShell elevado.</summary>
internal static class PrerequisiteBootstrapper
{
    public sealed record InstallSelection(bool InstallIisIfMissing, bool InstallSdkIfMissing, bool InstallHostingIfMissing);

    public sealed record InstallPlan(bool RunIis, bool RunSdk, bool RunHosting);

    /// <summary>Traduce la selección del usuario y el estado actual en pasos concretos.</summary>
    public static InstallPlan ResolvePlan(PrerequisiteProbe.Status s, InstallSelection sel)
    {
        var hostingIncomplete = !s.AspNetCoreModuleV2 || !s.AspNetCoreRuntime9;
        return new InstallPlan(
            RunIis: sel.InstallIisIfMissing && !s.IisWebServerRole,
            RunSdk: sel.InstallSdkIfMissing && !s.Sdk9,
            RunHosting: sel.InstallHostingIfMissing && hostingIncomplete);
    }

    public static async Task<(string? SdkPath, string? HostingPath)> DownloadIfNeededAsync(
        InstallPlan plan,
        DotNetReleaseResolver.DownloadPair urls,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        string? sdk = null;
        string? hb = null;
        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

        if (plan.RunSdk)
        {
            sdk = Path.Combine(Path.GetTempPath(), $"dotnet-sdk-{urls.ReleaseVersion}-win-x64.exe");
            progress?.Report("Descargando SDK…");
            await DownloadToFileAsync(http, urls.SdkInstallerUrl, sdk, progress, cancellationToken).ConfigureAwait(false);
        }

        if (plan.RunHosting)
        {
            hb = Path.Combine(Path.GetTempPath(), $"dotnet-hosting-{urls.ReleaseVersion}-win.exe");
            progress?.Report("Descargando Hosting Bundle…");
            await DownloadToFileAsync(http, urls.HostingBundleUrl, hb, progress, cancellationToken).ConfigureAwait(false);
        }

        return (sdk, hb);
    }

    private static async Task DownloadToFileAsync(
        HttpClient http,
        string url,
        string destPath,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var inStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var outStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        var buf = new byte[81920];
        long total = response.Content.Headers.ContentLength ?? -1;
        long readTotal = 0;
        int n;
        while ((n = await inStream.ReadAsync(buf.AsMemory(0, buf.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            await outStream.WriteAsync(buf.AsMemory(0, n), cancellationToken).ConfigureAwait(false);
            readTotal += n;
            if (total > 0 && readTotal % (1024 * 1024 * 5) < 81920)
                progress?.Report($"Descargando… {readTotal * 100 / Math.Max(1, total)} %");
        }
    }

    public static void RunElevatedInstallScript(InstallPlan plan, string? sdkPath, string? hostingPath)
    {
        var ps1 = BuildPowerShellScript(plan, sdkPath, hostingPath);
        var temp = Path.Combine(Path.GetTempPath(), "siif-prereq-" + Guid.NewGuid().ToString("N") + ".ps1");
        File.WriteAllText(temp, ps1, new UTF8Encoding(true));

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{temp}\"",
            UseShellExecute = true,
            Verb = "runas"
        });
    }

    private static string BuildPowerShellScript(InstallPlan plan, string? sdkPath, string? hostingPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("$ErrorActionPreference = 'Stop'");
        sb.AppendLine("Write-Host 'Prerrequisitos SIIF Nación (elevado).'");

        if (plan.RunIis)
        {
            sb.AppendLine(@"
if (-not (Test-Path 'HKLM:\SOFTWARE\Microsoft\InetStp')) {
  Write-Host 'Instalando rol IIS...'
  if (Get-Command Install-WindowsFeature -ErrorAction SilentlyContinue) {
    Install-WindowsFeature Web-Server -IncludeManagementTools | Out-Null
  } else {
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All -NoRestart -ErrorAction Stop | Out-Null
  }
} else { Write-Host 'IIS ya detectado en el registro.' }
");
        }

        if (plan.RunSdk && !string.IsNullOrEmpty(sdkPath))
        {
            sb.AppendLine($"if (-not (Test-Path -LiteralPath '{EscapePs(sdkPath)}')) {{ throw 'No existe el instalador del SDK.' }}");
            sb.AppendLine($"Write-Host 'Instalando SDK…'");
            sb.AppendLine($"Start-Process -FilePath '{EscapePs(sdkPath)}' -ArgumentList '/quiet','/norestart' -Wait");
        }

        if (plan.RunHosting && !string.IsNullOrEmpty(hostingPath))
        {
            sb.AppendLine($"if (-not (Test-Path -LiteralPath '{EscapePs(hostingPath)}')) {{ throw 'No existe el Hosting Bundle.' }}");
            sb.AppendLine($"Write-Host 'Instalando Hosting Bundle…'");
            sb.AppendLine($"Start-Process -FilePath '{EscapePs(hostingPath)}' -ArgumentList '/install','/quiet','/norestart' -Wait");
        }

        sb.AppendLine("Write-Host 'Finalizado. Si Windows lo pide, reinicie el servidor.'");
        sb.AppendLine("Read-Host 'Pulse Enter para cerrar'");
        return sb.ToString();
    }

    private static string EscapePs(string path) => path.Replace("'", "''", StringComparison.Ordinal);
}
