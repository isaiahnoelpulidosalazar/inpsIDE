using System.Text.RegularExpressions;
using inpsIDE.Models;

namespace inpsIDE.Languages;

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
    public virtual string? PackageManagerName => null;
    public virtual string? PreparePackageManager(string executable, string projectRoot) => null;
    public virtual RunCommand? CreatePackageCommand(string executable, string projectRoot, IReadOnlyList<string> arguments) => null;
}
