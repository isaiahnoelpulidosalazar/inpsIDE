using inpsIDE.Languages;

namespace inpsIDE.Models;

public enum ProjectKind
{
    Empty,
    Console,
    Web
}

public sealed record ProjectTemplate(
    string Id,
    string DisplayName,
    string Description,
    string? LanguageId,
    ProjectKind Kind,
    IReadOnlyDictionary<string, string> Files,
    string? EntryFile = null,
    IReadOnlyList<string>? Packages = null
)
{
    public LanguageSupport? Language => LanguageRegistry.Find(LanguageId);

    public string Title => Language is { } language ? $"{language.DisplayName} - {DisplayName}" : DisplayName;
}
