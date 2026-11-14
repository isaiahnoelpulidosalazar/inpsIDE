using inpsIDE.Languages;

namespace inpsIDE.Models;

/// <summary>An opened project: a folder on disk, optionally with a bundled language runtime in a sub folder.</summary>
public sealed class IdeProject
{
    public const string FileName = "project.inpsproj";

    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public string TemplateId { get; init; } = TemplateCatalog.EmptyId;

    /// <summary>The project's main language (null for projects without a runtime).</summary>
    public LanguageSupport? Language { get; init; }

    /// <summary>Set when the project was created but something (runtime extraction, pip...) went wrong.</summary>
    public string? SetupWarning { get; set; }

    public ProjectTemplate? Template => TemplateCatalog.Find(TemplateId);
    public string ProjectFilePath => Path.Combine(RootPath, FileName);

    public string? RuntimePath => Language?.GetRuntimePath(RootPath);
    public string? RuntimeExecutable => Language?.FindExecutable(RootPath);
    public bool HasRuntime => RuntimeExecutable is not null;
}
