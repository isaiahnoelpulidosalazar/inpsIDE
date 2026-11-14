using inpsIDE.Models;

namespace inpsIDE.Languages;

/// <summary>All project templates: every language's templates plus the language-less "Empty Project".</summary>
public static class TemplateCatalog
{
    public const string EmptyId = "empty";

    public static IReadOnlyList<ProjectTemplate> All { get; } = Build();

    public static ProjectTemplate? Find(string? id) =>
        id is null ? null : All.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));

    static IReadOnlyList<ProjectTemplate> Build()
    {
        var list = new List<ProjectTemplate>();
        foreach (LanguageSupport language in LanguageRegistry.All) list.AddRange(language.Templates);

        list.Add(new ProjectTemplate(
            EmptyId,
            "Empty Project (no runtime)",
            "Just a folder and a README. Nothing is extracted, so this is instant.",
            LanguageId: null,
            ProjectKind.Empty,
            new Dictionary<string, string> { ["README.md"] = "# $PROJECT_NAME$\n" },
            EntryFile: "README.md"));

        return list;
    }
}
