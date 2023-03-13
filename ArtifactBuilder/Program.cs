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

throw new NotImplementedException("We should implement archiving bin to ZIP.");

// Console.WriteLine($"Success. Solution dir: {solutionDir}");
