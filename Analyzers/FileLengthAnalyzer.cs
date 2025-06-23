using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace TodoLists.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FileLengthAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor FileTooLongRule = new DiagnosticDescriptor(
        "AK0001",
        "File is too long",
        "File '{0}' has {1} lines, which exceeds the recommended maximum of 200 lines",
        "Maintainability",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Files with more than 200 lines can be difficult to maintain and understand. Consider breaking this file into smaller, more focused components.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(FileTooLongRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        // Analyze C# syntax trees
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        
        // Analyze additional files (non-C# files like .vue, .js, .ts, .md)
        context.RegisterAdditionalFileAction(AnalyzeAdditionalFile);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxTree = context.Tree;
        var filePath = syntaxTree.FilePath;
        
        if (string.IsNullOrEmpty(filePath))
            return;

        var fileName = Path.GetFileName(filePath);
        
        // Skip generated files
        if (IsGeneratedFile(fileName, filePath))
            return;

        // Count lines in the syntax tree
        var sourceText = syntaxTree.GetText();
        var lines = sourceText.Lines;
        
        if (lines.Count > 200)
        {
            var diagnostic = Diagnostic.Create(
                FileTooLongRule,
                Location.Create(syntaxTree, new TextSpan(0, 1)),
                fileName,
                lines.Count);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeAdditionalFile(AdditionalFileAnalysisContext context)
    {
        var additionalFile = context.AdditionalFile;
        var filePath = additionalFile.Path;
        var fileName = Path.GetFileName(filePath);
        
        // Skip generated files
        if (IsGeneratedFile(fileName, filePath))
            return;

        // Only analyze specific file types
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (!IsAnalyzableFileType(extension))
            return;

        try
        {
            var sourceText = additionalFile.GetText(context.CancellationToken);
            if (sourceText == null)
                return;

            var lines = sourceText.Lines;
            
            if (lines.Count > 200)
            {
                var diagnostic = Diagnostic.Create(
                    FileTooLongRule,
                    Location.Create(filePath, new TextSpan(0, 0), new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)),
                    fileName,
                    lines.Count);
                
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception)
        {
            // File might be inaccessible or in an unsupported format, skip analysis
        }
    }

    private static bool IsAnalyzableFileType(string extension)
    {
        return extension switch
        {
            ".md" => true,
            ".vue" => true,
            ".js" => true,
            ".ts" => true,
            _ => false
        };
    }

    private static bool IsGeneratedFile(string fileName, string filePath)
    {
        // Skip common generated files and directories
        if (fileName.Contains(".Designer.") ||
            fileName.Contains(".generated.") ||
            fileName.EndsWith(".g.cs") ||
            fileName.EndsWith(".g.i.cs") ||
            filePath.Contains("\\bin\\") ||
            filePath.Contains("\\obj\\") ||
            filePath.Contains("/bin/") ||
            filePath.Contains("/obj/") ||
            filePath.Contains("Migrations") ||
            filePath.Contains("node_modules") ||
            filePath.Contains("\\.git\\") ||
            filePath.Contains("/.git/") ||
            fileName.StartsWith("package-lock.") ||
            fileName.Equals("package.json"))
        {
            return true;
        }

        return false;
    }
}