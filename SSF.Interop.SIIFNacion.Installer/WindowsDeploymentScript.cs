using System.Diagnostics;
using System.IO;
using System.Text;

namespace SSF.Interop.SIIFNacion.Installer;

/// <summary>Genera scripts temporales y los ejecuta elevados (UAC).</summary>
internal static class WindowsDeploymentScript
{
    private const string WorkerDllFileName = "SSF.Interop.SIIFNacion.Worker.dll";

    public static string DefaultDotnetPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");

    /// <summary>Ejecuta un .bat elevado que recrea el servicio Windows con <c>sc create</c> y rutas con espacios.</summary>
    public static void RunElevatedWorkerInstall(string serviceName, string dotnetExePath, string workerPublishFolder)
    {
        var dll = Path.Combine(workerPublishFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), WorkerDllFileName);
        if (!File.Exists(dotnetExePath))
            throw new FileNotFoundException("No se encontró dotnet.exe.", dotnetExePath);
        if (!File.Exists(dll))
            throw new FileNotFoundException($"No se encontró el Worker publicado ({WorkerDllFileName}).", dll);

        var bat = Path.Combine(Path.GetTempPath(), "siif-worker-" + Guid.NewGuid().ToString("N") + ".bat");
        // Línea sc: binPath= "\"dotnet.exe\" \"Worker.dll\""
        var createLine =
            "sc create \"" + EscapeBatchArg(serviceName) + "\" binPath= \"\\\"" + EscapeBatchArg(dotnetExePath) + "\\\" \\\"" + EscapeBatchArg(dll) + "\\\"\" start= delayed-auto";
        var lines = new[]
        {
            "@echo off",
            "sc stop \"" + EscapeBatchArg(serviceName) + "\" 2>nul",
            "sc delete \"" + EscapeBatchArg(serviceName) + "\" 2>nul",
            "timeout /t 2 /nobreak >nul",
            createLine,
            "sc description \"" + EscapeBatchArg(serviceName) + "\" \"Integracion SIIF Nacion (Worker)\"",
            "sc start \"" + EscapeBatchArg(serviceName) + "\"",
            "echo Listo.",
            "pause"
        };
        File.WriteAllText(bat, string.Join(Environment.NewLine, lines), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        Process.Start(new ProcessStartInfo
        {
            FileName = bat,
            UseShellExecute = true,
            Verb = "runas"
        });
    }

    private static string EscapeBatchArg(string s) => s.Replace("\"", "\"\"", StringComparison.Ordinal);

    /// <summary>Ejecuta PowerShell elevado para crear o actualizar sitio IIS (requiere módulo WebAdministration).</summary>
    public static void RunElevatedIisSetup(string siteName, string appPoolName, int port, string physicalPath)
    {
        if (string.IsNullOrWhiteSpace(physicalPath) || !Directory.Exists(physicalPath))
            throw new DirectoryNotFoundException("La carpeta física de la API no existe.");

        var ps1 = BuildIisPs1(siteName, appPoolName, port, physicalPath);
        var temp = Path.Combine(Path.GetTempPath(), "siif-iis-install-" + Guid.NewGuid().ToString("N") + ".ps1");
        File.WriteAllText(temp, ps1, new UTF8Encoding(true));

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{temp}\"",
            UseShellExecute = true,
            Verb = "runas"
        };
        Process.Start(psi);
    }

    private static string PsQuote(string s) => s.Replace("'", "''", StringComparison.Ordinal);

    private static string BuildIisPs1(string siteName, string appPoolName, int port, string physicalPath)
    {
        var sn = PsQuote(siteName);
        var pool = PsQuote(appPoolName);
        var path = PsQuote(physicalPath);
        return $@"
$ErrorActionPreference = 'Stop'
Import-Module WebAdministration
$siteName = '{sn}'
$poolName = '{pool}'
$port = {port}
$path = '{path}'

if (-not (Test-Path -LiteralPath $path)) {{ throw ""Ruta física no existe: $path"" }}

if (-not (Test-Path ""IIS:\AppPools\$poolName"")) {{
  New-WebAppPool -Name $poolName -Force | Out-Null
}}
Set-ItemProperty ""IIS:\AppPools\$poolName"" -Name managedRuntimeVersion -Value ''

$existing = Get-Website -Name $siteName -ErrorAction SilentlyContinue
if ($existing) {{
  Remove-Website -Name $siteName
}}

New-Website -Name $siteName -Port $port -PhysicalPath $path -ApplicationPool $poolName | Out-Null
Start-WebAppPool -Name $poolName
Write-Host ""Sitio IIS '$siteName' en puerto $port -> $path""
Read-Host ""Pulse Enter para cerrar""
";
    }
}
