using System.Text;
using System.Text.RegularExpressions;
using inpsIDE.Models;
using inpsIDE.Services;
using inpsNuGet;

namespace inpsIDE.Languages.Python;

public sealed class PythonLanguage : LanguageSupport
{
    public const string LanguageId = "python";
    const string ServerUrl = "http://127.0.0.1:8000/";
    static readonly IReadOnlyDictionary<string, string> PythonEnvironment = new Dictionary<string, string>
    {
        ["PYTHONUNBUFFERED"] = "1",
        ["PYTHONUTF8"] = "1"
    };

    static readonly Regex Tokens = new(
        @"(?<comment>#[^\r\n]*)" +
        @"|(?<string>""""""[\s\S]*?""""""|'''[\s\S]*?'''|""(?:\\.|[^""\\\r\n])*""|'(?:\\.|[^'\\\r\n])*')" +
        @"|(?<tag>@[A-Za-z_][\w.]*)" +
        @"|(?<keyword>\b(?:False|None|True|and|as|assert|async|await|break|class|continue|def|del|elif|else|except|finally|for|from|global|if|import|in|is|lambda|nonlocal|not|or|pass|raise|return|self|try|while|with|yield)\b)" +
        @"|(?<number>\b\d+(?:\.\d+)?\b)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override string Id => LanguageId;
    public override string DisplayName => "Python";
    public override string RuntimeFolderName => "Python";
    public override IReadOnlyList<string> FileExtensions { get; } = [".py", ".pyw"];
    public override IReadOnlyList<string> InitialFileCandidates { get; } = ["main.py", "app.py"];
    public override Regex? TokenPattern => Tokens;
    public override string IndentTriggers => ":";
    public override string? PackageManagerName => "pip";

    public override IReadOnlyList<ProjectTemplate> Templates { get; } =
    [
        new ProjectTemplate(
            "python-console",
            "Console Application",
            "A command-line Python script with the bundled Python 3.13 runtime. Run it with F5. Type into the console to answer input().",
            LanguageId,
            ProjectKind.Console,
            new Dictionary<string, string>
            {
                ["main.py"] = """
                    import sys

                    print("Welcome to $PROJECT_NAME_TEXT$ (Python " + sys.version.split()[0] + ")")

                    name = input("What is your name? ")
                    print("Hello, " + name + "!")
                    """
            },
            EntryFile: "main.py"),

        new ProjectTemplate(
            "python-flask",
            "Flask Web Application",
            "A Flask website with the bundled Python 3.13 runtime. Flask is installed with pip when the project is created (internet required). Start it with Ctrl+F5.",
            LanguageId,
            ProjectKind.Web,
            new Dictionary<string, string>
            {
                ["app.py"] = """
                    from flask import Flask

                    app = Flask(__name__)


                    @app.route("/")
                    def index():
                        return "<h1>Hello from $PROJECT_NAME$</h1><p>Running on Flask.</p>"


                    if __name__ == "__main__":
                        app.run(host="127.0.0.1", port=8000)
                    """,
                ["requirements.txt"] = "flask\n"
            },
            EntryFile: "app.py",
            Packages: ["flask"])
    ];

    public override string? FindExecutable(string projectRoot)
    {
        string exe = Path.Combine(GetRuntimePath(projectRoot), "python.exe");
        return File.Exists(exe) ? exe : null;
    }

    public override string? InstallRuntime(string projectRoot, ProjectTemplate template)
    {
        try
        {
            var py = new PyCS(projectRoot);
            py.Initialize();
            AddProjectRootToSearchPath(GetRuntimePath(projectRoot));

            if (template.Packages is not { Count: > 0 } packages)
            {
                return null;
            }

            var pip = py.InstallPip();
            if (pip is not null && pip.ExitCode != 0)
            {
                return "Python was installed, but pip could not be installed (is there an internet connection?):\n\n" + pip.Combined;
            }

            var result = py.Pip(packages.ToArray());
            return result.ExitCode == 0
                ? null
                : $"Python was installed, but 'pip install {string.Join(' ', packages)}' failed (is there an internet connection?):\n\n{result.Combined}";
        }
        catch (Exception ex)
        {
            return "The Python runtime could not be set up:\n\n" + ex.Message;
        }
    }

    public override string? PreparePackageManager(string executable, string projectRoot)
    {
        string pipFolder = Path.Combine(GetRuntimePath(projectRoot), "Lib", "site-packages", "pip");
        if (Directory.Exists(pipFolder))
        {
            return null;
        }

        try
        {
            var pip = new PyCS(projectRoot).InstallPip();
            return pip is not null && pip.ExitCode != 0
                ? "pip could not be installed (is there an internet connection?):\n\n" + pip.Combined
                : null;
        }
        catch (Exception ex)
        {
            return "pip could not be set up:\n\n" + ex.Message;
        }
    }

    public override RunCommand? CreatePackageCommand(string executable, string projectRoot, IReadOnlyList<string> arguments) =>
        new(executable, ["-m", "pip", .. arguments], projectRoot, "pip " + ArgumentParser.Join(arguments), PythonEnvironment);

    static void AddProjectRootToSearchPath(string pythonDirectory)
    {
        string pth = Path.Combine(pythonDirectory, "python313._pth");
        if (!File.Exists(pth))
        {
            return;
        }

        List<string> lines = File.ReadAllLines(pth).ToList();
        if (lines.Any(l => l.Trim() == ".."))
        {
            return;
        }

        int dot = lines.FindIndex(l => l.Trim() == ".");
        lines.Insert(dot >= 0 ? dot + 1 : lines.Count, "..");
        File.WriteAllLines(pth, lines, new UTF8Encoding(false));
    }

    public override RunCommand CreateRunFileCommand(string executable, string projectRoot, string filePath) => new(executable, [filePath], projectRoot, $"python \"{filePath}\"", PythonEnvironment);

    public override bool SupportsServer(ProjectTemplate? template) => template is { Kind: ProjectKind.Web, EntryFile: not null };

    public override RunCommand? CreateServerCommand(string executable, string projectRoot, ProjectTemplate? template)
    {
        if (template is not { Kind: ProjectKind.Web, EntryFile: { } entry })
        {
            return null;
        }

        string script = Path.Combine(projectRoot, entry);
        if (!File.Exists(script))
        {
            return null;
        }

        return new RunCommand(
            executable,
            [script],
            projectRoot,
            $"python \"{script}\"",
            PythonEnvironment,
            Url: ServerUrl,
            OpenBrowserDelayMs: 1500);
    }
}
