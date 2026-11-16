using inpsIDE.Languages;

namespace inpsIDE.Models;

public sealed class IdeProject
{
    public const string FileName = "project.inpsproj";

    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public string TemplateId { get; init; } = TemplateCatalog.EmptyId;

    public LanguageSupport? Language { get; init; }

    public string? SetupWarning { get; set; }

    public ProjectTemplate? Template => TemplateCatalog.Find(TemplateId);
    public string ProjectFilePath => Path.Combine(RootPath, FileName);

    public string? RuntimePath => Language?.GetRuntimePath(RootPath);
    public string? RuntimeExecutable => Language?.FindExecutable(RootPath);
    public bool HasRuntime => RuntimeExecutable is not null;
}
