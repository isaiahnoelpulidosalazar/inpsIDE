using System.Text.RegularExpressions;
using inpsIDE.Models;

namespace inpsIDE.Languages;

/// <summary>
/// Everything inpsIDE needs to know about one language / runtime.
///
/// HOW TO ADD A LANGUAGE
///   1. Create Languages\&lt;Name&gt;\&lt;Name&gt;Language.cs deriving from this class.
///   2. Declare its templates (<see cref="Templates"/>), how to find / install its runtime,
///      and how to build the command lines for "run file" (and optionally "web server").
///   3. Optionally provide a <see cref="TokenPattern"/> for syntax highlighting.
///   4. Add one instance to <see cref="LanguageRegistry.All"/>.
/// Nothing else (forms, editor, runner) needs to change.
/// </summary>
public abstract class LanguageSupport
{
    // ---- identity ----

    /// <summary>Stable id stored in templates ("php", "python").</summary>
    public abstract string Id { get; }

    /// <summary>Shown in the UI ("PHP", "Python").</summary>
    public abstract string DisplayName { get; }

    /// <summary>Sub folder of the project that holds the extracted runtime.</summary>
    public abstract string RuntimeFolderName { get; }

    /// <summary>Source file extensions including the dot, lower case. The first one is used for new files.</summary>
    public abstract IReadOnlyList<string> FileExtensions { get; }

    /// <summary>Project types offered in "Create new project".</summary>
    public abstract IReadOnlyList<ProjectTemplate> Templates { get; }

    // ---- editor ----

    /// <summary>Files (relative to the project root) tried in order when a project is opened.</summary>
    public virtual IReadOnlyList<string> InitialFileCandidates => [];

    /// <summary>Regex with named groups comment / string / tag / variable / keyword / number, or null for no highlighting.</summary>
    public virtual Regex? TokenPattern => null;

    /// <summary>Pressing Enter after one of these characters indents the next line.</summary>
    public virtual string IndentTriggers => "{:";

    public virtual string NewFileName => "untitled" + FileExtensions[0];

    /// <summary>Initial contents of a freshly created file.</summary>
    public virtual string NewFileContent(string filePath) => "";

    // ---- runtime ----

    public string GetRuntimePath(string projectRoot) => Path.Combine(projectRoot, RuntimeFolderName);

    /// <returns>Full path of the interpreter inside the project, or null if the runtime isn't there.</returns>
    public abstract string? FindExecutable(string projectRoot);

    /// <summary>Extracts / installs the runtime (and the template's packages) into the project. May be slow: call from a background thread.</summary>
    /// <returns>null on success, otherwise a warning to show the user. May throw IOException for fatal problems.</returns>
    public abstract string? InstallRuntime(string projectRoot, ProjectTemplate template);

    public virtual string MissingRuntimeMessage(string projectRoot) =>
        $"No {DisplayName} runtime was found in:\n{GetRuntimePath(projectRoot)}\n\nCreate the project from a {DisplayName} template.";

    // ---- running ----

    public bool Handles(string filePath) =>
        FileExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);

    public virtual bool CanRun(string filePath) => Handles(filePath);

    public abstract RunCommand CreateRunFileCommand(string executable, string projectRoot, string filePath);

    /// <summary>Whether "Start Web Server" makes sense for a project created from this template (null = unknown / opened folder).</summary>
    public virtual bool SupportsServer(ProjectTemplate? template) => false;

    public virtual RunCommand? CreateServerCommand(string executable, string projectRoot, ProjectTemplate? template) => null;
}
