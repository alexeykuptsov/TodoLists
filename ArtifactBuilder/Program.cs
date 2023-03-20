using System.Diagnostics;
using TodoLists.ArtifactBuilder;

var solutionDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../.."));

var binDir = Path.Combine(solutionDir, "bin");
if (!Directory.Exists(binDir))
    Directory.CreateDirectory(binDir);

var pgsqlUnzippedDir = Environment.GetEnvironmentVariable("TODO_LISTS_PGSQL_DIR");
var pgsqlDir = Path.Combine(binDir, "pgsql");
if (Directory.Exists(pgsqlDir))
    Directory.Delete(pgsqlDir, true);
new DirectoryInfo(pgsqlUnzippedDir!).CopyTo(pgsqlDir, new[] {Path.DirectorySeparatorChar + "pgAdmin 4"});

var appDir = Path.Combine(binDir, "App");
if (Directory.Exists(appDir))
    Directory.Delete(appDir, true);
new DirectoryInfo(Path.Combine(solutionDir, "App/bin/Debug/net7.0")).CopyTo(appDir);
new DirectoryInfo(Path.Combine(solutionDir, "App/wwwroot")).CopyTo(Path.Combine(appDir, "wwwroot"));

var launcherDir = Path.Combine(binDir, "Launcher");
if (Directory.Exists(launcherDir))
    Directory.Delete(launcherDir, true);
new DirectoryInfo(Path.Combine(solutionDir, "Launcher/bin/Debug/net7.0-windows")).CopyTo(launcherDir);

var artifactsDir = Path.Combine(solutionDir, "artifacts");
if (Directory.Exists(artifactsDir))
    Directory.Delete(artifactsDir, true);
Directory.CreateDirectory(artifactsDir);

var process = Process.Start(new ProcessStartInfo
{
    FileName = "7z",
    Arguments = "a artifacts/TodoLists.zip bin/*",
    WorkingDirectory = solutionDir,
});
await process!.WaitForExitAsync();
