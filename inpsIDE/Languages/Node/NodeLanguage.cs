using System.Text.RegularExpressions;
using inpsIDE.Models;
using inpsIDE.Services;

namespace inpsIDE.Languages.Node;

public sealed class NodeLanguage : LanguageSupport
{
    public const string LanguageId = "node";
    const string ServerUrl = "http://127.0.0.1:3000/";

    static readonly Regex Tokens = new(
        @"(?<comment>//[^\r\n]*|/\*[\s\S]*?\*/)" +
        @"|(?<string>`(?:\\[\s\S]|[^`\\])*`|""(?:\\.|[^""\\\r\n])*""|'(?:\\.|[^'\\\r\n])*')" +
        @"|(?<keyword>\b(?:async|await|break|case|catch|class|const|continue|debugger|default|delete|do|else|export|extends|false|finally|for|from|function|if|import|in|instanceof|let|new|null|of|return|static|super|switch|this|throw|true|try|typeof|undefined|var|void|while|with|yield)\b)" +
        @"|(?<number>\b\d+(?:\.\d+)?\b)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override string Id => LanguageId;
    public override string DisplayName => "Node.js";
    public override string RuntimeFolderName => "node";
    public override IReadOnlyList<string> FileExtensions { get; } = [".js", ".mjs", ".cjs"];
    public override IReadOnlyList<string> InitialFileCandidates { get; } = ["main.js", "server.js", "index.js"];
    public override Regex? TokenPattern => Tokens;
    public override string IndentTriggers => "{([";
    public override string? PackageManagerName => "npm";

    public override IReadOnlyList<ProjectTemplate> Templates { get; } =
    [
        new ProjectTemplate(
            "node-console",
            "Console Application",
            "A command-line Node.js script with the bundled Node.js runtime. Run it with F5. Type into the console to answer readline prompts.",
            LanguageId,
            ProjectKind.Console,
            new Dictionary<string, string>
            {
                ["main.js"] = """
                    const readline = require("readline");

                    console.log("Welcome to $PROJECT_NAME_TEXT$ (Node.js " + process.versions.node + ")");

                    const rl = readline.createInterface({ input: process.stdin, output: process.stdout });

                    rl.question("What is your name? ", (name) => {
                        console.log("Hello, " + name + "!");
                        rl.close();
                    });
                    """,
                ["package.json"] = """
                    {
                        "name": "app",
                        "version": "1.0.0",
                        "private": true,
                        "main": "main.js"
                    }
                    """
            },
            EntryFile: "main.js"),

        new ProjectTemplate(
            "node-web",
            "Web Application",
            "A small website served by Node.js' built-in http module on port 3000. Start it with Ctrl+F5.",
            LanguageId,
            ProjectKind.Web,
            new Dictionary<string, string>
            {
                ["server.js"] = """
                    const http = require("http");

                    const port = 3000;

                    const server = http.createServer((request, response) => {
                        response.writeHead(200, { "Content-Type": "text/html; charset=utf-8" });
                        response.end("<h1>Hello from $PROJECT_NAME$</h1><p>Running on Node.js " + process.versions.node + ".</p>");
                    });

                    server.listen(port, "127.0.0.1", () => {
                        console.log("Listening on http://127.0.0.1:" + port + "/");
                    });
                    """,
                ["package.json"] = """
                    {
                        "name": "app",
                        "version": "1.0.0",
                        "private": true,
                        "main": "server.js"
                    }
                    """
            },
            EntryFile: "server.js"),

        new ProjectTemplate(
            "node-empty",
            "Empty Project",
            "An empty folder with a single index.js, a package.json and the bundled Node.js runtime.",
            LanguageId,
            ProjectKind.Empty,
            new Dictionary<string, string>
            {
                ["index.js"] = "",
                ["package.json"] = """
                    {
                        "name": "app",
                        "version": "1.0.0",
                        "private": true,
                        "main": "index.js"
                    }
                    """
            },
            EntryFile: "index.js")
    ];

    public static string? FindNodeExecutable(string runtimeDirectory)
    {
        if (!Directory.Exists(runtimeDirectory))
        {
            return null;
        }

        string direct = Path.Combine(runtimeDirectory, "node.exe");
        if (File.Exists(direct))
        {
            return direct;
        }

        foreach (string directory in Directory.EnumerateDirectories(runtimeDirectory))
        {
            string nested = Path.Combine(directory, "node.exe");
            if (File.Exists(nested))
            {
                return nested;
            }
        }

        return null;
    }

    public override string? FindExecutable(string projectRoot) => FindNodeExecutable(GetRuntimePath(projectRoot));

    public override string? InstallRuntime(string projectRoot, ProjectTemplate template) =>
        NodeRuntime.InstallInto(GetRuntimePath(projectRoot))
            ? null
            : "This build of inpsIDE has no Node.js zip embedded, so the project was created without a runtime.\n\n" +
              "Put the Node.js Windows x64 zip into inpsIDE\\Resources, rebuild, and create the project again.";

    public override string MissingRuntimeMessage(string projectRoot) =>
        $"No node.exe was found in:\n{GetRuntimePath(projectRoot)}\n\n" +
        "Create the project from a Node.js template (with the Node.js zip embedded) or copy a Node.js build into that folder.";

    public override RunCommand CreateRunFileCommand(string executable, string projectRoot, string filePath) => new(executable, [filePath], projectRoot, $"node \"{filePath}\"", RuntimeEnvironment(executable));

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
            $"node \"{script}\"",
            RuntimeEnvironment(executable),
            Url: ServerUrl,
            OpenBrowserDelayMs: 1000);
    }

    public override RunCommand? CreatePackageCommand(string executable, string projectRoot, IReadOnlyList<string> arguments)
    {
        string? nodeDirectory = Path.GetDirectoryName(executable);
        if (nodeDirectory is null)
        {
            return null;
        }

        string cli = Path.Combine(nodeDirectory, "node_modules", "npm", "bin", "npm-cli.js");
        if (!File.Exists(cli))
        {
            return null;
        }

        return new RunCommand(
            executable,
            [cli, .. arguments],
            projectRoot,
            "npm " + ArgumentParser.Join(arguments),
            RuntimeEnvironment(executable));
    }

    static Dictionary<string, string> RuntimeEnvironment(string executable)
    {
        string nodeDirectory = Path.GetDirectoryName(executable) ?? "";
        string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        return new Dictionary<string, string>
        {
            ["PATH"] = nodeDirectory + Path.PathSeparator + currentPath,
            ["NPM_CONFIG_UPDATE_NOTIFIER"] = "false"
        };
    }
}
