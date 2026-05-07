using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace SSF.Interop.SIIFNacion.Installer;

/// <summary>Comprueba SDK 9, runtime 9, módulo ASP.NET Core en IIS y rol IIS.</summary>
internal static class PrerequisiteProbe
{
    private static readonly string DotNetExe = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
        "dotnet",
        "dotnet.exe");

    public sealed record Status(
        bool Sdk9,
        bool NetCoreRuntime9,
        bool AspNetCoreRuntime9,
        bool AspNetCoreModuleV2,
        bool IisWebServerRole);

    public static Status Evaluate()
    {
        return new Status(
            Sdk9: HasSdk9(),
            NetCoreRuntime9: HasSharedRuntime("Microsoft.NETCore.App"),
            AspNetCoreRuntime9: HasSharedRuntime("Microsoft.AspNetCore.App"),
            AspNetCoreModuleV2: File.Exists(GetAspNetCoreModuleV2Path()),
            IisWebServerRole: HasIisWebServerRole());
    }

    /// <summary>Worker: runtime .NET 9 (shared) o SDK 9.</summary>
    public static bool IsReadyForWorker(Status s) =>
        s.NetCoreRuntime9 || s.Sdk9;

    /// <summary>API IIS: IIS + Hosting Bundle (módulo v2 + runtime ASP.NET Core 9).</summary>
    public static bool IsReadyForIisApi(Status s) =>
        s.IisWebServerRole && s.AspNetCoreModuleV2 && s.AspNetCoreRuntime9;

    public static string FormatSummary(Status s)
    {
        static string B(bool ok) => ok ? "Sí" : "No";
        return
            $".NET SDK 9.x: {B(s.Sdk9)}\n" +
            $"Runtime Microsoft.NETCore.App 9.x: {B(s.NetCoreRuntime9)}\n" +
            $"Runtime Microsoft.AspNetCore.App 9.x: {B(s.AspNetCoreRuntime9)}\n" +
            $"Módulo IIS ASP.NET Core (ANCM v2): {B(s.AspNetCoreModuleV2)}\n" +
            $"Rol IIS (Web Server): {B(s.IisWebServerRole)}";
    }

    private static bool HasSharedRuntime(string frameworkName)
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "dotnet",
            "shared",
            frameworkName);
        if (!Directory.Exists(root))
            return false;
        foreach (var d in Directory.GetDirectories(root))
        {
            var name = Path.GetFileName(d);
            if (name.StartsWith("9.", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static bool HasSdk9()
    {
        if (!File.Exists(DotNetExe))
            return false;
        try
        {
            using var p = Process.Start(new ProcessStartInfo
            {
                FileName = DotNetExe,
                Arguments = "--list-sdks",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (p is null)
                return false;
            var o = p.StandardOutput.ReadToEnd();
            p.WaitForExit(15000);
            foreach (var line in o.Split('\n'))
            {
                var t = line.TrimStart();
                if (t.StartsWith("9.", StringComparison.Ordinal))
                    return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string GetAspNetCoreModuleV2Path() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "IIS",
            "Asp.Net Core Module",
            "V2",
            "aspnetcorev2.dll");

    /// <summary>Comprueba si IIS está instalado (clave de registro estándar).</summary>
    private static bool HasIisWebServerRole()
    {
        using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\InetStp");
        return k != null;
    }
}
