using System.Net;
using System.Text.Json;
using inpsNuGet;
using inpsIDE.Languages;
using inpsIDE.Models;

namespace inpsIDE.Services;

public static class ProjectService
{
    sealed record ProjectFile(string Name, string Template, DateTime CreatedUtc);

    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Please enter a project name.";
        }
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return "The project name contains characters that are not allowed in a folder name.";
        }
        if (name.EndsWith('.') || name.EndsWith(' '))
        {
            return "The project name cannot end with a dot or a space.";
        }
        return null;
    }

    public static IdeProject Create(ProjectTemplate template, string name, string location)
    {
        string root = Path.GetFullPath(Path.Combine(location, name));
        if (Directory.Exists(root) && Directory.EnumerateFileSystemEntries(root).Any())
        {
            throw new IOException($"The folder '{root}' already exists and is not empty.");
        }

        Directory.CreateDirectory(root);

        foreach ((string relativePath, string content) in template.Files)
        {
            string full = Path.Combine(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            SimpleFileHandler.Write(full, Expand(content, name));
        }

        LanguageSupport? language = template.Language;
        var project = new IdeProject
        {
            Name = name,
            RootPath = root,
            TemplateId = template.Id,
            Language = language
        };
        project.SetupWarning = language?.InstallRuntime(root, template);

        Save(project);
        return project;
    }

    public static IdeProject Open(string folder)
    {
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder));
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"The folder '{root}' does not exist.");
        }

        string name = new DirectoryInfo(root).Name;
        string templateId = TemplateCatalog.EmptyId;

        string file = Path.Combine(root, IdeProject.FileName);
        if (File.Exists(file))
        {
            try
            {
                ProjectFile? pf = JsonSerializer.Deserialize<ProjectFile>(SimpleFileHandler.Read(file));
                if (pf is not null && !string.IsNullOrWhiteSpace(pf.Name))
                {
                    name = pf.Name;
                    templateId = pf.Template ?? TemplateCatalog.EmptyId;
                }
            }
            catch (JsonException) { }
        }

        LanguageSupport? language = TemplateCatalog.Find(templateId)?.Language ?? LanguageRegistry.Detect(root);

        return new IdeProject
        {
            Name = name,
            RootPath = root,
            TemplateId = templateId,
            Language = language
        };
    }

    static string Expand(string content, string name) => content
        .Replace("$PROJECT_NAME_TEXT$", EscapeForSource(name))
        .Replace("$PROJECT_NAME$", WebUtility.HtmlEncode(name));

    static string EscapeForSource(string text) => text.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");

    static void Save(IdeProject project)
    {
        var data = new ProjectFile(project.Name, project.TemplateId, DateTime.UtcNow);
        SimpleFileHandler.Write(project.ProjectFilePath, JsonSerializer.Serialize(data, JsonOptions));
    }
}
