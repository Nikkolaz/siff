using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SSF.Interop.SIIFNacion.Installer;

public partial class MainWindow : Window
{
    private const string DotNetChannel = "9.0";

    private string? _apiDefaultPath;
    private string? _workerDefaultPath;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TxtDotnetPath.Text = WindowsDeploymentScript.DefaultDotnetPath;
        LoadDefaultsFromEmbeddedTemplates();
        RefreshPrerequisiteStatus();
    }

    private void RefreshPrerequisiteStatus()
    {
        var s = PrerequisiteProbe.Evaluate();
        TxtPrereqStatus.Text = PrerequisiteProbe.FormatSummary(s);
    }

    private void PrereqRefresh_Click(object sender, RoutedEventArgs e) => RefreshPrerequisiteStatus();

    private async void PrereqInstall_Click(object sender, RoutedEventArgs e)
    {
        TxtPrereqProgress.Text = "";
        var s = PrerequisiteProbe.Evaluate();
        var sel = new PrerequisiteBootstrapper.InstallSelection(
            ChkPreIis.IsChecked == true,
            ChkPreSdk.IsChecked == true,
            ChkPreHosting.IsChecked == true);
        var plan = PrerequisiteBootstrapper.ResolvePlan(s, sel);
        if (!plan.RunIis && !plan.RunSdk && !plan.RunHosting)
        {
            MessageBox.Show(this, "No hay pasos pendientes según su selección y el estado actual.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            DotNetReleaseResolver.DownloadPair? urls = null;
            if (plan.RunSdk || plan.RunHosting)
            {
                TxtPrereqProgress.Text = "Consultando metadatos de releases .NET…";
                urls = await DotNetReleaseResolver.ResolveLatestAsync(DotNetChannel).ConfigureAwait(true);
            }

            string? sdkPath = null;
            string? hbPath = null;
            if (urls != null)
            {
                var progress = new Progress<string>(msg => TxtPrereqProgress.Text = msg);
                (sdkPath, hbPath) = await PrerequisiteBootstrapper.DownloadIfNeededAsync(plan, urls, progress, CancellationToken.None).ConfigureAwait(true);
            }

            PrerequisiteBootstrapper.RunElevatedInstallScript(plan, sdkPath, hbPath);
            MessageBox.Show(this, "Se abrirá PowerShell como administrador para ejecutar la instalación.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshPrerequisiteStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Instalador", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            TxtPrereqProgress.Text = "";
        }
    }

    private static string DefaultsDir =>
        Path.Combine(AppContext.BaseDirectory, "Defaults");

    private void LoadDefaultsFromEmbeddedTemplates()
    {
        _apiDefaultPath = Path.Combine(DefaultsDir, "ApiAppsettings.json");
        _workerDefaultPath = Path.Combine(DefaultsDir, "WorkerAppsettings.json");

        if (File.Exists(_apiDefaultPath))
            TxtApiJson.Text = JsonFormatting.TryFormat(JsonFormatting.ReadAllTextUtf8(_apiDefaultPath));
        else
            TxtApiJson.Text = "{\n  \"_comment\": \"No se encontró Defaults\\\\ApiAppsettings.json. Compile el instalador junto al repositorio.\"\n}";

        if (File.Exists(_workerDefaultPath))
            TxtWorkerJson.Text = JsonFormatting.TryFormat(JsonFormatting.ReadAllTextUtf8(_workerDefaultPath));
        else
            TxtWorkerJson.Text = "{\n  \"_comment\": \"No se encontró Defaults\\\\WorkerAppsettings.json.\"\n}";
    }

    private void ReloadApiDefault_Click(object sender, RoutedEventArgs e)
    {
        if (File.Exists(_apiDefaultPath))
            TxtApiJson.Text = JsonFormatting.TryFormat(JsonFormatting.ReadAllTextUtf8(_apiDefaultPath!));
        else
            MessageBox.Show(this, "No hay plantilla en " + _apiDefaultPath, "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void ReloadWorkerDefault_Click(object sender, RoutedEventArgs e)
    {
        if (File.Exists(_workerDefaultPath))
            TxtWorkerJson.Text = JsonFormatting.TryFormat(JsonFormatting.ReadAllTextUtf8(_workerDefaultPath!));
        else
            MessageBox.Show(this, "No hay plantilla en " + _workerDefaultPath, "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void LoadApiFromFolder_Click(object sender, RoutedEventArgs e)
    {
        var dir = TxtApiFolder.Text.Trim();
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
        {
            MessageBox.Show(this, "Indique una carpeta de publicación de la API válida.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var path = Path.Combine(dir, "appsettings.json");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, "No existe appsettings.json en esa carpeta.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TxtApiJson.Text = JsonFormatting.TryFormat(JsonFormatting.ReadAllTextUtf8(path));
    }

    private void LoadWorkerFromFolder_Click(object sender, RoutedEventArgs e)
    {
        var dir = TxtWorkerFolder.Text.Trim();
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
        {
            MessageBox.Show(this, "Indique una carpeta de publicación del Worker válida.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var path = Path.Combine(dir, "appsettings.json");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, "No existe appsettings.json en esa carpeta.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TxtWorkerJson.Text = JsonFormatting.TryFormat(JsonFormatting.ReadAllTextUtf8(path));
    }

    private void FormatApiJson_Click(object sender, RoutedEventArgs e)
    {
        var f = JsonFormatting.TryFormat(TxtApiJson.Text);
        if (!JsonFormatting.TryValidate(f, out var err))
        {
            MessageBox.Show(this, "JSON no válido: " + err, "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TxtApiJson.Text = f;
    }

    private void FormatWorkerJson_Click(object sender, RoutedEventArgs e)
    {
        var f = JsonFormatting.TryFormat(TxtWorkerJson.Text);
        if (!JsonFormatting.TryValidate(f, out var err))
        {
            MessageBox.Show(this, "JSON no válido: " + err, "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TxtWorkerJson.Text = f;
    }

    private void BrowseApiFolder_Click(object sender, RoutedEventArgs e)
    {
        var d = new OpenFolderDialog { Title = "Carpeta publicada de la API" };
        if (d.ShowDialog() == true)
            TxtApiFolder.Text = d.FolderName;
    }

    private void BrowseWorkerFolder_Click(object sender, RoutedEventArgs e)
    {
        var d = new OpenFolderDialog { Title = "Carpeta publicada del Worker" };
        if (d.ShowDialog() == true)
            TxtWorkerFolder.Text = d.FolderName;
    }

    private void SaveBoth_Click(object sender, RoutedEventArgs e)
    {
        if (!JsonFormatting.TryValidate(TxtApiJson.Text, out var errApi))
        {
            MessageBox.Show(this, "API — JSON no válido: " + errApi, "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            Tabs.SelectedIndex = 1;
            return;
        }

        if (!JsonFormatting.TryValidate(TxtWorkerJson.Text, out var errW))
        {
            MessageBox.Show(this, "Worker — JSON no válido: " + errW, "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            Tabs.SelectedIndex = 2;
            return;
        }

        var apiDir = TxtApiFolder.Text.Trim();
        var workerDir = TxtWorkerFolder.Text.Trim();
        if (string.IsNullOrEmpty(apiDir) || !Directory.Exists(apiDir))
        {
            MessageBox.Show(this, "La carpeta de la API no es válida.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(workerDir) || !Directory.Exists(workerDir))
        {
            MessageBox.Show(this, "La carpeta del Worker no es válida.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var apiJson = JsonFormatting.TryFormat(TxtApiJson.Text);
        var workerJson = JsonFormatting.TryFormat(TxtWorkerJson.Text);

        try
        {
            JsonFormatting.WriteAllTextUtf8(Path.Combine(apiDir, "appsettings.json"), apiJson);
            JsonFormatting.WriteAllTextUtf8(Path.Combine(workerDir, "appsettings.json"), workerJson);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo guardar: " + ex.Message, "Instalador", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        TxtApiJson.Text = apiJson;
        TxtWorkerJson.Text = workerJson;
        MessageBox.Show(this, "Se guardaron appsettings.json en ambas carpetas.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void InstallWorkerService_Click(object sender, RoutedEventArgs e)
    {
        var st = PrerequisiteProbe.Evaluate();
        if (!PrerequisiteProbe.IsReadyForWorker(st))
        {
            MessageBox.Show(
                this,
                "Falta el runtime .NET 9 o el SDK en esta máquina (necesario para ejecutar el Worker).\n\n" +
                PrerequisiteProbe.FormatSummary(st) +
                "\n\nUse la pestaña Prerrequisitos para instalarlos.",
                "Instalador",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Tabs.SelectedIndex = 0;
            return;
        }

        var folder = TxtWorkerFolder.Text.Trim();
        var name = TxtServiceName.Text.Trim();
        var dotnet = TxtDotnetPath.Text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show(this, "Indique el nombre del servicio.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            MessageBox.Show(this, "Indique la carpeta de publicación del Worker.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(dotnet))
        {
            MessageBox.Show(this, "Indique la ruta a dotnet.exe.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            WindowsDeploymentScript.RunElevatedWorkerInstall(name, dotnet, folder);
            MessageBox.Show(this, "Se abrirá una ventana elevada de PowerShell para instalar el servicio.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Instalador", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InstallIis_Click(object sender, RoutedEventArgs e)
    {
        var st = PrerequisiteProbe.Evaluate();
        if (!PrerequisiteProbe.IsReadyForIisApi(st))
        {
            MessageBox.Show(
                this,
                "Falta IIS, el Hosting Bundle o los runtimes ASP.NET Core 9 (necesarios para la API en IIS).\n\n" +
                PrerequisiteProbe.FormatSummary(st) +
                "\n\nUse la pestaña Prerrequisitos para instalarlos.",
                "Instalador",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Tabs.SelectedIndex = 0;
            return;
        }

        var folder = TxtApiFolder.Text.Trim();
        var site = TxtIisSite.Text.Trim();
        var pool = TxtIisPool.Text.Trim();

        if (string.IsNullOrEmpty(site) || string.IsNullOrEmpty(pool))
        {
            MessageBox.Show(this, "Complete nombre de sitio y app pool.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(TxtIisPort.Text.Trim(), out var port) || port <= 0 || port > 65535)
        {
            MessageBox.Show(this, "Puerto HTTP no válido.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            MessageBox.Show(this, "Indique la carpeta de publicación de la API (misma que usará IIS).", "Instalador", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            WindowsDeploymentScript.RunElevatedIisSetup(site, pool, port, folder);
            MessageBox.Show(this, "Se abrirá una ventana elevada de PowerShell para configurar IIS.", "Instalador", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Instalador", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
