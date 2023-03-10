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

throw new NotImplementedException(
    "We should implement copying of App and Launcher bin folders to the solution bin, then archiving bin to ZIP.");

// Console.WriteLine($"Success. Solution dir: {solutionDir}");
