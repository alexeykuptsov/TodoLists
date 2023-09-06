namespace TodoLists.ArtifactBuilder;

public static class ExtensionMethods {

    public static void CopyTo(this DirectoryInfo srcPath, string destPath, IEnumerable<string>? excludedDirs = null) {
        Directory.CreateDirectory(destPath);
        excludedDirs ??= Array.Empty<string>();

        var excludedDirsArray = excludedDirs as string[] ?? excludedDirs.ToArray();
        
        Parallel.ForEach(srcPath.GetDirectories("*", SearchOption.AllDirectories), 
            srcInfo =>
            {
                var subPath = srcInfo.FullName[srcPath.FullName.Length..];
                if (excludedDirsArray.Any(x => subPath.StartsWith(x)))
                    return;
                Directory.CreateDirectory($"{destPath}{subPath}");
            });
        Parallel.ForEach(srcPath.GetFiles("*", SearchOption.AllDirectories), 
            srcInfo =>
            {
                var subPath = srcInfo.FullName[srcPath.FullName.Length..];
                if (excludedDirsArray.Any(x => subPath.StartsWith(x)))
                    return;
                File.Copy(srcInfo.FullName, $"{destPath}{subPath}", true);
            });
    }
}