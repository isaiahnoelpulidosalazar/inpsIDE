using inpsIDE.Languages;

namespace inpsIDE.Models;

public enum ProjectKind
{
    /// <summary>Just files, nothing to run.</summary>
    Empty,
    /// <summary>A program that is run with F5.</summary>
    Console,
    /// <summary>A website / web app started with Ctrl+F5.</summary>
    Web
}

/// <summary>
/// A project type shown in the "Create new project" picker. Templates are declared by their language
/// (see <see cref="LanguageSupport.Templates"/>); the only language-less one lives in <see cref="TemplateCatalog"/>.
/// </summary>
/// <param name="LanguageId">Id of the owning <see cref="LanguageSupport"/>, or null for no runtime.</param>
/// <param name="Files">Relative path -> contents. "$PROJECT_NAME$" is replaced with the HTML-encoded project name,
/// "$PROJECT_NAME_TEXT$" with the name escaped for use inside a quoted string in source code.</param>
/// <param name="EntryFile">Main file (opened first; for web projects the file the server runs).</param>
/// <param name="Packages">Packages the language installs after extracting its runtime (e.g. pip packages).</param>
public sealed record ProjectTemplate(
    string Id,
    string DisplayName,
    string Description,
    string? LanguageId,
    ProjectKind Kind,
    IReadOnlyDictionary<string, string> Files,
    string? EntryFile = null,
    IReadOnlyList<string>? Packages = null)
{
    public LanguageSupport? Language => LanguageRegistry.Find(LanguageId);

    /// <summary>"Python - Flask Web Application"</summary>
    public string Title => Language is { } language ? $"{language.DisplayName} - {DisplayName}" : DisplayName;
}
