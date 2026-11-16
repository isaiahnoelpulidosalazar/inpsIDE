using System.Text.RegularExpressions;
using inpsIDE.Models;

namespace inpsIDE.Languages;

/*
 * HOW TO ADD A LANGUAGE
 * 1. Create Languages\<Name>\<Name>Language.cs deriving from this class.
 * 2. Declare its templates (see Templates in other languages to know more), how to find/install its runtime,
 *    and how to build the command lines for "run file" (and optionally "web server").
 * 3. Optionally provide a TokenPattern for syntax highlighting.
 * 4. Add one instance to LanguageRegistry.All.
 * Nothing else (forms, editor, runner) needs to change.
 */
public abstract class LanguageSupport
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string RuntimeFolderName { get; }
    public abstract IReadOnlyList<string> FileExtensions { get; }
    public abstract IReadOnlyList<ProjectTemplate> Templates { get; }
    public virtual IReadOnlyList<string> InitialFileCandidates => [];
    public virtual Regex? TokenPattern => null;
    public virtual string IndentTriggers => "{:";
    public virtual string NewFileName => "untitled" + FileExtensions[0];
    public virtual string NewFileContent(string filePath) => "";
    public string GetRuntimePath(string projectRoot) => Path.Combine(projectRoot, RuntimeFolderName);
    public abstract string? FindExecutable(string projectRoot);
    public abstract string? InstallRuntime(string projectRoot, ProjectTemplate template);
    public virtual string MissingRuntimeMessage(string projectRoot) => $"No {DisplayName} runtime was found in:\n{GetRuntimePath(projectRoot)}\n\nCreate the project from a {DisplayName} template.";
    public bool Handles(string filePath) => FileExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);
    public virtual bool CanRun(string filePath) => Handles(filePath);
    public abstract RunCommand CreateRunFileCommand(string executable, string projectRoot, string filePath);
    public virtual bool SupportsServer(ProjectTemplate? template) => false;
    public virtual RunCommand? CreateServerCommand(string executable, string projectRoot, ProjectTemplate? template) => null;
}
